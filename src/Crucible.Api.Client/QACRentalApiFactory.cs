using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;

namespace Crucible.Api.Client;

public static class QACRentalApiFactory
{
    public static IQACRentalApi Create(string baseUrl, string apiToken)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
        };

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(apiToken);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions),
        };

       return RestService.For<IQACRentalApi>(httpClient , refitSettings); 
    }
}