using System.Linq;


namespace Monster.Middle.Persist.Instance
{
    public class ReferenceDataRepository
    {
        private readonly ProcessPersistContext _dataContext;
        public MonsterDataContext Entities => _dataContext.Entities;


        public ReferenceDataRepository(ProcessPersistContext dataContext)
        {
            _dataContext = dataContext;
        }

        // Reference data
        //
        static object RefDataLock = new object();

        public ReferenceData RetrieveAcumaticaRefData()
        {
            lock (RefDataLock)
            {
                if (!_dataContext.Entities.ReferenceDatas.Any())
                {
                    var newdata = new ReferenceData();
                    _dataContext.Entities.ReferenceDatas.Add(newdata);
                    _dataContext.Entities.SaveChanges();
                }
            }

            return _dataContext.Entities.ReferenceDatas.First();
        }

        public void SaveChanges()
        {
            Entities.SaveChanges();
        }
    }
}

