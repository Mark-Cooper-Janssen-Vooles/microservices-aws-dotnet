using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Nest;
using Polly;
using Polly.CircuitBreaker;
using SearchApi.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var circuitBreakerPolicy = Polly.Policy<List<Hotel>>
    .Handle<Exception>()
    .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, TimeSpan.FromSeconds(30)); // when circuit is open, it is so for 30 seconds

app.MapGet("/search", async (string? city, int? rating) =>
{
    var result = new HttpResponseMessage();

    try
    {
        var hotels = circuitBreakerPolicy.ExecuteAsync(async () => { return await SearchHotels(city, rating); });

        result.StatusCode = HttpStatusCode.OK;
        result.Content = new StringContent(JsonSerializer.Serialize(hotels));
        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return result;
    }
    catch (BrokenCircuitException e)
    {
        result.StatusCode = HttpStatusCode.NotAcceptable; // frontend needs to know this to show a specific message
        result.ReasonPhrase = "Circuit is OPEN";
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    return result;
});

async Task<List<Hotel>> SearchHotels(string? city, int? rating)
{
    var host = Environment.GetEnvironmentVariable("host ");
    var userName = Environment.GetEnvironmentVariable("userName");
    var password = Environment.GetEnvironmentVariable("password");
    var indexName = Environment.GetEnvironmentVariable("indexName");

    var connectionSetting = new ConnectionSettings(new Uri(host));
    connectionSetting.BasicAuthentication(userName, password);
    connectionSetting.DefaultIndex(indexName);
    connectionSetting.DefaultMappingFor<Hotel>(m => m.IdProperty(p => p.Id));
    var client = new ElasticClient(connectionSetting);

    if (rating is null)
    {
        rating = 1;
    }

    // exact match: cityName = "Sydney"
    // prefix: "par" will match "paris" => we will use this
    // range 
    // fuzzy match (i.e. we search pariss)
    ISearchResponse<Hotel> result;
    if (city is null)
    {
        // return all hotels
        result = await client.SearchAsync<Hotel>(s => s.Query(q =>
            q.MatchAll()
            && q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
        ));
    }
    else
    {
        result = await client.SearchAsync<Hotel>(s => 
            s.Query(q =>
                q.Prefix(p => p.Field(f => f.CityName).Value(city).CaseInsensitive())
                && q.Range(r => r.Field(f => f.Rating).GreaterThanOrEquals(rating))
            )
        );
    }

    return result.Hits.Select(x => x.Source).ToList();
}

app.Run();
