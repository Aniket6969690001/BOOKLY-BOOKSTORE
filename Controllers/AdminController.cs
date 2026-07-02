using EBook.Model;
using EBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;


namespace EBook.Controllers
{
    public class AdminController : Controller
    {
        private readonly Data_Context _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(Data_Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Admin()
        {
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.BookCount = await _context.Books.CountAsync();
            ViewBag.GenreCount = await _context.Genres.CountAsync();
            ViewBag.StockCount = await _context.Stocks.CountAsync();
            ViewBag.ContactCount = await _context.ContactMessages.CountAsync();
            return View();
        }

        public IActionResult Order()
        {
            var orders = _context.Orders
         .Include(o => o.Books)
         .Include(o => o.Registration)
         .Where(o => !o.IsDelete)
         .ToList();

            return View(orders);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus([FromBody] OrderUpdateModel model)
        {
            if (model == null)
                return BadRequest();

            var order = _context.Orders.FirstOrDefault(o => o.Id == model.OrderId);
            if (order != null)
            {
                order.IsPaid = true;
                order.IsOrderStatus = true;
                _context.SaveChanges();
            }

            return Ok();
        }
        public IActionResult Stock()
        {
            var stocks = _context.Stocks
         .Include(s => s.Book)
         .Select(s => new StockViewModel
         {
             Id = s.Id,
             BookId = s.BookId,
             Quantity = s.Quantity,
             BookName = s.Book != null ? s.Book.BookName : "N/A"
         })
         .ToList();

            ViewBag.Books = new SelectList(_context.Books.ToList(), "Id", "BookName");
            return View(stocks);
        }

        [HttpPost]
        public IActionResult AddStock(int BookId, int Quantity)
        {
            if (Quantity <= 0)
            {
                ModelState.AddModelError("", "Quantity must be greater than zero.");
                return RedirectToAction("Stock");
            }

            var existingStock = _context.Stocks.FirstOrDefault(s => s.BookId == BookId);
            if (existingStock != null)
            {
                existingStock.Quantity += Quantity;
            }
            else
            {
                var newStock = new Stock
                {
                    BookId = BookId,
                    Quantity = Quantity
                };
                _context.Stocks.Add(newStock);
            }

            _context.SaveChanges();
            return RedirectToAction("Stock");
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateStock([FromBody] StockViewModel model)
        {
            try
            {
                if (model == null || model.Id == 0)
                    return Json(new { success = false, message = "Invalid data." });

                var stock = _context.Stocks.FirstOrDefault(s => s.Id == model.Id);
                if (stock == null)
                {
                    return Json(new { success = false, message = "Stock not found." });
                }

                if (model.Quantity < 0)
                {
                    return Json(new { success = false, message = "Quantity cannot be negative." });
                }

                stock.Quantity = model.Quantity;
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Stock updated successfully!",
                    updatedQuantity = stock.Quantity
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred: " + ex.Message
                });
            }
        }
        public IActionResult ManangeStock()
        {
            return View();
        }

        public IActionResult Genre()
        {

            var viewModel = new GenreViewModel
            {
                Genres = _context.Genres
                 .Select(g => new GenreWithCount
                 {
                     Id = g.Id,
                     GenreName = g.GenreName,
                     BookCount = _context.Books.Count(b => b.GenreId == g.Id)
                 }).ToList()
            };

            return View(viewModel);

        }

        [HttpPost]
        public IActionResult AddGenre(string genreName)
        {
            if (!string.IsNullOrEmpty(genreName))
            {
                var genre = new Genre { GenreName = genreName };
                _context.Genres.Add(genre);
                _context.SaveChanges();
                return Json(new { success = true, message = "Genre added successfully!" });
            }
            return Json(new { success = false, message = "Genre name cannot be empty." });
        }

        [HttpGet]
        public IActionResult EditGenre(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return Json(new { success = false, message = "Genre not found!" });
            }
            return Json(new { success = true, id = genre.Id, genreName = genre.GenreName });
        }

        [HttpPost]
        public IActionResult EditGenre(int id, string genreName)
        {
            if (string.IsNullOrWhiteSpace(genreName))
            {
                return Json(new { success = false, message = "Genre name is required." });
            }

            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return Json(new { success = false, message = "Genre not found!" });
            }

            genre.GenreName = genreName;
            _context.SaveChanges();

