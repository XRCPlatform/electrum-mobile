using ElectrumMobileXRC.Entities;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ElectrumMobileXRC.Services
{
    public class TxDbService : ISQLiteService<Transaction>
    {
        SQLiteAsyncConnection db;

        public async Task Init()
        {
            if (db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, DbConfig.SQLITE_DBFILENAME);

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<Transaction>();
        }

        public async Task<int> Add(Transaction value)
        {
            await Init();

            value.Id = 0;

            var id = await db.InsertAsync(value);

            return id;
        }

        public async Task<Transaction> Get(int id)
        {
            await Init();

            var data = await db.Table<Transaction>()
                .FirstOrDefaultAsync(c => c.Id == id);

            return data;
        }

        public async Task<IEnumerable<Transaction>> GetAll()
        {
            await Init();

            var data = await db.Table<Transaction>().ToListAsync();

            return data;
        }

        public async Task Remove(int id)
        {
            await Init();

            await db.DeleteAsync<Transaction>(id);
        }
    }
}
