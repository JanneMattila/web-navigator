using System;
using System.Collections.Generic;

namespace WebNavigator;

public class Statistics
{
    public string MachineName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<HTTPStatusStatistics> HTTPStats { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class HTTPStatusStatistics
{
    public string Time { get; set; } = DateTime.UtcNow.ToString("HH:mm");

    public int StatusCode2XX { get; set; }
    public int StatusCode3XX { get; set; }
    public int StatusCode4XX { get; set; }
    public int StatusCode5XX { get; set; }
}