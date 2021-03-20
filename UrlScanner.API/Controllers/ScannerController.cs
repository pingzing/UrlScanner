using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UrlScanner.API.Services;

namespace UrlScanner.API.Controllers
{
    [ApiController]
    public class ScannerController : ControllerBase
    {
        private readonly ILogger<ScannerController> _logger;
        private readonly IEnumerable<IUrlScannerService> _scannerServices;

        public ScannerController(IEnumerable<IUrlScannerService> scannerServices,
            ILogger<ScannerController> logger)
        {
            _scannerServices = scannerServices;
            _logger = logger;
        }

        /// <summary>
        /// Takes in a body of text, scans it for potential URLs, and returns a list of any that it finds.
        /// Does not accept null or empty values. Limit of 100 000 UTF-16 characters.
        /// </summary>
        /// <param name="input">The body text to scan.</param>
        /// <param name="scanBehavior">The underlying scanner to use. UseRegexDetector uses the hand-rolled
        /// regex-based detector. UseLinkedInLibraryDetector uses LinkedIn's URL detection library.
        /// Defaults to UseLinkedInLibraryDetector.</param>
        /// <returns>An array of fully canonicalized URLs discovered.</returns>
        [HttpPost]
        [Route("scan-for-urls")]
        [Consumes("text/plain")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Uri[]))]
        public ActionResult<string[]> ScanForUrls([FromBody] string input, [FromQuery] ScanBehavior scanBehavior = ScanBehavior.UseLinkedInLibraryDetector)
        {
            // No null-checking necessary--the framework takes care of that for us.
            if (input.Length > 100_000)
            {
                return BadRequest("Input body must be 100 000 (UTF-16) characters or less.");
            }

            IUrlScannerService? scannerService = scanBehavior switch
            {
                ScanBehavior.UseRegexDetector => _scannerServices.FirstOrDefault(x => x is RegexUrlScanner),
                ScanBehavior.UseLinkedInLibraryDetector => _scannerServices.FirstOrDefault(x => x is LinkedInUrlScanner),
                _ => null
            };
            if (scannerService == null)
            {
                return BadRequest("The given value for the scanBehavior parameter was invalid.");
            }

            return scannerService.GetUrls(input);
        }
    }

    public enum ScanBehavior
    {
        UseRegexDetector,
        UseLinkedInLibraryDetector,
    }
}
