using EasyTemplate.Tool.Util;
using Microsoft.AspNetCore.Mvc;

namespace EasyTemplate.Service;

/// <summary>
/// /swagger/all/swagger.json
/// </summary>
[DynamicController]
[ApiException]
[ApiAuthorize]
[ApiResult]
public class BaseService { }