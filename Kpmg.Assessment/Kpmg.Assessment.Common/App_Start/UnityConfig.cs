using Kpmg.Assessment.Interfaces;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace Kpmg.Assessment.Common
{
    public static class UnityConfig
    {
        public static UnityContainer container;
        public static void RegisterComponents()
        {
			container = new UnityContainer();

            //Setting IoC for Readonly objects...
            container.RegisterType<IReadOnlyDataProvider<TransactionData>, ReadOnlyTransactionDataProvider>(new HierarchicalLifetimeManager());

            //Setting IoC for write objects...
            container.RegisterType<ICanWriteDataProvider<TransactionData>, TransactionDataProvider>(new HierarchicalLifetimeManager());

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}