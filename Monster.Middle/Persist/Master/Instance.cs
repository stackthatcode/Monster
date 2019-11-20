﻿using System;

namespace Monster.Middle.Persist.Master
{
    public class Instance
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string Nickname { get; set; }
        public string OwnerUserId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
