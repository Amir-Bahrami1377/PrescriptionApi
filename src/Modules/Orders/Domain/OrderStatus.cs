namespace Prescription.Modules.Orders.Domain;

public enum OrderStatus
{
    Draft = 0,
    PendingDoctorApproval = 1,
    AwaitingPayment = 2,
    InProgress = 3,
    Completed = 4,
    Rejected = 5,
}
