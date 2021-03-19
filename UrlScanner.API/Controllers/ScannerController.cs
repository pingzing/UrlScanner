using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using UrlScanner.API.Services;

namespace UrlScanner.API.Controllers
{
    [ApiController]
    public class ScannerController : ControllerBase
    {
        private readonly ILogger<ScannerController> _logger;
        private readonly IUrlScannerService _scannerService;

        public ScannerController(IUrlScannerService scannerService,
            ILogger<ScannerController> logger)
        {
            _scannerService = scannerService;
            _logger = logger;
        }

        /// <summary>
        /// Takes in a body of text, scans it for potential URLs, and returns a list of any that it finds.
        /// Does not accept null or empty values.
        /// </summary>
        /// <param name="input">The body text to scan.</param>
        /// <returns>An array of fully canonicalized URLs discovered.</returns>
        [HttpPost]
        [Route("scan-for-urls")]
        [Consumes("text/plain")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Uri[]))]
        public ActionResult<Uri[]> ScanForUrls([FromBody] string input)
        {
            // No null-checking necessary--the framework takes care of that for us.
            return _scannerService.GetUrls(input);
        }
    }
}
