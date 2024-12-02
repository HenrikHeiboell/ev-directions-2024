using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemeApi.Models;

namespace MemeApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MemesController : ControllerBase
{
    private readonly MemeContext _context;

    public MemesController(MemeContext context)
    {
        _context = context;

        if (_context.Memes.Count() == 0)
        {
            _context.Memes.Add(new Meme { Caption = "dev life", Url = "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-2.jpeg" });
            _context.Memes.Add(new Meme { Caption = "dev life", Url = "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-7.jpg" });
            _context.Memes.Add(new Meme { Caption = "get it now", Url = "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-10.png" });
            _context.Memes.Add(new Meme { Caption = "maniac", Url = "https://file.forms.app/sitefile/55+Hilarious-developer-memes-that-will-leave-you-in-splits-23.png" });

            _context.Memes.Add(new Meme { Caption = "dev hack", Url = "<img src='invalid-url' onerror='alert(\"This is an injected script!\")'>" });
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
            var memes = _context.Memes
                .Where(m => EF.Functions.Like(m.Caption, "%" + query + "%"))
                .ToList();

            return memes;
    }
}