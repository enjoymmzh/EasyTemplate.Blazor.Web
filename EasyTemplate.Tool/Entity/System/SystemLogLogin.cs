using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统日志，记录登录信息
/// </summary>
[SugarTable(null, "系统日志，记录登录信息")]
public class SystemLogLogin : EntityBaseLite
{
    /// <summary>
    /// 日志信息
    /// </summary>
    [SugarColumn(ColumnDescription = "日志信息", IsNullable = true)]
    public string Info { get; set; }

    /// <summary>
    /// 用户id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户id", DefaultValue ="1", IsNullable = true)]
    public int UserId { get; set; }

    /// <summary>
    /// 登录IP地址
    /// </summary>
    [SugarColumn(ColumnDescription = "登录IP地址", IsNullable = true)]
    public string IpAddress { get; set; }
}
