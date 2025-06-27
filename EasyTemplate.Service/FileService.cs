using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EasyTemplate.Service.Common;
using EasyTemplate.Tool.Entity;
using Microsoft.AspNetCore.Components;
using EasyTemplate.Tool;
using static EasyTemplate.Tool.Entity.PublicEnum;
using System.Net;
using System;

namespace EasyTemplate.Service;

[ApiGroup(ApiGroupNames.System)]
public class FileService : BaseService
{
    /// <summary>
    /// 
    /// </summary>
    private readonly IHttpContextAccessor _contextAccessor;

    public FileService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    /// <param name="classification"></param>
    /// <returns></returns>
    [HttpPost, AllowAnonymous]
    public async Task<object> UploadImage(string classification = "default")
    {
        var host = _contextAccessor.GetHost();
        var file = _contextAccessor.HttpContext.Request.Form.Files[0];
        if (file.Length <= 0)
        {
            return "";
        }

        using var memoryStream = new MemoryStream();
        file.CopyTo(memoryStream);
        var fileBytes = memoryStream.ToArray();

        // 获取文件扩展名
        var fileExtension = Path.GetExtension(file.FileName);
        // 生成文件名
        var id = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
        var fileName = $"{id}{fileExtension}";
        // 获取当前日期
        var currentDate = DateTime.Now;
        // 构建目录路径
        var directoryPath = Path.Combine("wwwroot", "image", classification, currentDate.ToString("yyyy"), currentDate.ToString("MM"), currentDate.ToString("dd"));
        // 创建目录
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        // 构建文件路径
        var filePath = Path.Combine(directoryPath, fileName);
        // 保存文件
        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

        // 构建返回的相对路径
        var relativePath = $"/image/{classification}/{currentDate:yyyy}/{currentDate:MM}/{currentDate:dd}/{fileName}";
        return relativePath;
    }
}
