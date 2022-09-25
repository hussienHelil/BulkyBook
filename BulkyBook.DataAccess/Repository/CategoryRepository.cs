using DataAccess.Repository.IRrpository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db; 
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;   
        }

        public void Update(Category item)
        {
            _db.Update(item);
        }
    }
}
