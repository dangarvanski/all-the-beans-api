using all_the_breans_infrastructure.Interfaces;
using MediatR;

namespace all_the_beans_application.Commands
{
    public record DeleteRecordCommand(int index) : IRequest<bool>;

    public sealed class DeleteRecordCommandHandler : IRequestHandler<DeleteRecordCommand, bool>
    {
        private readonly IBeansDbRepository _beansDbRepo;

        public DeleteRecordCommandHandler(IBeansDbRepository beansDbRepository)
        {
            _beansDbRepo = beansDbRepository;
        }

        public async Task<bool> Handle(DeleteRecordCommand request, CancellationToken cancellationToken)
        {
            var record = await _beansDbRepo.GetRecordByIndexAsync(request.index);
            if (record == null)
            {
                return false;
            }

            await _beansDbRepo.DeleteRecordByIndexAsync(record, cancellationToken);
            return true;
        }
    }
}
