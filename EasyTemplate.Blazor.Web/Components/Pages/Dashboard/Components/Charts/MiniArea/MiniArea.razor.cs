public class ChartData
{
    public ChartDataItem[] VisitData { get; set; }
    public ChartDataItem[] VisitData2 { get; set; }
    public ChartDataItem[] SalesData { get; set; }
    public SearchDataItem[] SearchData { get; set; }
    public OfflineDataItem[] OfflineData { get; set; }
    public OfflineChartDataItem[] OfflineChartData { get; set; }
    public ChartDataItem[] SalesTypeData { get; set; }
    public ChartDataItem[] SalesTypeDataOnline { get; set; }
    public ChartDataItem[] SalesTypeDataOffline { get; set; }
    public RadarDataItem[] RadarData { get; set; }
}

public class ChartDataItem
{
    public string X { get; set; }
    public int Y { get; set; }
}

public class SearchDataItem
{
    public int Index { get; set; }
    public string Keywod { get; set; }
    public int Count { get; set; }
    public int Range { get; set; }
    public int Status { get; set; }
}

public class OfflineDataItem
{
    public string Name { get; set; }
    public float Cvr { get; set; }
}

public class OfflineChartDataItem
{
    public long X { get; set; }
    public int Y1 { get; set; }
    public int Y2 { get; set; }
}

public class RadarDataItem
{
    public string Name { get; set; }
    public string Label { get; set; }
    public int Value { get; set; }
}