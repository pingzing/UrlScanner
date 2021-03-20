using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using urldetector.detection;

namespace UrlScanner.API.Services
{
    public class LinkedInUrlScanner : IUrlScannerService
    {
        private readonly Lazy<List<string>> _tlds = new Lazy<List<string>>(() =>
        {
            List<string> tlds = new List<string>();
            foreach (string line in File.ReadLines("tlds.txt", Encoding.UTF8))
            {
                // Ignoring the wildcard and exception rules, because we're only using this as a 
                // heuristic anyway.
                if (line.StartsWith("//")
                    || line.StartsWith("*")
                    || line.StartsWith("!")
                    || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                tlds.Add(line);
            }
            return tlds;
        }, isThreadSafe: false);

        public string[] GetUrls(string body)
        {
            UrlDetector detector = new UrlDetector(body,
                UrlDetectorOptions.QUOTE_MATCH |
                UrlDetectorOptions.SINGLE_QUOTE_MATCH |
                UrlDetectorOptions.BRACKET_MATCH |
                UrlDetectorOptions.JSON |
                UrlDetectorOptions.JAVASCRIPT |
                UrlDetectorOptions.XML |
                UrlDetectorOptions.HTML |
                UrlDetectorOptions.ALLOW_SINGLE_LEVEL_DOMAIN,
                validSchemes: new HashSet<string> { "http", "https", "ftp", "ftps", "ws", "wss" });

            var discoveredUrls = detector.Detect();
            if (discoveredUrls == null || !discoveredUrls.Any())
            {
                return Array.Empty<string>();
            }
            return discoveredUrls
                .Where(x =>
                {
                    if (x == null)
                    {
                        return false;
                    }

                    // Check to see if it's an IP address. If so, we can skip the TLD check.
                    // Even if it doesn't parse to a C# Uri, it _may_ still be valid-enough, so run the TLD check on it.
                    if (Uri.TryCreate(x.GetFullUrl(), UriKind.Absolute, out Uri? parsedUri))
                    {
                        if (parsedUri.HostNameType == UriHostNameType.IPv4 || parsedUri.HostNameType == UriHostNameType.IPv6)
                        {
                            return true;
                        }
                    }

                    // TLD check, to make sure we don't pick up files
                    return _tlds.Value.Any(tld => x.GetHost().EndsWith($".{tld}"));
                })
                .Select(x => x.GetFullUrl())
                .ToArray()!;
        }
    }
}
