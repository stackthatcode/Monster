using Monster.Acumatica.Config;

namespace Monster.Acumatica.Http
{
    public class UrlBuilder
    {
        private readonly string _baseUrl;
        private readonly string _versionSegment;
        private readonly string _path;

        public UrlBuilder(string baseUrl, string versionSegment)
        {
            _baseUrl = baseUrl;
            _versionSegment = versionSegment;
        }

        public string Make(string path, string queryString = null)
        {
            return queryString == null
                ? $"{_baseUrl}{_versionSegment}{path}"
                : $"{_baseUrl}{_versionSegment}{path}?{queryString}";
        }
    }
}
