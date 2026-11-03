using System.Security.Cryptography;
using System.Text;

namespace HealthcareStore.Application.Carts;

public static class CartIdempotencyFingerprint
{
    public static string ForAdd(
        Guid productVariantId,
        int quantity)
    {
        return Compute(
            $"cart-add-v1|{productVariantId:N}|{quantity}");
    }

    public static string ForUpdate(
        Guid cartItemId,
        int quantity)
    {
        return Compute(
            $"cart-update-v1|{cartItemId:N}|{quantity}");
    }

    public static string ForRemove(Guid cartItemId)
    {
        return Compute(
            $"cart-remove-v1|{cartItemId:N}");
    }

    private static string Compute(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        return Convert.ToHexString(
            SHA256.HashData(bytes));
    }
}