﻿using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class EndBlockControl : BaseBlockControl
{
    public string DiagramText => "Кінець";

    public EndBlockControl(string blockId) : base(blockId)
    {
        InitializeComponent();
    }

    public override Block ToCoreBlock(EndBlockControl endBlock)
    {
        return new EndBlock(BlockId);
    }

    public override string GetDisplayText()
    {
        return $"Id:{BlockId}, {DiagramText}";
    }
}

