using BuilkyBook.DataAccess.Data;
using BuilkyBook.DataAccess.Repository.IRepository;
using BuilkyBook.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuilkyBook.DataAccess.Repository
{
    public class ShopingCartRepository : Repository<ShopingCart>, IShopingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShopingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public int DecrementCount(ShopingCart shopingCart, int count)
        {
            shopingCart.count -= count;
            return shopingCart.count;
        }

        public int IncrementCount(ShopingCart shopingCart, int count)
        {
            shopingCart.count += count;
            return shopingCart.count;
        }
    }
}
