using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;
using System.Reflection;

namespace all_the_beans_application.Commands
{
    public record UpdateRecordCommand(int index, UpdateRecordRequest updateRecordRequest) : IRequest<BeanDbRecord?>;

    public sealed class UpdateRecordCommandHandler : IRequestHandler<UpdateRecordCommand, BeanDbRecord?>
    {
        private readonly IBeansDbRepository _beansDbRepo;
        private static readonly PropertyInfo[] _updateProperties = typeof(UpdateRecordRequest).GetProperties();
        private static readonly PropertyInfo[] _recordProperties = typeof(BeanDbRecord).GetProperties();

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

            bool hasValidField = false;
            foreach (var prop in _updateProperties)
            {
                var value = prop.GetValue(request.updateRecordRequest) as string;
                if (value != null && value != string.Empty && value != "string")
                {
                    var recordProp = Array.Find(_recordProperties, p => p.Name == prop.Name);
                    if (recordProp != null && recordProp.CanWrite)
                    {
                        recordProp.SetValue(dbRecord, value);
                        hasValidField = true;
                    }
                }
            }

            if (!hasValidField)
            {
                throw new ArgumentException("At least one valid field must be provided");
            }

            var success = await _beansDbRepo.UpdateBeanRecordAsync(dbRecord, cancellationToken);
            if (!success)
            {
                return null;
            }
            return dbRecord;
        }
    }
}
