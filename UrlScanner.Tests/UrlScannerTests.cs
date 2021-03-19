using Microsoft.Extensions.Logging;
using Moq;
using System;
using UrlScanner.API.Services;
using Xunit;

namespace UrlScanner.Tests
{
    public class UrlScannerTests
    {
        private ILogger<UrlScannerService> _mockLogger;
        private UrlScannerService _urlScanner;

        public UrlScannerTests()
        {
            _mockLogger = Mock.Of<ILogger<UrlScannerService>>();
            _urlScanner = new UrlScannerService(_mockLogger);
        }

        [Theory]
        [InlineData("http://www.google.com")]
        [InlineData("http://somedomainname.com:1234")]
        [InlineData("500px.com")]
        [InlineData("https://www.google.com")]
        [InlineData("www.google.com")]
        [InlineData("http://google.com")]
        [InlineData("google.com")]
        [InlineData("htp://www.google.com")]
        [InlineData("http://www.google.co.uk")]
        [InlineData("http://www.google.dk")]
        [InlineData("http://www.google")]
        [InlineData("wwww.google.com")]
        [InlineData("http://foo.com/blah_blah")]
        [InlineData("http://foo.com/blah_blah/")]
        [InlineData("http://foo.com/blah_blah_(wikipedia)")]
        [InlineData("http://foo.com/blah_blah_(wikipedia)_(again)")]
        [InlineData("http://www.example.com/wpstyle/?p=364")]
        [InlineData("https://www.example.com/foo/?bar=baz&inga=42&quux")]
        [InlineData("http://✪df.ws/123")]
        [InlineData("http://userid:password@example.com:8080")]
        [InlineData("http://userid:password@example.com:8080/")]
        [InlineData("http://userid@example.com")]
        [InlineData("http://userid@example.com/")]
        [InlineData("http://userid@example.com:8080")]
        [InlineData("http://userid@example.com:8080/")]
        [InlineData("http://userid:password@example.com")]
        [InlineData("http://userid:password@example.com/")]
        [InlineData("http://142.42.1.1/")]
        [InlineData("http://142.42.1.1:8080/")]
        [InlineData("http://➡.ws/䨹")]
        [InlineData("http://⌘.ws")]
        [InlineData("http://⌘.ws/")]
        [InlineData("http://foo.com/blah_(wikipedia)#cite-1")]
        [InlineData("http://foo.com/blah_(wikipedia)_blah#cite-1")]
        [InlineData("http://foo.com/unicode_(✪)_in_parens")]
        [InlineData("http://foo.com/(something)?after=parens")]
        [InlineData("http://☺.damowmow.com/")]
        [InlineData("http://code.google.com/events/#&product=browser")]
        [InlineData("http://j.mp")]
        [InlineData("ftp://foo.bar/baz")]
        [InlineData("http://foo.bar/?q=Test%20URL-encoded%20stuff")]
        // [InlineData("http://مثال.إختبار")] // <-- This fails due to the RTL shenanigans. If the scanner checks for .StartsWith($"{مثال}."), then it works.
        [InlineData("http://例子.测试")]
        [InlineData("http://उदाहरण.परीक्षा")]
        [InlineData("http://-.~_!$&'()*+,;=:%40:80%2f::::::@example.com")]
        [InlineData("http://1337.net")]
        [InlineData("http://a.b-c.de")]
        [InlineData("http://223.255.255.254")]
        [InlineData("somedomainname.com:1234")] // This is right on the borderline. Most URL highlighters wouldn't highlight this, but a browser would accept it.
        [InlineData("http://greece.ευ")] // <-- new international unicode TLD
        [InlineData("http://xn--addas-o4a.de/")] // A punycode URL for...
        [InlineData("adıdas.de")] // an "adidas" website written with a dotless i. 
        public void UrlScanner_ShouldDetectUrls(string input)
        {
            var uris = _urlScanner.GetUrls(input);
            Assert.Single(uris);
        }

        [Theory]
        [InlineData("http://www.google.com", "http://www.google.com/")]
        [InlineData("🍕.ws", "http://🍕.ws/")]
        [InlineData("500px.com", "http://500px.com/")]
        [InlineData("www.google.com", "http://www.google.com/")]
        public void UrlScanner_ShouldCorrectlyCanonicalizeUrls(string input, string expected)
        {
            Uri.TryCreate(expected, UriKind.Absolute, out Uri? parsedUri);
            var uris = _urlScanner.GetUrls(input);
        }

        [Theory]
        [InlineData("bingmaps://?cp=40.726966~-74.006076")] // <-- opens the Maps app to New York in Windows 10
        [InlineData("ftp://someftpaddress.com")]
        [InlineData("ftp://someftpaddress.com:443")]
        [InlineData("wss://socketurl.com/socketendpoint")]
        public void UrlScanner_ShouldDetectNonHttpUris(string input)
        {
            var uris = _urlScanner.GetUrls(input);
            Assert.Single(uris);
        }

        [Theory]
        [InlineData("picture.dog.jpg")] // Most of these are technically valid relative URLs
        [InlineData("heresathing.bmp")] // But we only want to be returning valid absolute URLs
        [InlineData("fragmentary/url/data.html")]
        public void UrlScanner_ShouldNotReturnFalsePositives(string input)
        {
            var uris = _urlScanner.GetUrls(input);
            Assert.Empty(uris);
        }
    }
}
