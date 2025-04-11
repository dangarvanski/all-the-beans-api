using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;
using MediatR;

namespace all_the_beans_application.Queries
{
    public record GetBeanOfTheDayRecordQuery : IRequest<BeanDbRecord?>;

    public sealed class GetBeanOfTheDayRecordHandler : IRequestHandler<GetBeanOfTheDayRecordQuery, BeanDbRecord?>
    {
        private readonly IBeansDbRepository _beansDbRepo;

        public GetBeanOfTheDayRecordHandler(IBeansDbRepository beansDbRepo)
        {
            _beansDbRepo = beansDbRepo;
        }

        public async Task<BeanDbRecord?> Handle(GetBeanOfTheDayRecordQuery request, CancellationToken cancellationToken)
        {
            return await _beansDbRepo.GetBeanOfTheDayRecordAsync();
        }
    }
}
