namespace Prescription.SharedKernel.Exceptions;

/// <summary>Thrown when the current user is not permitted to perform a domain operation (e.g. reviewing someone else's order).</summary>
public sealed class UnauthorizedDomainException(string message) : Exception(message);
