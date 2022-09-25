using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRrpository
{
    public interface IRepository<T> where T : class
    {
        public IEnumerable<T> GetAll(string? IncludedNavigations = null, Expression<Func<T, bool>>? predicate = null);
        public T GetFirstOrDefault(Func<T, bool> predicate, string? IncludedNavigations = null);
        public void Add(T item);
        public void Remove(T item);
        public void RemoveRange(IEnumerable<T> items);
    }
}
