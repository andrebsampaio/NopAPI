using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class CountryDTO
    {
        public CountryDTO() { }
        public CountryDTO(Country c)
        {
            this.Id = c.Id;
            this.Name = c.Name;
            Provinces = new List<string>();
            foreach (StateProvince s in c.StateProvinces)
            {
                Provinces.Add(s.Name);
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<string> Provinces { get; set; }
    }
}
