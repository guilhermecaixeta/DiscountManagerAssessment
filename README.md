# DiscountManager Assessment

This repository is part of the technical challenge.

## Assessment Description

The system is designed to generate and use DISCOUNT codes. The system consists of a server-side and a clientside. 
Communication depends on the protocol specified below.

### Protocol

Do not use WEB APIs (REST) for this task. Everything else is allowed for example TCP sockets, websockets,SignalR, gRPC etc.

## About

All application is developed using .Net8 the latest LTS version of .Net.

This solution was built using Grpc Server to expose the API to be consumed by the FE (with a Grpc Client) there is a shared solution where can be found the business rules and share the rpc lib between the applications

## How to RUN

### Requirements:

There a few requirements to run properly this project:

- Docker
- Docker Compose
- VisualStudio

Once both applications were installed you'll be able to run this application.

### Step 1

Click in the arrow at the side of run button and selecting `Configure Startup Projects`

### Step 2

Then select the project `docker-compose`, then press `Ok` and `Apply`

Your solution is ready to run properly.

### Step 3

Once the docker-compose poject is selected as the startup project, the solution is able to run properly

### About the Database

To update the database settings are stored:

```json
# appsettings.development.json

  "ConnectionStrings": {
    "Postgres": "Host=postgres;Database=DiscountDevelopment;Username=postgres;Password=postgres"
  }
```

```env
# postgres.env

POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

### About the secret

The secret is a random value of 128bits used as pepper to generate the random keys. 

```json
# appsettings.development.json

  "Generator": {
    "SecretKey": "secre-key"
  },
```