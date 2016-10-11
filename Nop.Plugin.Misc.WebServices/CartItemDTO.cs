using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Misc.WebServices
{
    public class CartItemDTO 
    {
        public CartItemDTO () { }
        public CartItemDTO(ShoppingCartItem item)
        {
            this.Product = new ProductDTO(item.Product);
            this.Quantity = item.Quantity;
            this.Id = item.Id;
        }
        public ProductDTO Product { get; set; }
        public int Quantity { get; set; }

        public int Id { get; set; }
    }
}
