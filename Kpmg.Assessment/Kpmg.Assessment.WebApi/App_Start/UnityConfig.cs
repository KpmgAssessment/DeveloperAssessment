using Kpmg.Assessment.Interfaces;
using Kpmg.Assessment.TaskHandler;
using Kpmg.Assessment.TaskHandler.ClassAdditions;
using Kpmg.Assessment.TaskHandler.Interfaces;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace Kpmg.Assessment.WebApi
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            UnityContainer container = new UnityContainer();

            //Setting IoC for Readonly objects...
            container.RegisterType<IReadOnlyDataProvider<TransactionData>, ReadOnlyTransactionDataProvider>(new HierarchicalLifetimeManager());

            //Setting IoC for write objects...
            container.RegisterType<ICanWriteDataProvider<TransactionData>, TransactionDataProvider>(new HierarchicalLifetimeManager());
            container.RegisterType<ICanProduce<TransactionDataModel, ValidationResultModel>, Producer<TransactionDataModel, ValidationResultModel>>(new HierarchicalLifetimeManager());
            container.RegisterType<ICanConsume<TransactionDataModel, ValidationResultModel>, Consumer<TransactionDataModel, ValidationResultModel>>(new HierarchicalLifetimeManager());

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}