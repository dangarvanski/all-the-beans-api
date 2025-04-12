using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Queries
{
    public record GetAllRecordsQuery(int page, int pageSize) : IRequest<List<BeanDbRecord>>;

    public sealed class GetAllRecords : IRequestHandler<GetAllRecordsQuery, List<BeanDbRecord>>
    {
        private readonly IBeansDbRepository _appDbRepo;

        public GetAllRecords(IBeansDbRepository appDbRepo)
        {
            _appDbRepo = appDbRepo;
        }

        public async Task<List<BeanDbRecord>> Handle(GetAllRecordsQuery request, CancellationToken cancellationToken)
        {
            return await _appDbRepo.GetAllRecordsAsync(request.page, request.pageSize);
        }
    }
}
