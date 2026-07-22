using Prescription.SharedKernel.Entities;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Consultation.Domain;

public sealed class ConsultationRequest : AuditableEntity
{
    private ConsultationRequest() { }

    public Guid CustomerId { get; private set; }
    public string ImageFileKey { get; private set; } = null!;
    public string? CustomerNote { get; private set; }

    public ConsultationStatus Status { get; private set; }
    public Guid? DoctorId { get; private set; }
    public string? DoctorOpinion { get; private set; }
    public DateTimeOffset? AnsweredAtUtc { get; private set; }

    public static ConsultationRequest Create(Guid customerId, string imageFileKey, string? customerNote)
    {
        return new ConsultationRequest
        {
            CustomerId = customerId,
            ImageFileKey = imageFileKey,
            CustomerNote = customerNote,
            Status = ConsultationStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void SubmitOpinion(Guid doctorId, string opinion)
    {
        if (Status != ConsultationStatus.Pending)
        {
            throw new ConflictException("این درخواست مشاوره قبلاً پاسخ داده شده است.");
        }

        DoctorId = doctorId;
        DoctorOpinion = opinion;
        Status = ConsultationStatus.Answered;
        AnsweredAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
