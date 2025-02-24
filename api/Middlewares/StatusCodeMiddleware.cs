using System.Text.Json;

public class StatusCodeMiddleware
{
    private readonly RequestDelegate _next;

    public StatusCodeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var memoryStream = new MemoryStream();
        var originalBodyStream = context.Response.Body;

        try
        {
            context.Response.Body = memoryStream;
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var bodyContent = await new StreamReader(memoryStream).ReadToEndAsync();

            if (!string.IsNullOrEmpty(bodyContent))
            {


                using var doc = JsonDocument.Parse(bodyContent);

                if (doc.RootElement.TryGetProperty("code", out var codeElement))
                {
                    string code = codeElement.GetString()!;

                    context.Response.StatusCode = code switch
                    {
                        "NOT_FOUND" => StatusCodes.Status404NotFound,
                        _ => StatusCodes.Status200OK
                    };
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            memoryStream.Dispose();
        }
    }
}
