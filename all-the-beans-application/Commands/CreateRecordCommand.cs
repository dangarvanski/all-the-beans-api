using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Commands
{
    public record CreateRecordCommand(CreateRecordRequest createRecordRequest) : IRequest<Dictionary<bool, int>>;

    public sealed class CreateRecordCommandHandler : IRequestHandler<CreateRecordCommand, Dictionary<bool, int>>
    {
        private readonly IBeansDbRepository _beansDbRepo;

        public CreateRecordCommandHandler(IBeansDbRepository beansDbRepo)
        {
            _beansDbRepo = beansDbRepo;
        }

        public async Task<Dictionary<bool, int>> Handle(CreateRecordCommand request, CancellationToken cancellationToken)
        {
            // Keeping it the same as .Count returns exact number of items while the indexes start from 0
            var newIndex = (await _beansDbRepo.GetAllRecordsAsync()).Count; 
            
            var newRecord = new BeanDbRecord
            {
                _id = Guid.NewGuid().ToString(),
                index = newIndex,
                IsBOTD = false,
                Cost = request.createRecordRequest.Cost,
                Image = request.createRecordRequest.Image,
                Color = request.createRecordRequest.Color,
                Name = request.createRecordRequest.Name,
                Description = request.createRecordRequest.Description,
                Country = request.createRecordRequest.Country
            };

            var result = await _beansDbRepo.InsertNewBeanRecordAsync(newRecord, cancellationToken);
            var response = new Dictionary<bool, int>();

            if (result != null)
            {
                response.Add(true, result.index);
            }
            else
            {
                response.Add(false, 0);
            }
            
            return response;
        }
    }
}
