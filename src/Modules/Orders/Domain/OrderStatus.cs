namespace Prescription.Modules.Orders.Domain;

public enum OrderStatus
{
    Draft = 0,
    PendingDoctorApproval = 1,
    AwaitingPayment = 2,
    InProgress = 3,
    Completed = 4,
    Rejected = 5,

    /// <summary>Consultation orders only: paid, doctor has nothing further to do until the customer uploads their test result themselves.</summary>
    AwaitingTestResultUpload = 6,

    /// <summary>Consultation orders only: customer uploaded their test result, waiting for the doctor's written opinion.</summary>
    AwaitingConsultationOpinion = 7,
}
