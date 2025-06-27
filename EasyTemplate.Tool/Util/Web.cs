using System.Net;
using System.Text;
using RestSharp;

namespace EasyTemplate.Tool;

public class Web
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="headers">请求头</param>
    /// <returns></returns>
    public static string Get(string url, Dictionary<string, string> headers = null)
    {
        var client = new RestClient(url);
        var request = new RestRequest(Method.GET);
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
        }
        var response = client.Execute(request);
        return response.Content;
    }
    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="body">请求体</param>
    /// <param name="headers">请求头</param>
    /// <returns></returns>
    public static string Post(string url, object body, Dictionary<string, string> headers = null)
    {
        var client = new RestClient(url);
        var request = new RestRequest(Method.POST);
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
        }
        request.AddJsonBody(body);
        var response = client.Execute(request);
        return response.Content;
    }
    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="body">请求体</param>
    /// <param name="headers">请求头</param>
    /// <returns></returns>
    public static string Put(string url, object body, Dictionary<string, string> headers = null)
    {
        var client = new RestClient(url);
        var request = new RestRequest(Method.PUT);
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
        }
        request.AddJsonBody(body);
        var response = client.Execute(request);
        return response.Content;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetWanIp()
    {
        string ip = string.Empty;
        try
        {
            string url = "http://www.net.cn/static/customercare/yourip.asp";
            string html = "";
            using (var client = new HttpClient())
            {
                var reponse = client.GetAsync(url).GetAwaiter().GetResult();
                reponse.EnsureSuccessStatusCode();
                html = reponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!string.IsNullOrEmpty(html))
            {
                ip = Resove(html, "<h2>", "</h2>");
            }
        }
        catch (Exception)
        {
            return string.Empty;
        }
        return ip;
    }


    /// <summary>
    /// Get part Content from HTML by apply prefix part and subfix part
    /// </summary>
    /// <param name="html">souce html</param>
    /// <param name="prefix">prefix</param>
    /// <param name="subfix">subfix</param>
    /// <returns>part content</returns>
    public static string Resove(string html, string prefix, string subfix)
    {
        int inl = html.IndexOf(prefix);
        if (inl == -1)
        {
            return null;
        }
        inl += prefix.Length;
        int inl2 = html.IndexOf(subfix, inl);
        string s = html.Substring(inl, inl2 - inl);
        return s;
    }

    public static string GetIpLocation(string ipAddress)
    {
        string ipLocation = "未知";
        try
        {
            if (!IsInnerIP(ipAddress))
            {
                ipLocation = GetIpLocationFromPCOnline(ipAddress);
            }
            else
            {
                ipLocation = "本地局域网";
            }
        }
        catch (Exception)
        {
            return ipLocation;
        }
        return ipLocation;
    }

    private static string GetIpLocationFromPCOnline(string ipAddress)
    {
        string ipLocation = "未知";
        try
        {
            var res = "";
            using (var client = new HttpClient())
            {
                var URL = "http://whois.pconline.com.cn/ip.jsp?ip=" + ipAddress;
                var response = client.GetAsync(URL).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                res = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            if (!string.IsNullOrEmpty(res))
            {
                ipLocation = res.Trim();
            }
        }
        catch
        {
            ipLocation = "未知";
        }
        return ipLocation;
    }

    public static bool IsInnerIP(string ipAddress)
    {
        bool isInnerIp = false;
        long ipNum = GetIpNum(ipAddress);
        /**
            私有IP：A类 10.0.0.0-10.255.255.255
                        B类 172.16.0.0-172.31.255.255
                        C类 192.168.0.0-192.168.255.255
            当然，还有127这个网段是环回地址
       **/
        long aBegin = GetIpNum("10.0.0.0");
        long aEnd = GetIpNum("10.255.255.255");
        long bBegin = GetIpNum("172.16.0.0");
        long bEnd = GetIpNum("172.31.255.255");
        long cBegin = GetIpNum("192.168.0.0");
        long cEnd = GetIpNum("192.168.255.255");
        isInnerIp = IsInner(ipNum, aBegin, aEnd) || IsInner(ipNum, bBegin, bEnd) || IsInner(ipNum, cBegin, cEnd) || ipAddress.Equals("127.0.0.1");
        return isInnerIp;
    }

    /// <summary>
    /// 把IP地址转换为Long型数字
    /// </summary>
    /// <param name="ipAddress">IP地址字符串</param>
    /// <returns></returns>
    private static long GetIpNum(string ipAddress)
    {
        string[] ip = ipAddress.Split('.');
        long a = int.Parse(ip[0]);
        long b = int.Parse(ip[1]);
        long c = int.Parse(ip[2]);
        long d = int.Parse(ip[3]);

        long ipNum = a * 256 * 256 * 256 + b * 256 * 256 + c * 256 + d;
        return ipNum;
    }

    private static bool IsInner(long userIp, long begin, long end)
    {
        return (userIp >= begin) && (userIp <= end);
    }
}