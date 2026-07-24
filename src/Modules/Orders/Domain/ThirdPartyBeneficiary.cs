namespace Prescription.Modules.Orders.Domain;

/// <summary>Present only when the order is being placed on someone else's behalf.</summary>
public sealed record ThirdPartyBeneficiary(string NationalCode, string PhoneNumber);
