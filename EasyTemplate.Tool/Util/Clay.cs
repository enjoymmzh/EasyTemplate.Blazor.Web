using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Masuit.Tools;
using System.Xml.Linq;
using System.Xml;
using System.Text.Json;

namespace EasyTemplate.Tool;

public sealed class Clay : DynamicObject, IEnumerable
{
    //
    // 摘要:
    //     JSON 类型
    private enum JsonType
    {
        @string,
        number,
        boolean,
        @object,
        array,
        @null
    }

    //
    // 摘要:
    //     JSON 类型
    private readonly JsonType jsonType;

    //
    // 摘要:
    //     校验 Xml 标签格式
    private static readonly Func<string, Exception> TryVerifyNCName = (Func<string, Exception>)Delegate.CreateDelegate(typeof(Func<string, Exception>), typeof(XmlConvert).GetMethod("TryVerifyNCName", BindingFlags.Static | BindingFlags.NonPublic));

    //
    // 摘要:
    //     将被转换成字符串的类型
    private static readonly Type[] ToBeConvertStringTypes = new Type[1] { typeof(DateTimeOffset) };

    //
    // 摘要:
    //     是否是 Object 类型
    public bool IsObject => jsonType == JsonType.@object;

    //
    // 摘要:
    //     是否是 Array 类型
    public bool IsArray => jsonType == JsonType.array;

    //
    // 摘要:
    //     粘土对象 Xml 元数据
    public XElement XmlElement { get; private set; }

    //
    // 摘要:
    //     当 Clay 时 数组类型时的长度
    public int Length => XmlElement.Elements().Count();

    //
    // 摘要:
    //     配置读取不存在 Key 时行为
    //
    // 言论：
    //     如果设置 false，那么返回 null
    public bool ThrowOnUndefined { get; set; } = true;


    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    public Clay(bool throwOnUndefined = true)
    {
        XmlElement = new XElement("root", CreateTypeAttr(JsonType.@object));
        jsonType = JsonType.@object;
        ThrowOnUndefined = throwOnUndefined;
    }

    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   element:
    //     System.Xml.Linq.XElement
    //
    //   type:
    //     JSON 类型
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    private Clay(XElement element, JsonType type, bool throwOnUndefined = true)
    {
        XmlElement = element;
        jsonType = type;
        ThrowOnUndefined = throwOnUndefined;
    }

    //
    // 摘要:
    //     创建空的粘土对象
    //
    // 参数:
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Object(bool throwOnUndefined = true)
    {
        return new Clay(throwOnUndefined);
    }

    //
    // 摘要:
    //     基于现有对象创建粘土对象
    //
    // 参数:
    //   obj:
    //     对象
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Object(object obj, bool throwOnUndefined = true)
    {
        if (obj == null)
        {
            throw new ArgumentNullException("obj");
        }

        return Parse(CreateJsonString(new XStreamingElement("root", CreateTypeAttr(GetJsonType(obj)), CreateJsonNode(obj))), throwOnUndefined);
    }

    //
    // 摘要:
    //     基于现有对象创建粘土对象
    //
    // 参数:
    //   json:
    //     JSON 字符串
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Parse(string json, bool throwOnUndefined = true)
    {
        return Parse(json, Encoding.UTF8, throwOnUndefined);
    }

