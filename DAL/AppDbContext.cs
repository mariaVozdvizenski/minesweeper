using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<GameBoard> GameBoards { get; set; } = default!;
        public AppDbContext(DbContextOptions options): base(options)
        {
        }
    }
}