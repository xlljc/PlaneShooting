using System.Collections.Generic;
using Config;
using Godot;

namespace UI.MapEditorCreateMark;

public partial class MapEditorCreateMarkPanel : MapEditorCreateMark
{

    private UiGrid<MarkObject, MarkInfoItem> _grid;
    
    public override void OnCreateUi()
    {
        //隐藏模板对象
        S_ExpandPanel.Instance.Visible = false;
        S_NumberBar.Instance.Visible = false;
        
        S_AddMark.Instance.Pressed += OnAddMark;

        _grid = new UiGrid<MarkObject, MarkInfoItem>(S_MarkObject, typeof(MarkObjectCell));
        _grid.SetColumns(1);
        _grid.SetHorizontalExpand(true);
        _grid.SetCellOffset(new Vector2I(0, 5));

    }

    public override void OnDestroyUi()
    {
        _grid.Destroy();
    }
    
    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitData(RoomPreinstall preinstall, int waveIndex)
    {
        var optionButton = S_WaveOption.Instance;
        for (var i = 0; i < preinstall.WaveList.Count; i++)
        {
            optionButton.AddItem($"第{i + 1}波");
        }

        optionButton.Selected = waveIndex;
    }

    /// <summary>
    /// 获取填写的标记数据
    /// </summary>
    public MarkInfo GetMarkInfo()
    {
        var data = new MarkInfo();
        data.Position = new SerializeVector2();
        data.MarkList = new List<MarkInfoItem>();
        data.DelayTime = (float)S_DelayInput.Instance.Value;
        var gridCount = _grid.Count;
        for (var i = 0; i < gridCount; i++)
        {
            var uiCell = (MarkObjectCell)_grid.GetCell(i);
            var markInfoItem = uiCell.GetMarkInfoItem();
            data.MarkList.Add(markInfoItem);
        }

        return data;
    }

    /// <summary>
    /// 创建数值属性数据
    /// </summary>
    /// <param name="attrName">属性字符串名称</param>
    /// <param name="attrLabel">属性名称</param>
    public NumberAttribute CreateNumberBar(string attrName, string attrLabel)
    {
        var numberBar = S_NumberBar.Clone();
        numberBar.Instance.AttrName = attrName;
        numberBar.L_AttrName.Instance.Text = attrLabel;
        numberBar.Instance.Visible = true;
        return numberBar.Instance;
    }

    //点击添加标记按钮
    private void OnAddMark()
    {
        EditorWindowManager.ShowSelectObject(OnSelectObject, this);
    }

    //选中物体回调
    private void OnSelectObject(ExcelConfig.ActivityObject activityObject)
    {
        _grid.Add(new MarkInfoItem()
        {
            Id = activityObject.Id,
            Weight = 100
        });
    }
}
