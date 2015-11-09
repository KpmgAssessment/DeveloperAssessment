using Kpmg.Assessment.Common;
using Kpmg.Assessment.Common.Filter;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;

namespace Kpmg.Assessment.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //The idea here was to dynamically load all controllers, the benefits to this approach
            //is that controllers would not have to be part of the api project, it can be dynamically
            //dropped into any specified location and dynamically bootstrapped to the application

            //However i decided to disable this feature as i felt it was an overkill in this scenario..
            //The DynamicsAssembliesResolver class is responsible for bootstrapping external assemblies
            //GlobalConfiguration.Configuration.Services.Replace(typeof(IAssembliesResolver), new DynamicAssembliesResolver());
            GlobalConfiguration.Configuration.Filters.Add(new TransformExceptionFilter());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
            //Set up the json serializer to default to camel casing..
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
