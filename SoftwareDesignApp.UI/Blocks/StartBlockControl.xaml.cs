﻿using SoftwareDesignApp.Core;
using SoftwareDesignApp.UI.Blocks.Base;

namespace SoftwareDesignApp.UI.Blocks;

public partial class StartBlockControl : OneNextBlockControl
{
    public string DiagramText => "Початок";

    public StartBlockControl(string blockId) : base(blockId)
    {
        InitializeComponent();
    }

    public override Block ToCoreBlock(EndBlockControl endBlock)
    {
        return new StartBlock(BlockId, NextBlock?.BlockId ?? endBlock.BlockId);
    }

    public override string GetDisplayText()
    {
        return $"Id:{BlockId}, {DiagramText}";
    }
}

