using Amazon.DynamoDBv2.DataModel;

namespace HotelCreatedEventHandler.models;

[DynamoDBTable("Hotels_Order_Domain")]
public class Hotel
{
    [DynamoDBHashKey("userId")] public string? UserId { get; set; }
    [DynamoDBHashKey("Id")] public string? Id { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public int? Rating { get; set; }
    public string? CityName { get; set; }
    public string? FileName { get; set; }
    public DateTime CreationDateTime { get; set; }
}