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

        [HttpPost]
        [Route("scan-for-urls")]    
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Uri[]))]
        public ActionResult<Uri[]> ScanForUrls(string input)
        {
            if (input == null)
            {
                return BadRequest("Input cannot be null.");
            }
            return _scannerService.GetUrls(input);
        }
    }
}
