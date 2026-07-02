using EBook.Model;
using EBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBook.Controllers
{
    public class ProfileController : Controller
    {
        private readonly Data_Context _context;
        public ProfileController(Data_Context context)
        {
            _context = context;
        }
        public IActionResult UserDetails()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Registration"); // Redirect if the user is not logged in
            }

            var user = _context.RegistrationTables.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User details not found.");
            }
            return View(user); // Pass user details to the view
        }
        private RegistrationTable? GetLoggedInUser()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return (null);

            var registration = _context.RegistrationTables.FirstOrDefault(u => u.Id == userId);

            return (registration);
        }
        public IActionResult Editprofile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Registration");
            }
            var registration = GetLoggedInUser();
            if (registration == null)
            {
                return RedirectToAction("Login", "Registration");
            }
            var model = new EditProfileViewModel
            {
                FullName = registration.FullName,
                Username = registration.Username,
                Email = registration.Email,
                PhoneNumber = registration.Mobile,
                Address = registration.Address,
                Pincode = registration.Pincode
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            var registration = GetLoggedInUser();
            if (registration == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            if (ModelState.IsValid)
            {
                // Updating RegistrationTable
                registration.FullName = model.FullName;
                registration.Username = model.Username;
                registration.Email = model.Email;
                registration.Mobile = model.PhoneNumber;
                registration.Address = model.Address;
                registration.Pincode = model.Pincode;

                _context.SaveChanges(); // Save changes in both tables
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("EditProfile");
            }

            return View(model);
        }
        public async Task<IActionResult> PurchaseHistory()
        {
            int? registrationId = HttpContext.Session.GetInt32("RegistrationId");

            if (registrationId == null)
            {
                ViewBag.Message = "User session expired. Please log in again.";
                return View(new List<OrderWithBookViewModel>());
            }

            var orders = await _context.Orders
                .Include(o => o.Books)
                    .ThenInclude(b => b.Genre) // Include Genre from Book
                .Where(o => o.RegistrationId == registrationId && !o.IsDelete)
                .Select(o => new OrderWithBookViewModel
                {
                    OrderId = o.Id,
                    CreateDate = o.CreateDate,
                    IsOrderStatus = o.IsOrderStatus,
                    TotalAmount = (decimal)o.Books.Price, // Or update based on quantity if needed
                    Books = new List<BookInOrderViewModel>
                    {
                new BookInOrderViewModel
                {
                    BookTitle = o.Books.BookName,
                    Genre = o.Books.Genre.GenreName, // Get Genre Name
                    Image = o.Books.Image ?? "noimage.jpg",
                    UnitPrice = (decimal)o.Books.Price,
                    Quantity = 1 // Replace with actual quantity if available
                }
                    }
                }).ToListAsync();

            return View(orders);
        }
    }
}
