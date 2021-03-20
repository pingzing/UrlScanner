using System;
using System.Collections.Generic;
using UrlScanner.API.Services;
using Xunit;

namespace UrlScanner.Tests
{
    public class LinkedInUrlScannerTests
    {
        private LinkedInUrlScanner _urlScanner;

        public LinkedInUrlScannerTests()
        {
            _urlScanner = new LinkedInUrlScanner();
        }

        [Theory]
        [InlineData("http://🍕.ws/")]
        [InlineData("http://www.google.com")]
        [InlineData("http://somedomainname.com:1234")]
        [InlineData("500px.com")]
        [InlineData("https://www.google.com")]
        [InlineData("www.google.com")]
        [InlineData("http://google.com")]
        [InlineData("google.com")]
        [InlineData("http://www.google.co.uk")]
        [InlineData("http://www.google.dk")]
        [InlineData("http://www.google")]
        [InlineData("wwww.google.com")]
        // The LinkedIn library doesn't handle localhost, but that's not really a showstopper. It was only ever a nice-to-have, really.
        //[InlineData("localhost")]
        //[InlineData("https://localhost")]
        //[InlineData("https://localhost/")]
        //[InlineData("https://localhost:123")]
        //[InlineData("https://localhost:44325/swagger/index.html")]
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
        [InlineData("http://142.42.1.1")]
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
        //[InlineData("http://مثال.إختبار")] // <-- This may fail due RTL shenanignans, or the parser correctly realizing that this is a reserved TLD.
        [InlineData("http://例子.测试")]
        [InlineData("http://उदाहरण.परीक्षा")]
        [InlineData("http://-.~_!$&'()*+,;=:%40:80%2f::::::@example.com")]
        [InlineData("http://1337.net")]
        [InlineData("http://a.b-c.de")]
        [InlineData("http://223.255.255.254")]
        [InlineData("somedomainname.com:1234")] // This is right on the borderline. Most URL highlighters wouldn't highlight this, but a browser would accept it.
        [InlineData("http://greece.ευ")] // <-- new international unicode TLD
        [InlineData("http://xn--addas-o4a.de/")] // A punycode URL for...
        [InlineData("adıdas.de")] // ...an "adidas" website written with a dotless i.        
        // Long URL with lots of query params
        [InlineData("https://www.google.com/search?q=fuzzy+URL+validation&client=firefox-b-d&sxsrf=ALeKk00vuqPHMzCg6dIW40i9ZVePIjly9A%3A1616107199897&ei=v9ZTYMmjNu3HrgS9rqOQDw&oq=fuzzy+URL+validation&gs_lcp=Cgdnd3Mtd2l6EAMyBwghEAoQoAEyBwghEAoQoAE6BwgjELADECc6BQghEKABUN4pWL0wYI0xaAJwAHgAgAF4iAH3BpIBAzkuMZgBAKABAaoBB2d3cy13aXrIAQHAAQE&sclient=gws-wiz&ved=0ahUKEwiJ_8W89LrvAhXto4sKHT3XCPIQ4dUDCAw&uact=5")]
        [InlineData("http://ddos-link.com/[test.......................................]")]
        // Stupidly-long URL
        [InlineData("https://longurlmaker.com/go?id=EXLVPCEEVMCJMZAHPCWLLUKQSDBNNSRSQDOGFTLAFNJRMJZCMVNYQXOIAQWOJOHZRVFXOQYTEHLRRKTEFGDVDSEEIBTRPARGVWDJNBDSJOJBYDGEJKANCFSKGLCVMTCHQFREHUWRIFTHIAMSKNGMSYXZKZUWTWEKCVOKPKCYPVGYWTRGHXTZWRYGRTCKPHUSCICSSFRIAYHXRYEYVXYROXYGQBQGILYKUBDMGUVHSCEKAJIWGHOVENBUOSBXDAHXBLKIFGQYIVIJERFLYUGCHIXVBZSFZACBMNJRTZPBUYLYPQKNLQQSZOOANHFMIINUWXLPWBRRACQPTADFRUXRIMSVTYIBHWWDTISQKKIKNZAHZDMRYJJGWBCPZKRHHNKAVDRFOZPBYQIYTUQREZYABAQBLHISVSTRWLWKKMLJCUIWOYOTYMUZNKAZSISOBMFXXCHIOTRRNANCYCVWPQWCBWCUFUIGHODEQLMFEEWSEKUDMQABGWEITHCYBLEIZINQTZVYERQJCLNPBNRIKVQDCTCEEGNEPAUIAPHFHXOJIBZKPQSAMJDOCGTHMGLCHGKIVRNDPCNYDMITCBTOKXDNDWVQDGSLMDTIVHLKJYJNLRGNSZBDWFRACMQFTIRFLKOHSARBZAOKSUXLUZOQEFQGRHMKPEQBOFIHFAIEBWOURMFLLUBQASXRZKCPOTRDZAKEBSJDPXXGKCRVXTMXMKPLUSBJLSNLSXPUQZXFXUMHXDDUXPRJAGVVYWEKORVHPLBFKYLHTXPQBSLZICIIJDDIZISOZOWOAYULXFSQDIHFASEIVMQEQUMQCFFCTBRTZLEXRXNVGVAGOFOJSFOIABYCGEFAFWUFHZCZXQUDMVJAAYJSEYBFZAIFENGJJAPRASQXILBVHKVIJFAODBTCCBIGOOPYPFPPXGNGSOBZZXHJCIXFKRHMTPSIEIGTOWJLDWSVKUVJRDSTMQYWHFJBVCJZKPTZMCKPYDKIXGSOJLIVBGSJMCKHEUHPKAOWDZLBCWKZCDOMOWIQDAAPDLXHFFWPGKMJJXLMTVMWZAIOXQEARFQURNCWSZMYJYITWTGPPSMOTUCQLHRDMHYDUAANZNMJLUDHNJXCCKQVETLQADTNKSQFTRSTEECPYGXMQVPSFFNKZZAZCUMNNDSGULZYPCOOILAZHPRMVFXUFYUITGVGJOMDZMCOOQDXCEWJUOWPUTKZRFIKLDRVSDZRQBGERJCTOZDIOZYISJHKOPVATMVMQVDGWKLOUOIINLQBLPJEYROMHKNBWINNDRTABFPVTXEHJUIVKSZIKOVSYITVRHIVYCVAILVBJAITVROFJOOUCKMBTGXKNGGMICMRNISWIBPDTEJDSXXVHJXAPVLDBSPKUCBHKUETVUXOZGRRDPNLYLMOGYSHQHRKKTSUNXOJRLXQRPIVEWGDHTSNRKVHRNSBGKWTILDZXBQOQZKVVRYCKRPCBLJTYCVENSYVDBVACLPTKZPFROIBFYEGJNZHQUMDMKYQMTQPFFIQFYWWMEYRDCYMQXUGSGJFQIVDCLSHRBXWZUTMYRDBTCOKZIQSPAXGISJLDCSVDQRDKKPJCTLWVZGVIWNXXKDGCNRGPJBSATWLELUGEGCAGIVOFJMCTQIWDZDSJFLKYHVCSQIXWLYCTTRYCEBWUKTXKWQUBBEAIACBQLYNWWQPQIOTMQQGAJELFUFHWKHEKKFBVEORBFHRNWLZNBGOKLZQGFYCPGGAQMCMQQESWLKJIVIVXPJHAIYGOXIDDPEUCGGTXKLTWVCERLZOAJWBRVIFSRJWGNQJUWCRHOKDKNIBYIPZRMBJHJPZAYVHMMQGJTYQHIURVCSULITCUVLBEBBEAXLMJBTSURJCAETWHMQSVKVPFGRJISOIQZUZBOSWGCYHGSDOEUGZECPKJGURZIZPIUPJIIGPLEWSXAGCUNCZPRJYYDPYMOOUIDDRMKHXOEPMEVOZJQYYHHWGLMEXBSSKWWBIGJFVNUHSQVRZLQYTYVZJHDHIWZSSWUACEGXSBEKRZCRKSPEQKDASG")]
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
        [InlineData("google.com", "http://google.com/")]
        [InlineData("</span>google.com", "http://google.com/")]
        [InlineData("http://span>google.com", "http://google.com/")]
        [InlineData("<span>✉</span>email@gmail.com", "http://email@gmail.com/")]
        public void UrlScanner_ShouldCorrectlyCanonicalizeUrls(string input, string expected)
        {
            Uri.TryCreate(expected, UriKind.Absolute, out Uri? parsedUri);
            var uris = _urlScanner.GetUrls(input);
            Assert.Equal(parsedUri?.ToString(), uris[0]);
        }

