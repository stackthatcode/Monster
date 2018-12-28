using System.Web.Mvc;
using Monster.Web.Attributes;

namespace Monster.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttributeImpl());
        }
    }
}
