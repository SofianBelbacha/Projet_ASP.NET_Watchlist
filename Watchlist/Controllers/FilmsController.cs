using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Watchlist.Data;
using Watchlist.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using X.PagedList.EF;
using X.PagedList.Extensions;
using Watchlist.Services;


namespace Watchlist.Controllers
{
    [Authorize]
    public class FilmsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _gestionnaire;
        private readonly FilmService _filmService;


        public FilmsController(ApplicationDbContext context, UserManager<Utilisateur> gestionnaire, FilmService filmService)
        {
            _context = context;
            _gestionnaire = gestionnaire;
            _filmService = filmService;
        }

        [HttpGet]
        public async Task<string?> RecupererIdUtilisateurCourant()
        {
            Utilisateur utilisateur = await GetCurrentUserAsync();
            return utilisateur?.Id;
        }

        private Task<Utilisateur> GetCurrentUserAsync() =>
        _gestionnaire.GetUserAsync(HttpContext.User);


        // GET: Films
        public async Task<IActionResult> Index(int? page = null, string? searchQuery = null, int? sortOption = null, int pageSize = 8)
        {
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            // Récupération de la moyennes des notes de films
            var moyenneNotes = await _filmService.GetMoyennesNotes();

            // Récupération la liste des films avec la recherche et le tri appliqués
            var filmsQuery = _filmService.GetFilmsQuery(searchQuery, sortOption);

            // Pagination
            int pageNumber = page ?? 1; // Page actuelle
            int totalFilms = await filmsQuery.CountAsync(); // Récupèration du nombre total de films
            ViewBag.TotalFilms = totalFilms; // Envoie la donnée à la vue

            var films = await filmsQuery
                .Select(x => new ModeleVueFilm
                {
                    IdFilm = x.IdFilm,
                    Titre = x.Titre,
                    Annee = x.Annee,
                    Autheur = x.Autheur,
                    Durée = x.Durée,
                    CouvertureUrl = x.CouvertureUrl
                })
                .ToListAsync();
            List<ModeleVueFilm> updatedFilms = new List<ModeleVueFilm>();

            // Complétion des informations pour chaque film
            foreach (var film in films)
            {
                // Complétion des informations spécifiques à l'utilisateur
                var filmWithDetails = await _filmService.GetFilmDetailsForUser(idUtilisateur, film); // Retrouver les infos à jour

                // Ajout de la moyenne des notes
                filmWithDetails.MoyenneNote = Math.Round(moyenneNotes.FirstOrDefault(m => m.IdFilm == filmWithDetails.IdFilm)?.MoyenneNote ?? 0.0, 1);

                // Ajout de la nouvelle liste
                updatedFilms.Add(filmWithDetails);
            }
            var pagedFilms = updatedFilms.ToPagedList(pageNumber, pageSize); // Convertion de la liste en IPagedList

            return View(pagedFilms);
        }

        [HttpGet]
        public async Task<JsonResult> AjouterSupprimer(int id, int val)
        {
            var valret = -1;
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            if (val == 0)
            {
                // Ajout du film à la liste
                var filmUtilisateur = _context.FilmsUtilisateur
                    .FirstOrDefault(x => x.IdFilm == id && x.IdUtilisateur == idUtilisateur);

                if (filmUtilisateur == null)
                {
                    _context.FilmsUtilisateur.Add(
                        new FilmUtilisateur
                        {
                            IdUtilisateur = idUtilisateur,
                            IdFilm = id,
                            Vu = false,
                            Note = 0
                        });
                    valret = 1;  // Film ajouté
                }
            }
            else if (val == 1)
            {
                // Suppression du film de la liste
                var film = _context.FilmsUtilisateur
                    .FirstOrDefault(x => x.IdFilm == id && x.IdUtilisateur == idUtilisateur);

                if (film != null)
                {
                    _context.FilmsUtilisateur.Remove(film);
                    valret = 0;  // Film supprimé
                }
            }

            // Enregistrez les modifications dans la base de données
            await _context.SaveChangesAsync();

            // Retourne la valeur indiquant si l'ajout ou la suppression a réussi
            return Json(valret);
        }

        // GET: Films/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .FirstOrDefaultAsync(m => m.IdFilm == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

    }
}
