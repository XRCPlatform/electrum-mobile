using SQLite;

namespace ElectrumMobileXRC.Entities
{
    public class Configuration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
    }
}
