using Microsoft.EntityFrameworkCore;

namespace RoyalVilla_API.Data
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {

    }
}
