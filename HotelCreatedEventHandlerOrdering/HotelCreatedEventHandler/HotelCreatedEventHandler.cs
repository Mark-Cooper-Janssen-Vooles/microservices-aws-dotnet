using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using HotelCreatedEventHandler.models;
using Nest;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace HotelCreatedEventHandler;

public class HotelCreatedEventHandler
{
    public async Task Handler(SNSEvent snsEvent)
    {
        // todo: implement idempotency 
        var dbClient = new AmazonDynamoDBClient();
        using var dbContext = new DynamoDBContext(dbClient);

        foreach (var eventRecord in snsEvent.Records)
        {
            var hotel = JsonSerializer.Deserialize<Hotel>(eventRecord.Sns.Message);
            await dbContext.SaveAsync(hotel);
        }
    }
}