using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BudgetProgram.Startup))]
namespace BudgetProgram
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
