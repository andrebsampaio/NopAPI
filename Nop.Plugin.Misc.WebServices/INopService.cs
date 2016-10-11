//Contributor: Nicolas Muniere

using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{

    [ServiceContract]
    public interface INopService
    {
        //[OperationContract]
        //DataSet GetPaymentMethod(string usernameOrEmail, string userPassword);

        [OperationContract]
        bool CheckLogin(string usernameOrEmail, string userPassword);

        [OperationContract]
        bool CheckLoginClient(string UsernameOrEmail, string UserPassword);

        [OperationContract]
        bool RegisterClient(bool Male, string Firstname, string Lastname, DateTime Birthday, string Email, string Company, string Password);

        [OperationContract]
        ProductDTO[] FeaturedProducts();

        [OperationContract]
        ProductDTO[] RecentlyViewedProducts(int number);

        [OperationContract]
        CategoryDTO[] GetMainCategories();

        [OperationContract]
        CategoryDTO[] GetSubCategoriesFromParent(int Id);

        [OperationContract]
        bool RemoveFromCart(string Username, int CartItemId);

        [OperationContract]
        string GetStoreUrl();

        [OperationContract]
        ProductDTO[] CategoryProductsSortedFiltered(int CatId, bool Sorted, bool Filtered, ProductSortingEnum Sorting, decimal Max, decimal Min);

        [OperationContract]
        string[] ShippingMethods();

        [OperationContract]
        string [] PaymentMethods();

        [OperationContract]
        decimal GetShippingFees(int CustomerId);

        [OperationContract]
        decimal GetTaxFees(int CustomerId);

        [OperationContract]
        bool ReOrder(int OrderId);

        [OperationContract]
        string GetPdfInvoice(int OrderId);

        [OperationContract]
        bool AddAddress(string Email, string Firstname, string Lastname, string CountryName, string Province, string City, string Street, string PostalCode, string PhoneNumber);

        [OperationContract]
        CountryDTO[] GetAllCountries();

        [OperationContract]
        string[] AddNewOrder(int CustomerId, int BillingAddressId, int ShippingAddressId, string ShippingMethod, string PaymentMethod, bool GiftWrap);

        [OperationContract]
        bool AddToCart(string Username, int ProdId, int Quantity, string[] Attributes, ShoppingCartType Type);

        [OperationContract]
        ProductDTO GetProductById(int id);

        [OperationContract]
        ProductDTO[] GetAllProductsFromCategory(int id);

        [OperationContract]
        CategoryDTO GetCategoryById(int id);

        [OperationContract]
        ProductDTO[] GetProductsByKeywords(string Words);

        [OperationContract]
        string GetUserName(string usernameOrEmail);

        [OperationContract]
        Task<List<OrderDTO>> GetOrders(int storeId,
            int vendorId, int customerId,
            int productId, int affiliateId,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
            string billingEmail, int pageIndex, int pageSize);

        [OperationContract]
        OrderDTO GetOrderById(int id);

        [OperationContract]
        List<CustomerDTO> GetCustomerList();

        [OperationContract]
        decimal GetPendingTotal();

        [OperationContract]
        void EndSession();

        [OperationContract]
        int GetCompleteTotal();

        [OperationContract]
        int GetCancelledTotal();

        [OperationContract]
        string GetStoreName();

        [OperationContract]
        int GetRegisteredUsersCountByTime(int days);

        [OperationContract]
        decimal GetTotalSalesByTime(int days);

        [OperationContract]
        decimal GetTotalPendingByReason(string reason);

        [OperationContract]
        int GetPendingOrdersCount();

        [OperationContract]
        int GetCurrentCartsCount();

        [OperationContract]
        int GetWishlistCount();

        [OperationContract]
        int GetOnlineCount();

        [OperationContract]
        int GetRegisteredCustomersCount();

        [OperationContract]
        int GetVendorsCount();

        [OperationContract]
        KeywordDTO [] GetPopularKeywords(int KeywordNumber);

        [OperationContract]
        BestsellerDTO[] GetBestsellersByAmount();

        [OperationContract]
        BestsellerDTO[] GetBestsellersByQuantity();

        [OperationContract]
        string GetCurrency();

        [OperationContract]
        List<CustomerDTO> GetCustomersByEmail(string Email);

        [OperationContract]
        List<CustomerDTO> GetCustomersByUsername(string Username);

        [OperationContract]
        List<CustomerDTO> GetCustomersByFirstname(string Firstname);

        [OperationContract]
        List<CustomerDTO> GetCustomersByLastname(string Lastname);

        [OperationContract]
        List<CustomerDTO> GetCustomersByFullname(string Fullname);

        [OperationContract]
        List<CustomerDTO> GetCustomersByCompany(string Company);

        [OperationContract]
        List<CustomerDTO> GetCustomersByPhone(string Phone);

        [OperationContract]
        List<CustomerDTO> GetCustomersByPostalCode(string PostalCode);

        [OperationContract]
        List<CustomerDTO> GetCurrentCarts(string Filter, string Email, int? LowerItems, int? HigherItems, decimal? LowerTotal, decimal? HigherTotal, bool? Abandoned);

        [OperationContract]
        List<CustomerDTO> GetBestCustomers();

        //[OperationContract]
        //DataSet ExecuteDataSet(string[] sqlStatements, string usernameOrEmail, string userPassword);
        [OperationContract]
        void ExecuteNonQuery(string sqlStatement, string usernameOrEmail, string userPassword);
        [OperationContract]
        object ExecuteScalar(string sqlStatement, string usernameOrEmail, string userPassword);


        [OperationContract]
        List<OrderError> DeleteOrders(int[] ordersId, string usernameOrEmail, string userPassword);

        [OperationContract]
        List<OrderError> DeleteOrdersWithoutUser(int[] ordersId);

        [OperationContract]
        void ChangeOrderStatus(int id, OrderStatus status);

        [OperationContract]
        void ChangePaymentStatus(int id, PaymentStatus status);

        [OperationContract]
        void ChangeShippingStatus(int id, ShippingStatus status);

        [OperationContract]
        void AddCommentToCustomer(string email, string comment);

        [OperationContract]
        void AddOrderNote(int orderId, string note, bool displayToCustomer);
        [OperationContract]
        void UpdateOrderBillingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2, string city, string region, string country, string postalCode, string usernameOrEmail, string userPassword);
        [OperationContract]
        void UpdateOrderShippingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2, string city, string region, string country, string postalCode, string usernameOrEmail, string userPassword);
        
        [OperationContract]
        void SetOrderPaymentPaid(int orderId, string usernameOrEmail, string userPassword);
        [OperationContract]
        void SetOrderPaymentPaidWithMethod(int orderId, string paymentMethodName, string usernameOrEmail, string userPassword);
        [OperationContract]
        void SetOrderPaymentPending(int orderId, string usernameOrEmail, string userPassword);
        [OperationContract]
        void SetOrderPaymentRefund(int orderId, bool offline, string usernameOrEmail, string userPassword);

        [OperationContract]
        List<OrderError> SetOrdersStatusCanceled(int[] ordersId, string usernameOrEmail, string userPassword);
    }
}
