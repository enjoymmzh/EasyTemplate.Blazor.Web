using System.ComponentModel.DataAnnotations;
using Masuit.Tools.Models;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统区域
/// </summary>
[SugarTable(null, "系统区域")]
[SugarIndex("ParentCode", nameof(ParentCode), OrderByType.Asc)]
public class SystemArea : EntityBaseLite
{
    /// <summary>
    /// 父级编码
    /// </summary>
    [SugarColumn(ColumnDescription = "父级编码", IsNullable = true)]
    public long ParentCode { get; set; }

    /// <summary>
    /// 地区编码
    /// </summary>
    [SugarColumn(ColumnDescription = "地区编码", IsNullable = true, IsTreeKey = true)]
    public long AreaCode { get; set; }

    /// <summary>
    /// 键
    /// </summary>
    [SugarColumn(ColumnDescription = "地区名称", IsNullable = true)]
    public string AreaName { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [SugarColumn(ColumnDescription = "层级", IsNullable = true)]
    public int Level { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue ="0", IsNullable = true)]
    public long Sort { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<SystemArea> Children { get; set; }
}

public class SystemAreaSeedData : ISeedData<SystemArea>
{
    public IEnumerable<SystemArea> Generate()
    {
        var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}wwwroot\\data\\area.json");
        if (!string.IsNullOrWhiteSpace(json))
        {
            var list = json.ToEntity<List<SystemArea>>();
            if (list?.Count > 0)
            {
                return list;
            }
        }
        return new List<SystemArea>();
    }
}


