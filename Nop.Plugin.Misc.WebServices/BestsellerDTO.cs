using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Misc.WebServices
{
    public class BestsellerDTO
    {
        public BestsellerDTO() { }
        public BestsellerDTO(BestsellersReportLine Bs)
        {
            var _productService = EngineContext.Current.Resolve<IProductService>();
            Product = new ProductDTO(_productService.GetProductById(Bs.ProductId));
            Amount = Bs.TotalAmount;
            Quantity = Bs.TotalQuantity;
        }

        public ProductDTO Product { get; set; }

        public decimal Amount { get; set; }

        public int Quantity { get; set; }
    }
}
