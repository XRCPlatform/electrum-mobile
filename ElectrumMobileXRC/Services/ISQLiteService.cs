using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElectrumMobileXRC.Services
{
    public interface ISQLiteService<T>
    {
        Task Init();
        Task<int> AddCoffee(T value);
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task Remove(int id);
    }
}