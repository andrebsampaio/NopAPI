using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class AttributeDTO
    {
        public AttributeDTO(){}
        public AttributeDTO(ProductVariantAttribute p)
        {
            this.Id = p.Id;
            this.Name = p.ProductAttribute.Name;
            this.AttributeControl = p.AttributeControlType;
            this.Values = new List<string>();
            foreach (ProductVariantAttributeValue pv in p.ProductVariantAttributeValues)
            {
                this.Values.Add(pv.Name);
            }
            this.isRequired = p.IsRequired;
        }
        public List<string> Values { get; set; }

        public AttributeControlType AttributeControl { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public bool isRequired { get; set; }
    }
}
