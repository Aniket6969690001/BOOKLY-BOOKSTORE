using EBook.Model;
using EBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;
using System.Data.Common;

namespace EBook.Controllers
{
    public class UserController : Controller
    {
        private readonly Data_Context _context;
        public UserController(Data_Context context)
        {
            _context = context;
        }
        public IActionResult Home(string searchString)
        {
            try
            {
                // Fetch all books from the database
                var allBooks = _context.Books.ToList();

                // If a search string is provided, filter the books
                if (!string.IsNullOrEmpty(searchString))
                {
                    allBooks = allBooks.Where(b =>
                        b.BookName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        b.AuthorName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                //if (!string.IsNullOrEmpty(searchString))
                //{
                //    allBooks = allBooks.Where(b =>
                //        b.BookName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                //        b.AuthorName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                //        (b.Genre != null) ).ToList();
                //}

                // Separate the books into BestSelling and Recommended categories
                ViewData["BestSellingBooks"] = allBooks.Where(b => b.IsBestSelling == true).ToList();
                ViewData["RecommendedBooks"] = allBooks.Where(b => b.IsRecommended == true).ToList();

                // Return the filtered list of all books
                return View(allBooks);
            }
            catch (Exception ex)
            {
                // Log and handle any errors
                Debug.WriteLine($"Error in Home action: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new { results = new List<object>() });
            }

            var books = await _context.Books
                .Where(b => b.BookName.Contains(query) || b.AuthorName.Contains(query))
                .Select(b => new
                {
                    b.Id,
                    b.BookName,
                    b.AuthorName,
                    b.Description,
                    b.Image,
                    b.Price
                })
                .ToListAsync();

            return Json(new { results = books });


            //if (string.IsNullOrEmpty(query))
            //    return Json(new { results = new string[] { } });

            //var books = await _context.Books
            //    .Where(b => b.BookName.Contains(query) || b.AuthorName.Contains(query))
            //    .Select(b => new { b.Id, b.BookName })
            //    .ToListAsync();

            //return Json(new { results = books });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookDetails(int id)
        {

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound();

            return PartialView("_BookDetailsPartial", book);
        }

        public IActionResult Wishlist()
        {
            int? registrationId = HttpContext.Session.GetInt32("UserId");

            if (registrationId == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            var wishlistJson = HttpContext.Session.GetString("Wishlist");
            var wishlist = string.IsNullOrEmpty(wishlistJson)
                ? new List<BookWishlistViewModel>()
                : JsonSerializer.Deserialize<List<BookWishlistViewModel>>(wishlistJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<BookWishlistViewModel>();

            return View(wishlist);
        }
        //var wishlist = new List<BookWishlistViewModel>();

        //if (!string.IsNullOrEmpty(wishlistJson))
        //{
        //    wishlist = JsonSerializer.Deserialize<List<BookWishlistViewModel>>(wishlistJson, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    }) ?? new List<BookWishlistViewModel>();
        //}
        //return View(wishlist);

        [HttpPost]
        public IActionResult AddToWishlist(int id)
        {
            int? registrationId = HttpContext.Session.GetInt32("UserId");

            if (registrationId == null)
            {
                return Json(new { success = false, message = "Please login to add to wishlist." });
            }

            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                return Json(new { success = false, message = "Book not found" });
            }

            // Session-based wishlist
            var wishlistJson = HttpContext.Session.GetString("Wishlist");
            var wishlist = string.IsNullOrEmpty(wishlistJson)
                ? new List<BookWishlistViewModel>()
                : JsonSerializer.Deserialize<List<BookWishlistViewModel>>(wishlistJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<BookWishlistViewModel>();

            if (!wishlist.Any(w => w.BookId == id))
            {
                wishlist.Add(new BookWishlistViewModel
                {
                    BookId = book.Id,
                    BookName = book.BookName,
                    AuthorName = book.AuthorName,
                    Image = book.Image ?? "noimage.jpg",
                    Price = book.Price
                });

                HttpContext.Session.SetString("Wishlist", JsonSerializer.Serialize(wishlist));
                HttpContext.Session.SetInt32("WishlistCount", wishlist.Count);
            }

            Wishlist? existingWishlistItem = null;

            try
            {
                existingWishlistItem = _context.Wishlists
                    .FirstOrDefault(w => w.BookId == id && w.RegistrationId == registrationId);
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error while checking existing wishlist item: {ex.Message}");
                //return Json(new { success = false, message = "Something went wrong while accessing wishlist." });
            }


            if (existingWishlistItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    BookId = book.Id,
                    RegistrationId = registrationId.Value
                };

                _context.Wishlists.Add(wishlistItem);
                _context.SaveChanges();
            }

            return Json(new { success = true, bookName = book.BookName, wishlistCount = wishlist.Count });
        }

        public IActionResult GetWishlistCount()
        {
            var wishlistJson = HttpContext.Session.GetString("Wishlist");
            var wishlist = string.IsNullOrEmpty(wishlistJson)
                ? new List<BookWishlistViewModel>()
                : JsonSerializer.Deserialize<List<BookWishlistViewModel>>(wishlistJson) ?? new List<BookWishlistViewModel>();

            return Json(new { count = wishlist.Count });
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist(int id)
        {
            int? registrationId = HttpContext.Session.GetInt32("UserId");

            if (registrationId == null)
            {
                return Json(new { success = false, message = "Please login to remove from wishlist." });
            }

            // Remove from session-based wishlist
            var wishlistJson = HttpContext.Session.GetString("Wishlist");
            var wishlist = string.IsNullOrEmpty(wishlistJson)
                ? new List<BookWishlistViewModel>()
                : JsonSerializer.Deserialize<List<BookWishlistViewModel>>(wishlistJson) ?? new List<BookWishlistViewModel>();

            var itemToRemove = wishlist.FirstOrDefault(w => w.BookId == id);
            if (itemToRemove != null)
            {
                wishlist.Remove(itemToRemove);
                HttpContext.Session.SetString("Wishlist", JsonSerializer.Serialize(wishlist));
            }

            HttpContext.Session.SetInt32("WishlistCount", wishlist.Count);

            // Remove from database wishlist
            var wishlistItem = _context.Wishlists.FirstOrDefault(w => w.BookId == id && w.RegistrationId == registrationId);
            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                _context.SaveChanges();
            }

            return Json(new { success = true, wishlistCount = wishlist.Count });
        }

        public IActionResult Cart()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            return View(cartItems);
        }

       public IActionResult AddToCart(int id)
        {
            Console.WriteLine($"Received Book ID in AddToCart: {id}"); // Debugging line

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Please login to add items to cart." });
            }

            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
            {
                Console.WriteLine("Book not found in the database."); // Debugging line
                return Json(new { success = false, message = "Book not found" });
            }

            // Process the session cart
            var cartJson = HttpContext.Session.GetString("Cart");
            var sessionCart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            var existingSessionItem = sessionCart.FirstOrDefault(c => c.BookId == id);
            if (existingSessionItem != null)
            {
                existingSessionItem.Quantity++;
            }
            else
            {
                sessionCart.Add(new CartItem
                {
                    BookId = book.Id,
                    BookName = book.BookName,
                    AuthorName = book.AuthorName,
                    Image = book.Image ?? "noimage.jpg",
                    Price = book.Price,
                    Quantity = 1
                });
            }

            // Update session cart
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(sessionCart));
            int cartCount = sessionCart.Sum(item => item.Quantity);
            HttpContext.Session.SetInt32("CartCount", cartCount);

            // Update database cart
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    connection.Open();

                    // First check if the item exists in the cart
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM Cart WHERE RegistrationId = @userId AND BookId = @bookId";

                        var userIdParam = checkCommand.CreateParameter();
                        userIdParam.ParameterName = "@userId";
                        userIdParam.Value = userId.Value;

                        var bookIdParam = checkCommand.CreateParameter();
                        bookIdParam.ParameterName = "@bookId";
                        bookIdParam.Value = id;

                        checkCommand.Parameters.Add(userIdParam);
                        checkCommand.Parameters.Add(bookIdParam);

                        var itemExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                        using (var command = connection.CreateCommand())
                        {
                            if (itemExists)
                            {
                                // Update existing item
                                command.CommandText = @"
                            UPDATE Cart 
                            SET Quantity = Quantity + 1, AddedDate = @addedDate 
                            WHERE RegistrationId = @userId AND BookId = @bookId";
                            }
                            else
                            {
                                // Insert new item
                                command.CommandText = @"
                            INSERT INTO Cart (RegistrationId, BookId, Quantity, AddedDate)
                            VALUES (@userId, @bookId, 1, @addedDate)";
                            }

                            var updateUserIdParam = command.CreateParameter();
                            updateUserIdParam.ParameterName = "@userId";
                            updateUserIdParam.Value = userId.Value;

                            var updateBookIdParam = command.CreateParameter();
                            updateBookIdParam.ParameterName = "@bookId";
                            updateBookIdParam.Value = id;

                            var addedDateParam = command.CreateParameter();
                            addedDateParam.ParameterName = "@addedDate";
                            addedDateParam.Value = DateTime.Now;

                            command.Parameters.Add(updateUserIdParam);
                            command.Parameters.Add(updateBookIdParam);
                            command.Parameters.Add(addedDateParam);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine($"Book with ID {id} successfully added to cart for user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating cart in database: {ex.Message}");
                // Don't return error to user, just log it - the session cart will still work
            }

            return Json(new { success = true, cartCount = cartCount });
        }
        
        public IActionResult GetCartCount()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return Json(new { count = 0 });

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            int count = cart.Sum(item => item.Quantity);

            return Json(new { count = count });
        }

        [HttpPost]
        public IActionResult UpdateCartQuantity(int id, bool isIncrease)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Please login" });
            }

