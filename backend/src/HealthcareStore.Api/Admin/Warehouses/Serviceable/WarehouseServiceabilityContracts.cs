namespace HealthcareStore.Api.Admin.Warehouses.Serviceable;

public sealed record AddWarehouseServiceablePincodesApiRequest(
    IReadOnlyCollection<string> Pincodes);

public sealed record AddWarehouseServiceablePincodesApiResponse(
    int RequestedCount,
    int AddedCount);

public sealed record RemoveWarehouseServiceablePincodeApiResponse(
    string Pincode,
    bool Removed);