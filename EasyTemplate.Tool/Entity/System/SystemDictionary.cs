using System.ComponentModel.DataAnnotations;
using SqlSugar;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 系统字典
/// </summary>
[SugarTable(null, "系统字典")]
public class SystemDictionary : EntityBase
{
    /// <summary>
    /// 键
    /// </summary>
    [SugarColumn(ColumnDescription = "键", IsNullable = true)]
    public string KeyName { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [SugarColumn(ColumnDescription = "值", IsNullable = true)]
    public string ValueName { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [SugarColumn(ColumnDescription = "字典类型", IsNullable = true)]
    public string DicType { get; set; }

    /// <summary>
    /// 是否为系统参数
    /// </summary>
    [SugarColumn(ColumnDescription = "是否为系统参数，系统参数无法通过后台删除", DefaultValue ="0", IsNullable = true)]
    public bool IsSystem { get; set; }

}

public class SystemDictionarySeedData : ISeedData<SystemDictionary>
{
    public IEnumerable<SystemDictionary> Generate()
        =>
        [
            new SystemDictionary() { Id = 1, KeyName="PublishKey", ValueName="asf234dasdfa234sdfa234565645sdfsdfadfgsdf",DicType="System", IsSystem=true, CreateTime = DateTime.Now },
            new SystemDictionary() { Id = 2, KeyName="SecretKey", ValueName="bnvnvbndfdererere",DicType="System", IsSystem=true, CreateTime = DateTime.Now },
        ];
}
