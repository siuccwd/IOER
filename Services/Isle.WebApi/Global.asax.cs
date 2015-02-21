using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
//using System.Web.Optimization;
using System.Web.Routing;

using WebApiContrib.Formatting.Jsonp;

namespace Isle.WebApi
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
           // GlobalConfiguration.Configure(WebApiConfig.Register);

           // AreaRegistration.RegisterAllAreas();

            FormatterConfig.RegisterFormatters( GlobalConfiguration.Configuration.Formatters );

            WebApiConfig.Register( GlobalConfiguration.Configuration );

        }
    }
}