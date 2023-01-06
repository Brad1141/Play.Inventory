using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem inventoryItem, string Name, string Description)
        {
            return new InventoryItemDto(inventoryItem.CatalogItemId, Name, Description, inventoryItem.Quantity, inventoryItem.AcquiredDate);
        }

    }
}