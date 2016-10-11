using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class AddressDTO
    {
        public AddressDTO() { }
        public AddressDTO(Address a)
        {
            this.Id = a.Id;
            this.Country = a.Country.Name;
            this.Phone = a.PhoneNumber;
            this.City = a.City;
            this.PostalCode = a.ZipPostalCode;
            this.Street = a.Address1;
            this.Company = a.Company;
            this.Firstname = a.FirstName;
            this.Lastname = a.LastName;
        }

        public int Id { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Street { get; set; }

        public string Company { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }
    }
}
