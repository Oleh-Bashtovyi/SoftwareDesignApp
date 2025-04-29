using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SoftwareDesignApp.Core
{
    public class CompilationService
    {
        // Result of the compilation and execution
        public class ExecutionResult
        {
            public bool Success { get; set; }
            public string Output { get; set; }
            public string ErrorMessage { get; set; }
            public string GeneratedCode { get; set; }
        }

        public ExecutionResult CompileAndExecuteFromString(string sourceCode)
        {
            var result = new ExecutionResult();
            result.GeneratedCode = sourceCode;

            try
            {
                // Parse the syntax tree from the source code
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                // Get assembly references
                var references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location),
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"))
                };

                // Create compilation
                string assemblyName = "DynamicAssembly_" + Guid.NewGuid().ToString("N");
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                                                         optimizationLevel: OptimizationLevel.Release,
                                                         assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));

                // Compile to memory stream
                using (var ms = new MemoryStream())
                {
                    EmitResult emitResult = compilation.Emit(ms);

                    if (!emitResult.Success)
                    {
                        // Compilation failed
                        result.Success = false;

                        StringBuilder errorMessages = new StringBuilder();
                        foreach (Diagnostic diagnostic in emitResult.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
                        {
                            errorMessages.AppendLine($"Line {diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: {diagnostic.GetMessage()}");
                        }

                        result.ErrorMessage = errorMessages.ToString();
                        return result;
                    }

                    // Compilation succeeded, load the assembly
                    ms.Seek(0, SeekOrigin.Begin);

                    // Load the assembly in a custom context to be able to unload it later
                    var assemblyLoadContext = new AssemblyLoadContext(assemblyName, isCollectible: true);
                    Assembly assembly = assemblyLoadContext.LoadFromStream(ms);

                    // Find entry point method (Main or Execute)
                    MethodInfo entryPointMethod = null;
                    Type entryPointType = null;

                    foreach (Type type in assembly.GetTypes())
                    {
                        // Look for Execute method first
                        MethodInfo method = type.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
                        if (method != null)
                        {
                            entryPointType = type;
                            entryPointMethod = method;
                            break;
                        }

                        // If not found, look for Main method
                        method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            entryPointType = type;
                            entryPointMethod = method;
                            break;
                        }
                    }

                    if (entryPointMethod != null)
                    {
                        // Capture console output
                        var outputCapture = new StringBuilder();
                        var stringWriter = new StringWriter(outputCapture);
                        var originalOutput = Console.Out;

                        try
                        {
                            // Redirect console output
                            Console.SetOut(stringWriter);

                            // Execute the method based on its signature
                            if (entryPointMethod.ReturnType == typeof(string))
                            {
                                // If method returns string, use it
                                string methodOutput = (string)entryPointMethod.Invoke(null, null);
                                outputCapture.Append(methodOutput);
                            }
                            else
                            {
                                // Otherwise just call the method and use captured output
                                ParameterInfo[] methodParams = entryPointMethod.GetParameters();
                                object[] args = null;

                                if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(string[]))
                                {
                                    args = new object[] { new string[0] };
                                }

                                entryPointMethod.Invoke(null, args);
                            }

                            result.Output = outputCapture.ToString();
                            result.Success = true;
                        }
                        finally
                        {
                            // Restore original output
                            Console.SetOut(originalOutput);

                            // Unload the assembly context to free memory
                            assemblyLoadContext.Unload();
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "No Execute or Main method found in compiled code.";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Error while compiling or running: {ex.Message}\n{ex.StackTrace}";
            }

            return result;
        }
    }
}