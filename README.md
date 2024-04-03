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