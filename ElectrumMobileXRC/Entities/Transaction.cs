using SQLite;

namespace ElectrumMobileXRC.Entities
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Hash { get; set; }
    }
}
