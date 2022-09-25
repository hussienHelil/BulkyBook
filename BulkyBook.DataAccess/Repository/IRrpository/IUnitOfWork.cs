using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRrpository
{
    public interface IUnitOfWork
    {

        public ICategoryRepository Category { get; }
        public ICoverTypeRepository CoverType { get; }
        public IProductRepository Product { get; }
        public ICompanyRepository Company { get; }
        public IShoppingCartRepository ShoppingCartRepository{ get; }
        public IApplicationUserRepository ApplicationUserRepository { get; }
        public IOrderHeaderRepository OrderHeaderRepository { get; }
        public IOrderDetailRepository OrderDetailRepository{ get; }

        public void Save();
    }
}
