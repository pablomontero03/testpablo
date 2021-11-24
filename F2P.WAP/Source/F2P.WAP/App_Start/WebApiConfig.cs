using F2P.WAP.Controllers;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace F2P.WAP
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Filters.Add(new ValidateModelAttribute());
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.MediaTypeMappings.Add(new QueryStringMapping("type", "xml", new MediaTypeHeaderValue("application/xml")));
            //aplica seguridad a wsTuca crea SSL/TLS secure channel.
            bool flag = false;
            Boolean.TryParse(ConfigurationManager.AppSettings["ValidaProtocolosSSLTSL"], out flag);
            if (flag)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
    }
}
