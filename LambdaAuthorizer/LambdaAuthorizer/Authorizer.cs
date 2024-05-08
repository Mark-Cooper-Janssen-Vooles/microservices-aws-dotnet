using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.IdentityModel.Tokens;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaAuthorizer;

public class Authorizer
{
    public async Task<APIGatewayCustomAuthorizerResponse> Auth(APIGatewayCustomAuthorizerRequest request)
    {
        var idToken = request.QueryStringParameters["token"];
        var idTokenDetails = new JwtSecurityToken(jwtEncodedString: idToken);
        
        Console.WriteLine(idTokenDetails);

        var kid = idTokenDetails.Header["kid"].ToString();
        var issuer = idTokenDetails.Claims.First(x => x.Type == "iss").Value;
        var audience = idTokenDetails.Claims.First(x => x.Type == "aud").Value;
        var userId = idTokenDetails.Claims.First(x => x.Type == "sub").Value;
        
        var response = new APIGatewayCustomAuthorizerResponse()
        {
            PrincipalID = userId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement()
                    {
                        Action = new HashSet<string>(){"execute-api:Invoke"},
                        Effect = "Allow",
                        Resource = new HashSet<string>(){request.MethodArn}
                    }
                }
            }
        };
        
        // get the key stored in secrets manager:
        var secretManagerClient = new AmazonSecretsManagerClient();
        var secret = await secretManagerClient.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = "hotelCognitoKey"
        });
        
        Console.WriteLine(secret);

        var privateKeys = secret.SecretString;
        var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(privateKeys, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        var privateKey = jwks.Keys.First(x => x.Kid == kid);
        
        Console.WriteLine(privateKey);

        var handler = new JwtSecurityTokenHandler();
        var result = await handler.ValidateTokenAsync(idToken, new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = privateKey
        });
        
        Console.WriteLine(result);

        if (!result.IsValid) throw new UnauthorizedAccessException("Token not valid");
        
        // authorise the user for APIs that require specific cognito groups
        var apiGroupMapping = new Dictionary<string, string>()
        {
            { "litadminhotel+", "Admin" },
            { "admin+", "Admin" }
        };

        var expectedGroup = apiGroupMapping.FirstOrDefault(x => request.Path.Contains(x.Key, StringComparison.InvariantCultureIgnoreCase));
        if (!expectedGroup.Equals(default(KeyValuePair<string, string>)))
        {
            var userGroup = idTokenDetails.Claims.First(x => x.Type == "cognito:groups").Value;
            if (!userGroup.Equals(expectedGroup.Value, StringComparison.CurrentCultureIgnoreCase))
            {
                // user is not authorised
                response.PolicyDocument.Statement[0].Effect = "Deny";
                Console.Write("denying");
            }
        }

        return response;
    }
}