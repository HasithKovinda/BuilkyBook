using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuilkyBook.Model.ViewModel
{
    public class ShopingCartVM
    {
        public IEnumerable<ShopingCart> ListCart { get; set; }

        public OrderHeader orderHeader { get; set; }

    }
}
