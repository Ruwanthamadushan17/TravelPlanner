using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace TravelPlanner.Api.Configuration;

public sealed class ConfigureSwaggerUIOptions : IConfigureOptions<SwaggerUIOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    public ConfigureSwaggerUIOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerUIOptions options)
    {
        foreach (var d in _provider.ApiVersionDescriptions.Reverse())
            options.SwaggerEndpoint($"/swagger/{d.GroupName}/swagger.json",
                $"TravelPlanner API {d.ApiVersion}");

        options.DocumentTitle = "TravelPlanner API";
        options.DefaultModelsExpandDepth(-1);
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
    }
}
