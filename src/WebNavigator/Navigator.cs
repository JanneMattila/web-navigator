using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebNavigator;

public class Navigator
{
    private readonly ActivitySource _source = new("Navigator");
    private readonly bool _reportingEnabled;
    private readonly string _reportUri;
    private readonly int _reportInterval;
    private readonly Statistics _statistics = new()
    {
        MachineName = Environment.MachineName,
        HTTPStats =
        [
            new HTTPStatusStatistics()
        ]
    };
    private readonly HttpClient _client = new();
    private readonly Random _random = new();
    private readonly Regex _regEx = new(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(15));
    private readonly HashSet<Uri> _visited = [];
    private readonly HashSet<Uri> _queue = [];
    private readonly HashSet<Uri> _ignore = [];
    private readonly string _myipUri;

    public Navigator(string reportUri, int reportInterval, string reportLocation, string myipUri)
    {
        _reportUri = reportUri;
        _reportInterval = reportInterval;
        _reportingEnabled = !string.IsNullOrEmpty(reportUri) && reportInterval > 0;
        _statistics.Location = reportLocation;
        _myipUri = myipUri;

        _client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36 Edg/137.0.0.0"
        );
    }

    public async Task NavigateAsync(string navigateUri)
    {
        using var activity = _source.StartActivity("NavigateAsync");

        if (!string.IsNullOrEmpty(_myipUri))
        {
            try
            {
                var myIp = await _client.GetStringAsync(_myipUri);
                _statistics.IP = myIp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching IP: {ex.Message}");
                _statistics.IP = "Unknown";
            }
        }

        var uri = new Uri(navigateUri);
        var hostname = uri.Host;
        var timestamp = DateTime.UtcNow;

        while (true)
        {
            if (_reportingEnabled &&
                _reportInterval > 0 && (DateTime.UtcNow - timestamp).TotalSeconds >= _reportInterval)
            {
                timestamp = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(_reportUri))
                {
                    try
                    {
                        _statistics.LastUpdated = timestamp;
                        var json = JsonSerializer.Serialize(_statistics);
                        var body = new StringContent(json, Encoding.UTF8, "application/json");
                        await _client.PostAsync(_reportUri, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reporting: {ex.Message}");
                    }
                }
            }

            _visited.Add(uri);
            Console.Write(uri);
            var html = string.Empty;
            var statusCode = 0;
            try
            {
                html = await _client.GetStringAsync(uri);
                statusCode = 200; // Simulating a successful request
                Console.WriteLine(" OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" {ex.Message}");
                if (ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
                {
                    statusCode = (int)httpEx.StatusCode.Value;
                    if (statusCode == 404)
                    {
                        // Ignore this URI for future requests
                        _ignore.Add(uri); 
                        _visited.Remove(uri);
                    }
                }
                else
                {
                    statusCode = 500; // Simulating a server error for any other exception
                }
            }

            if (_reportingEnabled)
            {
                var httpStat = _statistics.HTTPStats.Last();
                var time = DateTime.UtcNow.ToString("HH:mm");
                if (httpStat.Time != time)
                {
                    _statistics.HTTPStats.Add(new HTTPStatusStatistics { Time = time });
                    httpStat = _statistics.HTTPStats.Last();
                }
                httpStat.Time = time;
                if (statusCode >= 200 && statusCode < 300)
                {
                    httpStat.StatusCode2XX++;
                }
                else if (statusCode >= 300 && statusCode < 400)
                {
                    httpStat.StatusCode3XX++;
                }
                else if (statusCode >= 400 && statusCode < 500)
                {
                    httpStat.StatusCode4XX++;
                }
                else if (statusCode >= 500 && statusCode < 600)
                {
                    httpStat.StatusCode5XX++;
                }
            }

            if (string.IsNullOrEmpty(html) || statusCode >= 400)
            {
                // If we got an error or no content, we won't follow links
                uri = GetNextUri();
                continue;
            }

            var matches = _regEx.Matches(html);
            for (int i = 0; i < matches.Count; i++)
            {
                var href = matches[i].Groups[1].Value;
                try
                {
                    var newUri = new Uri(uri, href);
                    if (newUri.Host != hostname) continue;
                    if (_visited.Contains(newUri)) continue;
                    if (_queue.Contains(newUri)) continue;
                    if (_ignore.Contains(newUri)) continue;

                    _queue.Add(newUri);
                }
                catch (Exception)
                {
                    // Anything can be in <a href="..."> but 
                    // we'll only take something that can be converted
                    // to System.Uri.
                }
            }

            uri = GetNextUri();
        }
    }

    private Uri GetNextUri()
    {
        if (_queue.Any())
        {
            var uri = _queue.First();
            _queue.Remove(uri);
            return uri;
        }
        else
        {
            var index = _random.Next(_visited.Count);
            var uri = _visited.Skip(index).First();
            return uri;
        }
    }
}
