﻿using Microsoft.AspNetCore.Components;

namespace EasyTemplate.Blazor.Web.Components.Pages.Dashboard.Components.Charts;

public partial class ChartCard
{
    [Parameter]
    public string Avatar { get; set; }

    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public RenderFragment Action { get; set; }

    [Parameter]
    public string Total { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment Footer { get; set; }

    [Parameter]
    public string ContentHeight { get; set; }
}