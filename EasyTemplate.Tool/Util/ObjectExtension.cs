using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EasyTemplate.Tool;

public static class ObjectExtensions
{
    //
    // 摘要:
    //     将 DateTimeOffset 转换成本地 DateTime
    //
    // 参数:
    //   dateTime:
    public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
        {
            return dateTime.UtcDateTime;
        }

        if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
        {
            return dateTime.ToLocalTime().DateTime;
        }

        return dateTime.DateTime;
    }

    //
    // 摘要:
    //     将 DateTimeOffset? 转换成本地 DateTime?
    //
    // 参数:
    //   dateTime:
    public static DateTime? ConvertToDateTime(this DateTimeOffset? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        return dateTime.Value.ConvertToDateTime();
    }

    //
    // 摘要:
    //     将 DateTime 转换成 DateTimeOffset
    //
    // 参数:
    //   dateTime:
    public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
    }

    //
    // 摘要:
    //     将 DateTime? 转换成 DateTimeOffset?
    //
    // 参数:
    //   dateTime:
    public static DateTimeOffset? ConvertToDateTimeOffset(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return null;
        }

        return dateTime.Value.ConvertToDateTimeOffset();
    }

    //
    // 摘要:
    //     将时间戳转换为 DateTime
    //
    // 参数:
    //   timestamp:
    internal static DateTime ConvertToDateTime(this long timestamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        int num = (int)Math.Floor(Math.Log10(timestamp) + 1.0);
        if (num != 13 && num != 10)
        {
            throw new ArgumentException("Data is not a valid timestamp format.");
        }

        return ((num == 13) ? dateTime.AddMilliseconds(timestamp) : dateTime.AddSeconds(timestamp)).ToLocalTime();
    }

    //
    // 摘要:
    //     将流保存到本地磁盘
    //
    // 参数:
    //   stream:
    //
    //   path:
    public static void CopyToSave(this Stream stream, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException("path");
        }

        using FileStream destination = File.Create(path);
        stream.CopyTo(destination);
    }

    //
    // 摘要:
    //     将字节数组保存到本地磁盘
    //
    // 参数:
    //   bytes:
    //
    //   path:
    public static void CopyToSave(this byte[] bytes, string path)
    {
        using MemoryStream stream = new MemoryStream(bytes);
        stream.CopyToSave(path);
    }

    //
    // 摘要:
    //     将流保存到本地磁盘
    //
    // 参数:
    //   stream:
    //
    //   path:
    public static async Task CopyToSaveAsync(this Stream stream, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException("path");
        }

        using FileStream fileStream = File.Create(path);
        await stream.CopyToAsync(fileStream);
    }

    //
    // 摘要:
    //     将字节数组保存到本地磁盘
    //
    // 参数:
    //   bytes:
    //
    //   path:
    public static async Task CopyToSaveAsync(this byte[] bytes, string path)
    {
        using MemoryStream stream = new MemoryStream(bytes);
        await stream.CopyToSaveAsync(path);
    }

    //
    // 摘要:
    //     判断是否是富基元类型
    //
    // 参数:
    //   type:
    //     类型
    internal static bool IsRichPrimitive(this Type type)
    {
        if (type.IsValueTuple())
        {
            return false;
        }

        if (type.IsArray)
        {
            return type.GetElementType().IsRichPrimitive();
        }

        if (type.IsPrimitive || type.IsValueType || type == typeof(string))
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return type.GenericTypeArguments[0].IsRichPrimitive();
        }

        return false;
    }

    //
    // 摘要:
    //     合并两个字典
    //
    // 参数:
    //   dic:
    //     字典
    //
    //   newDic:
    //     新字典
    //
    // 类型参数:
    //   T:
    internal static Dictionary<string, T> AddOrUpdate<T>(this Dictionary<string, T> dic, IDictionary<string, T> newDic)
    {
        foreach (string key in newDic.Keys)
        {
            if (dic.TryGetValue(key, out var value))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, newDic[key]);
            }
        }

        return dic;
    }

    //
    // 摘要:
    //     判断是否是元组类型
    //
    // 参数:
    //   type:
    //     类型
    internal static bool IsValueTuple(this Type type)
    {
        if (type.Namespace == "System")
        {
            return type.Name.Contains("ValueTuple`");
        }

        return false;
    }

    //
    // 摘要:
    //     判断方法是否是异步
    //
    // 参数:
    //   method:
    //     方法
    internal static bool IsAsync(this MethodInfo method)
    {
        if (method.GetCustomAttribute<AsyncMethodBuilderAttribute>() == null)
        {
            return method.ReturnType.ToString().StartsWith(typeof(Task).FullName);
        }

        return true;
    }

    //
    // 摘要:
    //     判断类型是否实现某个泛型
    //
    // 参数:
    //   type:
    //     类型
    //
    //   generic:
    //     泛型类型
    //
    // 返回结果:
    //     bool
    internal static bool HasImplementedRawGeneric(this Type type, Type generic)
    {
        if (type.GetInterfaces().Any(IsTheRawGenericType))
        {
            return true;
        }

        while (type != null && type != typeof(object))
        {
            if (IsTheRawGenericType(type))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
        bool IsTheRawGenericType(Type type)
        {
            return generic == (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
        }
    }

    //
    // 摘要:
    //     判断是否是匿名类型
    //
    // 参数:
    //   obj:
    //     对象
    internal static bool IsAnonymous(this object obj)
    {
        Type type2 = ((obj is Type type) ? type : obj.GetType());
        if (Attribute.IsDefined(type2, typeof(CompilerGeneratedAttribute), inherit: false) && type2.IsGenericType && type2.Name.Contains("AnonymousType") && (type2.Name.StartsWith("<>") || type2.Name.StartsWith("VB$")))
        {
            return type2.Attributes.HasFlag(TypeAttributes.AnsiClass);
        }

        return false;
    }

    //
    // 摘要:
    //     获取所有祖先类型
    //
    // 参数:
    //   type:
    internal static IEnumerable<Type> GetAncestorTypes(this Type type)
    {
        List<Type> list = new List<Type>();
        while (type != null && type != typeof(object) && IsNoObjectBaseType(type))
        {
            Type baseType = type.BaseType;
            list.Add(baseType);
            type = baseType;
        }

        return list;
        static bool IsNoObjectBaseType(Type type)
        {
            return type.BaseType != typeof(object);
        }
    }

    //
    // 摘要:
    //     获取方法真实返回类型
    //
    // 参数:
    //   method:
    internal static Type GetRealReturnType(this MethodInfo method)
    {
        bool num = method.IsAsync();
        Type returnType = method.ReturnType;
        if (!num)
        {
            return returnType;
        }

        return returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(void);
    }

    //
    // 摘要:
    //     将一个对象转换为指定类型
    //
    // 参数:
    //   obj:
    //
    // 类型参数:
    //   T:
    internal static T ChangeType<T>(this object obj)
    {
        return (T)obj.ChangeType(typeof(T));
    }

    //
    // 摘要:
    //     将一个对象转换为指定类型
    //
    // 参数:
    //   obj:
    //     待转换的对象
    //
    //   type:
    //     目标类型
    //
    // 返回结果:
    //     转换后的对象
    internal static object ChangeType(this object obj, Type type)
    {
        if (type == null)
        {
            return obj;
        }

        if (type == typeof(string))
        {
            return obj?.ToString();
        }

        if (type == typeof(Guid) && obj != null)
        {
            return Guid.Parse(obj.ToString());
        }

        if (type == typeof(bool) && obj != null && !(obj is bool))
        {
            switch (obj.ToString().ToLower())
            {
                case "1":
                case "true":
                case "yes":
                case "on":
                    return true;
                default:
                    return false;
            }
        }

        if (obj == null)
        {
            if (!type.IsValueType)
            {
                return null;
            }

            return Activator.CreateInstance(type);
        }

        Type underlyingType = Nullable.GetUnderlyingType(type);
        if (type.IsAssignableFrom(obj.GetType()))
        {
            return obj;
        }

        if ((underlyingType ?? type).IsEnum)
        {
            if (underlyingType != null && string.IsNullOrWhiteSpace(obj.ToString()))
            {
                return null;
            }

            return Enum.Parse(underlyingType ?? type, obj.ToString());
        }

        if (obj.GetType().Equals(typeof(DateTime)) && (underlyingType ?? type).Equals(typeof(DateTimeOffset)))
        {
            return ((DateTime)obj).ConvertToDateTimeOffset();
        }

        if (obj.GetType().Equals(typeof(DateTimeOffset)) && (underlyingType ?? type).Equals(typeof(DateTime)))
        {
            return ((DateTimeOffset)obj).ConvertToDateTime();
        }

        if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type))
        {
            try
            {
                return Convert.ChangeType(obj, underlyingType ?? type, null);
            }
            catch
            {
                return (underlyingType == null) ? Activator.CreateInstance(type) : null;
            }
        }

        TypeConverter converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(obj.GetType()))
        {
            return converter.ConvertFrom(obj);
        }

        ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor != null)
        {
            object obj3 = constructor.Invoke(null);
            PropertyInfo[] properties = type.GetProperties();
            Type type2 = obj.GetType();
            PropertyInfo[] array = properties;
            foreach (PropertyInfo propertyInfo in array)
            {
                PropertyInfo property = type2.GetProperty(propertyInfo.Name);
                if (propertyInfo.CanWrite && property != null && property.CanRead)
                {
                    propertyInfo.SetValue(obj3, property.GetValue(obj, null).ChangeType(propertyInfo.PropertyType), null);
                }
            }

            return obj3;
        }

        return obj;
    }

    //
    // 摘要:
    //     查找方法指定特性，如果没找到则继续查找声明类
    //
    // 参数:
    //   method:
    //
    //   inherit:
    //
    // 类型参数:
    //   TAttribute:
    internal static TAttribute GetFoundAttribute<TAttribute>(this MethodInfo method, bool inherit) where TAttribute : Attribute
    {
        Type declaringType = method.DeclaringType;
        Type typeFromHandle = typeof(TAttribute);
        if (!method.IsDefined(typeFromHandle, inherit))
        {
            if (!declaringType.IsDefined(typeFromHandle, inherit))
            {
                return null;
            }

            return declaringType.GetCustomAttribute<TAttribute>(inherit);
        }

        return method.GetCustomAttribute<TAttribute>(inherit);
    }

    //
    // 摘要:
    //     格式化字符串
    //
    // 参数:
    //   str:
    //
    //   args:
    internal static string Format(this string str, params object[] args)
    {
        if (args != null && args.Length != 0)
        {
            return string.Format(str, args);
        }

        return str;
    }

    //
    // 摘要:
    //     切割骆驼命名式字符串
    //
    // 参数:
    //   str:
    internal static string[] SplitCamelCase(this string str)
    {
        if (str == null)
        {
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(str))
        {
            return new string[1] { str };
        }

        if (str.Length == 1)
        {
            return new string[1] { str };
        }

        return (from u in Regex.Split(str, "(?=\\p{Lu}\\p{Ll})|(?<=\\p{Ll})(?=\\p{Lu})")
                where u.Length > 0
                select u).ToArray();
    }

    //
    // 摘要:
    //     JsonElement 转 Object
    //
    // 参数:
    //   jsonElement:
    internal static object ToObject(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                return jsonElement.GetString();
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.Number:
                return jsonElement.GetDecimal();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();
            case JsonValueKind.Object:
                {
                    JsonElement.ObjectEnumerator objectEnumerator = jsonElement.EnumerateObject();
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    {
                        foreach (JsonProperty item in objectEnumerator)
                        {
                            dictionary.Add(item.Name, item.Value.ToObject());
                        }

                        return dictionary;
                    }
                }
            case JsonValueKind.Array:
                {
                    JsonElement.ArrayEnumerator arrayEnumerator = jsonElement.EnumerateArray();
                    List<object> list = new List<object>();
                    {
                        foreach (JsonElement item2 in arrayEnumerator)
                        {
                            list.Add(item2.ToObject());
                        }

                        return list;
                    }
                }
            default:
                return null;
        }
    }

    //
    // 摘要:
    //     清除字符串前后缀
    //
    // 参数:
    //   str:
    //     字符串
    //
    //   pos:
    //     0：前后缀，1：后缀，-1：前缀
    //
    //   affixes:
    //     前后缀集合
    internal static string ClearStringAffixes(this string str, int pos = 0, params string[] affixes)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (affixes == null || affixes.Length == 0)
        {
            return str;
        }

        bool flag = false;
        bool flag2 = false;
        string text = null;
        foreach (string text2 in affixes)
        {
            if (string.IsNullOrWhiteSpace(text2))
            {
                continue;
            }

            if (pos != 1 && !flag && str.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
            {
                string text3 = str;
                int length = text2.Length;
                text = text3.Substring(length, text3.Length - length);
                flag = true;
            }

            if (pos != -1 && !flag2 && str.EndsWith(text2, StringComparison.OrdinalIgnoreCase))
            {
                string text3 = ((!string.IsNullOrWhiteSpace(text)) ? text : str);
                int length = text2.Length;
                text = text3.Substring(0, text3.Length - length);
                flag2 = true;
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = null;
                    flag2 = false;
                }
            }

            if (flag && flag2)
            {
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return str;
        }

        return text;
    }

    //
    // 摘要:
    //     判断集合是否为空
    //
    // 参数:
    //   collection:
    //     集合对象
    //
    // 类型参数:
    //   T:
    //     元素类型
    //
    // 返回结果:
    //     System.Boolean 实例，true 表示空集合，false 表示非空集合
    internal static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        if (collection != null)
        {
            return !collection.Any();
        }

        return true;
    }

    //
    // 摘要:
    //     获取类型自定义特性
    //
    // 参数:
    //   type:
    //     类类型
    //
    //   inherit:
    //     是否继承查找
    //
    // 类型参数:
    //   TAttribute:
    //     特性类型
    //
    // 返回结果:
    //     特性对象
    internal static TAttribute GetTypeAttribute<TAttribute>(this Type type, bool inherit = false) where TAttribute : Attribute
    {
        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        if (!type.IsDefined(typeof(TAttribute), inherit))
        {
            return null;
        }

        return type.GetCustomAttribute<TAttribute>(inherit);
    }
}
