using System.Web;
using System.Web.Mvc;

namespace Kpmg.Assessment.UIClient
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
