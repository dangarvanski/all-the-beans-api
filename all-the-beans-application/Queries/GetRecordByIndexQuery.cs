using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Queries
{
    public record GetRecordByIndexQuery(int id) : IRequest<BeanDbRecord?>;

    public sealed class GetRecordByIndexQueryHandler : IRequestHandler<GetRecordByIndexQuery, BeanDbRecord?>
    {
        private readonly IBeansDbRepository _beansDbRepo;

        public GetRecordByIndexQueryHandler(IBeansDbRepository beansDbRepo)
        {
            _beansDbRepo = beansDbRepo;
        }

        public async Task<BeanDbRecord?> Handle(GetRecordByIndexQuery request, CancellationToken cancellationToken)
        {
            return await _beansDbRepo.GetRecordByIndexAsync(request.id);
        }
    }
}
