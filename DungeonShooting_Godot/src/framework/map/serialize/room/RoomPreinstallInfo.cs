﻿
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// 房间预设数据
/// </summary>
public class RoomPreinstallInfo
{
    /// <summary>
    /// 预设名称
    /// </summary>
    [JsonInclude]
    public string Name;

    /// <summary>
    /// 预设权重
    /// </summary>
    [JsonInclude]
    public int Weight;

    /// <summary>
    /// 预设备注
    /// </summary>
    [JsonInclude]
    public string Remark;

    /// <summary>
    /// 是否自动填充数据
    /// </summary>
    [JsonInclude]
    public bool AutoFill;

    /// <summary>
    /// 波数数据
    /// </summary>
    [JsonInclude]
    public List<List<MarkInfo>> WaveList;

    /// <summary>
    /// 初始化波数据
    /// </summary>
    public void InitWaveList()
    {
        WaveList = new List<List<MarkInfo>>
        {
            new List<MarkInfo>()
        };
    }

    /// <summary>
    /// 初始化特殊标记
    /// </summary>
    public void InitSpecialMark(DungeonRoomType roomType)
    {
        var type = SpecialMarkType.Normal;
        if (roomType == DungeonRoomType.Inlet) //初始房间
        {
            type = SpecialMarkType.BirthPoint;
        }
        else if (roomType == DungeonRoomType.Outlet) //结束房间
        {
            type = SpecialMarkType.OutPoint;
        }

        if (type != SpecialMarkType.Normal)
        {
            var preloading = WaveList[0];
            var markInfo = new MarkInfo();
            markInfo.Position = new SerializeVector2();
            markInfo.Size = new SerializeVector2();
            markInfo.SpecialMarkType = type;
            markInfo.MarkList = new List<MarkInfoItem>();
            preloading.Add(markInfo);
        }
    }
}