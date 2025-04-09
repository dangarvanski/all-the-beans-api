using all_the_beans_application.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Queries
{
    public record GetRecordByIdQuery(int id) : IRequest<BeanDbRecord>;

    public sealed class GetRecordById : IRequestHandler<GetRecordByIdQuery, BeanDbRecord>
    {
        private IBeansService _beansService;

        public GetRecordById(IBeansService beansService)
        {
            _beansService = beansService;
        }

        public Task<BeanDbRecord> Handle(GetRecordByIdQuery request, CancellationToken cancellationToken)
        {
            return _beansService.GetRecordByIdAsync(request.id)!;
        }
    }
}
