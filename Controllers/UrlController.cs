using Microsoft.AspNetCore.Mvc;
using UrlShortner.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public UrlController(AppDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // for HTTPS dev
        })
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }


    [HttpPost("shorten")]
    public async Task<IActionResult> ShortenUrl([FromBody] string originalUrl)
    {
        if (!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            return BadRequest("Invalid URL format.");

        bool isReachable = await IsUrlReachable(originalUrl);
        if (!isReachable)
            return BadRequest("URL is not reachable or doesn't exist.");

        var shortCode = GenerateShortCode(originalUrl);
        var mapping = new UrlMapping { OriginalUrl = originalUrl, ShortCode = shortCode };
        _context.UrlMappings.Add(mapping);
        _context.SaveChanges();
        return Ok(new { shortCode });
    }

    [HttpGet("{shortCode}")]
    public IActionResult RedirectToOriginal(string shortCode)
    {
        var mapping = _context.UrlMappings.FirstOrDefault(u => u.ShortCode == shortCode);
        if (mapping == null) return NotFound("URL not found");
        return Redirect(mapping.OriginalUrl);
    }

    private string GenerateShortCode(string url)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(url + DateTime.Now));
        return Convert.ToBase64String(hash).Substring(0, 6).Replace("/", "").Replace("+", "");
    }

    private async Task<bool> IsUrlReachable(string url)
    {
        try
        {
            // Try HEAD
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            using var headResponse = await _httpClient.SendAsync(headRequest);
            if (headResponse.IsSuccessStatusCode)
                return true;

            // If HEAD fails, try GET
            using var getResponse = await _httpClient.GetAsync(url);
            return getResponse.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("HttpRequestException: " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            return false;
        }
    }



}
