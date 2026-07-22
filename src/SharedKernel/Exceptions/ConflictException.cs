namespace Prescription.SharedKernel.Exceptions;

/// <summary>Thrown when a domain rule / state-machine guard rejects an operation (e.g. an invalid Order state transition).</summary>
public sealed class ConflictException(string message) : Exception(message);
