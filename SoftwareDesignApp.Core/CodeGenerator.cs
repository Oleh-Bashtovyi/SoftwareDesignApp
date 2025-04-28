using System.Collections.Generic;
using System;
using System.Text;

namespace SoftwareDesignApp.Core;

public class CodeGenerator
{
    public CodeGenerator() { }

    public string GenerateCode(List<DiagramThread> threads)
    {
        foreach (var thread in threads)
        {
            thread.ChekStartBlock();
        }

        // Збираємо всі змінні, що використовуються в блоках
        var sharedVariables = CollectSharedVariables(threads);

        StringBuilder code = new StringBuilder();

        // Додаємо заголовки
        code.AppendLine("using System;");
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("using System.Threading;");
        code.AppendLine("using System.Threading.Tasks;");
        code.AppendLine();
        code.AppendLine("// Автоматично згенерований код з блок-схем");
        code.AppendLine($"// Згенеровано: {DateTime.Now}");
        code.AppendLine();
        code.AppendLine("namespace GeneratedMultithreadedProgram");
        code.AppendLine("{");
        code.AppendLine("    class Program");
        code.AppendLine("    {");

        // Оголошення спільних змінних
        code.AppendLine("        // Спільні змінні");
        foreach (var variable in sharedVariables)
        {
            code.AppendLine($"        private static int {variable} = 0;");
        }
        code.AppendLine();

        // Додаємо об'єкт синхронізації
        //code.AppendLine("        // Об'єкт синхронізації для потокобезпечності");
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
            //code.AppendLine("                // Потокобезпечний доступ до спільних змінних");
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
            else
            {
                //code.AppendLine("                    // Для цього потоку не визначено блоків");
            }

            code.AppendLine("                }");
            code.AppendLine("            }");
            code.AppendLine("            catch (Exception ex)");
            code.AppendLine("            {");
            code.AppendLine($"                Console.WriteLine($\"Помилка в {threadName}: {{ex.Message}}\");");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine();
        }

        // Генерація методу Main
        code.AppendLine("        static void Main(string[] args)");
        code.AppendLine("        {");
        code.AppendLine("            try");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine(\"Запуск багатопотокової програми...\");");
        code.AppendLine("                var tasks = new List<Task>();");
        code.AppendLine();

        // Створення потоків
        for (int i = 0; i < threads.Count; i++)
        {
            string threadName = !string.IsNullOrEmpty(threads[i].Name)
                ? threads[i].Name
                : $"Thread_{i + 1}";

            code.AppendLine($"                Console.WriteLine(\"Запуск {threadName}...\");");
            code.AppendLine($"                var task{i + 1} = Task.Run(() => {threadName}());");
            code.AppendLine($"                tasks.Add(task{i + 1});");
            code.AppendLine();
        }

        // Чекання завершення всіх потоків
        //code.AppendLine("                // Чекати завершення всіх потоків");
        code.AppendLine("                Task.WaitAll(tasks.ToArray());");
        code.AppendLine("                Console.WriteLine(\"Всі потоки завершено.\");");
        code.AppendLine("            }");
        code.AppendLine("            catch (Exception ex)");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine($\"Помилка в Main: {ex.Message}\");");
        code.AppendLine("            }");
        code.AppendLine("            finally");
        code.AppendLine("            {");
        code.AppendLine("                Console.WriteLine(\"Натисніть будь-яку клавішу для виходу...\");");
        //code.AppendLine("                Console.ReadKey();");
        code.AppendLine("            }");
        code.AppendLine("        }");
        code.AppendLine("    }");
        code.AppendLine("}");

        return code.ToString();
    }

    private HashSet<string> CollectSharedVariables(List<DiagramThread> threads)
    {
        var variables = new HashSet<string>();

        foreach (var thread in threads)
        {
            foreach (var block in thread.Blocks)
            {
                // Отримати змінні залежно від типу блоку
                if (block is AssignmentBlock assignBlock)
                {
                    variables.Add(assignBlock.TargetVariable);

                    // Перевірити, чи source не є виразом
                    if (!assignBlock.SourceVariable.Contains(" ") &&
                        !int.TryParse(assignBlock.SourceVariable, out _))
                    {
                        variables.Add(assignBlock.SourceVariable);
                    }
                }
                else if (block is ConstantBlock constBlock)
                {
                    variables.Add(constBlock.TargetVariable);
                }
                else if (block is InputBlock inputBlock)
                {
                    variables.Add(inputBlock.TargetVariable);
                }
                else if (block is PrintBlock printBlock)
                {
                    variables.Add(printBlock.SourceVariable);
                }
                else if (block is ConditionBlock condBlock)
                {
                    variables.Add(condBlock.Variable);
                }
            }
        }

        return variables;
    }
}