using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kpmg.Assessment.Interfaces
{
    public interface ICanWriteDataProvider<T> : IReadOnlyDataProvider<T>
    {
        int Create(T entity);
        int Create(ICollection<T> entities);
        int Update(T entity);
        int Update(ICollection<T> entities);
        int Delete(T entity);
        int Delete(ICollection<T> entities);
    }
}
