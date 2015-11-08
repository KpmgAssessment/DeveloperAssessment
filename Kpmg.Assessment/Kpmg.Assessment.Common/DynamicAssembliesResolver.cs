using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;

namespace Kpmg.Assessment.Common
{
    public class DynamicAssembliesResolver : IAssembliesResolver
    {
        public ICollection<Assembly> GetAssemblies()
        {
            IList<Assembly> baseAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            string directoryPath = ConfigManager.GetAppSetting("ControllerDir", defaultValue: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfiguredControllers"));
            if (Directory.Exists(directoryPath))
            {
                foreach (string file in Directory.EnumerateFiles(directoryPath))
                {
                    Assembly controllerDll = Assembly.LoadFrom(file);
                    baseAssemblies.Add(controllerDll);
                }
            }
            return baseAssemblies;
        }
    }
}
