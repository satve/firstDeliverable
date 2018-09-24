using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlogApp.Startup))]
namespace BlogApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
