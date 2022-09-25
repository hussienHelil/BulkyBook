using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRrpository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void IncrementCount(ShoppingCart shoppingCart, int count);
        void DecrementCount(ShoppingCart shoppingCart, int count);
    }
}
