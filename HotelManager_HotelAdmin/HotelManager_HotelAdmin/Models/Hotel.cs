using Amazon.DynamoDBv2.DataModel;

namespace HotelManager_HotelAdmin.Models;

// represents hotels table in dynamoDB table
[DynamoDBTable("Hotels")]
public class Hotel
{
    [DynamoDBHashKey("userId")] // maps it to the lower case version in dynamo => partition key
    public string UserId { get; set; }
    [DynamoDBRangeKey("Id")] // sort key
    public string Id { get; set; }
    
    public string Name { get; set; }
    public int Price { get; set; }
    public int Rating { get; set; }
    public string CityName { get; set; }
    public string FileName { get; set; }
}