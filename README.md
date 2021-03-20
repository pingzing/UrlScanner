# URL Scanner

A little demo project for identifying URLs in a body of text.

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

Once the project is up and running, it can be communicated with directly, or you can use the Swagger page at `<base url>/swagger/index.html`. It has one endpoint:

### scan-for-urls
|Parameter Type|Name|Info|
|--------------|------------|-------------|
|-----Query----|scanBehavior|An enum that accepts one of two values: `UseRegexDetector`, or `UseLinkedInLibraryDetector`. The underlying scanner to use. UseRegexDetector uses the hand-rolled regex-based detector. UseLinkedInLibraryDetector uses LinkedIn's URL detection library. Defaults to UseLinkedInLibraryDetector. |
|Content body--|input-------|A `text/plain` body containing text with URLs in it.|
Returns an array of strings containing all the discovered URLs.