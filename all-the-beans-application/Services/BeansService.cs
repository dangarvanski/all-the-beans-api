using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;
using all_the_breans_sharedKernal.Entities;

namespace all_the_beans_application.Services
{
    public class BeansService : IBeansService
    {
        private readonly IAppDbContext _appDbContext;

        public BeansService(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<BeanDbRecord?> GetRecordByIndexAsync(int id)
        {
            return await _appDbContext.GetRecordByIndexAsync(id);
        }
    }
}
