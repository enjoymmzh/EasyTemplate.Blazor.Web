namespace EasyTemplate.Tool.Entity;

/// <summary>
/// 分页列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class PageList<T>
{
    /// <summary>
    /// 列表
    /// </summary>
    public List<T> Data { get; set; } = new List<T>();
    /// <summary>
    /// 总数
    /// </summary>
    public int Total { get; set; } = 0;
}

public class QueryBase
{
    /// <summary>
    /// 页码，必须从1开始
    /// </summary>
    public int Pi { get; set; } = 1;
    /// <summary>
    /// 每页数据量
    /// </summary>
    public int Ps { get; set; } = 20;
}

public class BaseId 
{
    public int Id { get; set; }
}
