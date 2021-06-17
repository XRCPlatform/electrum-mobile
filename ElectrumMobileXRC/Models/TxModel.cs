using SQLite;

namespace ElectrumMobileXRC.Models
{
    public class TxModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Hash { get; set; }
    }
}
