Play.Inventory.Service web api
This microservice is implemented with minimum API to perform basic CRUD operation with .Net 8.0. MongoDB and MassTransit library is being
used for the asynchronous communication with other service.

This service takes record of every Items/SKU created on Catalogue service and keep track and record of inventory raised

Using Polly for retry mechanism to handle partial failure and also Circuit breaker pattern to prevent exhaustion of resources should in case we have a total down time of the called microservice.