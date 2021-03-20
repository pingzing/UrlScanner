using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UrlScanner.API.Services
{
    public class RegexUrlScanner : IUrlScannerService
    {
        private static readonly Regex _endPunctationRemover = new Regex(@".+[^\s`'""!.,<>?«»“”‘’]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Inspired by John Gruber's URL regex and heavily modified to be more liberal
        // Original source: https://gist.github.com/gruber/249502#gistcomment-1945967
        private static readonly Regex _urlRegex = new Regex(@"
(?:
    (?:
        [a-z][\w-]+: # Protocol followed by colon
        (?:
            \/{1,3}   # Either 1-3 slashes
            |
            [0-9]{1,6}  # Or a port number
            | 
            [a-z0-9%]   # Or a single letter, digit or percent sign
        )
        |        
        localhost(?::[0-9]{1,6})? # Localhost with optional port
        |
        (?:(?:[0-9].){1,3}){3}[0-9]{1,3}(?::[0-9]{1,6})? # IPv4 address with optional port
        | 
        [\p{L}\p{N}\p{S}\p{C}.\-0-9@~!&'*+=]+\.[{\p{L}]{2,26}(?::[0-9]{1,6})? # Unicode-friendly domain with optional port number
    )
    (?: # One or more:
        [^\s()<>] # Run of non-space, non-()<>
        |
        \(([^\s()<>]|(\([^\s()<>]+\)))*\) # balanced parens, up to 2 levels
        
    ){0,} # Optional, so anything that gets handled by the first block alone still gets matched
    (?:  # End with:
        \(([^\s()<>]|(\([^\s()<>]+\)))*\) # balanced parens, up to 2 levels
        |                                             
        [^\s`!()\[\]{};:'"".,<>?«»“”‘’] # not a space or one of these punctuation characters
    ){0,} # Optional, so anything that gets handled by the first or second block alone still gets matched
)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        // Differs only VERY slightly: the third block is no longer optional.
        private static readonly Regex _stricterRegex = new Regex(@"
(?:
    (?:
        [a-z][\w-]+: # Protocol followed by colon
        (?:
            \/{1,3}   # Either 1-3 slashes
            |
            [0-9]{1,6}  # Or a port number
            | 
            [a-z0-9%]   # Or a single letter, digit or percent sign
        )
        |        
        localhost(?::[0-9]{1,6})? # Localhost with optional port
        |
        (?:(?:[0-9].){1,3}){3}[0-9]{1,3}(?::[0-9]{1,6})? # IPv4 address with optional port
        | 
        [\p{L}\p{N}\p{S}\p{C}.\-0-9@~!&'*+=]+\.[{\p{L}]{2,26}(?::[0-9]{1,6})? # Unicode-friendly domain with optional port number
    )
    (?: # One or more:
        [^\s()<>] # Run of non-space, non-()<>
        |
        \(([^\s()<>]|(\([^\s()<>]+\)))*\) # balanced parens, up to 2 levels
        
    ){0,}
    (?:  # End with exactly one:
        \(([^\s()<>]|(\([^\s()<>]+\)))*\) # set of balanced parens, up to 2 levels
        |                                             
        [^\s`!()\[\]{};:'"".,<>?«»“”‘’] # not a space or one of these punctuation characters
    )
)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

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

        private readonly IdnMapping _idn = new IdnMapping();

        public string[] GetUrls(string body)
        {
            // List of candidate matches
            var matches = _urlRegex.Matches(body);
            if (matches == null)
            {
                return Array.Empty<string>();
            }

            // Attempt to process each candidate URL into a canonical URI
            return matches.Select(urlCandidate =>
            {
                // If we can't even *conceivably* turn it into a URI, we give it one more chance under the stricter regex
                // that cuts out more punctuation
                // If that still doesn't work, it's probably not a URL.
                if (!Uri.TryCreate(urlCandidate.Value, UriKind.RelativeOrAbsolute, out Uri? parsedUri))
                {
                    urlCandidate = _stricterRegex.Match(urlCandidate.Value);
                    if (!Uri.TryCreate(urlCandidate.Value, UriKind.RelativeOrAbsolute, out parsedUri))
                    {
                        return null;
                    }
                }

                // If it's not an absolute URI, let's try to make it one by adding an http:// to it
                if (!parsedUri.IsAbsoluteUri)
                {
                    if (!Uri.TryCreate($"http://{urlCandidate.Value}", UriKind.Absolute, out parsedUri))
                    {
                        // If we can't turn that into a Uri, give it one more try with the stricter regex, just like above
                        urlCandidate = _stricterRegex.Match(urlCandidate.Value);
                        if (!Uri.TryCreate($"http://{urlCandidate.Value}", UriKind.Absolute, out parsedUri))
                        {
                            return null;
                        }
                    }

                    // If this still still hasn't resulted in an absolute URI, it probably isn't valid.
                    if (parsedUri == null || !parsedUri.IsAbsoluteUri)
                    {
                        return null;
                    }
                }

                // One last attempt to remove any trailing punctuation our initial regex missed:
                var noTrailingPunctuation = _endPunctationRemover.Match(parsedUri.OriginalString);
                if (noTrailingPunctuation.Success)
                {
                    // If this fails, just continue using whatever's already in parsedUri.
                    if (Uri.TryCreate(noTrailingPunctuation.Value, UriKind.Absolute, out Uri? noTrailingPunctuationUri))
                    {
                        parsedUri = noTrailingPunctuationUri;
                    }
                }

                // If we have no hostname or host, it's possible that we gave Uri a bare 'domain.tld:1234', which it struggles with
                // if there's no scheme. Try to glue on an HTTP and see if that helps.
                if (parsedUri.HostNameType == UriHostNameType.Unknown && string.IsNullOrEmpty(parsedUri.Host))
                {
                    if (!Uri.TryCreate($"http://{urlCandidate.Value}", UriKind.Absolute, out parsedUri))
                    {
                        // There are a handful of special cases where our regex is TOO liberal and leaves trailing punctuation.
                        return null;
                    }
                    // If it didn't work, shrug our shoulders and return null.
                    if (parsedUri.HostNameType == UriHostNameType.Unknown || string.IsNullOrEmpty(parsedUri.Host))
                    {
                        return null;
                    }
                }

                // The goal is to make the URI compliant with the URI class's IsWellFormed check,
                // which means replacing emojis with punycode. 
                parsedUri = PunyEncodeHostname(parsedUri);
                if (parsedUri == null)
                {
                    return null;
                }

                // Throw out some final invalid values: empty URLs, URLs with no scheme, URIs that are not well-formed, etc, 
                if (string.IsNullOrWhiteSpace(parsedUri.ToString())
                    || !Uri.CheckSchemeName(parsedUri.Scheme)
                    || Uri.CheckHostName(parsedUri.Host) == UriHostNameType.Unknown
                    || !Uri.IsWellFormedUriString(parsedUri.ToString(), UriKind.Absolute))
                {
                    return null;
                }

                return parsedUri;

                // Remove any invalid URLs, and perform final validation
            }).Where(parsedUri =>
            {
                // If it's null, it wasn't a valid URI.
                if (parsedUri == null)
                {
                    return false;
                }

                // If it's an IP address and not a URL, it won't have a TLD.
                if (parsedUri.HostNameType == UriHostNameType.IPv4
                || parsedUri.HostNameType == UriHostNameType.IPv6
                || parsedUri.IsLoopback)
                {
                    // As long as it has a scheme and an IP, it's probably valid.
                    return !string.IsNullOrEmpty(parsedUri.Scheme);
                }

                // Now that we've made it absolute and possibly glued on an "http", check the scheme, and if it's a well-known
                // scheme that we know needs a TLD, make sure it has one.
                // This does not account for wacky shenanigans where a TLD *itself* has an A record pointing to some IP address,
                // resulting in valid URLs such as http://to./
                if (parsedUri.Scheme == "http" || parsedUri.Scheme == "https"
                    || parsedUri.Scheme == "ftp" || parsedUri.Scheme == "ftps"
                    || parsedUri.Scheme == "ws" || parsedUri.Scheme == "wss")
                {
                    parsedUri = PunyDecodeHostname(parsedUri);
                    return _tlds.Value.Any(tld => parsedUri.Host.EndsWith($".{tld}"));
                }

                return true;
            })
            .Select(x => PunyDecodeHostname(x)) // hilariously wasteful, but it gets the job done
            .Select(x => x.ToString())
            .ToArray()!;
        }

        private Uri? PunyEncodeHostname(Uri parsedUri)
        {
            try
            {
                return new Uri(parsedUri.ToString().Replace(parsedUri.Host, parsedUri.IdnHost));
            }
            catch (UriFormatException ex)
            {
                // Only certain emojis are valid hostnames. If our hostname is invalid, the URL is invalid.
                return null;
            }
        }

        private Uri PunyDecodeHostname(Uri parsedUri)
        {
            return new Uri(parsedUri.ToString().Replace(parsedUri.Host, _idn.GetUnicode(parsedUri.Host)));
        }
    }
}
