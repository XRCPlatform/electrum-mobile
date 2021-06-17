using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumMobileXRC.Models
{
    public class TxModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Hash { get; set; }
    }
}
