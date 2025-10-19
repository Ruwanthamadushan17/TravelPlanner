using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TravelPlanner.Api.Configuration;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider provider)
        => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title = "TravelPlanner API",
                Version = desc.ApiVersion.ToString()
            });
        }
    }
}
