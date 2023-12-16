﻿using System;
using System.Collections.Generic;
using Godot;

namespace UI.TileSetEditorCombination;

public partial class TileEditCombination : GridBg<TileSetEditorCombination.LeftTopBg>
{
    // ------------------------------- 笔刷相关 -------------------------------
    //笔刷数据, kay: 代表原图中的坐标, 单位: 格
    private Dictionary<Vector2I, CombinationCell> _brushData = new Dictionary<Vector2I, CombinationCell>();

    //笔刷偏移, 单位: 像素
    private Vector2I _brushOffset = Vector2I.Zero;
    //上一帧笔刷位置
    private Vector2 _brushPrevPos = Vector2.Zero;
    private bool _initBrush = false;
    private bool _drawBrushFlag = false;
    private int _xStart = int.MaxValue;
    private int _yStart = int.MaxValue;
    private int _xEnd = int.MinValue;
    private int _yEnd = int.MinValue;
    // -----------------------------------------------------------------------
    
    // ------------------------------- 画布相关 -------------------------------
    //画布数据, kay: 绘制坐标, 单位: 像素
    private Dictionary<Vector2I, CombinationCell> _canvas = new Dictionary<Vector2I, CombinationCell>();
    //画布数据是否需要更新
    private bool _canvasDirty = false;
    // -----------------------------------------------------------------------


    public override void SetUiNode(IUiNode uiNode)
    {
        base.SetUiNode(uiNode);
        UiNode.L_CombinationRoot.L_RectBrush.Instance.Root = UiNode.L_CombinationRoot.Instance;
        Grid = UiNode.L_Grid.Instance;
        ContainerRoot = UiNode.L_CombinationRoot.Instance;

        UiNode.UiPanel.AddEventListener(EventEnum.OnSelectCombinationCell, OnSelectCombinationCell);
        UiNode.UiPanel.AddEventListener(EventEnum.OnRemoveCombinationCell, OnRemoveCombinationCell);
        UiNode.UiPanel.AddEventListener(EventEnum.OnClearCombinationCell, OnClearCombinationCell);

        //删除按钮点击事件
        UiNode.L_DeleteBtn.Instance.Pressed += OnDeleteClick;
        //聚焦按钮点击事件
        UiNode.L_FocusBtn.Instance.Pressed += OnFocusClick;
        //导入组合按钮点击事件
        UiNode.L_ImportBtn.Instance.Pressed += OnImportClick;
    }

