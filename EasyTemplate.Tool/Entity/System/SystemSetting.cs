using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统基本设置
/// </summary>
[SugarTable(null, "系统基本设置")]
public class SystemSetting
{
    /// <summary>
    /// Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsIdentity = true, IsPrimaryKey = true)]
    public virtual int Id { get; set; }

    /// <summary>
    /// copyright
    /// </summary>
    [SugarColumn(ColumnDescription = "copyright", IsNullable = true)]
    public string CopyRight { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    [SugarColumn(ColumnDescription = "公司名称", IsNullable = true)]
    public string CompanyName { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    [SugarColumn(ColumnDescription = "社会统一信用代码", IsNullable = true)]
    public string CompanyNo { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    [SugarColumn(ColumnDescription = "注册日期", IsNullable = true)]
    public DateTime RegisterDate { get; set; }

    /// <summary>
    /// 地区id
    /// </summary>
    [SugarColumn(ColumnDescription = "地区id", IsNullable = true)]
    public string AreaIds { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [SugarColumn(ColumnDescription = "地址", IsNullable = true)]
    public string Address { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [SugarColumn(ColumnDescription = "联系电话", IsNullable = true)]
    public string ContactPhone { get; set; }

    /// <summary>
    /// 联系邮箱
    /// </summary>
    [SugarColumn(ColumnDescription = "联系邮箱", IsNullable = true)]
    public string ContactEmail { get; set; }

    /// <summary>
    /// 网站URL
    /// </summary>
    [SugarColumn(ColumnDescription = "网站URL", IsNullable = true)]
    public string WebsiteUrl { get; set; }

    /// <summary>
    /// 最后一个区域id，用于级联回显
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string LastAreadId { get; set; }

    /// <summary>
    /// 地区全拼
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string Area { get; set; }
}

public class SystemSettingSeedData : ISeedData<SystemSetting>
{
    public IEnumerable<SystemSetting> Generate()
        =>
        [
            new SystemSetting() {
                Id = 1,
                CopyRight = "2025 Easy Template Blazor 后台管理系统",
                CompanyName = "示例公司",
                CompanyNo = "12345864848",
                RegisterDate = DateTime.Now,
                AreaIds="37,82,85",
                Address = "示例地址",
                ContactPhone = "123-456-7890",
                ContactEmail = "info@example.com",
                WebsiteUrl = "https://www.example.com"
            },
        ];
}
