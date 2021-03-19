using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.IO;
using System.Threading.Tasks;

namespace UrlScanner.API.Formatter
{
    public class TextPlainInputFormatter : InputFormatter
    {
        private const string TextPlainContentType = "text/plain";

        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(TextPlainContentType);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            HttpRequest? request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                string? content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        public override bool CanRead(InputFormatterContext context)
            => context.HttpContext.Request.ContentType.StartsWith(TextPlainContentType);
    }
}
