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