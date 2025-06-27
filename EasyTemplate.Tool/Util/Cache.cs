using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTemplate.Tool.Util;

public class Cache
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set(string key, object value, string lockkey = "")
    {
        var type = Setting.Get<string>("Cache:CacheType");
        if ((type?.ToLower().Contains("redis")).Value)
        {
            if (!string.IsNullOrWhiteSpace(lockkey))
            {
                Redis.SetWithLockAsync(lockkey, key, value);
            }
            else
            {
                Redis.SetAsync(key, value);
            }
        }
        else
        {
            MemoryCache.Set(key, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static async Task Increase(string key, int value = 1, string lockkey = "")
    {
        var current = await Get<int>(key);
        var type = Setting.Get<string>("Cache:CacheType");
        if ((type?.ToLower().Contains("redis")).Value)
        {
            if (!string.IsNullOrWhiteSpace(lockkey))
            {
                await Redis.SetWithLockAsync(lockkey, key, current+value);
            }
            else
            {
                await Redis.SetAsync(key, current + value);
            }
        }
        else
        {
            MemoryCache.Set(key, current + value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static async Task<T> Get<T>(string key)
    {
        var type = Setting.Get<string>("Cache:CacheType");
        if ((type?.ToLower().Contains("redis")).Value)
        {
            return await Redis.GetAsync<T>(key);
        }
        else
        {
            return MemoryCache.GetT<T>(key);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static async Task<bool> Exist(string key)
    {
        var type = Setting.Get<string>("Cache:CacheType");
        if ((type?.ToLower().Contains("redis")).Value)
        {
            return await Redis.ExistsAsync(key);
        }
        else
        {
            return MemoryCache.Exists(key);
        }
    }
}
