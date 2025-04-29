namespace SoftwareDesignApp.Core
{
    public class DiagramProcessor
    {
        public string ProcessThreads(List<(string threadName, List<Block> blocks)> threadData, Dictionary<string, int>? sharedVariablessss = null)
        {
            var threads = new List<DiagramThread>();

            // Обробляємо кожен потік і додаємо його до списку
            foreach (var (threadName, blocks) in threadData)
            {
                var thread = new DiagramThread(threadName);

                foreach (var block in blocks)
                {
                    thread.Blocks.Add(block);

                    // Перевіряємо, чи є блок стартовим
                    if (block is StartBlock start)
                    {
                        thread.StartBlock = start;
                    }
                }

                threads.Add(thread);
            }

            var generator = new CodeGenerator();
            string resultCode = generator.GenerateCode(threads, sharedVariablessss);

            return resultCode;
        }
    }
}