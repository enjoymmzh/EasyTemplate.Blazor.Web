using System.ComponentModel.DataAnnotations;
using Masuit.Tools.Models;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统信息提醒
/// </summary>
[SugarTable(null, "系统信息提醒")]
[SugarIndex("NotifyType", nameof(NotifyType), OrderByType.Asc)]
public class SystemNotification : EntityBaseLite
{
    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(ColumnDescription = "标题", Length =500, IsNullable = true)]
    public string Title { get; set; }

    /// <summary>
    /// 提醒信息
    /// </summary>
    [SugarColumn(ColumnDescription = "提醒信息", ColumnDataType ="text", IsNullable = true)]
    public string Info { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [SugarColumn(ColumnDescription = "通知类型", DefaultValue = "0", IsNullable = true)]
    public NotifyType NotifyType { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    [SugarColumn(ColumnDescription = "通知状态", DefaultValue = "0", IsNullable = true)]
    public NotifyStatus NotifyStatus { get; set; }

    /// <summary>
    /// 已读
    /// </summary>
    [SugarColumn(ColumnDescription = "已读", DefaultValue = "0", IsNullable = true)]
    public bool Seen { get; set; }
}

public class SystemNotificationSeedData : ISeedData<SystemNotification>
{
    public IEnumerable<SystemNotification> Generate()
        =>
        [
            new SystemNotification() { Id = 1, Title="提醒1", Info="一切正常", NotifyType = NotifyType.Notify, NotifyStatus=NotifyStatus.Todo, CreateTime = DateTime.Now },
            new SystemNotification() { Id = 2, Title="提醒2", Info="一切不正常", NotifyType = NotifyType.Notify, NotifyStatus=NotifyStatus.Processing, Seen=true, CreateTime = DateTime.Now },
            new SystemNotification() { Id = 3, Title="消息1",Info="一切正常111", NotifyType = NotifyType.Info, Seen=false, CreateTime = DateTime.Now },
        ];
}


