using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;
using Stripe;
using Stripe.FinancialConnections;
using StripeImplement.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace StripeImplement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext dbContext;
        public HomeController(ILogger<HomeController>logger, AppDbContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        public IActionResult Index(string msg="",string color="",string ChargeId="")
        {
            ViewBag.msg = msg;
            ViewBag.color = color;
            ViewBag.Chargeid = ChargeId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChargePayment(
    string stripeEmail,
    string stripeToken,
    string stripedescs = "",
    string workCharges = "",
    string paidBy = "")
        {
            long ChargableAmount = long.Parse(workCharges);
            ChargableAmount = 2;
            var ChargePaymentStatus = Charge(stripeEmail, stripeToken, ChargableAmount, stripedescs);

            if (!string.IsNullOrEmpty(ChargePaymentStatus))
            {
                var chargeId = ChargePaymentStatus;
                var CreatePaymentHistory = AddPaymentHistory(chargeId);
                var AddOrder = AddNewOrder(chargeId, ChargableAmount, "Card");
                var transferStatus = TransferFunds("acct_1NXiqEQxxqPzzOoK", ChargableAmount);

                if (transferStatus)
                {
                    return RedirectToAction("Index", "Home", new { msg = "Payment and Transfer Successful", color = "green", ChargeId = chargeId });
                }
                else
                {
                    return RedirectToAction("Index", "Home", new { msg = "Payment Successful, but Transfer Failed. Please check the logs.", color = "red", ChargeId = chargeId });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", new { msg = "Something Went Wrong ! Please Try Again!", color = "Red", ChargeId = ChargePaymentStatus });
            }
        }

        public string CreatePaymentIntent(string stripeEmail, string stripeToken, long stripeAmount = -1, string stripeDescription = "")
        {
            try
            {
                var paymentMethodService = new PaymentMethodService();
                var paymentIntentService = new PaymentIntentService();

                // Create a PaymentMethod using the token
                var paymentMethodCreateOptions = new PaymentMethodCreateOptions
                {
                    Type = "card",
                    Card = new PaymentMethodCardOptions
                    {
                        Token = stripeToken,
                    },
                };

                var paymentMethod = paymentMethodService.Create(paymentMethodCreateOptions);
                // Create a PaymentIntent with the connected account as the destination
                var paymentIntentCreateOptions = new PaymentIntentCreateOptions
                {
                    Amount = stripeAmount * 100,
                    Currency = "CAD",
                    PaymentMethodTypes = new List<string> { "card" },
                    PaymentMethod = paymentMethod.Id,
                    Confirm = true,
                    ConfirmationMethod = "manual",
                    Description = stripeDescription,
                    TransferData = new PaymentIntentTransferDataOptions
                    {
                        Destination = "acct_1NXIIkR86z456i2J", // Replace with the connected account ID
                    },
                    ReceiptEmail = stripeEmail,
                };

                var paymentIntent = paymentIntentService.Create(paymentIntentCreateOptions);

                if (paymentIntent.Status == "requires_action" && paymentIntent.NextAction.Type == "use_stripe_sdk")
                {
                    // Payment requires authentication, handle it in the frontend
                    // You can use the paymentIntent.ClientSecret to complete the payment in the frontend
                    return "requires_action";
                }

                if (paymentIntent.Status == "succeeded")
                {
                    return paymentIntent.Id;
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        public bool TransferFunds(string destinationAccountId, long amount)
        {
            try
            {
                StripeConfiguration.ApiKey = "sk_test_51LdJU1JGItIO6che6rYKSSzY2NEhOmMJtbUKUAxe1H95dl8oQPI6jWPmWHNBLfRsC8PdeqVi2TY1CFWjwsxWrlfp00D0eREv8W";
                // Perform the transfer to the connected account
                var transferCreateOptions = new TransferCreateOptions
                {
                    Amount = 2 * 100, // Convert amount to cents
                    Currency = "usd",
                    Destination = destinationAccountId,
                };

                var transferService = new TransferService();
                var transfer = transferService.Create(transferCreateOptions);

                return true; 
               
            }
            catch
            {
                return false;
            }
        }

        public string Charge(string stripeEmail, string stripeToken, long stripeAmount = -1, string stripeDescription = "")
        {
            try
            {
                var customerService = new CustomerService();
                var chargeService = new ChargeService();
                var serviceToken = new TokenService();
                var customer = customerService.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    Source = stripeToken,
                });
                var options = new ChargeCreateOptions
                {
                    Amount = stripeAmount * 100,
                    Currency = "CAD",
                    Description = stripeDescription,
                    Customer = customer.Id,
                };
                var charge = chargeService.Create(options);

                if (charge.Paid == true)
                {
                    return charge.Id;
                }

                return "";

            }
            catch
            {
                return "";

            }

        }

        public IActionResult RefundPayment(string chargeId)
        {
            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                Charge = chargeId,
            };

            var refund = refundService.Create(refundOptions);

            if (refund.Status == "succeeded")
            {
                return RedirectToAction("Index", "Home", new { msg = "Payment Refunded Successfully", color = "green" });
            }
            else
            {
                return RedirectToAction("Index", "Home", new { msg = "Refund Failed. Please Try Again!", color = "red" });
            }
        }

        #region Order

        public bool AddNewOrder(string ChargeId,long Amount,string paidby="")
        {
            try
            {
                // Create a new Order instance with dummy data
                var order = new Order
                {
                    OrderTitle = "My Order",
                    OrderDescription = "This is a dummy order.",
                    OrderPrice = Amount,
                    ChargeId = ChargeId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddHours(1),
                    paidBy = paidby,
                    IsActive = 1,
                    CreatedAt = DateTime.Now,
                    BuyerId = 2,
                    SellerId = 3
                };

                dbContext.Orders.Add(order);


                dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IActionResult AddOrderByPackage(int BuyerId=-1)
        {
            BuyerId = 3;
            var GetCustomerPackage=dbContext.Packages.Where(x=>x.BuyerId==BuyerId).FirstOrDefault();
            if(GetCustomerPackage!=null)
            {
                var AddOrder = AddNewOrder( "N/A", 25, "Package");
                GetCustomerPackage.PackagesCount = GetCustomerPackage.PackagesCount - 1;

                dbContext.Entry(GetCustomerPackage).State = EntityState.Modified;
                dbContext.SaveChanges();
                return RedirectToAction("Index", "Home", new { msg = "Your Order Placed Successfully", color = "green" });

            }


            return RedirectToAction("Index", "Home", new { msg = "Something Went Wrong", color = "red" });
            
        }

        #endregion

        public bool AddPaymentHistory(string ChargeId="")
        {
            try
            {
                // Create a new Order instance with dummy data
                PaymentHistory history = new PaymentHistory()
                {
                    ChargeId = ChargeId,
                    PaidAmount = "2",
                    CustomerId = 2,
                    ButlerId = null,
                    AdminId = null,
                    AmountReleased = "20",
                    ReleasedStatus = "pending",
                    OrderId = 1,
                };

                dbContext.PaymentHistory.Add(history);
                var RecordAdded = dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region payment history
       


        #endregion

        #region package

        public IActionResult BuyPackage(string msg="",string color="")
        {
            ViewBag.msg=msg;
            ViewBag.color=color;
            return View();
        }

        [HttpPost]
        public IActionResult AddPackage(string stripeEmail, string stripeToken, long stripeAmount = -1, string stripeDescription = "", string packageType="")
        {
            try
            {
                // Create a new Packages object with dummy data
                var package = new Packages
                {
                    PakageType = packageType,
                    SellerId = 2,
                    BuyerId = 3,
                    PackagesCount = 5,
                    IsActive = 1,
                    CreatedAt = DateTime.Now,
                    
                };
                if (packageType=="One Year")
                {
                    package.PackagesCount = 6;

                }
                else
                {
                    package.PackagesCount=12;
                }

                dbContext.Packages.Add(package);

                var AddPackage= dbContext.SaveChanges();

                if(AddPackage > 0)
                {
                    var ChargePayment = Charge(stripeEmail,stripeToken,stripeAmount,stripeDescription);
                    if(!string.IsNullOrEmpty(ChargePayment))
                    {
                        var CreatePaymentHistory = AddPaymentHistory(ChargePayment);

                        return RedirectToAction("BuyPackage", "Home", new { msg = "Your Package Is Activated Now You Can Use Your Free Sessions", color = "green" });
                    }

                }
                return RedirectToAction("BuyPackage", "Home", new { msg = "Something went wrong Please try again!", color = "red" });

            }
            catch (Exception ex)
            {
                // Handle any exceptions if needed
                return RedirectToAction("BuyPackage", "Home", new { msg = "Something went wrong Please try again!", color = "red" });

            }
        }
        #endregion

    }
}