        [Theory]
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
        [InlineData("www.")]
        [InlineData("google")]
        [InlineData("http://")]
        [InlineData("http://w")]
        [InlineData("http://www")]
        [InlineData("http://google")]
        [InlineData("http://www.")]
        [InlineData("http://subd.")]
        //[InlineData("www.⚖🦈‍‍‍‍🚀🎉🤞😘👨‍👨‍👧‍👧.com")] // <-- The LinkedIn scanner is more forgiving, and allows technically invalid URLs. It'll let this one through.
        public void UrlScanner_ShouldNotReturnFalsePositives(string input)
        {
            var uris = _urlScanner.GetUrls(input);
            Assert.Empty(uris);
        }

        [Theory]
        [MemberData(nameof(BodyTexts))]
        public void UrlScanner_ShouldExtractAllLinksFromLongerText(string bodyText, string[] expectedUris)
        {
            var actualUris = _urlScanner.GetUrls(bodyText);
            Assert.Equal(expectedUris, actualUris);
        }

        [Fact]
        public void UrlScanner_ShouldHandleSpecialCases()
        {
            string inputUrl = "http://use rid:password@example.com/";
            string expectedUrl = "http://rid:password@example.com/";

            var parsedUrls = _urlScanner.GetUrls(inputUrl);

            Assert.Equal(parsedUrls[0], expectedUrl);
        }

