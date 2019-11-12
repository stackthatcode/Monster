namespace Push.Foundation.Utilities.General
{
    public static class PagingHelper
    {
        public static int StartingRecord(int pageNumber, int pageSize)
        {
            return (pageNumber - 1) * pageSize;
        }
    }
}
