using all_the_beans_application.Interfaces;
using all_the_breans_infrastructure.Interfaces;

namespace all_the_beans_application.Services
{
    public class BeansService : IBeansService
    {
        private readonly IBeansDbRepository _appDbContext;

        public BeansService(IBeansDbRepository appDbContext)
        {
            _appDbContext = appDbContext;
        }
    }
}