        public static IEnumerable<object[]> BodyTexts =>
            new List<object[]>
            {
                new object[] { @"Visit photo hosting sites such as www.flickr.com, 500px.com, www.freeimagehosting.net and 
https://postimage.io, and upload these two image files, picture.dog.png and picture.cat.jpeg, 
there. After that share their links at https://www.facebook.com/ and http://🍕.ws",
                    new string[] {
                        new Uri("http://www.flickr.com").ToString(),
                        new Uri("http://500px.com").ToString(),
                        new Uri("http://www.freeimagehosting.net").ToString(),
                        new Uri("https://postimage.io").ToString(),
                        new Uri("https://www.facebook.com").ToString(),
                        new Uri("http://🍕.ws").ToString(),
                    }},

                new object[] { @"Now let's see here, let's take a link with a typo in the middle: https://www .fimfiction.net/story/34928
throw in an ftp link like ftp://freenode.net/90sK00lWarez and one a Java bundle ID for maximum sillness com.facebook.messenger.
Here's an example of link splitting resulting in some interesting behavior ftps://www. someancientftpserver.com/allTheMp3s.",
                    new string[] {
                        "http://fimfiction.net/story/34928", // The LinkedIn scanner accepts the leading dot, but loses the https and www
                        "ftp://freenode.net/90sK00lWarez",
                        "http://someancientftpserver.com/allTheMp3s.", // The ftps:// gets chopped off, but we optimistically glue an http onto the front                        
                    }},
            };
    }
}
