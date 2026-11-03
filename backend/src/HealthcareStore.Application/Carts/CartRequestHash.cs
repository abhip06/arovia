using System.Security.Cryptography;
using System.Text;

namespace HealthcareStore.Application.Carts;

public static class CartRequestHash
{
    public static string ForAdd(
        Guid productVariantId,
        int quantity)
    {
        return Hash(
            $"add|{productVariantId:N}|{quantity}");
    }

    public static string ForSetQuantity(
        Guid cartItemId,
        int quantity)
    {
        return Hash(
            $"set|{cartItemId:N}|{quantity}");
    }

    public static string ForRemove(
        Guid cartItemId)
    {
        return Hash(
            $"remove|{cartItemId:N}");
    }

    private static string Hash(string value)
    {
        var bytes =
            Encoding.UTF8.GetBytes(value);

        var hash =
            SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}