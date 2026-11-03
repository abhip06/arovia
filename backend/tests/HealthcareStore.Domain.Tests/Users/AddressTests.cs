using HealthcareStore.Domain.Users;

namespace HealthcareStore.Domain.Tests.Users;

public sealed class AddressTests
{
    [Fact]
    public void Create_ShouldCreateAddress_WithValidData()
    {
        var userId = Guid.NewGuid();

        var address = Address.Create(
            userId,
            "Home",
            "John Doe",
            "9123456780",
            "123 Main Street",
            "Apartment 4B",
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            true);

        Assert.NotEqual(Guid.Empty, address.Id);
        Assert.Equal(userId, address.UserId);
        Assert.Equal("Home", address.Label);
        Assert.Equal("John Doe", address.RecipientName);
        Assert.Equal("9123456780", address.RecipientPhone);
        Assert.Equal("123 Main Street", address.AddressLine1);
        Assert.Equal("Apartment 4B", address.AddressLine2);
        Assert.Equal("Bhopal", address.City);
        Assert.Equal("Madhya Pradesh", address.State);
        Assert.Equal("462001", address.PostalCode);
        Assert.Equal("India", address.Country);
        Assert.True(address.IsPrimary);
        Assert.NotEqual(default, address.CreatedAt);
        Assert.Equal(address.CreatedAt, address.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldSetIsPrimaryToFalse_WhenFalseIsProvided()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            "9123456780",
            "123 Main Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            false);

        Assert.False(address.IsPrimary);
    }

    [Fact]
    public void Create_ShouldSetIsPrimaryToTrue_WhenTrueIsProvided()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            "9123456780",
            "123 Main Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            true);

        Assert.True(address.IsPrimary);
    }

    [Fact]
    public void Create_ShouldAllowOptionalFieldsToBeNull()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            null,
            "123 Main Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            false);

        Assert.Null(address.RecipientPhone);
        Assert.Null(address.AddressLine2);
        Assert.False(address.IsPrimary);
    }

    [Fact]
    public void Create_ShouldTrimStringFields()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "  Home  ",
            "  John Doe  ",
            "  9123456780  ",
            "  123 Main Street  ",
            "  Apartment 4B  ",
            "  Bhopal  ",
            "  Madhya Pradesh  ",
            "  462001  ",
            "  India  ",
            true);

        Assert.Equal("Home", address.Label);
        Assert.Equal("John Doe", address.RecipientName);
        Assert.Equal("9123456780", address.RecipientPhone);
        Assert.Equal("123 Main Street", address.AddressLine1);
        Assert.Equal("Apartment 4B", address.AddressLine2);
        Assert.Equal("Bhopal", address.City);
        Assert.Equal("Madhya Pradesh", address.State);
        Assert.Equal("462001", address.PostalCode);
        Assert.Equal("India", address.Country);
        Assert.True(address.IsPrimary);
    }

    [Fact]
    public void Create_ShouldTreatWhitespaceOptionalFieldsAsNull()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            "   ",
            "123 Main Street",
            "   ",
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            false);

        Assert.Null(address.RecipientPhone);
        Assert.Null(address.AddressLine2);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        var firstAddress = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            null,
            "123 Main Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            false);

        var secondAddress = Address.Create(
            Guid.NewGuid(),
            "Work",
            "John Doe",
            null,
            "456 Office Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462002",
            "India",
            false);

        Assert.NotEqual(firstAddress.Id, secondAddress.Id);
    }

    [Fact]
    public void Create_ShouldInitializeUpdatedAtToCreatedAt()
    {
        var address = Address.Create(
            Guid.NewGuid(),
            "Home",
            "John Doe",
            null,
            "123 Main Street",
            null,
            "Bhopal",
            "Madhya Pradesh",
            "462001",
            "India",
            false);

        Assert.Equal(
            address.CreatedAt,
            address.UpdatedAt);
    }

    [Fact]
    public void Create_ShouldThrow_WhenUserIdIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.Empty,
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenLabelIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("label", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenLabelIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "   ",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("label", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenRecipientNameIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("recipientName", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenRecipientNameIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "   ",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("recipientName", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenAddressLine1IsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("addressLine1", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenAddressLine1IsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "   ",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("addressLine1", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCityIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("city", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCityIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "   ",
                "Madhya Pradesh",
                "462001",
                "India",
                false));

        Assert.Equal("city", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenStateIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "",
                "462001",
                "India",
                false));

        Assert.Equal("state", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenStateIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "   ",
                "462001",
                "India",
                false));

        Assert.Equal("state", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPostalCodeIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "",
                "India",
                false));

        Assert.Equal("postalCode", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPostalCodeIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "   ",
                "India",
                false));

        Assert.Equal("postalCode", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCountryIsEmpty()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "",
                false));

        Assert.Equal("country", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCountryIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Address.Create(
                Guid.NewGuid(),
                "Home",
                "John Doe",
                null,
                "123 Main Street",
                null,
                "Bhopal",
                "Madhya Pradesh",
                "462001",
                "   ",
                false));

        Assert.Equal("country", exception.ParamName);
    }
}