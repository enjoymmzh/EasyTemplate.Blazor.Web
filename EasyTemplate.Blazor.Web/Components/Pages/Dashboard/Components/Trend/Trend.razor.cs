using Microsoft.AspNetCore.Components;

namespace EasyTemplate.Blazor.Web.Components.Pages.Dashboard.Components;

public partial class Trend
{
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public string Flag { get; set; }
}