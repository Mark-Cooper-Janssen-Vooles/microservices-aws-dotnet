using System.Text.Json;
using Amazon.DynamoDBv2;
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
        var dbClient = new AmazonDynamoDBClient();
        var table = Table.LoadTable(dbClient, "hotel-created-event-ids");
        
        // put it in elastic search, we need the endpoint from elastic.co
        var host = Environment.GetEnvironmentVariable("host");
        var userName = Environment.GetEnvironmentVariable("userName");
        var password = Environment.GetEnvironmentVariable("password");
        // index name in elastic search is like a table name in dynamoDB. one record in elastic search is a 'document'
        var indexName = Environment.GetEnvironmentVariable("indexName");

        // using the NEST sdk which can connect with elastic search:
        var connectionSettings = new ConnectionSettings(new Uri(host));
        connectionSettings.BasicAuthentication(userName, password);
        connectionSettings.DefaultIndex(indexName);
        // when we store a model in elastic search, it generates a random id. but we want our id to be used:
        connectionSettings.DefaultMappingFor<Hotel>(m => m.IdProperty(p => p.Id));

        var elasticSearchClient = new Nest.ElasticClient(connectionSettings);
        var eventIsNotPresent = !(await elasticSearchClient.Indices.ExistsAsync(indexName)).Exists;
        if (eventIsNotPresent)
        {
            await elasticSearchClient.Indices.CreateAsync(indexName);
        }
        
        foreach (var eventRecord in snsEvent.Records)
        {
            var eventId = eventRecord.Sns.MessageId;
            var foundItem = await table.GetItemAsync(eventId);
            if (foundItem == null)
            {
                await table.PutItemAsync(new Document()
                {
                    ["eventId"] = eventId
                });
            }
            
            // serialize the message we got from SNS to a hotel object
            var hotel = JsonSerializer.Deserialize<Hotel>(eventRecord.Sns.Message);
            await elasticSearchClient.IndexDocumentAsync<Hotel>(hotel);
        }
    }
}