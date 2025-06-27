using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统日志，记录api请求相关信息
/// </summary>
[SugarTable(null, "系统日志，记录api请求相关信息")]
public class SystemLogApi : EntityBaseLite
{
    /// <summary>
    /// 日志信息
    /// </summary>
    [SugarColumn(ColumnDescription = "日志信息", IsNullable = true)]
    public string Info { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [SugarColumn(ColumnDescription = "IP地址", IsNullable = true)]
    public string IpAddress { get; set; }

    /// <summary>
    /// UserAgent
    /// </summary>
    [SugarColumn(ColumnDescription = "UserAgent", Length = 1000, IsNullable = true)]
    public string UserAgent { get; set; }
}
