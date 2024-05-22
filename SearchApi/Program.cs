using Nest;
using SearchApi.Models;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/search", async (string? city, int? rating) =>
{

    var connectionSetting = new ConnectionSettings(new Uri(host));
    connectionSetting.BasicAuthentication(username, password);
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
});

app.Run();
