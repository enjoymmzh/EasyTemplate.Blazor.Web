using System.Net;

namespace EasyTemplate.Tool.Entity;

public class Result
{
    public HttpStatusCode Code { get; set; }
    public bool IsSuccess { get { return Code is HttpStatusCode.OK; } }
    public string Message { get; set; }
    public object Data { get; set; }
    public object Extra { get; set; }
    public long Timestamp { get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); } }

    public static Result Success(string Message = "成功", object Data = null)
        => new Result { Code = HttpStatusCode.OK, Message = Message, Data = Data };

    public static Result Fail(HttpStatusCode Code = HttpStatusCode.BadRequest, string Message = "失败", object Data = null)
        => new Result { Code = Code, Message = Message, Data = Data };
}

public class Result<T>
{
    public HttpStatusCode Code { get; set; }
    public bool IsSuccess { get { return Code is HttpStatusCode.OK; } }
    public string Message { get; set; }
    public T Data { get; set; }
    public object Extra { get; set; }
    public long Timestamp { get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); } }

    public static Result Success(string Message = "成功", object Data = null)
        => new Result { Code = HttpStatusCode.OK, Message = Message, Data = Data };

    public static Result Fail(HttpStatusCode Code = HttpStatusCode.BadRequest, string Message = "失败", object Data = null)
        => new Result { Code = Code, Message = Message, Data = Data };
}
