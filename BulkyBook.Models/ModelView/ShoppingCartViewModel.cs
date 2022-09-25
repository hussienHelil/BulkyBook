using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ModelView
{
    public class ShoppingCartViewModel
    {
        public List<ShoppingCart> CartList { get; set; }
        public OrderHeader OrderHeader{ get; set; }
    }
}
