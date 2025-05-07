using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Automatically generated code from flowcharts
// Generated: 07.05.2025 22:28:13

namespace GeneratedMultithreadedProgram
{
    class Program
    {
        private static int V1 = 3;
        private static int _4_ = 4;
        private static int V2 = 0;

        private static readonly object _syncLock = new object();

        private static void Diagram_1()
        {
            try
            {
                lock (_syncLock)
                {
                        Start:
                        // Start execution
                        goto Block_1;
                        End:
                        // End execution
                        return;
                        Block_1:
                        Console.WriteLine("V1 = " + V1);
                        goto Block_2;
                        Block_2:
                        if (V1 <= 45)
                            goto Block_4;
                        else
                            goto Block_3;
                        Block_3:
                        Console.WriteLine("V1 = " + V1);
                        goto Block_7;
                        Block_4:
                        V1 = V1 * _4_;
                        goto Block_5;
                        Block_5:
                        V2 = V1;
                        goto Block_6;
                        Block_6:
                        Console.WriteLine("V2 = " + V2);
                        goto Block_2;
                        Block_7:
                        System.Threading.Thread.Sleep(100);
                        goto End;
                        Block_8:
                        System.Threading.Thread.Sleep(222);
                        goto End;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Diagram_2: {ex.Message}");
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting multithreaded program...");
                var tasks = new List<Task>();

                Console.WriteLine("Starting Diagram_1...");
                var task1 = Task.Run(() => Diagram_1());
                tasks.Add(task1);

                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("All threads completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Main: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
            }
        }
    }
}
