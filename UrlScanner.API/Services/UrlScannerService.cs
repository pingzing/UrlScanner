using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UrlScanner.API.Services
{
    public class UrlScannerService : IUrlScannerService
    {
        // Shamelessly lifted from John Gruber's URL regex and slightly modified to handle Unicode
        // Original source: https://gist.github.com/gruber/249502#gistcomment-1945967
        private static readonly Regex _urlRegex = new Regex(@"
\b  # Only look potential URLs that begin on a word boundary
(   # Capture entire URL
    (?:         
        [a-z][\w-]+:    # URL protocol and colon
        (?:
            /{1,3}      # 1-3 slashes
            |           # or!
            [a-z0-9%]   # a single letter, or digit, or percent sign (%)
        )        
        |
        [0-9\p{L}]+[.]   # Any number of unicode letters followed  by a dot        
        |
        [0-9\p{L}]+[.]\p{L}{2,24} # Something that looks like a domain name (i.e. letters, followed by a dot, then a TLD)
        |
        (?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(?::[0-9]{1,5})?  # IPv4 address with optional port
    )
    (?:                                     # One or more:
        [^\s()<>]                           # Non-whitespace, non paren, non angle-bracket characters
        |
        \(([^\s()<>]+|(\([^\s()<>]+\)))*\)  # Balanced parens, around non-whistepsace, non-paren chars up to two levels deep
    )+
    (?: # Ending with:
        \(([^\s()<>]+|(\([^\s()<>]+\)))*\)  # Balenced parens around non-whitespace, non-paren chars up to two levels deep
        |
        [^\s\!()\[\]{};:\'\""\.\,<>?«»“”‘’] # Not a space or these punctuation characters
    )
)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private readonly ILogger<UrlScannerService> _logger;
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

        public UrlScannerService(ILogger<UrlScannerService> logger)
        {
            _logger = logger;
        }

        public Uri[] GetUrls(string body)
        {
            // List of candidate matches
            var matches = _urlRegex.Matches(body);
            if (matches == null)
            {
                return Array.Empty<Uri>();
            }
            return matches.Select(urlCandidate =>
            {
                // If we can't even *conceivably* turn it into a URI, throw it out
                if (!Uri.TryCreate(urlCandidate.Value, UriKind.RelativeOrAbsolute, out Uri? parsedUri))
                {
                    return null;
                }

                // If it's not an absolute URI, let's try to make it one by adding an http:// to it
                if (!parsedUri.IsAbsoluteUri)
                {
                    if (!Uri.TryCreate($"http://{urlCandidate}", UriKind.Absolute, out parsedUri))
                    {
                        return null;
                    }
                }

                // If we have no hostname or host, it's possible that we gave Uri a bare 'domain.tld:1234', which it struggles with
                // if there's no scheme. Try to glue on an HTTP and see if that helps.
                if (parsedUri.HostNameType == UriHostNameType.Unknown && string.IsNullOrEmpty(parsedUri.Host))
                {
                    if (!Uri.TryCreate($"http://{urlCandidate}", UriKind.Absolute, out parsedUri))
                    {
                        return null;
                    }
                    // If it didn't work, shrug our shoulders and return null.
                    if (parsedUri.HostNameType == UriHostNameType.Unknown || string.IsNullOrEmpty(parsedUri.Host))
                    {
                        return null;
                    }
                }

                return parsedUri;
            }).Where(parsedUri =>
            {
                // If it's null, it wasn't a valid URI.
                if (parsedUri == null)
                {
                    return false;
                }

                // If it's an IP address and not a URL, it won't have a TLD.
                if (parsedUri.HostNameType == UriHostNameType.IPv4 || parsedUri.HostNameType == UriHostNameType.IPv6)
                {
                    // If it has a scheme and an IP, it's probably valid.
                    return !string.IsNullOrEmpty(parsedUri.Scheme);
                }

                // Now that we've made it absolute and possibly glued on an "http", check the scheme, and if it's http,
                // make sure it has a valid TLD.
                if (parsedUri.Scheme == "http" || parsedUri.Scheme == "https")
                {
                    return _tlds.Value.Any(tld => parsedUri.Host.EndsWith($".{tld}"));
                }

                return true;
            }).ToArray()!;
        }
    }
}
