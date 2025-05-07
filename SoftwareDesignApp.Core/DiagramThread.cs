namespace SoftwareDesignApp.Core;

public class DiagramThread
{
    public string Name { get; }
    public List<Block> Blocks { get; }
    public StartBlock StartBlock { get; }
    public EndBlock EndBlock { get; }

    public DiagramThread(string name, List<Block> blocks)
    {
        Name = name;
        Blocks = blocks?.Where(x => x is not Core.StartBlock || x is not Core.EndBlock).ToList() ?? throw new ArgumentNullException(nameof(blocks));
        StartBlock = blocks.OfType<StartBlock>().FirstOrDefault() ?? throw new InvalidOperationException("No start block found.");
        EndBlock = blocks.OfType<EndBlock>().FirstOrDefault() ?? throw new InvalidOperationException("No end block found.");
    }
}