using Play.Common;
using Play.Common.MongoDB;
using Play.Inventory.Service;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddMongo()
                  .AddMongoRepository<InventoryItem>("inventoryitems");

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

 Items.MapGet("/", async (IRepository<InventoryItem> itemsRepository, Guid userId) =>
 {
    if( userId == Guid.Empty) return Results.BadRequest();

    var items = (await itemsRepository.GetAllAsync(item => item.UserId == userId))
            .Select(item => item.AsDto());

    return Results.Ok(items);
 });


 Items.MapPost("/", async (IRepository<InventoryItem> itemsRepository, GrantItemsDto grantItemDto) =>
 {
    var inventoryItem = await itemsRepository.GetAsync(
        item => item.UserId == grantItemDto.UserId && item.CatalogueId == grantItemDto.CatalogueItemId);

    if (inventoryItem == null)
    {
        inventoryItem = new InventoryItem
        {
            CatalogueId = grantItemDto.CatalogueItemId,
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

