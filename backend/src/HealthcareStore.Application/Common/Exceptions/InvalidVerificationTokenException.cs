namespace HealthcareStore.Application.Common.Exceptions;

public sealed class InvalidVerificationTokenException(string message) : Exception(message);