using Play.Common;
using Play.Common.MongoDB;
using Play.Inventory.Service;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddMongo()
                  .AddMongoRepository<InventoryItem>("inventoryitems");

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5023");
})
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt))
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Using Minimal api 
 
 var Items = app.MapGroup("/items");

 Items.MapGet("/", async (IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient, Guid userId) =>
 {
    if( userId == Guid.Empty) return Results.BadRequest();
     
    //calling external play.catalogue service to retrive items details
    var catalogItems = await catalogClient.GetCatalogItemsAsync();
    var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.UserId == userId);

    var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
    {
        var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogId);
        return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
    });

    return Results.Ok(inventoryItemDtos);
 });


 Items.MapPost("/", async (IRepository<InventoryItem> itemsRepository, GrantItemsDto grantItemDto) =>
 {
    var inventoryItem = await itemsRepository.GetAsync(
        item => item.UserId == grantItemDto.UserId && item.CatalogId == grantItemDto.CatalogItemId);

    if (inventoryItem == null)
    {
        inventoryItem = new InventoryItem
        {
            CatalogId = grantItemDto.CatalogItemId,
            UserId = grantItemDto.UserId,
            Quantity = grantItemDto.Quantity,
            AcquiredDate = DateTimeOffset.UtcNow
        };
        await itemsRepository.CreateAsync(inventoryItem); 
    }  
    else
    {
        inventoryItem.Quantity += grantItemDto.Quantity;
        await itemsRepository.UpdateAsync(inventoryItem);
    }

     return Results.Ok();
 });

 app.Run();

