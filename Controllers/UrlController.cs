using Microsoft.AspNetCore.Mvc;
using UrlShortner.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;

    public UrlController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("shorten")]
    public IActionResult ShortenUrl([FromBody] string originalUrl)
    {
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
}
