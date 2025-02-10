using Microsoft.EntityFrameworkCore;
using Watchlist.Data;
using Watchlist.Models;
using X.PagedList;

namespace Watchlist.Services
{
    public class FilmService
    {
        private readonly ApplicationDbContext _context;

        public FilmService(ApplicationDbContext context)
        {
            _context = context;
        }


        #region Méthode refactorisée

        /// <summary>
        /// Récupère la requête des films en appliquant une recherche et un tri.
        /// </summary>
        public IQueryable<Film> GetFilmsQuery(string? searchQuery, int? sortOption)
        {
            var filmsQuery = _context.Films.AsQueryable();

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filmsQuery = filmsQuery.Where(x =>
                    EF.Functions.Like(x.Titre, $"{searchQuery}%") ||
                    EF.Functions.Like(x.Annee.ToString(), $"%{searchQuery}%"));
            }

            // Appliquer le tri
            switch (sortOption)
            {
                case 1: // Trier par année
                    filmsQuery = filmsQuery.OrderBy(f => f.Annee);
                    break;
                case 2: // Trier par titre (A-Z)
                    filmsQuery = filmsQuery.OrderBy(f => f.Titre);
                    break;
                case 3: // Trier par titre (Z-A)
                    filmsQuery = filmsQuery.OrderByDescending(f => f.Titre);
                    break;
            }

            return filmsQuery;
        }

        /// <summary>
        /// Récupère la moyenne des notes pour chaque film.
        /// </summary>
        public async Task<List<MoyenneNoteFilm>> GetMoyennesNotes()
        {
            return await _context.FilmsUtilisateur
                .GroupBy(x => x.IdFilm)
                .Select(g => new MoyenneNoteFilm
                {
                    IdFilm = g.Key,
                    MoyenneNote = g.Average(x => (double?)x.Note) ?? 0.0
                })
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les détails du film spécifiques à l'utilisateur.
        /// </summary>
        public async Task<ModeleVueFilm> GetFilmDetailsForUser(string idUtilisateur, ModeleVueFilm film)
        {
            var filmUtilisateur = await _context.FilmsUtilisateur
                .FirstOrDefaultAsync(x => x.IdUtilisateur == idUtilisateur && x.IdFilm == film.IdFilm);

            if (filmUtilisateur != null)
            {
                film.PresentDansListe = true;
                film.Note = filmUtilisateur.Note;
                film.Vu = filmUtilisateur.Vu;
            }

            return film;
        }

        #endregion   
    }
}
