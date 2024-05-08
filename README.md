# Building Microservices-based Systems in AWS with .NET and C# 

- 92% of enterprises uses public cloud (aws, azure etc)
- AWS has the largest market share (9%)
- 75% of companies were using or planning to use event-driven architecture 

The course has:
- understand concepts and design pattersn for secure, scalable, available and resilient systems
- design system architecture based in microservices in AWS 
- code and event-driven microservices with C#

Contents:
- [Introduction to Microservices](#introduction-to-microservices)
- [Hotel booking system](#hotel-booking-system)
- [Identity and Access Management (IAM) AWS Cognito](#identity-and-access-management-iam-aws-cognito)
- [API Gateway](#api-gateway)
  - pattern
  - creating a mock API with AWS Gateway
  - Authenticating API Requests
- [Building Serverless Microservices](#building-serverless-microservices)


### Local Dev
- open IIS, right-click sites and add new website 
  - specify the path as this repo
  - bind it as http - localhost - 6060 
- open up localhost:6060
  - create new user 


## Introduction to Microservices

### Microservice vs Monolithc applications

monolithic => services orientated (maintain session / state, SOAP APIs) => microservices (stateless => but use events / messages or restful APIs)

Microservices:
- UI (client side, written in react e.g.)
- Individual services with their own database 
  - if need to use each others API, use restful API. prefer events / messages over API usage

### Anatomy of an event-driven microservices-based application

- system has a user. accesses our app via web or mobile app built using JS framework (react, angular)
- backend is then made of microservices:
  - i.e. product microservice, cart microservice, payment microservices. exposed by restful APIs
  - we have an abstraction infront of the microservices (API Gateway)
  - firewall between UI as well. i.e. UI => firewall => API gateway => various microservices
- to minimise API usage between microservices, we use an event bus. 
  - event bus is like a queue
  - microservices that have something to say push to the event bus, to upload data. 
  - microservices that need to read this data and pull from the event bus.
  - the microservices don't talk to each other directly 
- if a user needs to login, we also need an identity provider (IDP) / authentication
  - microservices also need to validate against this IDP. authentication can be delegated to the API gateway

---

## Hotel Booking System

- 3 types of users:
  - system admins - can do anything
  - customers - can sign up, sign in, perform search
  - hotel managers - see lists of bookings and accept or reject bookings
- architecture:
  - identity provider (Defines users / auth details)
  - hotels microservice (CRUD actions on hotels) + DB
  - search microservice 
  - payment microservice (we won't integrate with any payment service)
  - booking microservice + DB
  - admin microservice 

---

## Identity and Access Management (IAM) AWS Cognito

### Intro to AWS Cognito
- user identity and access management 
- provides user identity, user authentication via email or Federated Access (fb or google)
- users can have attributes (ie names and address), groups and other attributes
- users can be created manually by an admin or via a sign-up page 
- AWS cognito provides built-in signup and login page for web apps 
- AWS cognito based on Open Authentication (OAuth)

### Setting up AWS Cofnito for Hotel Booking System
- go to cognito in AWS => "create user pool"
  - various options here
  - using cognito domain and the cognito generated UI: https://hotel-booking-serivce.auth.ap-southeast-2.amazoncognito.com
  - for 'allowed callback urls' make sure we don't run our website as a file. i.e. file://user/local etc => need to setup a web server, i.e. IIS / apache tomcat 
    - this is the URL to redirect the user back to after authentication. i.e. https://localhost:8080/hotel
  - in the 'OpenId Connect Scopes' (in 'advanced app client settings') we need OpenId and Email
  - add sign-out URL (the same i.e. https://localhost:8080/hotel)
  - once created we need 'user pool id' (at the top) and the client id (in 'App integration' section)
  - we also need a user to administer other uers
    - in users tab, create user 
    - we've got 3 users: admins, customers, and managers. any user not part of a group we assume are customers
  - do make our user an admin, we to go groups and create group:
    - create 'Admin' group as well as 'HotelManager' group

### Sign in with AWS Cognito 
- to integrate with a front end app, in aws in the cognito user pool click 'getting started' then 'integrate your user pool with an app': https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-integrate-apps.html
- Amplify works with react: https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-integrate-apps.html#cognito-integrate-apps-amplify
- gives you boilerplate JS 
- instead we'll use very basic JS that the course guy provides => add my details to the cognito.js file 
  - back in the amazon cognito area in AWS, click into the 'App integration' tab then the 'app client name' then edit hosted UI. change the OAuth 2.0 Grant types to 'Implicit grant' as well.
- then he goes to the html page (in my case its doing the file thing just opening index.html) - so need to use IIS for windows. 
  - just used chatgpt to figure that out, its now on http://localhost:6060/hotel => make sure this is the same in the Cognito UI (i had https://localhost:8080/hotel and it didn't work)


### Creating hotels 
- sign in as an admin user (the one we made earlier)
- 'add hotel' button now visible
- click it and you see a basic form, in addHotel.html. this doesn't have an action yet - we need the API for it first. note the encryption type.  (enctype)

---

## API Gateway 

### API Gateway Pattern and Tools
- stops the consumer of a microservice from directly accessing it 
- simplifies the systems interface by combining multiple APIs to one 
- can perform authentication and authorization (instead of all the microservices needing this)
- simplifies monitoring (instead of all the microservices needing this)
- API catalogue and documentation 

common api gateway tools:
- aws api gateway:
  - part of AWS
  - easy to learn
  - no complex setup (cloud formation or can use console / UI)
  - needs to be created per API 
- google Apigee
  - enterprise level / use (costly)
  - supports various deployment models 
  - hybrid cloud (i.e. aws and on local data centres)
  - complex
- Kong
  - enterprise level (very expensive)
  - high throughput

### Create a Mock API with AWS API Gateway
- will create one and use it for the addHotel.html form 
- go to api gateway in aws console => create REST API => build => name it "newHotelApi" => create
- all the API definitions will be in the 'resources', but we need to deploy the API to stages first  
- "create method" will create an API
  - choose post 
  - it can go to lambda, http, mock, aws service, vpc link... we'll use mock + create 
  - we can click test as well 
- to deploy this, click deploy and make a new stage. make stage name 'test'
  - once deployed, you can see an invoke URL. paste this in the addHotel.html form. 
  - when we go to submit the form, we get a 500 error. its because the mock only accepts application/json but our form is sending 'multipart/form-data'. in resources => post => integration request => mapping templates we can edit this and add a new mapping template for multipart/form-data. then we need to re-deploy
  - now when we submit the form its a 403 CORS error. in the resource details click 'enable cors' and tick POST, confirm with save and redeploy
  - now it responds with 200 OK

### Authenticating API Requests 
- we don't want our API open to the public, only if they have a user and password in our system (authentication) - and authorise them that this API is relevant to them (authentication).
-  in API gateway click on the authorizers tab
  - click create an authorizer and choose cognito as the type, and select the one we made earlier
  - name it and add to the token source "Authorization", and test it. without a token it should be 403, with a token (this is logging from cognito)
- we now need to go to resources/post and click on the 'method request' and add the authorization to it 

---

## Building Serverless Microservices 

### The concept of a microservice Chassis :: Servelress & Containerisation 
- every microservice has a "service template build logic" and "cross cutting concerns" (i.e. security, external config, logging / monitoring, health check, tracing)
- we want to take out all cross-cutting concerns and leave them to an external entity: the chassis
  - a microservice chassis is the foundation for developing microservices and performing cross-cutting tasks 
  - if we look at an example of an "order service":
    - service specific build logic
    - service-specific cross-cutting concerns
    - application logic
    - template build logic 
    - template cross-cutting concerns
  - we can see it can use some DRY template, anything that is common goes to the "Service Chassis" or the "Service Template" 
- the types of Chassis tells us what kinds of service we can create 

Types of microservices (Chassis and templates)  
- Serverless
  - specific to cloud envs
  - AWS provides the chassis 
  - scaling-out based on usage and is done fully controlled by AWS 
  - fully integrated into other AWS services (IAM, CloudWatch, etc)
  - we do not have access to the execution environment 
  - dev languages / frameworks based on AWS offering only
- Containerized 
  - app and its operating system packaged into one image, i.e. a docker image
  - must be deployed to a container orchestration playform, i.e. kubernetes
  - access to execution environment, i.e. via ssh 
  - no limitation for development languages or technologies 
  - can support complex deployment scenarios effectively, i.e. blue/green deployment
- Other Chasis types:
  - spring boot / micronaut / quarkus (java)
  - express.js (node.js)
  - flask (python)
- in AWS:
  - AWS lambda is the microservice framework (including chassis and template) for creating serverless microservices
  - AWS supports kubernetes (as a managed service) and has its own containerisation platform called ECS (elastic container services) for container-based microservices 

### Creating and deploying an AWS Lambda microservice 
- to start with, we need to know the payload: what our hotel form is pushing to API gateway. check this in the network tab on the browser. 
- in AWS Lambda service in the console, we see we can use .net 8.0 
- need to create the lambda: in rider, create new project and choose 'class library' - all lambdas are class libraries 
- using nuget add `amazon.lambda.core` as well as `amazon.lambda.apigatewayevents`, `amazon.lambda.serialization.systemtext.json`
- add the response headers, and the assembly for the lambda serialisers. its in the "HotelManager_HotelAdmn/HotelAdmin.cs" file 
- to package this and send to AWS, use this terminal command: `dotnet tool install -g Amazon.Lambda.Tools`
  - go to where csproj file is, then use `dotnet lambda package HotelManager_HotelAdmin.csproj -o HotelAdmin.zip`
  - creates a file 'HotelAdmin.zip'

- now we want to upload our zip file to AWS 
- go to create the lambda in the AWS UI, add the name and use runtime .net 8
- in the code tab, upload .zip file from earlier
- we then need to change the handler in the runtime settings - this is the endpoint where AWS looks to access the code.
  - for .net it works like this: `<Assembly name>::<namespace name>.<class name>::<method name>`
  - i.e. ours will be "HotelMAnager_HotelAdmin::HotelManager_HotelAdmin.HotelAdmin::AddHotel"
- we can then go to the test tab and press test, and see that its working
- we can also go to the 'monitor' tab to see the cloudwatch logs 

### Capturing the Request body in AWS Lambda as an API Backend
- install using nuget `HttpMultipartParser` 
- to get the information out of our form:

````c#
// below is from HotelAdmin.cs "AddHotel"
var bodyContent = request.IsBase64Encoded
    ? Convert.FromBase64String(request.Body)
    : Encoding.UTF8.GetBytes(request.Body);

using var memStream = new MemoryStream(bodyContent);
var formData = MultipartFormDataParser.Parse(memStream);

// strings are from the html frontend file names
var hotelName = formData.GetParameterValue("hotelName");
var hotelRating = formData.GetParameterValue("hotelRating");
var hotelCity = formData.GetParameterValue("hotelCity");
var hotelPrice = formData.GetParameterValue("hotelPrice");

var file = formData.Files.FirstOrDefault();
var fileName = file.FileName;
// file.Data 

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
````
- we need to deserialize the JWT authentication token
  - need to install using nuget `System.IdentityModel.Tokens.Jwt`

### Storing Data and files in AWS
- i.e. how to store the hotels image in AWS 
- database or file storage
  - relational db or no-sql db
    - aws RDS offers relational databases, i.e. mySQL as a managed service 
    - DynamoDB is no-sql. data is stored as json
    - one RDS instance is OK, however each microservice MUST have its own db
    - no cross-database queries or access
  - file storage
    - S3 is used for storing files 
    - AWS Elastic file system is used for short-term storage
- the flow looks like this:
  - user uploads file to browser, which calls the api gateway, which calls lambda, which then tries to upload to the s3 file storage (for the image) and puts additional information (name etc) in the database 
  - by default lambda does not have access to s3 or db, we need to use IAM to give it permissions and give this role to our lambda. called an "execution role"
- go to IAM: create role, aws service, lambda. Give it access to cloudwatch, s3, dynamoDB
  - named "HotelAdminLambdaExecutionRole"
- go to Lambda
  - find our addHotel lambda, go to configuration tab, then permissions tab, edit and change the lambda execution role to the one we just made

### Create and Configure S3 Buckets
- go to s3, create bucket, give it a name and untick 'block all public access'
- need to add a policy to the bucket, he's done it for us here: https://github.com/aussiearef/MicroservicesWithAWS/blob/main/S3-Policy.json
  - need to include our bucket arn and our IAM arn ^

### Uploading Files and Images to AWS S3
- to upload to s3 we can use the nuget package `awssdk.s3`
- in the lambda:
````c#
await using var fileContentStream = new MemoryStream();
await file.Data.CopyToAsync(fileContentStream);
fileContentStream.Position = 0;

var region = Environment.GetEnvironmentVariable("AWS_REGION"); // pre-defined env variable available to all lambdas
var bucketName = Environment.GetEnvironmentVariable("bucketName");

// using the actual s3 sdk:
var client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
await client.PutObjectAsync(new PutObjectRequest
{
    BucketName = bucketName,
    Key = fileName, // name of the file
    InputStream = fileContentStream,
    AutoCloseStream = true
});
````
- the AWS_REGION env variable exists by default on all lambdas, but we will need to make the "bucketName" variable of our s3 bucket ourselves in the UI. 
  - lambda => configuration => environment variables => add

### Creating and configuring a dynamoDB table
- dynamoDB => create table => name it 
  - it needs a partition key which refers to where data is stored in shards
    - i.e. we can use the admin's userId and store all the hotels created by that user in one location. or we could use the cityName - the user wants to see all the hotels in Paris for e.g. we will use userId
  - sort key is optional but improves perfomance, sort through the items listed under a specific partition key.
  - everything else is fine, create table

### Storing Information in DynamoDB
- firstly our putObjectAsync into s3 could fail, we need to wrap that in a try/catch block 
- need to install the dynamo SDK: `AWSSDK.DynamoDBv2`
- create a model/Hotel.cs model 
- hook it up in the code:
````c#
var dbClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));

try
{
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
````

### Connecting API Gateway to Lambda via a Proxy Resource
- our API in api gateway is just a mock API now - just connected to a mock
- we want to attach our AddHotel lambda to our API 
- when you create an API - if the request is a POST of PUT that has headers/body, the header/body will not be forwarded to the lambda
  - not a problem for GET, but it is for PUT and POST 
  - in order to forward the headers and body, we need to create a 'proxy resource'
- go to the API in the API Gateway, click on the exisitng POST resource, then click actions, then resource. tick to create a proxy resource, add a resource name like 'admin' , and tick CORS
  - resource path needs a specific format called "greedy format" like `{Admin+}`
  - after this is made, you should see /{Admin+} under POST - click on any, then click edit integration
  - select Lambda function, find the lambdas ARN, add the execution role we made earlier, save 
- click method request
  - make the authorisation "NewHotelAuth" (which we used before in incognito)
- in the OPTIONS area - this is only used for pre-flight to see if CORS headers are working. 
  - integration request for OPTIONS can be mock, just make sure the mapping application/json request is returning the cors headers, i.e.
````json
{ 
  statusCode: 200, 
  headers: {
    "Access-Control-Allow-Headers": "*",
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "*"
  }
}
````
- OPTIONS returns the CORS headers in the `integration request`
- ANY in the `integration request` has the lambda proxy (also must return CORS headers - this is in our C# code), and the `method request` uses the incognito auth 
- now we deploy the changes
  - to test them, we must open the /{admin+} to find the invoke URL to use
  - add this URL to the addHotel.html form action
  - sign in, go to add hotel, check network tab. preflight request works, lambda seems to fail. can run this in the proxy POST test area, seems IAM issue
  - needed to add apigateway to the IAM role:
  ````
  "Principal": {
    "Service": [
        "apigateway.amazonaws.com",
        "lambda.amazonaws.com"
    ]
  }
  ````
- had some issues here, had to remake the API Gateway and it worked - might have picked the wrong role or something. This was the solution on stack overflow :/
- once its working, should be able to go to s3 and see the image. and also go to dynamoDB to see the 


### Domains and Boundaries 
- now we can create a hotel, storing the image in s3 and the data in dynamoDB
- we want to now see all the hotels created, i.e. using GET
- do we need a new lambda/microservice for this or can we just add a new method to our existing lambda function? 
  - in OOP we have 'single responsibility principle' - each entity must do only one job. not so simple in microservices... we need to know the domain and the domain boundary.
  - a domain is a specific area of business or application functionality. the limit that separates domains is called the "domain boundary"
  - we have a entity called HOTEL. an Admin adds,edit,delets hotels. A customer searches for hotel / books. A hotel manager views bookings and approves bookings. 
  - the HOTEL entity means different things to different users, in this case the meaning is the domain, i.e.: admin domain, customer domain, booking management domain
    - i.e. we could have one microservice for ADMIN domain that does get/add/edit/delete of hotels
    - one that does CUSTOMER domain searching/booking 
    - one that does BOOKING MANAGEMENT domain viewing bookings, approving bookings
  - to improve scalability / performance, we pay need to break a microservice down further forever. and if a domain is quite large.
- "Each microservice is designed to handle a specific business capability"
- "Domain boundary separates one microservice from another and ensures each microservice is responsible for a specific set of business capabilities"
- By defining clear domain boundaries, microservices can be independently developed / deloped / scaled which improves the agility and scalability of the overall system

- given the above, we'll add the GET to our existing microservice.

### Creating a Restful GET API in API Gateway with a Lambda microserivce
- Add new function to HotelManager_HotelAdmin `HotelAdmin.cs`:
````c#
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
        var userId = tokenDetails.Claims.FirstOrDefault(x => x.Type == "sub"); // OAuth thing, always carrys the unique id of the user
        
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
````
- now we need to create a new lambda function in AWS, because we want a separate API that returns this list of API (needs to point to this new method)
  - created this 'ListHotels', gave it correct execution role, uploaded ZIP file 
  - updated handler to point to `HotelMAnager_HotelAdmin::HotelManager_HotelAdmin.HotelAdmin::ListHotels`

````
Creating the Lambda Function
- Get the code of the HotelMan_HotelAdmin project from GitHub (or write it yourself following several previous lectures).
- Package the code using the "dotnet lambda package HotelMan_HotelAdmin.csproj -o HotelAdmin.zip" command. This command will create HotelAdmin.zip, including all the files required to complete the Lambda function.
- Log in to AWS Console.
- Make sure you have created and configured the DynamoDB table before.
- Go to Lambda service.
- Then create a new Lambda function called ListAdminHotels and upload the HotelAdmin.zip file as its code (use dotnet 6 as runtime).
Go to the Code table, and under the Runtime Settings section, edit the Handler attribute and point it to HotelMan_HotelAdmin::HotelMan_HotelAdmin.HotelAdmin::ListHotels

Creating and Assigning the Execution Role
- Now you must create an execution IAM Role and assign it to Lambda.
- Go to IAM service.
- Go to the Roles tab.
- Create a new Role.
- Choose Lambda as your use case.
- In the Permissions step, add the AmazonDynamoDBReadOnlyAccess policy to the Role.
- Click on Next and provide a name for your Role. i.e., ListAdminHotelsExecutionRole.
- Once the Role is created, go back to Lambda service.
- Find the ListAdminHotels function and click on it.
- Go to the Configuration tab.
- On the left side of the screen, click on the Permissions tab.
- Edit the Execution Role and assign ListAdminHotelsExecutionRole as the execution role of the Lambda.
````
- now we need to create a new API Gateway 
````
Creating a Proxy REST API in AWS API Gateway
This practice will teach you to pass Query String parameters to your Lambda function.
- Login to AWS Console.
- Go to the API Gateway service's dashboard.
- Click on the "Create API" button and then choose REST API. Then click on the Build button.
- Call the new API as ListAdminHotels.
- From the Actions drop-down, choose Create Method and add a GET method to the API.
- Ensure "Use Lambda Proxy integration" is enabled when creating the GET method.
- Choose the "Lambda Function" as the integration type of the API.
- Enable "Use Lambda Proxy integration."
- In the Lambda Function box, type L so the ListAdminHotels appears. Then select it.
- Click on the Save button, then click on OK.
- From the Actions drop-down button, select Enable CORS.
- Click on the "Enable CORS and replace existing CORS headers" button.
- Click on Resources on the left side of the screen. The GET and OPTIONS methods will appear.
- Click on Options.
- Click on Integration Request.
- Expand Mapping Templates.
- Click on application/json.
- A box appears. Add the response code (200) and the CORS headers. Allow OPTIONS and GET. You can use the example template from this address and change it as required: https://shorturl.at/ikCDZ
- Click on Save.
- DO NOT configure authorization for this API.
- Create a Proxy resource with a path like {listadminhotels+}.
- Enable "Configure as a proxy resource" and "Enable API Gateway CORS".
- Set Resource Path to "{listadminhotels+}" and Resource Name to "listadminhotels".
- Once the resource is created, click on "ANY" (under the resource path {listadminhotels+}) and connect it to the ListAdminHotels Lambda function. Then click on Save.
- Under the resource of your method, click on ANY to see the "Method Execution" page (where four boxes are seen).
- Click on the "Method Request" box.
- Then, expand "URL Query String Parameters" in the Method Request page.
- Click on the "Add query string" button.
- type in "token" in the name field. Then click on the tick button on the right to save.
- Enable the "required" option.
- Click on OPTIONS under the created resource.
- Make sure its integration type is MOCK and its Integration returns the HTTP 200 and the CORS headers.
- Deploy your API to a new Stage (i.e., Test).
- Make a GET request to the Invoke URL of the GET proxy resource.
````


NOTE: Some issue with the s3 upload. seems to work manually uploaded into s3, so going from the form to the c# lambda to the s3 seems to be a problem. Guys code is here: 
https://github.com/aussiearef/MicroservicesWithAWS_Dotnet_HotelMan/blob/main/HotelMan_HotelAdmin/HotelMan_HotelAdmin/HotelAdmin.cs

### Exploring JWT and JSON Web Key Sets (JWKS)
- we've now got a POST (new hotel) and GET (list admin hotels) methods
- lambda authorisers vs cognito:
  - lambda authorisers are lambda functions: when a client tries to access a resource behind api gateway, api gateway triggers the lambda auth function, and says if the user has the right to access the api or not.
  - cognito authorisers can only perform authentication, lambda authorisers we can validate the token as well as check if the user has the correct rights (i.e. part of the admin group)
  - if we want to defer the authorisation completely to API Gateway (and not perform it in the microservice code), lambda authorisers are the way to go.
    - in the POST method we had a check `if (group == null || group.Value != "Admin")` using cognito, but in this GET we will use a lambda authoriser 

- to create a new lambda authoriser in API Gateway, go to the api then click 'authorizers' and 'create authorizer'
  - make it lambda, the token source should be "Authorization" for POST / PUT. we could make it a request instead of a token, i.e. for a GET we could pass the token through a query string called "token". 

- we can pull the JWT out of the localstorage on the browser and chuck it in jwt.io to decode it
  - we can see in the header: the kid (id of a private key made and stored by aws cognito), "alg" is the encryption algorithm, 
  -  in the body the aud (audience), iss (issuer)
  - documentation on doing this: https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-verifying-a-jwt.html
  - need to use this: `https://cognito-idp.<Region>.amazonaws.com/<userPoolId>/.well-known/jwks.json`
    - returns a JSON, there are two keys: one to sign access key, another to sign idToken
    - we can compare the kid's to see which one is used to sign the idToken, and grab that one to validate the JWT
    - we can use secrets manager, create a new secret of the key, choose 'other type of secret' and paste the whole key into the Plaintext area
      - now to access this from our lambda, we need a lambda execution role that has access to secrets manager 
      - jwt.io has a libraries section which has lots of libraries for all coding languages to validate tokens  (installed below)

### Protecting a GET API with Lambda Authorizers 
- create a new class library 'LambdaAuthorizer'
- install nuget packages: `amazon.lambda.core`, `amazon.lambda.apigatewayevents`, `awssdk.secretsmanager`, `System.identitymodel.tokens.jwt`
- Lambda Authorizer code:
````c#
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.IdentityModel.Tokens;

namespace LambdaAuthorizer;

public class Authorizer
{
    public async Task<APIGatewayCustomAuthorizerResponse> Auth(APIGatewayCustomAuthorizerRequest request)
    {
        // in api gateway UI we have 'request' and query string param as 'token' in the lambda event payload, which populates:
        var idToken = request.AuthorizationToken;
        var idTokenDetails = new JwtSecurityToken(jwtEncodedString: idToken);

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
                       Action = new HashSet<string>{"execute-api:Invoke"},
                       Effect = "Allow",
                       Resource = new HashSet<string>{request.MethodArn}
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

        var privateKeys = secret.SecretString;
        var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(privateKeys, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

        var privateKey = jwks.Keys.First(x => x.Kid == kid);

        var handler = new JwtSecurityTokenHandler();
        var result = await handler.ValidateTokenAsync(idToken, new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = privateKey
        });

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
            }
        }

        return response;
    }
}
````
- zip this up `dotnet lambda package LambdaAuthorizer.csprok -o LambdaAuthorizer.zip`
- create a lambda authorizer role in IAM:
  - create role lambda
  - choose roles: awslambdabasicexecutionrole, awslambdarole, secretsmanagerreadwrite
  - call it `hotel-lambda-authoriser-execution-role` + create
- create a new lambda function 'HotelLambdaAuthorizer' with dotnet runtime
  - upload the zip
  - edit the handler, `LambdaAuthorizer::LambdaAuthorizer::Authorizer::Auth`
  - go to configuration tab, edit permissions, add the correct execution role
- go to the ListAdminHotels api gateway, authorizers, create new authorizer
  - call it "CustomAuthorizer"
  - select the lambda
  - set payload as request, query string, token
  - enable token for 300 