using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace all_the_beans_application.Commands
{
    public record UpdateRecordCommand(int index, UpdateRecordRequest updateRecordRequest) : IRequest<BeanDbRecord?>;

    public sealed class UpdateRecordCommandHandler : IRequestHandler<UpdateRecordCommand, BeanDbRecord?>
    {
        private readonly IBeansDbRepository _beansDbRepo;

        public UpdateRecordCommandHandler(IBeansDbRepository beansDbRepo)
        {
            _beansDbRepo = beansDbRepo;
        }

        public async Task<BeanDbRecord?> Handle(UpdateRecordCommand request, CancellationToken cancellationToken)
        {
            var dbRecord = await _beansDbRepo.GetRecordByIndexAsync(request.index);
            if (dbRecord == null)
            {
                return null;
            }

            var updatedRecord = dbRecord;
            var updateProperties = typeof(UpdateRecordRequest).GetProperties();
            foreach (var prop in updateProperties)
            {
                var value = prop.GetValue(request.updateRecordRequest) as string;
                if (value != null && value != string.Empty && value != "string")
                {
                    var recordProp = typeof(BeanDbRecord).GetProperty(prop.Name);
                    if (recordProp != null && recordProp.CanWrite)
                    {
                        recordProp.SetValue(updatedRecord, value);
                    }
                }
            }

            var success = await _beansDbRepo.UpdateBeanRecordAsync(dbRecord, updatedRecord, cancellationToken);

            if (!success)
            {
                return null;
            }

            return updatedRecord;
        }
    }
}
