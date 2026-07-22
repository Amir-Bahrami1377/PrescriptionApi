namespace Prescription.SharedKernel.Abstractions;

/// <summary>
/// Strategy abstraction over the SMS gateway. Implemented by Prescription.Modules.Notifications
/// (MeliPayamakSmsProvider) so the provider can be swapped without touching callers.
/// </summary>
public interface ISmsProvider
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
