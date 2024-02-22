namespace  Play.Inventory.Service.Dtos
{
    public record GrantItemsDto(Guid UserId, Guid CatalogueItemId, int Quantity);

    public record InventoryItemDto(Guid CatalogItemId, int Quantity, DateTimeOffset AcquiredDate);
}  