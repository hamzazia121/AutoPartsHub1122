using _Helper;
using AutoPartsHub.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoPartsHub._Helper;

namespace AutoPartsHub.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly AutoPartsHubContext _context;
        private readonly IMailService _mailService;
		public HomeController(ILogger<HomeController> logger,IMailService mailService, AutoPartsHubContext context)
		{
			_logger = logger;
            _context = context;
            _mailService = mailService;
		}

	
        public async Task<IActionResult> Index()
        {
            var items = await _context.TblItems.Include(x=>x.TblItemImages).ToListAsync();
            return View(items);
        }
        [Route("privacy")]
        public IActionResult Privacy()
		{
			return View();
		}

		[Route("about")]
		public IActionResult About()
		{
			return View();
		}
        [Route("shop")]
        public async Task<IActionResult> Shop()
        {
            var items = await _context.TblItems.Include(t => t.TblItemImages).ToListAsync();
            return View(items);
        }

        //public async Task<IActionResult> Shop()
        //public IActionResult Shop()
        //{
        //    //var items = await _context.GetAllItemsAsync();
        //    //return View(items);
        //    return View();
        //}
        [Route("blog")]
        public IActionResult Blog()
        {
            return View();
        }


     
        public IActionResult Gallery()
        {
            return View();
        }   


        //[Route("Pages")]
        public async Task<IActionResult> Cart()
        {

           
            List<TblItem> tblItems=new List<TblItem>();

            if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["AutoHubCart"]))
            {
                var data = HttpContext.Request.Cookies["AutoHubCart"];
                var DecriptData = Protection.Decrypt(data);

                ListCartModel listCartModel = JsonConvert.DeserializeObject<ListCartModel>(DecriptData);

                if (listCartModel != null && listCartModel.Carts.Count > 0)
                {
                    List<int> ProductIds=listCartModel.Carts.Select(x=>x.ProductId).ToList();   

                    tblItems = (await _context.TblItems
                                      .Where(x => x.MDelete == false || x.MDelete == null)
                                      .Where(x => ProductIds.Contains(x.ItemId))
                                      .Include(t => t.Brand).Include(t => t.TblItemImages)
                                      .ToListAsync());

                    foreach (var item in tblItems)
                    {
                        var cartItem = listCartModel.Carts.FirstOrDefault(c => c.ProductId == item.ItemId);
                        if (cartItem != null)
                        {
                            item.Quantity = cartItem.Quantity;
                        }
                    }


                }
                else
                {

                }
            }
           
            

            ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName");

            return View(tblItems);
        }
        public IActionResult Orderconfirm() { 
        return View();
        }
        [Route("GetCartCount")]

        public async Task<IActionResult> GetCartCount()
        {
            try
            {
               
                int itemCount = 0;
                if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["AutoHubCart"]))
                {
                    var data = HttpContext.Request.Cookies["AutoHubCart"];
                    var DecriptData = Protection.Decrypt(data);

                    ListCartModel listCartModel = JsonConvert.DeserializeObject<ListCartModel>(DecriptData);

                    itemCount = listCartModel.Carts.Count();
                }


                return Json(new { success = true, message = itemCount });

              
            }
            catch (Exception exp)
            {
                return Json(new {success = false,message=exp.Message});
            }
           
            
        }

        public async Task<IActionResult> ConfirmOrder()
        {
            var orderDetails = await _context.TblOrderDetails
                                           
                                             .Include(od => od.Item)
                                             .ThenInclude(item => item.TblItemImages)
                                             .ToListAsync();

            ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName");

            return View(orderDetails);
        }

        public IActionResult ContactUS()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUS([Bind("ContectUsId,ContectUsName,ContectUsEmail,ContectUsPhoneNo,ContectUsSubject,ContectUsMassage,mDelete")] TblContectU tblContact)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(tblContact);
                    await _context.SaveChangesAsync();

                }


                return View();
            }
            catch (Exception exp)
            {

                ViewBag.Message = exp.Message;
                return View(tblContact);
            }

        }
        [Route("itemDetail")]
        public async Task<IActionResult> ItemDetail(int id)
        {
            var item = await _context.TblItems.Include(t => t.TblOrderDetails).Include(t => t.TblItemImages)
                                              .FirstOrDefaultAsync(x => x.ItemId == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                List<TblItem> tblItems = new List<TblItem>();

                if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["AutoHubCart"]))
                {
                    var data = HttpContext.Request.Cookies["AutoHubCart"];
                    var DecriptData = Protection.Decrypt(data);

                    ListCartModel listCartModel = JsonConvert.DeserializeObject<ListCartModel>(DecriptData);

                    if (listCartModel != null && listCartModel.Carts.Count > 0)
                    {
                        List<int> ProductIds = listCartModel.Carts.Select(x => x.ProductId).ToList();

                        tblItems = (await _context.TblItems
                                          .Where(x => x.MDelete == false || x.MDelete == null)
                                          .Where(x => ProductIds.Contains(x.ItemId))
                                          .Include(t => t.Brand).Include(t => t.TblItemImages)
                                          .ToListAsync());

                        foreach (var item in tblItems)
                        {
                            var cartItem = listCartModel.Carts.FirstOrDefault(c => c.ProductId == item.ItemId);
                            if (cartItem != null)
                            {
                                item.Quantity = cartItem.Quantity;
                            }
                        }


                    }
                    else
                    {

                    }
                }



                var orders = new TblOrdersMain
                {
                    Item = tblItems
                };


                ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName");
                ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName");
                ViewData["CityId"] = new SelectList(_context.TblCities, "CityId", "CityName");
                ViewData["CountryId"] = new SelectList(_context.TblCountries, "CountryId", "CountryName");
                ViewData["ProvinceId"] = new SelectList(_context.TblProvinces, "ProvinceId", "ProvinceName");
                ViewData["StatusId"] = new SelectList(_context.TblStatuses, "StatusId", "StatusName");

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Checkout");
                ViewBag.Message = ex.Message;
                return View();
            }
        }


        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("OrderId,ItemId,UserId,UserName,GrandTotal,OrderDate,ItemName,Email,PhoneNo,CountryId,ProvinceId,CityId,PostelCode,CityName,CountryName,ProvinceName,DeliveryAddress,PaidAmount,PaymentId,PaymentType,Remarks,ShippingAmount,Status")] TblOrdersMain tblOrdersMain)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (ModelState.IsValid)
                    {
                        tblOrdersMain.OrderDate = DateTime.Now;
                        tblOrdersMain.StatusId = 1;
                        tblOrdersMain.PaymentId = 1;
                        tblOrdersMain.ShippingAmount = 100;
                        tblOrdersMain.GrandTotal = 12;

                        _context.Add(tblOrdersMain);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Success");
                    }
                }
                else
                {
                    var user = await _context.TblUsers
                        .Where(x => x.Email == tblOrdersMain.Email)
                        .FirstOrDefaultAsync();

                    if (user == null || user.UserId <= 0)
                    {
                        var newUser = new TblUser
                        {
                            UserName = "Hamza Zia",
                            Email = tblOrdersMain.Email,
                            Password = GeneratePassword.GenerateRandomPassword(10),
                            PhoneNumber = tblOrdersMain.PhoneNo,
                            RollId = 2 // Set role to 2 for non-admin users
                        };



                        _context.TblUsers.Add(newUser);
                        await _context.SaveChangesAsync();

                        var reciver = tblOrdersMain.Email;
                        var subject = $"Welcome to AutoPartsHub";
                        var message = $"Your Login Password is {newUser.Password} and your Id is {newUser.UserId}";
                        if (!IsValidEmail(reciver))
                        {
                            return Json(new { success = false, error = "Invalid email address" });
                        }

                        try
                        {
                            await _mailService.SendMailAsync(reciver, subject, message);
                            var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, newUser.UserName),
                            new Claim(ClaimTypes.Email, newUser.Email),
                            new Claim(ClaimTypes.Role, "Customer"),
                            new Claim("RoleId", newUser.RollId.ToString()),
                         };

                            var claimsIdentity = new ClaimsIdentity(
                                claims, CookieAuthenticationDefaults.AuthenticationScheme);



                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity));
                            return Json(new { success = true });

                        }
                        catch (Exception ex)
                        {
                            // Log the exception message
                            Console.WriteLine($"Error sending email: {ex.Message}");
                            return Json(new { success = false, error = "Failed to send email" });
                        }
                    }
                    else
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Roll.RollName),
                            new Claim("RoleId", user.RollId.ToString()),
                                       };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);



                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        return Json(new { success = true });
                    }



                }

                return View(tblOrdersMain);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(tblOrdersMain);
            }
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActionResult> WishList()
        {
            var orderDetails = await _context.TblOrderDetails
                                            .Include(od => od.Item)
                                            .ThenInclude(item => item.TblItemImages)
                                            .ToListAsync();

            ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName");

            return View(orderDetails);
        }

    
        [Route("addToCart")]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int itemId, int quantity)
        {

            try
            {
                CookieOptions cookieOptions= new CookieOptions();
                cookieOptions.Secure = true;
                cookieOptions.HttpOnly = true;
                cookieOptions.Expires=DateTime.Now.AddDays(30);
                cookieOptions.IsEssential = true;

                if (string.IsNullOrEmpty(HttpContext.Request.Cookies["AutoHubCart"]))
                {
                    CartModel cartModel = new CartModel()
                    {
                        ProductId = itemId,
                        Quantity = quantity 
                    };

                    ListCartModel listCartModel = new ListCartModel();
                    listCartModel.Carts.Add(cartModel);

                     string JsonData=JsonConvert.SerializeObject(listCartModel);

                    var ProtectedData=Protection.Encrypt(JsonData);

                    HttpContext.Response.Cookies.Append("AutoHubCart", ProtectedData, cookieOptions);
                }
                else
                {
                    var data = HttpContext.Request.Cookies["AutoHubCart"];

                    var DecriptData=Protection.Decrypt(data);

                    ListCartModel listCartModel=JsonConvert.DeserializeObject<ListCartModel>(DecriptData);

                    if (listCartModel != null && listCartModel.Carts.Count > 0)
                    {
                        if (listCartModel.Carts.Any(x => x.ProductId == itemId))
                        {
                            return Json(new { success = false, message = "Item  Already in the cart" });

                        }
                        else
                        {
                            listCartModel.Carts.Add(new CartModel() 
                            {
                                ProductId= itemId,
                                Quantity = quantity
                            });


                            string JsonData = JsonConvert.SerializeObject(listCartModel);

                            var ProtectedData = Protection.Encrypt(JsonData);

                            HttpContext.Response.Cookies.Append("AutoHubCart", ProtectedData, cookieOptions);
                        
                         }
                    }
                    else
                    {
                        CookieOptions cookieOptionsNew = new CookieOptions();
                        cookieOptionsNew.Secure = true;
                        cookieOptionsNew.HttpOnly = true;
                        cookieOptionsNew.Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Response.Cookies.Append("AutoHubCart", "", cookieOptionsNew);
                        HttpContext.Response.Cookies.Delete("AutoHubCart");
                        throw new Exception("Some things went wrong");

                    }

                }

                return Json(new { success = true, message = "Item added to cart successfully" });
            }
            catch (Exception exp)
            {

                return Json(new { success = false, message = exp.Message });

            }



        }






      
        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["AutoHubCart"]))
                {
                    var data = HttpContext.Request.Cookies["AutoHubCart"];
                    var DecriptData = Protection.Decrypt(data);

                    ListCartModel listCartModel = JsonConvert.DeserializeObject<ListCartModel>(DecriptData);

                    var cartData = listCartModel.Carts.FirstOrDefault(x => x.ProductId == id);

                    listCartModel.Carts.Remove(cartData);

                    CookieOptions cookieOptions = new CookieOptions();
                    cookieOptions.Secure = true;
                    cookieOptions.HttpOnly = true;
                    cookieOptions.Expires = DateTime.Now.AddDays(30);
                    cookieOptions.IsEssential = true;


                    string JsonData = JsonConvert.SerializeObject(listCartModel);

                    var ProtectedData = Protection.Encrypt(JsonData);

                    HttpContext.Response.Cookies.Append("AutoHubCart", ProtectedData, cookieOptions);


                }

                ViewBag.SuccessMsg = "Item Removed Successfully";

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception exp)
            {
                ViewBag.ErrorMsg=exp.Message;
                return RedirectToAction(nameof(Cart));
            }


        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
