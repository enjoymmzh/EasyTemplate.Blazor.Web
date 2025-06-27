using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool.Entity;

// 令牌token
public class TokenModelJwt
{
    /// <summary>
    /// Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 用户类型
    /// </summary>
    public UserType UserType { get; set; }
}
