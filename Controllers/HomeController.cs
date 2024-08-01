using _Helper;
using AutoPartsHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoPartsHub.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly AutoPartsHubContext _context;
		public HomeController(ILogger<HomeController> logger , AutoPartsHubContext context)
		{
			_logger = logger;
            _context = context;
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


        [Route("gallery")]
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

        // GET: Checkout
        [HttpGet("Checkout")]
        public IActionResult Checkout()
        {
          
            ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName");
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName");
            ViewData["CityId"] = new SelectList(_context.TblCities, "CityId", "CityName");
            ViewData["CountryId"] = new SelectList(_context.TblCountries, "CountryId", "CountryName");
            ViewData["ProvinceId"] = new SelectList(_context.TblProvinces, "ProvinceId", "ProvinceName");
            ViewData["StatusId"] = new SelectList(_context.TblStatuses, "StatusId", "StatusName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("OrderId,Email,PhoneNo,CountryId,ProvinceId,CityId,PostelCode ,CityName,CountryName,ProvinceName,DeliveryAddress,GrandTotal ")] TblOrdersMain tblOrdersMain)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(tblOrdersMain);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Success");
                }

                ViewData["ItemId"] = new SelectList(_context.TblItems, "ItemId", "ItemName", tblOrdersMain.ItemId);
                ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName", tblOrdersMain.UserId);
                ViewData["CityId"] = new SelectList(_context.TblCities, "CityId", "CityName", tblOrdersMain.CityId);
                ViewData["CountryId"] = new SelectList(_context.TblCountries, "CountryId", "CountryName", tblOrdersMain.CountryId);
                ViewData["ProvinceId"] = new SelectList(_context.TblProvinces, "ProvinceId", "ProvinceName", tblOrdersMain.ProvinceId);
                ViewData["StatusId"] = new SelectList(_context.TblStatuses, "StatusId", "StatusName", tblOrdersMain.StatusId);
                return View(tblOrdersMain);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(tblOrdersMain);
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
