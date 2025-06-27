using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统部门
/// </summary>
[SugarTable(null, "系统部门")]
public class SystemDepartment : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", IsNullable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 负责人
    /// </summary>
    [SugarColumn(ColumnDescription = "负责人", IsNullable = true)]
    public string Leader { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用", DefaultValue = "1", IsNullable = true)]
    public bool Enabled { get; set; }
}

public class SystemDepartmentSeedData : ISeedData<SystemDepartment>
{
    public IEnumerable<SystemDepartment> Generate()
        =>
        [
            new SystemDepartment() { Id = 1, Name="研发部", Leader="赵四", Enabled=true, CreateTime = DateTime.Now },
            new SystemDepartment() { Id = 2, Name="市场部", Leader="赵二", Enabled=false, CreateTime = DateTime.Now },
        ];
}
