using SqlSugar;

namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 基类Id
/// </summary>
public abstract class EntityBaseId
{
    /// <summary>
    /// Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsIdentity = true, IsPrimaryKey = true)]
    public virtual int Id { get; set; }
}

/// <summary>
/// 框架实体基类
/// </summary>
public abstract class EntityBase : EntityBaseId, IDeletedFilter
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true)]
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true)]
    public virtual int? CreateUserId { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(CreateUserId))]
    public virtual SystemUser? CreateUser { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id")]
    public virtual int? UpdateUserId { get; set; }

    /// <summary>
    /// 修改者
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(UpdateUserId))]
    public virtual SystemUser? UpdateUser { get; set; }

    /// <summary>
    /// 软删除,1:删除，0:不删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除", DefaultValue = "0")]
    public virtual bool IsDelete { get; set; } = false;
}

/// <summary>
/// 框架实体基类
/// </summary>
public abstract class EntityBaseLite
{
    /// <summary>
    /// Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsIdentity = true, IsPrimaryKey = true)]
    public virtual int Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true)]
    public virtual DateTime? CreateTime { get; set; }

}

/// <summary>
/// 假删除接口过滤器
/// </summary>
public interface IDeletedFilter
{
    /// <summary>
    /// 软删除
    /// </summary>
    bool IsDelete { get; set; }
}
