using System.Web;
using System.Web.Mvc;
using Push.Foundation.Utilities.Json;

namespace Push.Foundation.Web.Helpers
{
    public static class JsonHelperExtensions
    {
        public static IHtmlString ToJsonRawHtml<TModel>(this WebViewPage<TModel> view, object input)
        {
            return view.Html.Raw(input.SerializeToJson());
        }
    }
}