            try
            {
                // Get cart from session
                var cartJson = HttpContext.Session.GetString("Cart");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var item = cart.FirstOrDefault(c => c.BookId == id);
                if (item == null)
                {
                    return Json(new { success = false, message = "Book not found in cart." });
                }

                if (isIncrease)
                {
                    item.Quantity++;
                }
                else
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                    {
                        cart.Remove(item);
                    }
                }

                // Save updated cart back to session
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

                // Update database accordingly
                using (var connection = _context.Database.GetDbConnection())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.Parameters.Clear();
                        if (item.Quantity > 0)
                        {
                            command.CommandText = @"
                        UPDATE Cart 
                        SET Quantity = @quantity, AddedDate = @addedDate 
                        WHERE RegistrationId = @userId AND BookId = @bookId";

                            command.Parameters.Add(CreateParam(command, "@quantity", item.Quantity));
                            command.Parameters.Add(CreateParam(command, "@addedDate", DateTime.Now));
                            command.Parameters.Add(CreateParam(command, "@userId", userId.Value));
                            command.Parameters.Add(CreateParam(command, "@bookId", id));
                        }
                        else
                        {
                            command.CommandText = @"
                        DELETE FROM Cart 
                        WHERE RegistrationId = @userId AND BookId = @bookId";

                            command.Parameters.Add(CreateParam(command, "@userId", userId.Value));
                            command.Parameters.Add(CreateParam(command, "@bookId", id));
                        }

