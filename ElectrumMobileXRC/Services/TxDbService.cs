using ElectrumMobileXRC.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ElectrumMobileXRC.Services
{
    public class TxDbService : ISQLiteService<TxModel>
    {
        SQLiteAsyncConnection db;

        public async Task Init()
        {
            if (db != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "MyData.db");

            db = new SQLiteAsyncConnection(databasePath);

            await db.CreateTableAsync<TxModel>();
        }

        public async Task<int> AddCoffee(TxModel value)
        {
            await Init();

            value.Id = 0;

            var id = await db.InsertAsync(value);

            return id;
        }

        public async Task<TxModel> Get(int id)
        {
            await Init();

            var data = await db.Table<TxModel>()
                .FirstOrDefaultAsync(c => c.Id == id);

            return data;
        }

        public async Task<IEnumerable<TxModel>> GetAll()
        {
            await Init();

            var data = await db.Table<TxModel>().ToListAsync();

            return data;
        }

        public async Task Remove(int id)
        {
            await Init();

            await db.DeleteAsync<TxModel>(id);
        }
    }
}
