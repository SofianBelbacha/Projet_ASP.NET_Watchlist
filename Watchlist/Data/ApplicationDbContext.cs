using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Watchlist.Models;

namespace Watchlist.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilisateur>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Film> Films { get; set; }
        public DbSet<FilmUtilisateur> FilmsUtilisateur { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FilmUtilisateur>()
                .HasKey(t => new { t.IdUtilisateur, t.IdFilm });  // Définir la clé composite

            // Configurer la clé étrangère explicite pour la relation entre FilmUtilisateur et Film
            modelBuilder.Entity<FilmUtilisateur>()
                .HasOne(fu => fu.Film)
                .WithMany() // Film n'a pas besoin de navigation explicite dans FilmUtilisateur
                .HasForeignKey(fu => fu.IdFilm) // Spécifiez explicitement la colonne IdFilm comme clé étrangère
                .OnDelete(DeleteBehavior.Cascade); // Définir le comportement de suppression en cascade

            // Optionnel : Configurer la relation avec l'utilisateur
            modelBuilder.Entity<FilmUtilisateur>()
                .HasOne(fu => fu.User)
                .WithMany() // Utilisateur n'a pas besoin de navigation explicite dans FilmUtilisateur
                .HasForeignKey(fu => fu.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade); // Définir le comportement de suppression en cascade


        }
        public DbSet<Watchlist.Models.ModeleVueFilm> ModeleVueFilm { get; set; } = default!;
    }
}
