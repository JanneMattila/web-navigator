using System;
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

        public async Task NavigateAsync(string navigateUri)
        {
            var uri = new Uri(navigateUri);
            var hostname = uri.Host;

            while (true)
            {
                var html = await _client.GetStringAsync(uri);
                var matches = _regEx.Matches(html);
                if (matches.Count == 0)
                {
                    break;
                }

                var index = _random.Next(matches.Count);
                var href = matches[index].Groups[1].Value;
                var newUri = new Uri(uri, href);
                if (newUri.Host != hostname)
                {
                    break;
                }

                uri = newUri;
            }
        }
    }
}
