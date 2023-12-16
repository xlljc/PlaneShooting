﻿using Godot;

namespace UI.TileSetEditorCombination;

public partial class CombinationCell : Sprite2D
{
    public override void _Ready()
    {
        Centered = false;
        RegionEnabled = true;
    }

    /// <summary>
    /// 初始化数据, 设置纹理和显示的区域
    /// </summary>
    public void InitData(Texture2D texture, Vector2I texturePos)
    {
        Texture = texture;
        RegionRect = new Rect2(texturePos * GameConfig.TileCellSize, GameConfig.TileCellSizeVector2I);
    }
}