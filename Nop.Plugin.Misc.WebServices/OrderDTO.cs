using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Misc.WebServices
{
    public class OrderDTO 
    {
        public OrderDTO() { }
        public OrderDTO(Order order)
        {
            OrderID = order.Id;
            OrderEmail = order.Customer.Username;
            OrderStatus = order.OrderStatus;
            PayStatus = order.PaymentStatus;
            CreateDate = order.CreatedOnUtc;
            Address = order.BillingAddress.Address1;
            ShippingStatus = order.ShippingStatus;
            ProductsList = new List<OrderItemDTO>();
            for (IEnumerator<OrderItem> i = order.OrderItems.GetEnumerator(); i.MoveNext();)
            {
                var current = i.Current;
                ProductsList.Add(new OrderItemDTO(current));
            }
            Total = order.OrderTotal;
            BillingAddress = new AddressDTO(order.BillingAddress);
            ShippingAddress = new AddressDTO(order.ShippingAddress);
            
        }
        public int OrderID { get; set;}
        public DateTime CreateDate { get;set; }
        public OrderStatus OrderStatus {get; set;}
        public PaymentStatus PayStatus { get; set; }
        public string OrderEmail { get; set; }
        public List<OrderItemDTO> ProductsList{get;set;}

        public String Address { get; set; }

        public ShippingStatus ShippingStatus { get; set; }

        public decimal Total { get; set; }

        public AddressDTO BillingAddress { get; set; }

        public AddressDTO ShippingAddress { get; set; }
    }
}
