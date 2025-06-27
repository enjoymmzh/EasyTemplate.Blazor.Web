using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using Masuit.Tools.Systems;
using static EasyTemplate.Tool.Entity.PublicEnum;

namespace EasyTemplate.Tool;

public class Oops
{
    /// <summary>
    /// 友好异常抛出
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <returns></returns>
    public static FriendlyException Oh(string errorMessage)
    {
        var ex = new FriendlyException(errorMessage, 400, true);
        return ex;
    }

    /// <summary>
    /// 友好异常抛出，仅支持简体中文
    /// </summary>
    /// <param name="errorCode">错误码</param>
    /// <returns></returns>
    public static FriendlyException Oh(ErrorCode errorCode)
    {
        var ex = new FriendlyException(errorCode.GetDescription(), errorCode);
        ex.StatusCode = 600;
        return ex;
    }

    /// <summary>
    /// 扩展方法，获得枚举的Description
    /// </summary>
    /// <param name="value">枚举值</param>
    /// <param name="nameInstead">当枚举值没有定义DescriptionAttribute，是否使用枚举名代替，默认是使用</param>
    /// <returns>枚举的Description</returns>
    public static string GetDescription(Enum value, Boolean nameInstead = true)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name == null)
        {
            return null;
        }

        FieldInfo field = type.GetField(name);
        DescriptionAttribute attribute = System.Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        if (attribute == null && nameInstead == true)
        {
            return name;
        }
        return attribute?.Description;
    }

}

public class FriendlyException : Exception
{
    //
    // 摘要:
    //     错误码
    public object ErrorCode { get; set; }

    //
    // 摘要:
    //     错误码（没被复写过的 ErrorCode ）
    public object OriginErrorCode { get; set; }

    //
    // 摘要:
    //     错误消息（支持 Object 对象）
    public object ErrorMessage { get; set; }

    //
    // 摘要:
    //     状态码
    public int StatusCode { get; set; } = 400;


    //
    // 摘要:
    //     是否是数据验证异常
    public bool ValidationException { get; set; }

    //
    // 摘要:
    //     额外数据
    public new object Data { get; set; }

    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   message:
    //
    //   errorCode:
    public FriendlyException(string message, object errorCode)
        : base(message)
    {
        ErrorMessage = message;
        ErrorCode = (OriginErrorCode = errorCode);
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorCode"></param>
    public FriendlyException(string message, object errorCode,bool validationException)
        : base(message)
    {
        ErrorMessage = message;
        ErrorCode = (OriginErrorCode = errorCode);
        ValidationException = validationException;
    }
    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   message:
    //
    //   errorCode:
    //
    //   innerException:
    public FriendlyException(string message, object errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorMessage = message;
        ErrorCode = (OriginErrorCode = errorCode);
    }

    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   info:
    //
    //   context:
    public FriendlyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}