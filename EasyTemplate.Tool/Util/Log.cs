using Masuit.Tools.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EasyTemplate.Tool;

public static class Log
{
    /// <summary>
    /// 初始化配置，仅限appsettings.json文件
    /// </summary>
    /// <returns></returns>
    public static void AddLocalLog(this IServiceCollection services)
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        if (path.EndsWith("\\"))
        {
            LogManager.LogDirectory = $"{path}Logs\\";
        }
        else
        {
            LogManager.LogDirectory = $"{path}\\Logs\\";
        }
    }

    /// <summary>
    /// 致命错误
    /// </summary>
    /// <param name="message"></param>
    public static void Fatal(string message)
        => LogManager.Fatal(new Exception(message));

    /// <summary>
    /// 调试信息
    /// </summary>
    /// <param name="message"></param>
    public static void Debug(string message)
        => LogManager.Debug(message);

    /// <summary>
    /// 信息
    /// </summary>
    /// <param name="message"></param>
    public static void Info(string message)
        => LogManager.Info(message);

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="message"></param>
    public static void Error(string message)
        => LogManager.Error(new Exception(message));

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="message"></param>
    public static void Error(Exception message)
        => LogManager.Error(message);
}
