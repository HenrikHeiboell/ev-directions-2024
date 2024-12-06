using MemeApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MemeApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MemesController : ControllerBase
{
    private readonly MemeContext _context;
    private readonly ILogger<MemesController> _logger;

    public MemesController(
        MemeContext context,
        ILogger<MemesController> logger
        )
    {
        _context = context;
        _logger = logger;

        if (_context.Memes.Count() == 0)
        {
            _context.Memes.Add(
                new Meme
                {
                    Caption = "dev life",
                    Url =
                        "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-2.jpeg",
                }
            );
            _context.Memes.Add(
                new Meme
                {
                    Caption = "dev life",
                    Url =
                        "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-7.jpg",
                }
            );
            _context.Memes.Add(
                new Meme
                {
                    Caption = "get it now",
                    Url =
                        "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-10.png",
                }
            );
            _context.Memes.Add(
                new Meme
                {
                    Caption = "maniac",
                    Url =
                        "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-23.png",
                }
            );

            _context.Memes.Add(
                new Meme
                {
                    Caption = "dev hack",
                    Url =
                        "<img src='invalid-url' onerror='alert(\"This is an injected script!\")'>",
                }
            );
            _context.SaveChanges();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Meme>>> GetMemes()
    {
        return await _context.Memes.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Meme>> GetMeme(int id)
    {
        var meme = await _context.Memes.FindAsync(id);

        if (meme == null)
        {
            return NotFound();
        }

        return meme;
    }

    [HttpPost]
    public async Task<ActionResult<Meme>> CreateMeme(Meme meme)
    {
        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeme(int id, Meme meme)
    {
        var existingMeme = await _context.Memes.FindAsync(id);
        if (existingMeme == null)
        {
            return NotFound();
        }

        existingMeme.Url = meme.Url;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeme(int id)
    {
        var meme = await _context.Memes.FindAsync(id);
        if (meme == null)
        {
            return NotFound();
        }

        _context.Memes.Remove(meme);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Endpoint demonstrating SQL injection vulnerability
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Meme>>> SearchMemes(string query)
    {
        var memes = _context
            .Memes.Where(m => EF.Functions.Like(m.Caption, "%" + query + "%"))
            .ToList();

        return memes;
    }

    // Endpoint demonstrating Command Injection vulnerability
    [HttpGet("run")]
    public async Task<IActionResult> RunCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return BadRequest("No command provided.");
        }

        if (!IsValidCommand(cmd))
        {
            return BadRequest("Invalid command.");
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + cmd,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.Start();
        string result = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();

        return Ok(result);
    }

    private bool IsValidCommand(string cmd)
    {
        var allowedCommands = new List<string> { "dir", "echo", "ping" };
        var parts = cmd.Split(' ');
        return allowedCommands.Contains(parts[0]);
    }

    // Endpoint demonstrating Insecure Deserialization vulnerability
    [HttpPost("deserialize")]
    public IActionResult DeserializeMeme([FromBody] string serializedMeme)
    {
        var meme = JsonConvert.DeserializeObject<Meme>(serializedMeme);
        _context.Memes.Add(meme);
        _context.SaveChanges();
        return Ok(meme);
    }

    [HttpGet("file")]
    public IActionResult GetFile(string filename)
    {
        var filePath = Path.Combine("uploads", filename);
        if (System.IO.File.Exists(filePath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", filename);
        }
        return NotFound();
    }

    // Endpoint demonstrating Insecure File Upload vulnerability
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMeme(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var filePath = Path.Combine("uploads", file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { filePath });
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(
            new
            {
                GoogleApiKey = ApiConfig.GoogleApiKey,
                DatabasePassword = ApiConfig.DatabasePassword,
            }
        );
    }

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        // Logging sensitive information
        _logger.LogInformation($"User login attempt: {username} with password: {password}");
        // Authentication logic
        return Ok();
    }
}
