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
- [API Gateway Pattern and Tools](#api-gateway-pattern-and-tools)

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

## API Gateway Pattern and Tools
