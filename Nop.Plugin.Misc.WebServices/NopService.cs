//Contributor: Nicolas Muniere


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.WebServices.Security;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Core.Domain.Common;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Shipping;

namespace Nop.Plugin.Misc.WebServices
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class NopService : INopService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IPermissionService _permissionSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkContext _workContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly IStoreContext _storeContext;
        private readonly IOrderReportService _orderReportService;
        private readonly IProductService _productService;
        private readonly ISearchTermService _searchTermService;
        private readonly ICustomerReportService _customerReportService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ICategoryService _categoryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWebHelper _webHelper;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPdfService _pdfService;
        
        #endregion 

        #region Ctor

        public NopService()
        {
            _addressService = EngineContext.Current.Resolve<IAddressService>();
            _countryService = EngineContext.Current.Resolve<ICountryService>();
            _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            _customerService = EngineContext.Current.Resolve<ICustomerService>();
            _customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            _customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            _permissionSettings = EngineContext.Current.Resolve<IPermissionService>();
            _orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();
            _orderService = EngineContext.Current.Resolve<IOrderService>();
            _orderReportService = EngineContext.Current.Resolve<IOrderReportService>();
            _authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
            _storeContext = EngineContext.Current.Resolve<IStoreContext>();
            _productService = EngineContext.Current.Resolve<IProductService>();
            _searchTermService = EngineContext.Current.Resolve<ISearchTermService>();
            _customerReportService = EngineContext.Current.Resolve<ICustomerReportService>();
            _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            _recentlyViewedProductsService = EngineContext.Current.Resolve<IRecentlyViewedProductsService>();
            _categoryService = EngineContext.Current.Resolve<ICategoryService>();
            _shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
            _productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            _webHelper = EngineContext.Current.Resolve<IWebHelper>();
            _shippingService = EngineContext.Current.Resolve<IShippingService>();
            _paymentService = EngineContext.Current.Resolve<IPaymentService>();
            _orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
            _checkoutAttributeParser = EngineContext.Current.Resolve<ICheckoutAttributeParser>();
            _checkoutAttributeService = EngineContext.Current.Resolve<ICheckoutAttributeService>();
            _pdfService = EngineContext.Current.Resolve<IPdfService>();
        }

        #endregion 

        #region Utilities

        public void EndSession()
        {
            _authenticationService.SignOut();
        }

        protected void CheckAccess(string usernameOrEmail, string userPassword)
        {
            //check whether web service plugin is installed
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Misc.WebServices");
            if (pluginDescriptor == null)
                throw new ApplicationException("Web services plugin cannot be loaded");
            if (!_pluginFinder.AuthenticateStore(pluginDescriptor, _storeContext.CurrentStore.Id))
                throw new ApplicationException("Web services plugin is not available in this store");

            if (_customerRegistrationService.ValidateCustomer(usernameOrEmail, userPassword)!= CustomerLoginResults.Successful)
                    throw new ApplicationException("Not allowed");
            
            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(usernameOrEmail) : _customerService.GetCustomerByEmail(usernameOrEmail);

            _workContext.CurrentCustomer = customer;
            _authenticationService.SignIn(customer, true);

            //valdiate whether we can access this web service
            if (!_permissionSettings.Authorize(WebServicePermissionProvider.AccessWebService))
                throw new ApplicationException("Not allowed to access web service");
        }

        protected List<Order> GetOrderCollection(int[] ordersId)
        {
            var orders = new List<Order>();
            foreach (int id in ordersId)
            {
                orders.Add(_orderService.GetOrderById(id));
            }
            return orders;
        }

        #endregion

        #region Orders

        //public DataSet GetPaymentMethod(string usernameOrEmail, string userPassword)
        //{
        //    CheckAccess(usernameOrEmail, userPassword);
        //    if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
        //        throw new ApplicationException("Not allowed to manage orders");

        //    var plugins = from p in _pluginFinder.GetPlugins<IPaymentMethod>()
        //                  select p;

        //    var dataset = new DataSet();
        //    var datatable = dataset.Tables.Add("PaymentMethod");
        //    datatable.Columns.Add("SystemName");
        //    datatable.Columns.Add("Name");
        //    foreach (var plugin in plugins)
        //    {
        //        datatable.LoadDataRow(new object[] { plugin.PluginDescriptor.SystemName, plugin.PluginDescriptor.FriendlyName }, true);
        //    }
        //    return dataset;
        //}

        public bool CheckLogin(string usernameOrEmail, string userPassword)
        {
            try
            {
                CheckAccess(usernameOrEmail, userPassword);
                if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                    return false;
                return true;
            }
            catch (ApplicationException ex)
            {
                return false;
            }
        }

        public bool CheckLoginClient(string usernameOrEmail, string userPassword)
        {
            try
            {
                CheckAccess(usernameOrEmail, userPassword);
                if (!_permissionSettings.Authorize(StandardPermissionProvider.PublicStoreAllowNavigation))
                    return false;
                return true;
            }
            catch (ApplicationException ex)
            {
                return false;
            }
        }

        public bool RegisterClient(bool Male, string Firstname, string Lastname, DateTime Birthday, string Email, string Company, string Password)
        {
            try
            {
                var Role = _customerService.GetCustomerRoleBySystemName("Registered");
                if (_customerService.GetCustomerByEmail(Email) != null)
                {
                    return false;
                }
                var customer = new Customer()
                {
                    CustomerGuid = Guid.NewGuid(),
                    Email = Email,
                    Username = Email,
                    Password = Password,
                    CreatedOnUtc = DateTime.UtcNow,
                    Active = true,
                    LastActivityDateUtc = DateTime.UtcNow,
                };
                _customerService.InsertCustomer(customer);
                if (Male)
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, "Male");
                }
                else
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Gender, "Female");
                }
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, Firstname);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, Lastname);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.DateOfBirth, Birthday);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Company, Company);
                customer.CustomerRoles.Add(Role);
                _customerService.UpdateCustomer(customer);
                return true;

            }
            catch (Exception ex)
            {

                return false;
            }
           
        }

        public ProductDTO[] FeaturedProducts()
        {
            var Products = _productService.GetAllProductsDisplayedOnHomePage();
            var Result = new List<ProductDTO>();
            foreach (Product p in Products)
            {
                Result.Add(new ProductDTO(p));
            }
            return Result.ToArray();
        }

        public ProductDTO[] RecentlyViewedProducts(int number)
        {
            var Products = _recentlyViewedProductsService.GetRecentlyViewedProducts(number);
            var Result = new List<ProductDTO>();
            foreach (Product p in Products)
            {
                Result.Add(new ProductDTO(p));
            }
            return Result.ToArray();
        }

        public CategoryDTO [] GetMainCategories(){
            var Result = new List<CategoryDTO>();
            var Categories = _categoryService.GetAllCategories();
            foreach (Category c in Categories)
            {
                if ((c.IncludeInTopMenu || c.ShowOnHomePage) && c.ParentCategoryId == 0)
                {
                    Result.Add(new CategoryDTO(c));
                }
            }
            return Result.ToArray();
        }

        public CategoryDTO[] GetSubCategoriesFromParent(int Id)
        {
            var Result = new List<CategoryDTO>();
            var Categories = _categoryService.GetAllCategoriesByParentCategoryId(Id);
            foreach (Category c in Categories)
            {
                Result.Add(new CategoryDTO(c));
            }
            return Result.ToArray();
        }

        public bool RemoveFromCart(string Username, int CartItemId)
        {
            try
            {
                var CustomerCart = _customerService.GetCustomerByEmail(Username).ShoppingCartItems.Cast<ShoppingCartItem>();
                ShoppingCartItem Item = null;
                foreach (ShoppingCartItem s in CustomerCart)
                {
                    if (s.Id == CartItemId) Item = s;
                }
                _shoppingCartService.DeleteShoppingCartItem(Item);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        public bool AddToCart(string Username, int ProdId, int Quantity, string [] Attributes, ShoppingCartType Type)
        {
            try
            {
                var Product = _productService.GetProductById(ProdId);
                var AttributesXML = "";
                foreach (ProductVariantAttribute pv in Product.ProductVariantAttributes)
                {
                    foreach (string s in Attributes){

                        foreach (ProductVariantAttributeValue pva in pv.ProductVariantAttributeValues.ToList())
                        {
                            if (pva.Name.Equals(s))
                            {
                                AttributesXML = _productAttributeParser.AddProductAttribute(AttributesXML,
                                        pv, pva.Id.ToString());
                            }
                        }
                    }
                }
                var Customer = _customerService.GetCustomerByEmail(Username);
                _shoppingCartService.AddToCart(Customer, Product , Type, _storeContext.CurrentStore.Id, AttributesXML, decimal.Zero, Quantity, true);
                return true;
            } catch (Exception ex){
                throw new ApplicationException(ex.Message);
            }
             
        }

        public ProductDTO GetProductById(int id)
        {
            return new ProductDTO(_productService.GetProductById(id));
        }

        public CategoryDTO GetCategoryById(int id)
        {
            return new CategoryDTO (_categoryService.GetCategoryById(id));
        }

        public ProductDTO[] GetAllProductsFromCategory(int id)
        {
            var idList = new List<int>();
            idList.Add(id);
            var Result = new List<ProductDTO>();
            foreach (Product p in _productService.SearchProducts(categoryIds: idList)) {
                Result.Add(new ProductDTO(p));   
            }
            return Result.ToArray();
        }

        public ProductDTO[] GetProductsByKeywords(string Words)
        {
            var Products = _productService.SearchProducts(keywords: Words);
            var Result = new List<ProductDTO>();
            foreach (Product p in Products)
            {
                Result.Add(new ProductDTO(p));
            }
            return Result.ToArray();
        }

        public string GetStoreUrl()
        {
            return _webHelper.GetStoreLocation();
        }

        public ProductDTO[] CategoryProductsSortedFiltered(int CatId, bool Sorted, bool Filtered, ProductSortingEnum Sorting, decimal Max, decimal Min )
        {
            var Ids = new List<int>();
            Ids.Add(CatId);
            IPagedList<Product> Result = new PagedList<Product>(new List<Product>(), 0, Int32.MaxValue);
            
            if (Sorted && Filtered)
            {

            }

            else if (Sorted && !Filtered)
            {
                Result = _productService.SearchProducts(categoryIds: Ids, orderBy: Sorting);
            }

            else if (Filtered && !Sorted)
            {
                var AllProds = _productService.SearchProducts(categoryIds: Ids);
                foreach (Product p in AllProds)
                {
                    if (p.Price >= Min && p.Price <= Max)
                    {
                        Result.Add(p);
                    }
                }
            }

            var ReturnResult = new List<ProductDTO>();
            foreach (Product p in Result)
            {
                ReturnResult.Add(new ProductDTO(p));
            }

            return ReturnResult.ToArray();
        }

        public string[] ShippingMethods()
        {
            var Result = new List<string>();
            foreach (ShippingMethod s in _shippingService.GetAllShippingMethods())
            {
                Result.Add(s.Name);
            }
            return Result.ToArray();
        }

        public decimal GetShippingFees(int CustomerId)
        {
            return _orderTotalCalculationService.GetShoppingCartAdditionalShippingCharge(_customerService.GetCustomerById(CustomerId).ShoppingCartItems.ToList());
        }

        public decimal GetTaxFees(int CustomerId)
        {
            return _orderTotalCalculationService.GetTaxTotal(cart: _customerService.GetCustomerById(CustomerId).ShoppingCartItems.ToList());
        }

        public string[] PaymentMethods()
        {
            var Result = new List<string>();
            foreach (IPaymentMethod i in _paymentService.LoadActivePaymentMethods())
            {
                Result.Add(i.PluginDescriptor.FriendlyName);
            }
            return Result.ToArray();
        }

        public string[] AddNewOrder(int CustomerId, int BillingAddressId, int ShippingAddressId, string ShippingMethod, string PaymentMethod, bool GiftWrap)
        {
            var Customer = _customerService.GetCustomerById(CustomerId);
            Customer.BillingAddress = _addressService.GetAddressById(BillingAddressId);
            Customer.ShippingAddress = _addressService.GetAddressById(ShippingAddressId);
            _customerService.UpdateCustomer(Customer);
            var FoundMethod = new ShippingMethod();
            foreach (ShippingMethod Sm in _shippingService.GetAllShippingMethods())
            {
                if (Sm.Name.Equals(ShippingMethod))
                {
                    FoundMethod = Sm;
                    break;
                }
            }
            var ShippingOptions = _shippingService.GetShippingOptions(cart: Customer.ShoppingCartItems.ToList(), shippingAddress: Customer.ShippingAddress);
            foreach (ShippingOption so in ShippingOptions.ShippingOptions)
            {
                if (so.Name.Equals(ShippingMethod))
                {
                    _genericAttributeService.SaveAttribute<ShippingOption>(Customer, SystemCustomerAttributeNames.SelectedShippingOption, so, _storeContext.CurrentStore.Id);
                    break;
                }
            }
            var SelectedPaymentMethod = "";
            foreach (IPaymentMethod i in _paymentService.LoadActivePaymentMethods())
            {
                if (i.PluginDescriptor.FriendlyName.Equals(PaymentMethod))
                {
                    SelectedPaymentMethod = i.PluginDescriptor.SystemName;
                    break;
                }
            }
            var selectedAttributes = "";
            var Attributes = _checkoutAttributeService.GetAllCheckoutAttributes();
            CheckoutAttribute GiftAttribute = null;  
            foreach (CheckoutAttribute c in Attributes)
            {
                if (c.Name.Equals("Gift wrapping")) {
                    GiftAttribute = c;
                }
            }
            if (GiftWrap)
            {
                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                       GiftAttribute, GiftAttribute.CheckoutAttributeValues.First().Id.ToString());
            }
            else
            {
                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                       GiftAttribute, GiftAttribute.CheckoutAttributeValues.Last().Id.ToString());
            }
            _genericAttributeService.SaveAttribute(Customer, SystemCustomerAttributeNames.CheckoutAttributes, selectedAttributes);
            _genericAttributeService.SaveAttribute<string>(Customer,
               SystemCustomerAttributeNames.SelectedPaymentMethod, SelectedPaymentMethod, _storeContext.CurrentStore.Id);
            var processPaymentRequest = new ProcessPaymentRequest();
            processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
            processPaymentRequest.CustomerId = Customer.Id;
            processPaymentRequest.PaymentMethodSystemName = Customer.GetAttribute<string>(
                SystemCustomerAttributeNames.SelectedPaymentMethod,
                _genericAttributeService, _storeContext.CurrentStore.Id);
            var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
            if (placeOrderResult.Success)
            {
                return new string[0];
            }
            else
            {
                return placeOrderResult.Errors.ToArray();
            }


        }

        public bool ReOrder(int OrderId)
        {
            try
            {
                _orderProcessingService.ReOrder(_orderService.GetOrderById(OrderId));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetPdfInvoice(int OrderId)
        {
            return _pdfService.PrintOrderToPdf(_orderService.GetOrderById(OrderId), 0);

        }

        public CountryDTO[] GetAllCountries()
        {
            var Result = new List<CountryDTO>();
            foreach (Country c in _countryService.GetAllCountries())
            {
                Result.Add(new CountryDTO(c));
            }
            return Result.ToArray();
        }

        public bool AddAddress(string Email, string Firstname, string Lastname, string CountryName, string Province, string City, string Street, string PostalCode, string PhoneNumber )
        {
            try
            {
                var Customer = _customerService.GetCustomerByEmail(Email);
                var Country = new Country();
                var SelectedProvince = new StateProvince();
                foreach (Country c in  _countryService.GetAllCountries()){
                    if (c.Name.Equals(CountryName)){
                        Country = c;
                        if (c.StateProvinces.Count > 0)
                        {
                            foreach (StateProvince s in c.StateProvinces)
                            {
                                if (s.Name.Equals(Province))
                                {
                                    SelectedProvince = s;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            SelectedProvince = null;
                        }
                        break;
                    }
                }
                Random r = new Random();
                var Address = new Address();
                var Count = 0;
                while (!_addressService.IsAddressValid(Address) && Count < 3)
                {
                    var IdRand = r.Next(0, Int32.MaxValue);
                    Address = new Address()
                    {
                        Id = IdRand,
                        StateProvince = SelectedProvince,
                        CountryId = Country.Id,
                        FirstName = Firstname,
                        LastName = Lastname,
                        Email = Email,
                        Country = Country,
                        City = City,
                        Address1 = Street,
                        ZipPostalCode = PostalCode,
                        PhoneNumber = PhoneNumber,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    Count++;
                }
                if (Count == 3) return false;
                Customer.Addresses.Add(Address);
                _customerService.UpdateCustomer(Customer);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public string GetUserName(string email)
        {
            try
            {
                var customer = _customerService.GetCustomerByEmail(email);

                return customer.GetFullName();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }

        public string GetStoreName()
        {
            return _storeContext.CurrentStore.Name;
        }

        public int GetRegisteredUsersCountByTime(int days)
        {
            return _customerReportService.GetRegisteredCustomersReport(days);
        }

        public decimal GetTotalSalesByTime(int days)
        {
            return _orderReportService.ProfitReport(0,0,null,null,null, DateTime.UtcNow.AddDays(-days),null,null);
        }

        public int GetPendingOrdersCount()
        {
            return _orderService.GetAllOrderItems(null,null,null,null,OrderStatus.Pending, null,null,false).Count;
        }

        public int GetCurrentCartsCount()
        {
            return _customerService.GetAllCustomers(null,null,0,0,null,null,null,null,null,0,0,null,null,null,true,null,0,Int32.MaxValue).Count;
        }

        public int GetWishlistCount()
        {
            return _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, null, null, 0, 0, null, null, null, true, ShoppingCartType.Wishlist, 0, Int32.MaxValue).Count;
        }

        public int GetOnlineCount()
        {
            var customers = _customerService.GetOnlineCustomers(DateTime.Today, null, 0, Int32.MaxValue); 
            int onlineCounter = 0;
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.LastIpAddress != null)
                {
                    onlineCounter++;
                }
            }
            return onlineCounter;
        }


        public int GetRegisteredCustomersCount()
        {
            return GetCustomerList().Count;
        }


        public int GetVendorsCount()
        {
            int [] vendorId = {_customerService.GetCustomerRoleBySystemName("Vendors").Id};
            return _customerService.GetAllCustomers(null, null, 0, 0,vendorId, null, null, null, null, 0, 0, null, null, null, false, null, 0, Int32.MaxValue).Count;
        }

        public KeywordDTO [] GetPopularKeywords(int KeywordNumber)
        {
            var TermReport = _searchTermService.GetStats(0,KeywordNumber);
            var KeywordList = new List<KeywordDTO>();
            IEnumerator<SearchTermReportLine> termIt = TermReport.GetEnumerator();
            for (var i = termIt; termIt.MoveNext();)
            {
                KeywordList.Add(new KeywordDTO(i.Current));
            }
            return KeywordList.ToArray();
        }

        public BestsellerDTO [] GetBestsellersByQuantity()
        {
            var prodBest = _orderReportService.BestSellersReport(0, 0, 0, 0, null, null, null, null, null, 0, 1, 0, Int32.MaxValue, false);
            var Result = new List<BestsellerDTO>();
            foreach (BestsellersReportLine bs in prodBest)
            {
                Result.Add(new BestsellerDTO(bs));
            }
            return Result.ToArray();
        }

        public BestsellerDTO [] GetBestsellersByAmount()
        {
             
            var prodBest = _orderReportService.BestSellersReport(0, 0, 0, 0, null, null, null, null, null, 0, 2, 0, Int32.MaxValue, false);
            var Result = new List<BestsellerDTO>();
            foreach (BestsellersReportLine bs in prodBest)
            {
                Result.Add(new BestsellerDTO(bs));
            }
            return Result.ToArray();
        }

        public string GetCurrency()
        {
            return _workContext.WorkingCurrency.CurrencyCode;
        }

        public List<CustomerDTO> GetCustomerList()
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers();
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext();)
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != ""){
                    var CustomerDTO = new CustomerDTO(current);
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetBestCustomers()
        {
            var BestCustomers = _customerReportService.GetBestCustomersReport(null,null,null,null,null,1);
            var Result = new List<CustomerDTO>();
            foreach(BestCustomerReportLine item in BestCustomers){
                var Customer = _customerService.GetCustomerById(item.CustomerId);
                Result.Add(new CustomerDTO(Customer));
            }

            return Result;
        }

        public decimal GetPendingTotal()
        {
            var storeId = _storeContext.CurrentStore.Id;
            try
            {
                var report = _orderReportService.ProfitReport(0, 0, OrderStatus.Pending, null, null, null, null, null);
                return (int)report;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.StackTrace);
            } 
        }

        public int GetCompleteTotal()
        {
            var storeId = _storeContext.CurrentStore.Id;
            try
            {
                var report = _orderReportService.ProfitReport(0, 0, OrderStatus.Complete, null, null, null, null, null);
                return (int)report;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            } 
        }

        public int GetCancelledTotal()
        {
            var storeId = _storeContext.CurrentStore.Id;
            try {
                var report = _orderReportService.ProfitReport(0,0,OrderStatus.Cancelled,null,null,null,null,null);
                return (int)report;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw new ApplicationException(ex.Message);
            } 
            
        }

        public decimal GetTotalPendingByReason(string reason)
        {
            switch (reason)
            {
                case "unpaid":
                    return _orderReportService.ProfitReport(0, 0, OrderStatus.Pending, PaymentStatus.Pending, null, null, null, null);
                case "not shipped":
                    return _orderReportService.ProfitReport(0, 0, OrderStatus.Pending, null, ShippingStatus.NotYetShipped, null, null, null);        
            }
            return 0;
        }

        public async Task<List<OrderDTO>> GetOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            string billingEmail = null, int pageIndex = 0, int pageSize = 20)
        {

            return await Task.Factory.StartNew(() =>
            {

                List<OrderDTO> result = new List<OrderDTO>();
                try
                {
                    IEnumerator<Order> orderIt = _orderService.SearchOrders(storeId, vendorId, customerId, productId, affiliateId,
                        null, null, os, ps, ss, billingEmail, null, pageIndex, pageSize).GetEnumerator();
                    for (IEnumerator<Order> i = orderIt; orderIt.MoveNext(); )
                    {
                        var current = orderIt.Current;
                        var orderDTO = new OrderDTO(current);
                        result.Add(orderDTO);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }).ConfigureAwait(false);
            
        }

        public OrderDTO GetOrderById(int id)
        {
            return new OrderDTO(_orderService.GetOrderById(id));
        }

        public List<CustomerDTO> GetCustomersByEmail(string Email)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, Email,null, null, null, 0, 0, null, null, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current);
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByUsername(string Username)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, Username, null, null, 0, 0, null, null, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current);
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByFirstname(string Firstname)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null,null,0,0,null,null,null,Firstname,null,0,0,null,null,null,false,null,0,Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByLastname(string Lastname)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, null, Lastname, 0, 0, null, null, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByFullname(string Fullname)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            string []names = Fullname.Split(' ');
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, names[0], names[names.Length-1], 0, 0, null, null, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByCompany(string Company)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null,null, null, 0, 0, Company, null, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByPhone(string Phone)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, null, null, 0, 0, null, Phone, null, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCustomersByPostalCode(string PostalCode)
        {
            List<CustomerDTO> result = new List<CustomerDTO>();
            var customers = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, null, null, 0, 0, null, null, PostalCode, false, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = customers.GetEnumerator();
            for (IEnumerator<Customer> i = custIt; custIt.MoveNext(); )
            {
                var current = i.Current;
                if (current.Email != null && current.GetFullName() != "")
                {
                    var CustomerDTO = new CustomerDTO(current); 
                    result.Add(CustomerDTO);
                }
            }
            return result;
        }

        public List<CustomerDTO> GetCurrentCarts(string Filter, string Email, int? LowerItems = 0 , int? HigherItems = 0, decimal? LowerTotal = 0, decimal? HigherTotal = 0, bool? Abandoned = false)
        {
            var result = new List<CustomerDTO>();
            var CustomersCarts = _customerService.GetAllCustomers(null, null, 0, 0, null, null, null, null, null, 0, 0, null, null, null, true, null, 0, Int32.MaxValue);
            IEnumerator<Customer> custIt = CustomersCarts.GetEnumerator();
                switch (Filter)
                {
                    case "email":
                        CustomersCarts = _customerService.GetAllCustomers(null, null, 0, 0, null, Email, null, null, null, 0, 0, null, null, null, false, null, 0, Int32.MaxValue);
                        foreach (Customer current in CustomersCarts.ToArray())
                        {
                            if (current.Email != null && current.GetFullName() != "" && current.ShoppingCartItems.Count !=0)
                            {
                                var CustomerDTO = new CustomerDTO(current);
                                result.Add(CustomerDTO);
                            }
                        }
                        break;
                    case "lower items":
                        for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            var QuantityTmp = 0;
                            for (int j = 0; j < C.ShoppingCartItems.Count; j++)
                            {
                                QuantityTmp += C.ShoppingCartItems.ElementAt(j).Quantity;
                            }
                            if (QuantityTmp > LowerItems)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                    case "higher items":
                        for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            var QuantityTmp = 0;
                            for (int j = 0; j < C.ShoppingCartItems.Count; j++)
                            {
                                QuantityTmp += C.ShoppingCartItems.ElementAt(j).Quantity;
                            }
                            if (QuantityTmp < HigherItems)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                    case "lower total":
                        for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            decimal ValueTmp = 0;
                            for (int j = 0; j < C.ShoppingCartItems.Count; j++)
                            {
                                ValueTmp += C.ShoppingCartItems.ElementAt(j).Product.Price;
                            }
                            if (ValueTmp > LowerTotal)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                    case "higher total":
                        for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            decimal ValueTmp = 0;
                            for (int j = 0; j < C.ShoppingCartItems.Count; j++)
                            {
                                ValueTmp += C.ShoppingCartItems.ElementAt(j).Product.Price;
                            }
                            if (ValueTmp < HigherTotal)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                    case "abandoned":
                    for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            if (!C.Active)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                    case "active":
                        for (var i = custIt; custIt.MoveNext(); )
                        {
                            var C = i.Current;
                            if (C.Active)
                            {
                                result.Add(new CustomerDTO(C));
                            }
                        }
                        break;
                }
            return result;
        }
        
        //public DataSet ExecuteDataSet(string[] sqlStatements, string usernameOrEmail, string userPassword)
        //{
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);

            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //var dataset = new DataSet();

            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    foreach (var sqlStatement in sqlStatements)
            //    {
            //        DataTable dt = dataset.Tables.Add();
            //        SqlDataAdapter adapter = new SqlDataAdapter();
            //        adapter.SelectCommand = new SqlCommand(sqlStatement, connection);
            //        adapter.Fill(dt);

            //    }
            //}

            //return dataset;




        //    return null;
        //}
        
        public Object ExecuteScalar(string sqlStatement, string usernameOrEmail, string userPassword)
        {
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);
            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //Object result;
            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    SqlCommand cmd = new SqlCommand(sqlStatement, connection);
            //    connection.Open();
            //    result = cmd.ExecuteScalar();

            //}

            //return result;



            return null;
        }
        
        public void ExecuteNonQuery(string sqlStatement, string usernameOrEmail, string userPassword)
        {
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);
            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    SqlCommand cmd = new SqlCommand(sqlStatement, connection);
            //    connection.Open();
            //    cmd.ExecuteScalar();
            //}
        }

        public List<OrderError> DeleteOrders(int[] ordersId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            var errors = new List<OrderError>();
            foreach (var order in GetOrderCollection(ordersId))
            {
                try
                {
                    _orderProcessingService.DeleteOrder(order);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return errors;
        }
       
        public List<OrderError> DeleteOrdersWithoutUser(int[] ordersId)
        {
            var errors = new List<OrderError>();
            foreach (var order in GetOrderCollection(ordersId))
            {
                try
                {
                    _orderProcessingService.DeleteOrder(order);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return errors;
        }

        public void ChangeOrderStatus(int id, OrderStatus status)
        {
            Order order = _orderService.GetOrderById(id);
            order.OrderStatus = status;
            _orderService.UpdateOrder(order);
        }

        public void ChangePaymentStatus(int id, PaymentStatus status)
        {
            Order order = _orderService.GetOrderById(id);
            order.PaymentStatus = status;
            _orderService.UpdateOrder(order);
        }

        public void ChangeShippingStatus(int id, ShippingStatus status)
        {
            Order order = _orderService.GetOrderById(id);
            order.ShippingStatus = status;
            _orderService.UpdateOrder(order);
        }

        public void AddCommentToCustomer(string email, string comment)
        {
            try{
                Customer Customer = _customerService.GetCustomerByEmail(email);
                Customer.AdminComment = comment;
                _customerService.UpdateCustomer(Customer);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }

        public void AddOrderNote(int orderId, string note, bool displayToCustomer)
        {

            try
            {
                var order = _orderService.GetOrderById(orderId);
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = note,
                    DisplayToCustomer = displayToCustomer,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
        
        public void UpdateOrderBillingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2,
            string city, string stateProvinceAbbreviation, string countryThreeLetterIsoCode, string postalCode, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                var a = order.BillingAddress;
                a.FirstName = firstName;
                a.LastName = lastName;
                a.PhoneNumber = phone;
                a.Email = email;
                a.FaxNumber = fax;
                a.Company = company;
                a.Address1 = address1;
                a.Address2 = address2;
                a.City = city;
                StateProvince stateProvince = null;
                if (!String.IsNullOrEmpty(stateProvinceAbbreviation))
                    stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceAbbreviation);
                a.StateProvince = stateProvince;
                Country country = null;
                if (!String.IsNullOrEmpty(countryThreeLetterIsoCode))
                    country = _countryService.GetCountryByThreeLetterIsoCode(countryThreeLetterIsoCode);
                a.Country = country;
                a.ZipPostalCode = postalCode;

                _addressService.UpdateAddress(a);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
        
        public void UpdateOrderShippingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2,
            string city, string stateProvinceAbbreviation, string countryThreeLetterIsoCode, string postalCode, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                var a = order.ShippingAddress;
                a.FirstName = firstName;
                a.LastName = lastName;
                a.PhoneNumber = phone;
                a.Email = email;
                a.FaxNumber = fax;
                a.Company = company;
                a.Address1 = address1;
                a.Address2 = address2;
                a.City = city;
                StateProvince stateProvince = null;
                if (!String.IsNullOrEmpty(stateProvinceAbbreviation))
                    stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceAbbreviation);
                a.StateProvince = stateProvince;
                Country country = null;
                if (!String.IsNullOrEmpty(countryThreeLetterIsoCode))
                    country = _countryService.GetCountryByThreeLetterIsoCode(countryThreeLetterIsoCode);
                a.Country = country;
                a.ZipPostalCode = postalCode;

                _addressService.UpdateAddress(a);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPending(int orderId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                Order order = _orderService.GetOrderById(orderId);
                order.PaymentStatus = PaymentStatus.Pending;
                _orderService.UpdateOrder(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPaid(int orderId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                _orderProcessingService.MarkOrderAsPaid(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPaidWithMethod(int orderId, string paymentMethodName, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                order.PaymentMethodSystemName = paymentMethodName;
                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentRefund(int orderId, bool offline, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                if (offline)
                {
                    _orderProcessingService.RefundOffline(order);
                }
                else
                {
                    var errors = _orderProcessingService.Refund(order);
                    if (errors.Count > 0)
                    {
                        throw new ApplicationException(errors[0]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }

        
        public List<OrderError> SetOrdersStatusCanceled(int[] ordersId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            var errors = new List<OrderError>();
            foreach (var order in GetOrderCollection(ordersId))
            {
                try
                {
                    _orderProcessingService.CancelOrder(order, true);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return errors;
        }
        
        #endregion
    }
}
