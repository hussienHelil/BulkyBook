using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRrpository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        public void Update(Company item);

    }
}
