﻿
using UI.MapEditor;

/// <summary>
/// 编辑物体标记
/// </summary>
public class EditorMarkTool : EditorToolBase
{
    public EditorMarkTool(EditorTileMap editorTileMap) : base(
        ResourcePath.resource_sprite_ui_commonIcon_DoorTool_png, "编辑物体标记", true, editorTileMap, EditorToolEnum.MarkTool)
    {
    }

    public override void OnSetSelected(bool selected)
    {
        if (selected)
        {
            EditorTileMap.MapEditorPanel.S_MapEditorMapLayer.Instance.SetLayerVisible(MapLayer.MarkLayer, true);
        }
    }
}