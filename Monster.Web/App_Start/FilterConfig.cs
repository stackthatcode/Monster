using System.Web.Mvc;
using Monster.Middle.Attributes;

namespace Monster.Middle
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttributeImpl());
        }
    }
}