    public override void _Process(double delta)
    {
        if (!UiNode.UiPanel.IsOpen)
        {
            return;
        }

        var brushRoot = UiNode.L_CombinationRoot.L_BrushRoot.Instance;
        var combinationRoot = UiNode.L_CombinationRoot.Instance;
        brushRoot.Position = combinationRoot.GetLocalMousePosition().FloorAdsorption(GameConfig.TileCellSizeVector2I) + _brushOffset;

        if (_canvasDirty) //更新画布范围
        {
            _canvasDirty = false;
            if (_canvas.Count > 0)
            {
                //单位: 像素
                var canvasXStart = int.MaxValue;
                var canvasYStart = int.MaxValue;
                var canvasXEnd = int.MinValue;
                var canvasYEnd = int.MinValue;

                foreach (var keyValuePair in _canvas)
                {
                    var pos = keyValuePair.Key;
                    canvasXStart = Mathf.Min(pos.X, canvasXStart);
                    canvasYStart = Mathf.Min(pos.Y, canvasYStart);
                    canvasXEnd = Mathf.Max(pos.X + GameConfig.TileCellSize, canvasXEnd);
                    canvasYEnd = Mathf.Max(pos.Y + GameConfig.TileCellSize, canvasYEnd);
                }
                
                UiNode.L_CombinationRoot.L_RectBrush.Instance.SetDrawRect(
                    canvasXStart,
                    canvasYStart,
                    canvasXEnd - canvasXStart,
                    canvasYEnd - canvasYStart
                );
            }
            else
            {
                UiNode.L_CombinationRoot.L_RectBrush.Instance.ClearDrawRect();
            }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouse)
        {
            AcceptEvent();
            var brushRoot = UiNode.L_CombinationRoot.L_BrushRoot.Instance;
            var newPos = brushRoot.Position;
            
            sbyte flag = 0;
            //左键按下开始绘制
            if (_initBrush)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left)) //绘制
                {
                    flag = 1;
                    if (_brushPrevPos != newPos || !_drawBrushFlag)
                    {
                        _drawBrushFlag = true;
                        _brushPrevPos = newPos;
                        DrawBrush();
                    }
                }
                else if (Input.IsMouseButtonPressed(MouseButton.Right)) //擦除
                {
                    flag = -1;
                    brushRoot.Modulate = new Color(1, 1, 1, 0.3f);
                    if (_brushPrevPos != newPos || !_drawBrushFlag)
                    {
                        _drawBrushFlag = true;
                        _brushPrevPos = newPos;
                        EraseBrush();
                    }
                }
            }

            if (flag != 0)
            {
                _drawBrushFlag = false;
            }

            if (flag != -1)
            {
                brushRoot.Modulate = Colors.White;
            }
        }
    }

    //删除所有图块
    private void OnDeleteClick()
    {
        foreach (var keyValuePair in _canvas)
        {
            keyValuePair.Value.QueueFree();
        }

        _canvas.Clear();
        _canvasDirty = true;
    }
    
    //点击聚焦按钮
    private void OnFocusClick()
    {
        var pos = Size / 2;
        var center = UiNode.L_CombinationRoot.L_RectBrush.Instance.GetCenterPosition();
        ContainerRoot.Position = pos - center * ContainerRoot.Scale;
        RefreshGridTrans();
    }
    
    //导入按钮点击
    private void OnImportClick()
    {
        var size = UiNode.L_CombinationRoot.L_RectBrush.Instance.GetRectSize();
        if (size == Vector2.Zero)
        {
            EditorWindowManager.ShowTips("警告", "请先绘制组合图块！");
        }
        else if (size == GameConfig.TileCellSizeVector2I)
        {
            EditorWindowManager.ShowTips("警告", "导入一格大小的组合图块没有任何意义！");
        }
    }
    
    //绘制笔刷
    private void DrawBrush()
    {
        var brushRoot = UiNode.L_CombinationRoot.L_BrushRoot.Instance;
        foreach (var keyValuePair in _brushData)
        {
            var combinationCell = keyValuePair.Value;
            var pos = (combinationCell.Position + brushRoot.Position).AsVector2I();
            if (_canvas.TryGetValue(pos, out var canvasCell))
            {
                canvasCell.CloneFrom(combinationCell);
            }
            else
            {
                canvasCell = (CombinationCell)combinationCell.Duplicate();
                canvasCell.Position = pos;
                UiNode.L_CombinationRoot.L_CanvasRoot.AddChild(canvasCell);
                _canvas.Add(pos, canvasCell);
                _canvasDirty = true;
            }
        }
    }

    //擦除笔刷
    private void EraseBrush()
    {
        var brushRoot = UiNode.L_CombinationRoot.L_BrushRoot.Instance;
        foreach (var keyValuePair in _brushData)
        {
            var combinationCell = keyValuePair.Value;
            var pos = (combinationCell.Position + brushRoot.Position).AsVector2I();
            if (_canvas.TryGetValue(pos, out var canvasCell))
            {
                canvasCell.QueueFree();
                _canvas.Remove(pos);
                _canvasDirty = true;
            }
        }
    }

    //选中组合的图块
    private void OnSelectCombinationCell(object obj)
    {
        if (obj is Vector2I cell && !_brushData.ContainsKey(cell))
        {
            _initBrush = true;
            var cellData = new CombinationCell();
            cellData.Position = cell * GameConfig.TileCellSize;
            cellData.InitData(UiNode.UiPanel.EditorPanel.Texture, cell);
            UiNode.L_CombinationRoot.L_BrushRoot.AddChild(cellData);
            _brushData.Add(cell, cellData);
            
            //计算起始点和终点
            _xStart = Mathf.Min(cell.X, _xStart);
            _yStart = Mathf.Min(cell.Y, _yStart);
            _xEnd = Mathf.Max(cell.X, _xEnd);
            _yEnd = Mathf.Max(cell.Y, _yEnd);
            _brushOffset = new Vector2I(-(_xStart + (_xEnd - _xStart) / 2), -(_yStart + (_yEnd - _yStart) / 2)) * GameConfig.TileCellSize;
        }
    }
    
    //移除组合图块
    private void OnRemoveCombinationCell(object obj)
    {
        if (obj is Vector2I cell)
        {
            if (_brushData.TryGetValue(cell, out var cellData))
            {
                cellData.QueueFree();
                _brushData.Remove(cell);
            }
        }
    }

    //移除所有组合图块
    private void OnClearCombinationCell(object obj)
    {
        foreach (var keyValuePair in _brushData)
        {
            keyValuePair.Value.QueueFree();
        }
        _brushData.Clear();
        _initBrush = false;
        _brushOffset = Vector2I.Zero;
        _xStart = int.MaxValue;
        _yStart = int.MaxValue;
        _xEnd = int.MinValue;
        _yEnd = int.MinValue;
    }
}