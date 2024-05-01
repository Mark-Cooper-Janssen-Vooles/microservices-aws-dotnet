using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using HotelManager_HotelAdmin.Models;
using HttpMultipartParser;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HotelManager_HotelAdmin;

public class HotelAdmin
{
    public async Task<APIGatewayProxyResponse> ListHotels(APIGatewayProxyRequest request)
    {
        // query string param called token is passed to this lambda method.
        var response = new APIGatewayProxyResponse()
        {
            Headers = new Dictionary<string, string>(),
            StatusCode = 200
        };
        
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Headers", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS,GET");
        response.Headers.Add("Content-Type", "application/json");

        var token = request.QueryStringParameters["token"]; //jwt
        var tokenDetails = new JwtSecurityToken(jwtEncodedString: token);
        var userId = tokenDetails.Claims.FirstOrDefault(x => x.Type == "sub")?.Value; // OAuth thing, always carrys the unique id of the user
        
        var region = Environment.GetEnvironmentVariable("AWS_REGION"); // pre-defined env variable available to all lambdas
        var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));
        
        try
        {
            using var dbContext = new DynamoDBContext(dbClient);
            var hotels = await dbContext.ScanAsync<Hotel>(new[] { new ScanCondition("UserId", ScanOperator.Equal, userId) })
                .GetRemainingAsync();
            response.Body = JsonSerializer.Serialize(hotels);
        }
        catch (Exception e)
        {
            response.StatusCode = 400;
            Console.WriteLine(e);
        }

        return response;
    }
    
    // so api gateway can understand the response of the lambda
    // ILambdaContext gives us info on the execution context of the lambda
    public async Task<APIGatewayProxyResponse> AddHotel(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var response = new APIGatewayProxyResponse()
        {
            Headers = new Dictionary<string, string>(),
            StatusCode = 200
        };
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Headers", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS,POST");
        response.Headers.Add("Content-Type", "application/json");

        var bodyContent = request.IsBase64Encoded
            ? Convert.FromBase64String(request.Body)
            : Encoding.UTF8.GetBytes(request.Body);

        await using var memStream = new MemoryStream(bodyContent);
        var formData = await MultipartFormDataParser.ParseAsync(memStream).ConfigureAwait(false);

        // strings are from the html frontend file names
        var hotelName = formData.GetParameterValue("hotelName");
        var hotelRating = formData.GetParameterValue("hotelRating");
        var hotelCity = formData.GetParameterValue("hotelCity");
        var hotelPrice = formData.GetParameterValue("hotelPrice");

        var file = formData.Files.FirstOrDefault();
        var fileName = file.FileName;

        await using var fileContentStream = new MemoryStream();
        await file.Data.CopyToAsync(fileContentStream);

        var fileSize = fileContentStream.Length;
        Console.WriteLine($"File size: {fileSize} bytes");
        
        string contentType = file.ContentType;
        Console.WriteLine($"Content-Type: {contentType}");

        var userId = formData.GetParameterValue("userId");
        var idToken = formData.GetParameterValue("idToken");
        
       // we pass the json web token in both the headers and 
       var token = new JwtSecurityToken(jwtEncodedString: idToken);
       var group = token.Claims.First(x => x.Type == "cognito:groups");

       if (group == null || group.Value != "Admin")
       {
           response.StatusCode = 401;
           response.Body = JsonSerializer.Serialize(new { Error = "Unauthorised. Must be a member of admin group" });
       }

       var region = Environment.GetEnvironmentVariable("AWS_REGION"); // pre-defined env variable available to all lambdas
       var bucketName = Environment.GetEnvironmentVariable("bucketName");
       
       var client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
       var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));
       
       try
       {
           await client.PutObjectAsync(new PutObjectRequest
           {
               BucketName = bucketName,
               Key = fileName, // name of the file
               InputStream = file.Data
           });

           var hotel = new Hotel
           {
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
                Name = hotelName,
                CityName = hotelCity,
                Price = int.Parse(hotelPrice),
                Rating = int.Parse(hotelRating),
                FileName = fileName
           };

           using var dbContext = new DynamoDBContext(dbClient);
           await dbContext.SaveAsync(hotel);
       }
       catch (Exception e)
       {
           Console.WriteLine(e);
       }

       Console.WriteLine("OK.");
        
       return response;
    }
}