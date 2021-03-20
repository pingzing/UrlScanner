# URL Scanner

## Status
|Build|Deployment|
|-----|-------|
|[![Build Status](https://dev.azure.com/mcalistern/UrlScanner/_apis/build/status/pingzing.UrlScanner?branchName=main)](https://dev.azure.com/mcalistern/UrlScanner/_build/latest?definitionId=24&branchName=main)|[![Badge Image](https://vsrm.dev.azure.com/mcalistern/_apis/public/Release/badge/cb1df9d6-368d-49c6-97ac-a26f44ed77ce/1/1)](https://dev.azure.com/mcalistern/UrlScanner/_release?_a=releases&view=mine&definitionId=1)|

A little demo project for identifying URLs in a body of text.

Live demo: https://urlscannerapi.azurewebsites.net/swagger/index.html

## Building

The project requires .NET 5. Building it should just require a 
```bash
dotnet build
```
from the repo root.

## Running the project


```bash
dotnet run --project .\UrlScanner.API\UrlScanner.API.csproj
```

Once the project is up and running, it can be communicated with directly, or you can use the Swagger page at `<base url>/swagger/index.html`. It has one endpoint, which is described below.

Example CLI invocations:
```bash
# cURL
curl -X POST "https://localhost:5001/scan-for-urls?scanBehavior=UseLinkedInLibraryDetector" -H  "accept: text/plain" -H  "Content-Type: text/plain" -d "Sample text with a url www.google.com"
```
```PowerShell
# PowerShell
Invoke-RestMethod -Method Post https://localhost:5001/scan-for-urls?scanBehavior=UseLinkedInLibraryDetector -ContentType "text/plain" -Body "Sample text with a url www.google.com"
```

## Endpoints

### scan-for-urls

Returns a JSON array of strings containing all the discovered URLs.

|Parameter Type|Name|Info|
|--------------|------------|-------------|
|Query|scanBehavior|An enum that accepts one of two values: `UseRegexDetector`, or `UseLinkedInLibraryDetector`. The underlying scanner to use. UseRegexDetector uses the hand-rolled regex-based detector. UseLinkedInLibraryDetector uses LinkedIn's URL detection library. Defaults to UseLinkedInLibraryDetector. |
|Content body|input|A `text/plain` body containing text with URLs in it.|