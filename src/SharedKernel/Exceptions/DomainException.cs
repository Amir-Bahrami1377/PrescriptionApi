namespace Prescription.SharedKernel.Exceptions;

/// <summary>General-purpose 400-mapped exception for domain rule violations that aren't a state-machine conflict.</summary>
public class DomainException(string message) : Exception(message);
