using api.Middlewares;
using Serilog;

namespace api
{
    /// <summary>
    /// Provides extension methods for configuring the HTTP request pipeline.
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Configures the complete middleware pipeline.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
        /// <returns>The <see cref="WebApplication"/> for method chaining.</returns>
        public static WebApplication UseWebPipeline(this WebApplication app)
        {
            app.UseExceptionHandler();

            app.MapHealthChecks("/health");

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<LogEnrichmentMiddleware>();

            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            return app;
        }
    }
}
