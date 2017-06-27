using Microsoft.Azure.Documents;
using System.Web.Http;

namespace Lab3BotSolution
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            CosmosDBRepository.Initialize();
        }
    }
}