                        command.ExecuteNonQuery();
                    }
                }

                var cartHtml = RenderCartHtml(cart);
                var totalPrice = cart.Sum(c => c.Price * c.Quantity);
                var cartCount = cart.Sum(c => c.Quantity);
                return Json(new
                {
                    success = true,
                    updatedItem = new
                    {
                        bookId = item.BookId,
                        quantity = item.Quantity,
                        price = item.Price
                    },
                    totalPrice,
                    cartCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating cart: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        private DbParameter CreateParam(DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            return param;
        }
        private string RenderCartHtml(List<CartItem> cart)
            {
                var html = new StringBuilder();
                for (int i = 0; i < cart.Count; i++)
                {
                    var item = cart[i];
                    html.Append($@"
                <tr class='text-center align-middle'>
                    <td>{i + 1}</td>
                    <td><img src='/images/{item.Image}' alt='{item.BookName}' class='cart-img' /></td>
                    <td>{item.BookName}</td>
                    <td>{item.AuthorName}</td>
                    <td>
                        <div class='quantity-control'>
                            <button class='btn btn-sm btn-primary decrease-btn' data-id='{item.BookId}'>−</button>
                            {item.Quantity}
                            <button class='btn btn-sm btn-primary increase-btn' data-id='{item.BookId}'>+</button>
                        </div>
                    </td>
                    <td>₹{item.Price}</td>
                    <td>₹{item.Price * item.Quantity}</td>
                    <td>
                        <button class='btn btn-sm btn-danger remove-from-cart-btn' data-id='{item.BookId}'>Remove</button>
                    </td>
                </tr>");
                }
                return html.ToString();
            }

        [HttpPost]
        public JsonResult RemoveFromCart(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            var cartJson = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);

            var item = cart.FirstOrDefault(c => c.BookId == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }

            var totalPrice = cart.Sum(b => b.Price * b.Quantity);
            var cartCount = cart.Sum(item => item.Quantity);

            if (userId.HasValue)
            {
                try
                {
                    using (var connection = _context.Database.GetDbConnection())
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM Cart WHERE RegistrationId = @userId AND BookId = @bookId";

                            var userIdParam = command.CreateParameter();
                            userIdParam.ParameterName = "@userId";
                            userIdParam.Value = userId.Value;

                            var bookIdParam = command.CreateParameter();
                            bookIdParam.ParameterName = "@bookId";
                            bookIdParam.Value = id;

                            command.Parameters.Add(userIdParam);
                            command.Parameters.Add(bookIdParam);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing from database: {ex.Message}");
                }
            }

            return Json(new { success = true, cartItems = cart, totalPrice = totalPrice, cartCount = cartCount });
        }

        public IActionResult Checkout()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            var user = _context.RegistrationTables.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutModel
            {
                FullName = user.FullName ?? user.Username,
                Email = user.Email,
                PhoneNumber = user.Mobile,
                Address = user.Address,
                PostalCode = user.Pincode,
                CartItems = GetCartItems(userId.Value),
                TotalAmount = CalculateTotal(userId.Value)  // This is calculating the total
            };

            return View(model);
        }
        private List<CartItem> GetCartItems(int userId)
        {
            var query = from cart in _context.Carts
                        join book in _context.Books on cart.BookId equals book.Id
                        where cart.RegistrationId == userId
                        select new CartItem
                        {
                            BookId = book.Id,
                            BookName = book.BookName,
                            AuthorName = book.AuthorName,
                            Price = book.Price,
                            Quantity = cart.Quantity,
                            Image = book.Image
                        };

            return query.ToList();
        }
        private double CalculateTotal(int userId)
        {
            var total = (from cart in _context.Carts
                         join book in _context.Books on cart.BookId equals book.Id
                         where cart.RegistrationId == userId
                         select cart.Quantity * book.Price).Sum();

            return total;
        }

        public IActionResult PlaceOrder()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Please login to place an order.";
                    return RedirectToAction("Login", "Registration");
                }

                var registration = _context.RegistrationTables.FirstOrDefault(r => r.Id == userId);
                if (registration == null)
                {
                    TempData["ErrorMessage"] = "User registration not found.";
                    return RedirectToAction("Login", "Registration");
                }

                string fullName = Request.Form["FullName"];
                string email = Request.Form["Email"];
                string phone = Request.Form["PhoneNumber"];
                string address = Request.Form["Address"];
                string city = Request.Form["City"];

                if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(fullName))
                {
                    TempData["ErrorMessage"] = "Please provide all required information.";
                    return RedirectToAction("Checkout");
                }

                registration.FullName = fullName;
                registration.Email = email;
                registration.Mobile = phone;
                registration.Address = address;
                registration.Pincode = city;

                _context.RegistrationTables.Update(registration);
                _context.SaveChanges();

                string paymentMethod = Request.Form["PaymentMethod"];
                if (string.IsNullOrEmpty(paymentMethod)) paymentMethod = "COD";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var cartItems = _context.Carts
                            .Where(c => c.RegistrationId == registration.Id)
                            .Include(c => c.Book)
                            .ToList();

                        if (!cartItems.Any())
                        {
                            transaction.Rollback();
                            TempData["ErrorMessage"] = "Your cart is empty.";
                            return RedirectToAction("Cart");
                        }

                        var orders = new List<Order>();
                        var cartItemModels = new List<CartItem>();

                        foreach (var cartItem in cartItems)
                        {
                            var order = new Order
                            {
                                RegistrationId = registration.Id,
                                BookId = cartItem.BookId,
                                CreateDate = DateTime.Now,
                                IsOrderStatus = paymentMethod != "COD", // 🔥 Main Fix
                                IsDelete = false,
                                PaymentMethod = paymentMethod,
                                IsPaid = paymentMethod != "COD",
                            };

                            _context.Orders.Add(order);
                            orders.Add(order);

                            cartItemModels.Add(new CartItem
                            {
                                BookId = cartItem.BookId,
                                BookName = cartItem.Book.BookName,
                                AuthorName = cartItem.Book.AuthorName,
                                Quantity = cartItem.Quantity,
                                Price = cartItem.Book.Price,
                                Image = cartItem.Book.Image ?? "noimage.jpg"
                            });
                        }

                        _context.SaveChanges();

                        _context.Carts.RemoveRange(cartItems);
                        _context.SaveChanges();

                        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(new List<CartItem>()));
                        HttpContext.Session.SetInt32("CartCount", 0);

                        transaction.Commit();

                        var totalAmount = cartItemModels.Sum(x => x.Price * x.Quantity);

                        var confirmationModel = new OrderConfirmationModel
                        {
                            FullName = fullName,
                            Email = email,
                            OrderDate = DateTime.Now,
                            TotalAmount = totalAmount,
                            CartItems = cartItemModels,
                            OrderId = orders.FirstOrDefault()?.Id ?? 0
                        };

                        TempData["UpdateCartCount"] = true;
                        return View("OrderConfirmation", confirmationModel);
                    }
                    catch (Exception innerEx)
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = $"Error placing your order: {innerEx.Message}";
                        return RedirectToAction("Checkout");
                    }
                }
            }
            catch (DbUpdateException dbEx)
            {
                TempData["ErrorMessage"] = $"Database error occurred while placing your order: {dbEx.Message}";
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction("Checkout");
            }
        }

        public IActionResult AboutUs()
        {
            // You can pass wishlist items here later
            return View();
        }
        public IActionResult ContactUs()
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
        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(ContactMessage model)
        {
            // Retrieve the RegistrationId safely
            int registrationId = 0;
            if (TempData["newid"] != null)
            {
                registrationId = Convert.ToInt32(TempData["newid"]);
            }
            else
            {
                // Optional: if you want to allow null registration ID
                // or handle this differently
                ModelState.AddModelError("", "Registration ID is missing.");
                return View(model);
            }

            // Create a new entry
            var contactEntry = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Subject = model.Subject,
                Message = model.Message,
                RegistrationId = registrationId
            };

            // Add to database and save
            _context.ContactMessages.Add(contactEntry);
            await _context.SaveChangesAsync();

            // Set success message
            TempData["Success"] = "Your message has been sent successfully!";

            // Redirect to avoid form resubmission
            return RedirectToAction("ContactUs");
        }
    }
}


