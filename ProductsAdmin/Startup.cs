using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProductsAdmin.Startup))]
namespace ProductsAdmin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