    //
    // 摘要:
    //     基于现有对象创建粘土对象
    //
    // 参数:
    //   json:
    //     JSON 字符串
    //
    //   encoding:
    //     编码类型
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Parse(string json, Encoding encoding, bool throwOnUndefined = true)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(encoding.GetBytes(json), XmlDictionaryReaderQuotas.Max);
        return ToValue(XElement.Load(reader), throwOnUndefined);
    }

    //
    // 摘要:
    //     基于 Stream 对象创建粘土对象
    //
    // 参数:
    //   stream:
    //     System.IO.Stream
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Parse(Stream stream, bool throwOnUndefined = true)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
        return ToValue(XElement.Load(reader), throwOnUndefined);
    }

    //
    // 摘要:
    //     基于 Stream 对象创建粘土对象
    //
    // 参数:
    //   stream:
    //     System.IO.Stream
    //
    //   encoding:
    //     编码类型
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    //
    // 返回结果:
    //     Furion.ClayObject.Clay
    public static dynamic Parse(Stream stream, Encoding encoding, bool throwOnUndefined = true)
    {
        using XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(stream, encoding, XmlDictionaryReaderQuotas.Max, delegate
        {
        });
        return ToValue(XElement.Load(reader), throwOnUndefined);
    }

    //
    // 摘要:
    //     重写动态调用方法实现删除行为
    //
    // 参数:
    //   binder:
    //
    //   args:
    //
    //   result:
    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        result = (IsArray ? Delete((int)args[0]) : Delete((string)args[0]));
        return true;
    }

    //
    // 摘要:
    //     重写动态调用成员名称方法实现键是否存在行为
    //
    // 参数:
    //   binder:
    //
    //   args:
    //
    //   result:
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (args.Length != 0)
        {
            result = null;
            return false;
        }

        result = IsDefined(binder.Name);
        return true;
    }

    //
    // 摘要:
    //     重写类型转换方法实现粘土对象动态转换
    //
    // 参数:
    //   binder:
    //
    //   result:
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        if (binder.Type == typeof(IEnumerable) || binder.Type == typeof(object[]))
        {
            IEnumerable<object> enumerable = (IEnumerable<object>)(IsArray ? ((IEnumerable)(from x in XmlElement.Elements()
                                                                                            select ToValue(x, ThrowOnUndefined))) : ((IEnumerable)XmlElement.Elements().Select((Func<XElement, object>)((XElement x) => new KeyValuePair<string, object>((x.Name == "{item}item") ? x.Attribute("item").Value : x.Name.LocalName, ToValue(x, ThrowOnUndefined))))));
            IEnumerable<object> enumerable2;
            if (!(binder.Type == typeof(object[])))
            {
                enumerable2 = enumerable;
            }
            else
            {
                IEnumerable<object> enumerable3 = enumerable.ToArray();
                enumerable2 = enumerable3;
            }

            result = enumerable2;
        }
        else
        {
            result = Deserialize(binder.Type);
        }

        return true;
    }

    //
    // 摘要:
    //     重写根据索引获取值的行为
    //
    // 参数:
    //   binder:
    //
    //   indexes:
    //
    //   result:
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        bool isValid;
        if (!IsArray)
        {
            return TryGet(FindXElement((string)indexes[0], out isValid), out result, ThrowOnUndefined);
        }

        return TryGet(XmlElement.Elements().ElementAtOrDefault((int)indexes[0]), out result, ThrowOnUndefined);
    }

    //
    // 摘要:
    //     重写根据成员名称获取值的行为
    //
    // 参数:
    //   binder:
    //
    //   result:
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        bool isValid;
        if (!IsArray)
        {
            return TryGet(FindXElement(binder.Name, out isValid), out result, ThrowOnUndefined);
        }

        return TryGet(XmlElement.Elements().ElementAtOrDefault(int.Parse(binder.Name)), out result, ThrowOnUndefined);
    }

    //
    // 摘要:
    //     重写根据索引设置值的行为
    //
    // 参数:
    //   binder:
    //
    //   indexes:
    //
    //   value:
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        if (!IsArray)
        {
            return TrySet((string)indexes[0], value);
        }

        return TrySet((int)indexes[0], value);
    }

    //
    // 摘要:
    //     重写根据成员名称设置值的行为
    //
    // 参数:
    //   binder:
    //
    //   value:
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        if (!IsArray)
        {
            return TrySet(binder.Name, value);
        }

        return TrySet(int.Parse(binder.Name), value);
    }

    //
    // 摘要:
    //     重写获取所有动态成员名称行为
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (!IsArray)
        {
            return from x in XmlElement.Elements()
                   select (!(x.Name == "{item}item")) ? x.Name.LocalName : x.Attribute("item").Value;
        }

        return XmlElement.Elements().Select((XElement x, int i) => i.ToString());
    }

    //
    // 摘要:
    //     重写转换成字符串方法
    public override string ToString()
    {
        foreach (XElement item in from x in XmlElement.Descendants()
                                  where x.Attribute("type").Value == "null"
                                  select x)
        {
            item.RemoveNodes();
        }

        return CreateJsonString(new XStreamingElement("root", CreateTypeAttr(jsonType), XmlElement.Elements()));
    }

    //
    // 摘要:
    //     判断对象键是否存在
    //
    // 参数:
    //   name:
    public bool IsDefined(string name)
    {
        bool isValid;
        if (IsObject)
        {
            return FindXElement(name, out isValid) != null;
        }

        return false;
    }

    //
    // 摘要:
    //     判断数组索引是否存在
    //
    // 参数:
    //   index:
    public bool IsDefined(int index)
    {
        if (IsArray)
        {
            return XmlElement.Elements().ElementAtOrDefault(index) != null;
        }

        return false;
    }

    //
    // 摘要:
    //     根据键删除对象属性
    //
    // 参数:
    //   name:
    public bool Delete(string name)
    {
        bool isValid;
        XElement xElement = FindXElement(name, out isValid);
        if (xElement != null)
        {
            xElement.Remove();
            return true;
        }

        return false;
    }

    //
    // 摘要:
    //     根据索引删除数组元素
    //
    // 参数:
    //   index:
    public bool Delete(int index)
    {
        XElement xElement = XmlElement.Elements().ElementAtOrDefault(index);
        if (xElement != null)
        {
            xElement.Remove();
            return true;
        }

        return false;
    }

    //
    // 摘要:
    //     将粘土对象反序列化为特定类型
    //
    // 类型参数:
    //   T:
    public T Deserialize<T>()
    {
        return (T)Deserialize(typeof(T));
    }

    //
    // 摘要:
    //     将粘土对象转换为 object 类型
    public object Solidify()
    {
        return Solidify<object>();
    }

    //
    // 摘要:
    //     将粘土对象转换为特定类型
    //
    // 类型参数:
    //   T:
    public T Solidify<T>()
    {
        //return JSON.Deserialize<T>(ToString());
        return ToString().ToEntity<T>();
    }

    //
    // 摘要:
    //     将粘土对象转换为字典类型
    public IDictionary<string, object> ToDictionary()
    {
        if (IsArray)
        {
            throw new InvalidOperationException("Cannot convert a clay object with JsonType as an array to a dictionary object.");
        }

        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        IEnumerator enumerator = GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, object> keyValuePair = (KeyValuePair<string, object>)enumerator.Current;
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            }

            return dictionary;
        }
        finally
        {
            IDisposable disposable = enumerator as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }

    //
    // 摘要:
    //     转换成特定对象
    //
    // 参数:
    //   valueProvider:
    //     值提供器
    //
    // 类型参数:
    //   T:
    public IEnumerable<T> ConvertTo<T>(Func<PropertyInfo, object, object> valueProvider = null) where T : class, new()
    {
        IEnumerable<PropertyInfo> enumerable = from p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                               where p.CanWrite
                                               select p;
        List<T> list = new List<T>();
        if (IsObject)
        {
            list.Add(ConvertTo<T>(enumerable, (dynamic)this, valueProvider));
            return list;
        }

        if (IsArray)
        {
            foreach (dynamic item in AsEnumerator<object>())
            {
                list.Add(ConvertTo<T>(enumerable, item, valueProvider));
            }
        }

        return list;
    }

    //
    // 摘要:
    //     将粘土对象转换为特定类型
    //
    // 参数:
    //   properties:
    //
    //   clay:
    //
    //   valueProvider:
    //     值提供器
    //
    // 类型参数:
    //   T:
    private static T ConvertTo<T>(IEnumerable<PropertyInfo> properties, dynamic clay, Func<PropertyInfo, object, object> valueProvider = null) where T : class, new()
    {
        T val = new T();
        foreach (PropertyInfo property in properties)
        {
            object obj = ((clay.IsDefined(property.Name)) ? clay[property.Name] : null);
            if (valueProvider != null)
            {
                obj = valueProvider(property, obj);
            }

            property.SetValue(val, obj.ChangeType(property.PropertyType));
        }

        return val;
    }

    //
    // 摘要:
    //     XElement 对象转换成 C# 对象
    //
    // 参数:
    //   element:
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    private static dynamic ToValue(XElement element, bool throwOnUndefined = true)
    {
        JsonType jsonType = (JsonType)Enum.Parse(typeof(JsonType), element.Attribute("type").Value);
        switch (jsonType)
        {
            case JsonType.boolean:
                return (bool)element;
            case JsonType.number:
                if (element.Value.Contains('.'))
                {
                    return (double)element;
                }

                return (long)element;
            case JsonType.@string:
                return (string?)element;
            case JsonType.@object:
            case JsonType.array:
                return new Clay(element, jsonType, throwOnUndefined);
            default:
                return null;
        }
    }

    //
    // 摘要:
    //     获取 JSON 类型
    //
    // 参数:
    //   obj:
    private static JsonType GetJsonType(object obj)
    {
        if (obj == null)
        {
            return JsonType.@null;
        }

        Type type = obj.GetType();
        if (ToBeConvertStringTypes.Contains(type))
        {
            return JsonType.@string;
        }

        if (obj is ExpandoObject)
        {
            return JsonType.@object;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                return JsonType.boolean;
            case TypeCode.Char:
            case TypeCode.DateTime:
            case TypeCode.String:
                return JsonType.@string;
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return JsonType.number;
            case TypeCode.Object:
                return (obj is IEnumerable) ? JsonType.array : JsonType.@object;
            default:
                return JsonType.@null;
        }
    }

    //
    // 摘要:
    //     创建 XElement type 属性
    //
    // 参数:
    //   type:
    private static XAttribute CreateTypeAttr(JsonType type)
    {
        return new XAttribute("type", type.ToString());
    }

    //
    // 摘要:
    //     创建 XElement 节点值
    //
    // 参数:
    //   obj:
    private static object CreateJsonNode(object obj)
    {
        bool flag = obj?.GetType().IsEnum ?? false;
        switch (GetJsonType(obj))
        {
            case JsonType.@string:
            case JsonType.number:
                return flag ? ((object)(int)obj) : obj;
            case JsonType.boolean:
                return obj.ToString().ToLower();
            case JsonType.@object:
                return CreateXObject(obj);
            case JsonType.array:
                return CreateXArray(obj as IEnumerable);
            default:
                return null;
        }
    }

    //
    // 摘要:
    //     创建 XStreamingElement 对象
    //
    // 参数:
    //   obj:
    //
    // 类型参数:
    //   T:
    private static IEnumerable<XStreamingElement> CreateXArray<T>(T obj) where T : IEnumerable
    {
        return from object o in obj
               select new XStreamingElement("item", CreateTypeAttr(GetJsonType(o)), CreateJsonNode(o));
    }

    //
    // 摘要:
    //     创建 XStreamingElement 对象
    //
    // 参数:
    //   obj:
    private static IEnumerable<XStreamingElement> CreateXObject(object obj)
    {
        if (obj is ExpandoObject source)
        {
            return source.Select<KeyValuePair<string, object>, XStreamingElement>((KeyValuePair<string, object> a) => new XStreamingElement(a.Key, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value)));
        }

        return from pi in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
               select new
               {
                   Name = pi.Name,
                   Value = pi.GetValue(obj, null)
               } into a
               select new XStreamingElement(a.Name, CreateTypeAttr(GetJsonType(a.Value)), CreateJsonNode(a.Value));
    }

    //
    // 摘要:
    //     创建 JSON 字符串
    //
    // 参数:
    //   element:
    private static string CreateJsonString(XStreamingElement element)
    {
        using MemoryStream memoryStream = new MemoryStream();
        using XmlDictionaryWriter xmlDictionaryWriter = JsonReaderWriterFactory.CreateJsonWriter(memoryStream, Encoding.UTF8);
        element.WriteTo(xmlDictionaryWriter);
        xmlDictionaryWriter.Flush();
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    //
    // 摘要:
    //     读取值
    //
    // 参数:
    //   element:
    //
    //   result:
    //
    //   throwOnUndefined:
    private static bool TryGet(XElement element, out object result, bool throwOnUndefined = true)
    {
        if (element == null)
        {
            result = null;
            return !throwOnUndefined;
        }

        result = ToValue(element, throwOnUndefined);
        return true;
    }

    //
    // 摘要:
    //     根据键设置对象值
    //
    // 参数:
    //   name:
    //
    //   value:
    private bool TrySet(string name, object value)
    {
        JsonType jsonType = GetJsonType(value);
        if (value is Clay clay)
        {
            if (clay.IsObject)
            {
                value = value.ToExpandoObject();
            }
            else if (clay.IsArray)
            {
                List<object> list = new List<object>();
                foreach (object item in (dynamic)clay)
                {
                    list.Add((dynamic)((item is Clay value2) ? value2.ToExpandoObject() : item));
                }

                value = list;
            }
        }

        bool isValid;
        XElement xElement = FindXElement(name, out isValid);
        if (xElement == null)
        {
            if (isValid)
            {
                XmlElement.Add(new XElement(name, CreateTypeAttr(jsonType), CreateJsonNode(value)));
            }
            else
            {
                XElement xElement2 = XElement.Parse($"<a:item xmlns:a=\"item\" item=\"{name}\" type=\"{jsonType}\"></a:item>");
                xElement2.ReplaceNodes(CreateJsonNode(value));
                XmlElement.Add(xElement2);
            }
        }
        else
        {
            xElement.Attribute("type").Value = jsonType.ToString();
            xElement.ReplaceNodes(CreateJsonNode(value));
        }

        return true;
    }

    //
    // 摘要:
    //     根据索引设置数组值
    //
    // 参数:
    //   index:
    //
    //   value:
    private bool TrySet(int index, object value)
    {
        JsonType type = GetJsonType(value);
        XElement xElement = XmlElement.Elements().ElementAtOrDefault(index);
        if (xElement == null)
        {
            XmlElement.Add(new XElement("item", CreateTypeAttr(type), CreateJsonNode(value)));
        }
        else
        {
            xElement.Attribute("type").Value = type.ToString();
            xElement.ReplaceNodes(CreateJsonNode(value));
        }

        return true;
    }

    //
    // 摘要:
    //     反序列化
    //
    // 参数:
    //   type:
    private object Deserialize(Type type)
    {
        if (!IsArray)
        {
            return DeserializeObject(type);
        }

        return DeserializeArray(type);
    }

    //
    // 摘要:
    //     反序列化对象
    //
    // 参数:
    //   targetType:
    private object DeserializeObject(Type targetType)
    {
        object obj = Activator.CreateInstance(targetType);
        Dictionary<string, PropertyInfo> dictionary = (from p in targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                       where p.CanWrite
                                                       select p).ToDictionary((PropertyInfo pi) => pi.Name, (PropertyInfo pi) => pi);
        foreach (XElement item in XmlElement.Elements())
        {
            string key = ((item.Name == "{item}item") ? item.Attribute("item").Value : item.Name.LocalName);
            if (dictionary.TryGetValue(key, out var value))
            {
                dynamic val = DeserializeValue(item, value.PropertyType, ThrowOnUndefined);
                value.SetValue(obj, val, null);
            }
        }

        return obj;
    }

    //
    // 摘要:
    //     反序列化值
    //
    // 参数:
    //   element:
    //
    //   elementType:
    //
    //   throwOnUndefined:
    //     如果设置 false，则读取不存在的值返回 null，默认 true
    private static dynamic DeserializeValue(XElement element, Type elementType, bool throwOnUndefined = true)
    {
        dynamic val = ToValue(element, throwOnUndefined);
        if (val is Clay clay)
        {
            val = clay.Deserialize(elementType);
        }

        return ObjectExtensions.ChangeType(val, elementType);
    }

    //
    // 摘要:
    //     反序列化数组
    //
    // 参数:
    //   targetType:
    private object DeserializeArray(Type targetType)
    {
        if (targetType.IsArray)
        {
            Type elementType = targetType.GetElementType();
            dynamic val = Array.CreateInstance(elementType, XmlElement.Elements().Count());
            int num = 0;
            {
                foreach (XElement item in XmlElement.Elements())
                {
                    val[num++] = DeserializeValue(item, elementType, ThrowOnUndefined);
                }

                return val;
            }
        }

        Type elementType2 = targetType.GetGenericArguments()[0];
        dynamic val2 = Activator.CreateInstance(targetType);
        foreach (XElement item2 in XmlElement.Elements())
        {
            val2.Add(DeserializeValue(item2, elementType2, ThrowOnUndefined));
        }

        return val2;
    }

    //
    // 摘要:
    //     根据键查找 System.Xml.Linq.XElement 对象
    //
    // 参数:
    //   name:
    //
    //   isValid:
    private XElement FindXElement(string name, out bool isValid)
    {
        bool flag = (isValid = TryVerifyNCName(name) == null);
        XElement? xElement = (from e in XmlElement.Elements("{item}item")
                              where (string?)e.Attribute("item") == name
                              select e).FirstOrDefault();
        if (xElement == null)
        {
            if (!flag)
            {
                return null;
            }

            xElement = XmlElement.Element(name);
        }

        return xElement;
    }

    //
    // 摘要:
    //     初始化粘土对象枚举器
    public IEnumerator GetEnumerator()
    {
        if (!IsArray)
        {
            return new ClayObjectEnumerator(this);
        }

        return new ClayArrayEnumerator(this);
    }

    //
    // 摘要:
    //     将粘土对象转换成 IEnumerable{T} 对象
    public IEnumerable<T> AsEnumerator<T>()
    {
        return this.Cast<T>();
    }

    //
    // 摘要:
    //     内部粘土对象枚举器
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class ExpandoObjectExtensions
{
    //
    // 摘要:
    //     将对象转 ExpandoObject 类型
    //
    // 参数:
    //   value:
    public static ExpandoObject ToExpandoObject(this object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (value is Clay clay && clay.IsObject)
        {
            dynamic val = new ExpandoObject();
            IDictionary<string, object> dictionary = (IDictionary<string, object>)val;
            foreach (object item in (dynamic)clay)
            {
                KeyValuePair<string, object> keyValuePair = (KeyValuePair<string, object>)(dynamic)item;
                dictionary.Add(keyValuePair.Key, (dynamic)((keyValuePair.Value is Clay value2) ? value2.ToExpandoObject() : keyValuePair.Value));
            }

            return val;
        }

        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            dynamic val2 = new ExpandoObject();
            IDictionary<string, object> dictionary2 = (IDictionary<string, object>)val2;
            foreach (KeyValuePair<string, object> item2 in jsonElement.ToObject() as IDictionary<string, object>)
            {
                dictionary2.Add(item2);
            }

            return val2;
        }

        ExpandoObject expandoObject = value as ExpandoObject;
        if (expandoObject == null)
        {
            expandoObject = new ExpandoObject();
            IDictionary<string, object> dictionary3 = expandoObject;
            foreach (KeyValuePair<string, object> item3 in value.ToDictionary())
            {
                dictionary3.Add(item3);
            }
        }

        return expandoObject;
    }

    //
    // 摘要:
    //     移除 ExpandoObject 对象属性
    //
    // 参数:
    //   expandoObject:
    //
    //   propertyName:
    public static void RemoveProperty(this ExpandoObject expandoObject, string propertyName)
    {
        if (expandoObject == null)
        {
            throw new ArgumentNullException("expandoObject");
        }

        if (propertyName == null)
        {
            throw new ArgumentNullException("propertyName");
        }

        ((IDictionary<string, object>)expandoObject).Remove(propertyName);
    }

    //
    // 摘要:
    //     判断 ExpandoObject 是否为空
    //
    // 参数:
    //   expandoObject:
    public static bool Empty(this ExpandoObject expandoObject)
    {
        return !expandoObject.Any();
    }

    //
    // 摘要:
    //     判断 ExpandoObject 是否拥有某属性
    //
    // 参数:
    //   expandoObject:
    //
    //   propertyName:
    public static bool HasProperty(this ExpandoObject expandoObject, string propertyName)
    {
        if (expandoObject == null)
        {
            throw new ArgumentNullException("expandoObject");
        }

        if (propertyName == null)
        {
            throw new ArgumentNullException("propertyName");
        }

        return ((IDictionary<string, object>)expandoObject).ContainsKey(propertyName);
    }

    //
    // 摘要:
    //     实现 ExpandoObject 浅拷贝
    //
    // 参数:
    //   expandoObject:
    public static ExpandoObject ShallowCopy(this ExpandoObject expandoObject)
    {
        return Copy(expandoObject, deep: false);
    }

    //
    // 摘要:
    //     实现 ExpandoObject 深度拷贝
    //
    // 参数:
    //   expandoObject:
    public static ExpandoObject DeepCopy(this ExpandoObject expandoObject)
    {
        return Copy(expandoObject, deep: true);
    }

    //
    // 摘要:
    //     拷贝 ExpandoObject 对象
    //
    // 参数:
    //   original:
    //
    //   deep:
    private static ExpandoObject Copy(ExpandoObject original, bool deep)
    {
        ExpandoObject expandoObject = new ExpandoObject();
        IDictionary<string, object> dictionary = expandoObject;
        foreach (KeyValuePair<string, object> item in (IEnumerable<KeyValuePair<string, object>>)original)
        {
            dictionary.Add(item.Key, (deep && item.Value is ExpandoObject expandoObject2) ? expandoObject2.DeepCopy() : item.Value);
        }

        return expandoObject;
    }
}


