using EBook.Model;
using EBook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EBook.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly Data_Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegistrationController(Data_Context context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Route("Registration/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                if (await _context.RegistrationTables.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }


                // Create a new user object
                var newUser = new RegistrationTable
                {
                    FullName = model.FullName,
                    Address = model.Address,
                    Mobile = model.Mobile,
                    Username = model.Username,
                    Email = model.Email,
                    //Pincode = model.Pincode,
                    Password = model.Password, // ⚠️ Hash in production
                    CreatedAt = DateTime.UtcNow,
                    IsAdmin = false
                };

                // Save user to the database
                _context.RegistrationTables.Add(newUser);
                await _context.SaveChangesAsync();
                TempData["newid"] = newUser.Id;
                HttpContext.Session.SetInt32("UserId", newUser.Id);
                HttpContext.Session.SetString("Username", newUser.Username);

                return RedirectToAction("Home", "User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving data: " + ex.Message);
                return View(model);
            }
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)

        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.RegistrationTables
    .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }


            TempData["newid"] = user.Id;
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetInt32("RegistrationId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

             //✅ Load Wishlist from DB and store in session
            var wishlistItems = _context.Wishlists
                .Include(w => w.Book)
                .Where(w => w.RegistrationId == user.Id)
                .Select(w => new BookWishlistViewModel
                {
                    BookId = w.Book.Id,
                    BookName = w.Book.BookName,
                    AuthorName = w.Book.AuthorName,
                    Image = w.Book.Image ?? "noimage.jpg",
                    Price = w.Book.Price
                })
                .ToList();

            HttpContext.Session.SetString("Wishlist", JsonSerializer.Serialize(wishlistItems));
            HttpContext.Session.SetInt32("WishlistCount", wishlistItems.Count);

            // ✅ Load Cart from DB and store in session (if you have a Cart table)
            var cartItems = _context.Carts
      .Include(c => c.Book)
      .Where(c => c.RegistrationId == user.Id)
      .Select(c => new CartViewModel
      {
          BookId = c.Book.Id,
          BookName = c.Book.BookName,
          AuthorName = c.Book.AuthorName,
          Image = c.Book.Image ?? "noimage.jpg",
          Price = (decimal)c.Book.Price,
          Quantity = c.Quantity
      })
      .ToList();

            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cartItems));
            HttpContext.Session.SetInt32("CartCount", cartItems.Count);

            if (user.IsAdmin)
            {
                HttpContext.Session.SetString("Admin", "Admin");
              
                return RedirectToAction("Admin", "Admin");
            }

            Console.WriteLine("Redirecting to User Home...");
            return RedirectToAction("Home", "User");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Home", "User");
        }

      
    }
}