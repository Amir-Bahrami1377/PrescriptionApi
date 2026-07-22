using MediatR;
using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Consultation.Domain;
using Prescription.Modules.Consultation.Infrastructure.Persistence;
using Prescription.SharedKernel.Exceptions;

namespace Prescription.Modules.Consultation.Features.SubmitDoctorOpinion;

public sealed class SubmitDoctorOpinionHandler(ConsultationDbContext dbContext) : IRequestHandler<SubmitDoctorOpinionCommand>
{
    public async Task Handle(SubmitDoctorOpinionCommand request, CancellationToken cancellationToken)
    {
        var consultation = await dbContext.ConsultationRequests.FirstOrDefaultAsync(c => c.Id == request.ConsultationId, cancellationToken)
            ?? throw new NotFoundException(nameof(ConsultationRequest), request.ConsultationId);

        consultation.SubmitOpinion(request.DoctorId, request.Opinion);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
