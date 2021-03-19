using System;

namespace UrlScanner.API.Services
{
    public interface IUrlScannerService
    {
        /// <summary>
        /// Scans the text of <paramref name="body"/> for web links and returns an array of fully qualified URIs of any that are found.
        /// </summary>
        /// <param name="body">The body text to scan.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="body"/> is null.</exception>
        /// <returns>An array of any links found in the body text.</returns>
        Uri[] GetUrls(string body);
    }
}