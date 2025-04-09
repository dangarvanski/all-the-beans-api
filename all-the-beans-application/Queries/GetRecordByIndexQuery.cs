using all_the_beans_application.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Queries
{
    public record GetRecordByIndexQuery(int id) : IRequest<BeanDbRecord>;

    public sealed class GetRecordById : IRequestHandler<GetRecordByIndexQuery, BeanDbRecord>
    {
        private IBeansService _beansService;

        public GetRecordById(IBeansService beansService)
        {
            _beansService = beansService;
        }

        public Task<BeanDbRecord> Handle(GetRecordByIndexQuery request, CancellationToken cancellationToken)
        {
            return _beansService.GetRecordByIndexAsync(request.id)!;
        }
    }
}
