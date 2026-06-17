using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

        public LoginController(IConfiguration configuration, TokenStorageService tokenStorage)
        {
            _configuration = configuration;
            _tokenStorage = tokenStorage;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiBaseUrl"]);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine("=== LOGIN POST ATTEMPT ===");
            System.Diagnostics.Debug.WriteLine($"Username: {username}");

            var loginData = new { username = username, password = password };
            var json = JsonSerializer.Serialize(loginData);
            System.Diagnostics.Debug.WriteLine($"Request JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    // Use case-insensitive deserialization
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, options);
                    var token = result?.Token ?? "";

                    System.Diagnostics.Debug.WriteLine($"Token received: {(string.IsNullOrEmpty(token) ? "EMPTY" : "RECEIVED")}");
                    if (!string.IsNullOrEmpty(token))
                    {
                        System.Diagnostics.Debug.WriteLine($"Token (first 30 chars): {token.Substring(0, Math.Min(30, token.Length))}...");
                    }

                    // Generate a unique session ID
                    var sessionId = Guid.NewGuid().ToString();

                    // Store token server-side using TokenStorageService
                    if (!string.IsNullOrEmpty(token))
                    {
                        _tokenStorage.SetToken(sessionId, token);
                        System.Diagnostics.Debug.WriteLine($"Token stored for key: {sessionId}");

                        // Verify it was stored
                        var verifyToken = _tokenStorage.GetToken(sessionId);
                        System.Diagnostics.Debug.WriteLine($"Verification - Token exists: {!string.IsNullOrEmpty(verifyToken)}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Token is empty! Not storing.");
                    }

                    System.Diagnostics.Debug.WriteLine("Login SUCCESS!");
                    System.Diagnostics.Debug.WriteLine($"Session ID: {sessionId}");

                    // Redirect to Dashboard with session ID as query parameter
                    return Redirect($"/Home/Index?sid={sessionId}");
                }

                ViewBag.Error = "Invalid username or password";
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                ViewBag.Error = $"Cannot connect to API: {ex.Message}";
                return View();
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}