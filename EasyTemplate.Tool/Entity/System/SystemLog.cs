using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统日志，记录一般情况
/// </summary>
[SugarTable(null, "系统日志，记录一般情况")]
public class SystemLog : EntityBaseLite
{
    /// <summary>
    /// 日志信息
    /// </summary>
    [SugarColumn(ColumnDescription = "日志信息", IsNullable = true)]
    public string Info { get; set; }

    /// <summary>
    /// 日志类型
    /// </summary>
    [SugarColumn(ColumnDescription = "角色类型", DefaultValue ="1", IsNullable = true)]
    public string LogType { get; set; }

}
