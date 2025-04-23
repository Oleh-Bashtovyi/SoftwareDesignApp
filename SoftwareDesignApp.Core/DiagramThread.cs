namespace SoftwareDesignApp.Core;

public class DiagramThread
{
    public string Name { get; set; }
    public List<Block> Blocks { get; set; } = new List<Block>();
    public Block StartBlock { get; set; }

    public DiagramThread(string name)
    {
        Name = name;
    }

    public void ChekStartBlock()
    {
        if (StartBlock == null || Blocks.Count == 0)
            return;
    }
}