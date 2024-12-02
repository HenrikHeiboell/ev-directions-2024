using Microsoft.EntityFrameworkCore;

namespace MemeApi.Models
{
    public class MemeContext : DbContext
    {
        public MemeContext(DbContextOptions<MemeContext> options) : base(options) { }

        public DbSet<Meme> Memes { get; set; }
    }
}