using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.GameServer
{
    class WebHostHelper
    {
        public static WebServiceHost CreateWebHost(GameSimulation sim)
        {
            var service = new Service(sim);
            var host = new WebServiceHost(service, new Uri("http://localhost:8000/"));
            var behaviour = host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behaviour.InstanceContextMode = InstanceContextMode.Single;
            var ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
            host.Open();
            return host;
        }
    }
}
