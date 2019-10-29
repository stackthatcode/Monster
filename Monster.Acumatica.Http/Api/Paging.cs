using Push.Foundation.Utilities.Http;

namespace Monster.Acumatica.Api
{
    public class Paging
    {
        public static string QueryStringParams(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return $"$top={pageSize}&$skip={skip}";
        }
    }

    public static class PagingExtensions
    {
        public static QueryStringBuilder 
                AddPaging(this QueryStringBuilder builder, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return builder.Add("$top", page).Add("$skip", skip);
        }
    }
}
