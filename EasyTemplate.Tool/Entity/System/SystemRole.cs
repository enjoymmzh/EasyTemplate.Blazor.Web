using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统角色
/// </summary>
[SugarTable(null, "系统角色")]
public class SystemRole : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", IsNullable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    [SugarColumn(ColumnDescription = "角色类型", DefaultValue ="1", IsNullable = true)]
    public RoleType RoleType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用", DefaultValue = "1", IsNullable = true)]
    public bool Enabled { get; set; }
}

public class SystemRoleSeedData : ISeedData<SystemRole>
{
    public IEnumerable<SystemRole> Generate()
        =>
        [
            new SystemRole() { Id = 1, Name="管理员", RoleType= RoleType.System, Enabled=true, CreateTime = DateTime.Now },
            new SystemRole() { Id = 2, Name="研发", RoleType= RoleType.Normal, Enabled=true, CreateTime = DateTime.Now },
            new SystemRole() { Id = 3, Name="财务", RoleType= RoleType.Normal, Enabled=true, CreateTime = DateTime.Now },
        ];
}
