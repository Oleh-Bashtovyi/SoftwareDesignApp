namespace SoftwareDesignApp.UI.Blocks.Base;

public class OneNextBlockControl(string blockId) : BaseBlockControl(blockId)
{
    private BaseBlockControl? _nextBlock;

    public BaseBlockControl? NextBlock
    {
        get => _nextBlock;
        set
        {
            _nextBlock = value;
            OnPropertyChanged();
        }
    }

    public void SetNextBlock(BaseBlockControl block)
    {
        NextBlock = block;
    }

    public void RemoveNextBlock()
    {
        NextBlock = null;
    }
}