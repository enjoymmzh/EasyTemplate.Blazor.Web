using System.Reflection;

namespace EasyTemplate.Page.Common;

public static class TheAssembly
{
    /// <summary>
    /// 取得系统程序集列表。
    /// </summary>
    public static List<Assembly> Assemblies { get; } = [];

    /// <summary>
    /// 将该项目中所有页面注册到主应用中
    /// </summary>
    /// <param name="services"></param>
    public static void AddAssembly(this IServiceCollection services)
    {
        var assembly = typeof(CustomPages).Assembly;//这里简单使用了一个类用于注册，实际项目中可以使用更加复杂的方式，如果有多个项目，需要在此注册多个项目中的注册类
        Assemblies.Add(assembly);
    }
}
