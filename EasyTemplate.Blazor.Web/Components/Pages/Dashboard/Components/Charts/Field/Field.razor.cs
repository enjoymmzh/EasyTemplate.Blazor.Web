using Microsoft.AspNetCore.Components;

namespace EasyTemplate.Blazor.Web.Components.Pages.Dashboard.Components.Charts;

public partial class Field
{
    [Parameter]
    public string Label { get; set; }

    [Parameter]
    public string Value { get; set; }
}