﻿using System.Collections.Generic;

namespace Monster.TaxTransfer
{
    public class TaxCalcResult
    {
        public decimal TaxableAmount { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxAmount { get; set; }
        public List<string> ErrorMessages { get; set; }
        public bool Failed => ErrorMessages.Count > 0;

        public TaxCalcResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}
