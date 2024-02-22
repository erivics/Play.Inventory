using Play.Common;

namespace Play.Inventory. Service.Entities
{
    public class InventoryItem : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        public Guid CatalogueId { get; set; }
        public DateTimeOffset AcquiredDate {get; set;}
    }
}