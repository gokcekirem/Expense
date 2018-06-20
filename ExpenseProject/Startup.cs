using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExpenseProject.Startup))]
namespace ExpenseProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
