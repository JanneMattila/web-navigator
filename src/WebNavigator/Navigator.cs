using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebNavigator
{
    public class Navigator
    {
        private readonly HttpClient _client = new();
        private readonly Random _random = new();
        private readonly Regex _regEx = new(@"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(15));
        private readonly HashSet<Uri> _visited = new();
        private readonly HashSet<Uri> _queue = new();

        public async Task NavigateAsync(string navigateUri)
        {
            var uri = new Uri(navigateUri);
            var hostname = uri.Host;

            while (true)
            {
                _visited.Add(uri);
                Console.Write(uri);
                var html = string.Empty;
                try
                {
                    html = await _client.GetStringAsync(uri);
                    Console.WriteLine(" OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" {ex.Message}");
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
}
