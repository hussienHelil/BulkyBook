using DataAccess.Repository.IRrpository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }
        public IEnumerable<T> GetAll(string? IncludedNavigations = null, Expression<Func<T, bool>>? predicate = null)
        {
            IQueryable<T> result = _dbSet;
            if(predicate != null)
            {
                result = result.Where(predicate);
            }
            if (IncludedNavigations != null && IncludedNavigations.Trim() != "")
            {
                string[] models = IncludedNavigations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in models)
                {
                    result = result.Include(item);
                }
            }
            return result;
        }


        public T GetFirstOrDefault(Func<T, bool> predicate, string? IncludedNavigations = null)
        {
            IQueryable<T> result = _dbSet;
            if (IncludedNavigations != null && IncludedNavigations.Trim() != "")
            {
                string[] models = IncludedNavigations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in models)
                {
                    result = result.Include(item);
                }
            }
            return result.FirstOrDefault(predicate);
        }
        public void Add(T item)
        {

            _dbSet.Add(item);
        }

        public void Remove(T item)
        {
            _dbSet.Remove(item);
        }

        

        public void RemoveRange(IEnumerable<T> items)
        {
            _dbSet.RemoveRange(items);
        }
    }
}
