using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TokenStorageService _tokenStorage;

        public HomeController(ILogger<HomeController> logger, TokenStorageService tokenStorage)
        {
            _logger = logger;
            _tokenStorage = tokenStorage;
        }

        public IActionResult Index(string sid)
        {
            // If no session ID, redirect to login
            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            // Verify the session ID is valid
            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.SessionId = sid;
            return View();
        }

        public IActionResult Privacy(string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.SessionId = sid;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}