public sealed class ClayArrayEnumerator : IEnumerator
{
    //
    // 摘要:
    //     粘土对象
    public dynamic _clay;

    //
    // 摘要:
    //     当前索引
    private int position = -1;

    //
    // 摘要:
    //     当前元素
    public dynamic Current
    {
        get
        {
            try
            {
                return _clay[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    //
    // 摘要:
    //     当前元素（内部）
    object IEnumerator.Current => Current;

    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   clay:
    //     粘土对象
    public ClayArrayEnumerator(dynamic clay)
    {
        _clay = clay;
    }

    //
    // 摘要:
    //     推进（获取）下一个元素
    public bool MoveNext()
    {
        position++;
        return position < _clay.Length;
    }

    //
    // 摘要:
    //     将元素索引恢复初始值
    public void Reset()
    {
        position = -1;
    }
}

public sealed class ClayObjectEnumerator : IEnumerator
{
    //
    // 摘要:
    //     粘土对象
    public dynamic _clay;

    //
    // 摘要:
    //     当前索引
    private int position = -1;

    //
    // 摘要:
    //     当前元素
    public KeyValuePair<string, dynamic> Current
    {
        get
        {
            try
            {
                XElement xElement = ((XElement)_clay.XmlElement).Elements().ElementAtOrDefault(position);
                string text = ((xElement.Name == "{item}item") ? xElement.Attribute("item").Value : xElement.Name.LocalName);
                return new KeyValuePair<string, object>(text, _clay[text]);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    //
    // 摘要:
    //     当前元素（内部）
    object IEnumerator.Current => (KeyValuePair<string, object>)Current;

    //
    // 摘要:
    //     构造函数
    //
    // 参数:
    //   clay:
    //     粘土对象
    public ClayObjectEnumerator(dynamic clay)
    {
        _clay = clay;
    }

    //
    // 摘要:
    //     推进（获取）下一个元素
    public bool MoveNext()
    {
        position++;
        return position < _clay.Length;
    }

    //
    // 摘要:
    //     将元素索引恢复初始值
    public void Reset()
    {
        position = -1;
    }
}