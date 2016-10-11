using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class ProductDTO
    {
        public ProductDTO(){}
        public ProductDTO(Product prod)
        {
            this.Id = prod.Id;
            Name = prod.Name;
            Price = prod.Price;
            Image = new List<byte[]>();
            foreach (ProductPicture p in prod.ProductPictures)
            {
                Image.Add(p.Picture.PictureBinary);
            }
            Description = prod.ShortDescription;
            ExtendedDescription = prod.FullDescription;
            Attributes = new List<AttributeDTO>();
            foreach (ProductVariantAttribute p in prod.ProductVariantAttributes)
            {
                Attributes.Add(new AttributeDTO(p));
            }
            if (prod.StockQuantity > 0)
            {
                this.inStock = true;
            }
            else
            {
                this.inStock = false;
            }
            this.Rating = prod.ApprovedRatingSum;
            

        }
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Id { get; set; }

        public List<byte[]> Image { get; set; }

        public string Description { get; set; }

        public string ExtendedDescription { get; set; }

        public List<AttributeDTO> Attributes { get; set; }

        public bool inStock { get; set; }

        public int Rating { get; set; }
    }
}
