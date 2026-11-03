namespace HealthcareStore.Application.Inventory.Warehouses.Serviceable;

public sealed record AddWarehouseServiceablePincodesRequest(
    IReadOnlyCollection<string> Pincodes);

public sealed record AddWarehouseServiceablePincodesResult(
    int RequestedCount,
    int AddedCount);

public sealed record RemoveWarehouseServiceablePincodeResult(
    string Pincode,
    bool Removed);