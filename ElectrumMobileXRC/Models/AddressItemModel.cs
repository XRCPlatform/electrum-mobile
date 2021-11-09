using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumMobileXRC.Models
{
    public class AddressItemModel
    {
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public int TxCount { get; set; }
        public bool IsSelected { get; set; }
    }
}
