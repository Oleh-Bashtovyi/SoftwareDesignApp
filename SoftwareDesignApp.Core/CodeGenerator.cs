using System.Text;

namespace SoftwareDesignApp.Core;

public class CodeGenerator
{
    public string GenerateCode(List<DiagramThread> threads, Dictionary<string, int>? CommonVariables = null)
    {
        // Збираємо всі змінні, що використовуються в блоках
        var sharedVariables = CollectSharedVariables(threads);

        if (CommonVariables != null)
        {
            foreach (var item in CommonVariables)
            {
                sharedVariables[item.Key] = item.Value;
            }
        }

        StringBuilder code = new StringBuilder();

        // Додаємо заголовки
        code.AppendLine("using System;");
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("using System.Threading;");
        code.AppendLine("using System.Threading.Tasks;");
        code.AppendLine();
        code.AppendLine("// Automatically generated code from flowcharts");
        code.AppendLine($"// Generated: {DateTime.Now}");
        code.AppendLine();
        code.AppendLine("namespace GeneratedMultithreadedProgram");
        code.AppendLine("{");
        code.AppendLine("    class Program");
        code.AppendLine("    {");

        // Оголошення спільних змінних
        foreach (var kvp in sharedVariables)
        {
            code.AppendLine($"        private static int {kvp.Key} = {kvp.Value};");
        }
        code.AppendLine();

        // Додаємо об'єкт синхронізації
        code.AppendLine("        private static readonly object _syncLock = new object();");
        code.AppendLine();

        // Генерація методів для потоків
        for (int i = 0; i < threads.Count; i++)
        {
            string threadName = !string.IsNullOrEmpty(threads[i].Name)
                ? threads[i].Name
                : $"Thread_{i + 1}";

            code.AppendLine($"        private static void {threadName}()");
            code.AppendLine("        {");
            code.AppendLine("            try");
            code.AppendLine("            {");
            code.AppendLine("                lock (_syncLock)");
            code.AppendLine("                {");

            // Генерація коду для кожного блоку
            if (threads[i].Blocks.Count > 0)
            {
                foreach (var block in threads[i].Blocks)
                {
                    string blockCode = block.GenerateCode(6);
                    code.AppendLine(blockCode);
                }
            }

            code.AppendLine("                }");
            code.AppendLine("            }");
            code.AppendLine("            catch (Exception ex)");
            code.AppendLine("            {");
            code.AppendLine($"                Console.WriteLine($\"Error in {threadName}: {{ex.Message}}\");");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine();
        }

        // Генерація методу Main
        code.AppendLine("        static void Main(string[] args)");
        code.AppendLine("        {");
        code.AppendLine("            try");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine(\"Starting multithreaded program...\");");
        code.AppendLine("                var tasks = new List<Task>();");
        code.AppendLine();

        // Створення потоків
        for (int i = 0; i < threads.Count; i++)
        {
            string threadName = !string.IsNullOrEmpty(threads[i].Name)
                ? threads[i].Name
                : $"Thread_{i + 1}";

            code.AppendLine($"                Console.WriteLine(\"Starting {threadName}...\"");
            code.AppendLine($"                var task{i + 1} = Task.Run(() => {threadName}());");
            code.AppendLine($"                tasks.Add(task{i + 1});");
            code.AppendLine();
        }

        // Чекання завершення всіх потоків
        code.AppendLine("                Task.WaitAll(tasks.ToArray());");
        code.AppendLine("                Console.WriteLine(\"All threads completed.\");");
        code.AppendLine("            }");
        code.AppendLine("            catch (Exception ex)");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine($\"Error in Main: {ex.Message}\");");
        code.AppendLine("            }");
        code.AppendLine("            finally");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine(\"Press any key to exit...\");");
        code.AppendLine("            }");
        code.AppendLine("        }");
        code.AppendLine("    }");
        code.AppendLine("}");

        return code.ToString();
    }

    private Dictionary<string, int> CollectSharedVariables(List<DiagramThread> threads)
    {
        var variables = new Dictionary<string, int>();

        foreach (var thread in threads)
        {
            foreach (var block in thread.Blocks)
            {
                if (block is AssignmentBlock assignBlock)
                {
                    if (!variables.ContainsKey(assignBlock.TargetVariable))
                        variables[assignBlock.TargetVariable] = 0;

                    if (!assignBlock.SourceVariable.Contains(" ") &&
                        !int.TryParse(assignBlock.SourceVariable, out _))
                    {
                        if (!variables.ContainsKey(assignBlock.SourceVariable))
                            variables[assignBlock.SourceVariable] = 0;
                    }
                }
                else if (block is ConstantBlock constBlock)
                {
                    variables[constBlock.TargetVariable] = constBlock.Value;
                }
                else if (block is InputBlock inputBlock)
                {
                    if (!variables.ContainsKey(inputBlock.TargetVariable))
                        variables[inputBlock.TargetVariable] = 0;
                }
                else if (block is PrintBlock printBlock)
                {
                    if (!variables.ContainsKey(printBlock.SourceVariable))
                        variables[printBlock.SourceVariable] = 0;
                }
                else if (block is ConditionBlock condBlock)
                {
                    if (!variables.ContainsKey(condBlock.Variable))
                        variables[condBlock.Variable] = 0;
                }
            }
        }

        return variables;
    }

}