            return Json(new { success = true, message = "Genre updated successfully!" });
        }

        [HttpPost]
        public IActionResult DeleteGenre(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null)
            {
                return Json(new { success = false, message = "Genre not found!" });
            }

            _context.Genres.Remove(genre);
            _context.SaveChanges();

            return Json(new { success = true, message = "Genre deleted successfully!" });
        }

        public IActionResult Books()
        {
            var model = new BooksListViewModel
            {
                Books = _context.Books
                    .Include(b => b.Genre)
                    .Select(b => new BookViewModel
                    {
                        Id = b.Id,
                        BookName = b.BookName,
                        AuthorName = b.AuthorName,
                        Price = b.Price,
                        Image = b.Image,
                        SoftCopy = b.SoftCopy,
                        Description = b.Description,
                        GenreId = b.GenreId ?? 0,
                        GenreName = b.Genre.GenreName
                    }).ToList(),
                Genres = _context.Genres.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBook(IFormFile? Image, IFormFile? SoftCopy, [FromForm] BookViewModel model)
        {
            try
            {
                //    if (!ModelState.IsValid)
                //    {
                //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                //        return Json(new { success = false, message = "Invalid book data!", errors });
                //    }

                string imageFileName = null;
                string pdfFileName = null;

                string imageFolder = Path.Combine(_environment.WebRootPath, "images");
                string pdfFolder = Path.Combine(_environment.WebRootPath, "pdfs");

                if (!Directory.Exists(imageFolder)) Directory.CreateDirectory(imageFolder);
                if (!Directory.Exists(pdfFolder)) Directory.CreateDirectory(pdfFolder);

                if (Image != null)
                {
                    //imageFileName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
                    string imagePath = Path.Combine(imageFolder, Image.FileName.ToString());

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }
                }

                if (SoftCopy != null)
                {
                    //pdfFileName = Guid.NewGuid() + Path.GetExtension(SoftCopy.FileName);
                    string pdfPath = Path.Combine(pdfFolder, SoftCopy.FileName.ToString());

                    using (var stream = new FileStream(pdfPath, FileMode.Create))
                    {
                        await SoftCopy.CopyToAsync(stream);
                    }
                }

                if (model.Id == 0)
                {
                    // New book
                    var book = new Book
                    {
                        BookName = model.BookName,
                        AuthorName = model.AuthorName,
                        Price = model.Price,
                        Description = model.Description,
                        GenreId = model.GenreId ?? 0,
                        Image = Image?.FileName.ToString(),
                        SoftCopy = SoftCopy?.FileName.ToString()

                    };
                    book.CreatedDate = DateTime.Now;
                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();

                    var stock = new Stock
                    {
                        BookId = book.Id,
                        Quantity = 100
                    };
                    _context.Stocks.Add(stock);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Book added successfully!" });
                }
                else
                {
                    // Edit existing book
                    var book = await _context.Books.FindAsync(model.Id);
                    if (book == null)
                        return Json(new { success = false, message = "Book not found." });

                    book.BookName = model.BookName;
                    book.AuthorName = model.AuthorName;
                    book.Price = model.Price;
                    book.Description = model.Description;
                    book.GenreId = model.GenreId ?? 0;

                    if (imageFileName != null)
                        book.Image = imageFileName;

                    if (pdfFileName != null)
                        book.SoftCopy = pdfFileName;

                    _context.Books.Update(book);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Book updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == id);
                if (stock != null)
                {
                    _context.Stocks.Remove(stock);
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Book deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBook([FromForm] BookViewModel model, IFormFile BookImage)
        {
            try
            {
                var book = await _context.Books.FindAsync(model.Id);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found!" });
                }

                book.BookName = model.BookName;
                book.AuthorName = model.AuthorName;
                book.Price = model.Price;
                book.GenreId = model.GenreId ?? 0;

                if (BookImage != null && BookImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + BookImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await BookImage.CopyToAsync(stream);
                    }

                    book.Image = uniqueFileName;
                }

                _context.Books.Update(book);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Book updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating book: " + ex.Message });
            }
        }
        public async Task<IActionResult> ContactMessagesList()
        {
            var messages = await _context.ContactMessages
                                         .OrderByDescending(m => m.Id)
                                         .ToListAsync();

            return View(messages);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteContactMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message deleted successfully!";
            return RedirectToAction("ContactMessagesList");
        }

    }
}