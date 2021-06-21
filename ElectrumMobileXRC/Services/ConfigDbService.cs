using ElectrumMobileXRC.Entities;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ElectrumMobileXRC.Services
{
    public class ConfigDbService : ISQLiteService<Configuration>
    {
        SQLiteAsyncConnection db;

        public async Task Init()
        {
            if (db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DbConfiguration.SQLITE_DBFILENAME);

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<Configuration>();
        }

        public async Task<int> Add(string code, string data)
        {
            await Init();

            var cfg = new Configuration();
            cfg.Code = code;
            cfg.Value = data;

            var id = await db.InsertAsync(cfg);

            return id;
        }

        public async Task<int> Add(Configuration value)
        {
            await Init();

            value.Id = 0;

            var id = await db.InsertAsync(value);

            return id;
        }

        public async Task<Configuration> Get(int id)
        {
            await Init();

            var data = await db.Table<Configuration>()
                .FirstOrDefaultAsync(c => c.Id == id);

            return data;
        }

        public async Task<Configuration> Get(string code)
        {
            await Init();

            var data = await db.Table<Configuration>()
                .FirstOrDefaultAsync(c => c.Code == code);

            return data;
        }

        public async Task<IEnumerable<Configuration>> GetAll()
        {
            await Init();

            var data = await db.Table<Configuration>().ToListAsync();

            return data;
        }

        public async Task Remove(int id)
        {
            await Init();

            await db.DeleteAsync<Configuration>(id);
        }
    }
}
