using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElectrumMobileXRC.Services
{
    public interface ISQLiteService<T>
    {
        Task Init();
        Task<int> Add(T value);
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task Remove(int id);
    }
}