using System.Diagnostics;
using EBook.Model;
using EBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data_Context _context;

        public HomeController(ILogger<HomeController> logger, Data_Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //public IActionResult GetBooksForSearch()
        //{
        //    var books = _context.Book
        //        .Select(b => new
        //        {
        //            title = b.Title,
        //            author = b.Author
        //        }).ToList();

        //    return Json(books);
        //}


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
