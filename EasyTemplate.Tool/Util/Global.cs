using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AntDesign;
using CSRedis;
using EasyTemplate.Tool.Entity;
using EasyTemplate.Tool.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace EasyTemplate.Tool;

public static class Global
{
    /// <summary>
    /// 
    /// </summary>
    public static string? ConfigId { get { return Setting.Get<string>("dbConnection:connectionConfigs:0:configId"); } }
    /// <summary>
    /// 
    /// </summary>
    public static string? ConnectionString { get { return Setting.Get<string>("dbConnection:connectionConfigs:0:connectionString"); } }
    /// <summary>
    /// 
    /// </summary>
    public static string? CacheConnectionString { get { return $"{Setting.Get<string>("Cache:RedisConnectionString")},prefix={Setting.Get<string>("Cache:InstanceName")}"; } }
    /// <summary>
    /// 
    /// </summary>
    public static bool EnableInitTable { get { return Setting.Get<bool>("dbConnection:connectionConfigs:0:tableSettings:enableInitTable"); } }
    /// <summary>
    /// 
    /// </summary>
    public static bool encryptResult { get { return Setting.Get<bool>("cryptogram:enabled"); } }
    /// <summary>
    /// 
    /// </summary>
    public static string? CrypType { get { return Setting.Get<string>("cryptogram:cryptoType"); } }
    /// <summary>
    /// 登录过期时间
    /// </summary>
    public static int Expired { get { return Setting.Get<int>("App:ExpiredTime"); } }
    /// <summary>
    /// 
    /// </summary>
    public static CSRedisClient Redis { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public static int UserId { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public static SystemUser CurrentUser { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public static string? SystemName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public static List<SystemMenu> Menus { get; set; } = new List<SystemMenu>();
    
    /// <summary>
    /// 获取host
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public static string GetHost(this IHttpContextAccessor accessor)
    {
        try
        {
            string host = accessor.HttpContext.Request.Headers["Host"];
            if (!string.IsNullOrWhiteSpace(host))
            {
                return $"{accessor.HttpContext.Request.Scheme}://{host}";
            }
        }
        catch (Exception ex) { }

        return "Empty Or Error Host";
    }

    /// <summary>
    /// 获取IP
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public static string GetIpv4(this IHttpContextAccessor accessor)
    {
        try
        {
            // 尝试从 X-Forwarded-For 头中获取真实 IP 地址
            var forwardedHeader = accessor.HttpContext.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // X-Forwarded-For 可能包含多个 IP 地址（逗号分隔），第一个是客户端的真实 IP
                return forwardedHeader.ToString().Split(',')[0].Trim();
            }

            // 如果没有 X-Forwarded-For 头，则回退到 RemoteIpAddress
            var remoteIpAddress = accessor.HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress != null && remoteIpAddress.IsIPv4MappedToIPv6)
            {
                return remoteIpAddress.MapToIPv4().ToString();
            }
        }
        catch (Exception ex) { }
        return default;
    }

    public static async Task<SystemUser> GetUser(this IJSRuntime runtime)
    {
        var localStorageHelper = new LocalStorage(runtime);
        var user = await localStorageHelper.GetLocalStorage(LocalStorage.UserInfo);
        if (!string.IsNullOrWhiteSpace(user))
        {
            var suser = Crypto.AESDecrypt(user).ToEntity<SystemUser>();
            return suser;
        }
        return default;
    }

    public static SystemUser GetUserByToken(this IHttpContextAccessor accessor)
    {
        string token = accessor.HttpContext.Request.Headers["Authorization"];
        if (!string.IsNullOrEmpty(token))
        {
            token = token.Replace("Bearer ", "");
            var info = Jwt.Deserialize(token, out DateTime expired);
            if (info != null)
            {
                return new SystemUser() { Id = info.UserId, NickName = info.Name, };
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="navigationManager"></param>
    public static void RedirectLogin(this Microsoft.AspNetCore.Components.NavigationManager navigationManager)
    {
        if (CurrentUser != null
            && CurrentUser.Id > 0
            && !string.IsNullOrWhiteSpace(CurrentUser.Account)
            && !string.IsNullOrWhiteSpace(CurrentUser.Password))
            return;

        navigationManager?.NavigateTo("/account/login");
    }

    public static async Task RedirectLogin(this Microsoft.AspNetCore.Components.NavigationManager navigationManager, IJSRuntime jSRuntime)
    {
        var localStorageHelper = new LocalStorage(jSRuntime);
        var json = await localStorageHelper.GetLocalStorage(LocalStorage.UserInfo);
        var user = Crypto.AESDecrypt(json).ToEntity<SystemUser>();
        if (user != null 
            && user.Id > 0 
            && !string.IsNullOrWhiteSpace(user.Account) 
            && !string.IsNullOrWhiteSpace(user.Password)
            && user.Expired.Subtract(DateTime.Now).TotalDays > 0)
        {
            if (CurrentUser != null && user.Id == CurrentUser.Id)
            {
                return;
            }
        }

        navigationManager?.NavigateTo("/account/login");
    }
}

public class LongToDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (Utf8Parser.TryParse(reader.ValueSpan, out long value, out _))
            return DateTime.UnixEpoch.AddMilliseconds(value);

        throw new FormatException();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            JsonEncodedText.Encode(((long)(value - DateTime.UnixEpoch).TotalMilliseconds).ToString()));
    }
}
