namespace Monster.Middle.Persist.Instance
{
    public class ConnectionBuilder
    {
        public static string Build(string instanceDatabase)
        {
            return $"Server=localhost; Database={instanceDatabase}; Trusted_Connection=True;";
        }
    }
}
