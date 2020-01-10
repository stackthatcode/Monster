using System;

namespace Monster.Middle.Persist.Master
{
    public class Instance
    {
        public Guid Id { get; set; }
        public int AvailabilityOrder { get; set; }
        public string InstanceDatabase { get; set; }
        public string OwnerNickname { get; set; }
        public string OwnerDomain { get; set; }
        public string OwnerUserId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
