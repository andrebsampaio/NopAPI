using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class OrderItemDTO
    {
        public OrderItemDTO() { }
        public OrderItemDTO(OrderItem ordI) {
            OrderId = ordI.OrderId;
            ProductId = ordI.ProductId;
            Quantity = ordI.Quantity;
            Product = new ProductDTO(ordI.Product);
        }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ProductDTO Product {get;set;}
    }
   
   
}
