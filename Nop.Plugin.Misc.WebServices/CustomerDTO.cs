using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class CustomerDTO
    {
        public CustomerDTO(){}
        public CustomerDTO(Customer C){
            this.IsAdmin = C.IsAdmin();
            this.Id = C.Id;
            this.Username = C.Username;
            this.Email = C.Email;
            this.Addresses = new List<AddressDTO>();
            foreach (Address a in C.Addresses)
            {
                Addresses.Add(new AddressDTO(a));
            }
            this.AdminComment = C.AdminComment;
            this.Phone = C.GetAttribute<String>(SystemCustomerAttributeNames.Phone);
            this.FullName = C.GetAttribute<String>(SystemCustomerAttributeNames.FirstName) + " "  + C.GetAttribute<String>(SystemCustomerAttributeNames.LastName);
            this.Active = C.Active;
            this.Gender = C.GetAttribute<String>(SystemCustomerAttributeNames.Gender);
            this.Activity = C.LastActivityDateUtc;
            this.Birthday = C.GetAttribute<String>(SystemCustomerAttributeNames.DateOfBirth);
            this.Company = C.GetAttribute<String>(SystemCustomerAttributeNames.Company);
            ShoppingCart = new List<CartItemDTO>();
            this.Wishlist = new List<CartItemDTO>();
            foreach (ShoppingCartItem item in C.ShoppingCartItems){
                if (item.ShoppingCartType.Equals(ShoppingCartType.ShoppingCart))
                    ShoppingCart.Add(new CartItemDTO(item));
                else
                    Wishlist.Add(new CartItemDTO(item));
            }
            
        }

        public int Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string AdminComment { get; set; }

        public string Phone { get; set; }

        public string FullName { get; set; }

        public bool IsAdmin { get; set; }

        public List<CartItemDTO> ShoppingCart { get; set; }

        public bool Active { get; set; }

        public string Company { get; set; }

        public string Gender { get; set; }

        public DateTime Activity { get; set; }

        public string Birthday { get; set; }

        public List<AddressDTO> Addresses { get; set; }

        public List<CartItemDTO> Wishlist { get; set; }
    }
}
