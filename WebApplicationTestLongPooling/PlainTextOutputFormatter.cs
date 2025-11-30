using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace WebApplicationTestLongPooling
{
    public class PlainTextOutputFormatter : TextOutputFormatter
    {
        public PlainTextOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/message"));
            //SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/vcard"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.Default);
        }

        protected override bool CanWriteType(Type? type)
            => typeof(IAsyncEnumerable<string>).IsAssignableFrom(type);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            var asyncEnumerable = context.Object as IAsyncEnumerable<string>;
            if (asyncEnumerable != null)
            {
                await using var writer = context.WriterFactory(response.Body, selectedEncoding);
                await foreach (var value in asyncEnumerable)
                {
                    if (value != null)
                    {
                        await writer.WriteAsync(value);
                        await writer.FlushAsync();
                    }
                }
            }
        }
    }
}
