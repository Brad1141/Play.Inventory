namespace Play.Inventory.Service.Dtos
{
    // used to grant items to a user
    public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);

    // return items user has
    public record InventoryItemDto(Guid CatalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);

    public record CatalogItemDto(Guid Id, string Name, string Description);

}