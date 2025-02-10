using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Watchlist.Data;
using Watchlist.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList.Extensions;
using X.PagedList.EF;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;

namespace Watchlist.Controllers
{
    [Authorize]
    public class ListeFilmsController : Controller
    {
        private readonly ApplicationDbContext _contexte;
        private readonly UserManager<Utilisateur> _gestionnaire;

        public ListeFilmsController(ApplicationDbContext contexte, UserManager<Utilisateur> gestionnaire)
        {
            _contexte = contexte;
            _gestionnaire = gestionnaire;
        }
        private Task<Utilisateur> GetCurrentUserAsync() => _gestionnaire.GetUserAsync(HttpContext.User);

        [HttpGet]
        public async Task<string> RecupererIdUtilisateurCourant()
        {
            Utilisateur utilisateur = await GetCurrentUserAsync();
            return utilisateur?.Id;
        }


        #region Méthodes Index

        /// <summary>
        /// Récupèration de la requête des films de l'utilisateur avec recherche et filtres appliqués.
        /// </summary>
        private IQueryable<FilmUtilisateur> GetUserFilmsQuery(string idUtilisateur, string? searchQuery, int? filter)
        {
            var filmsQuery = _contexte.FilmsUtilisateur
                .Where(x => x.IdUtilisateur == idUtilisateur)
                .Include(x => x.Film)
                .AsQueryable();

            // Appliquer la recherche
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filmsQuery = filmsQuery.Where(x =>
                    EF.Functions.Like(x.Film.Titre, $"{searchQuery}%") ||
                    EF.Functions.Like(x.Film.Annee.ToString(), $"{searchQuery}%"));
            }

            return ApplyFiltersAndSorting(filmsQuery, filter);
        }

        /// <summary>
        /// Applique les filtres et le tri aux films.
        /// </summary>
        private IQueryable<FilmUtilisateur> ApplyFiltersAndSorting(IQueryable<FilmUtilisateur> query, int? filter)
        {
            switch (filter)
            {
                case 1: // Trier par titre
                    query = query.OrderBy(x => x.Film.Titre);
                    break;
                case 2: // Films vus
                    query = query.Where(x => x.Vu);
                    break;
                case 3: // Films non vus
                    query = query.Where(x => !x.Vu);
                    break;
                case 7: // Films bien notés (≥ 7 étoiles)
                    query = query.Where(x => x.Note >= 7);
                    break;
                case 4: // Films assez bien notés (≥ 4 étoiles)
                    query = query.Where(x => x.Note >= 4 && x.Note <= 6);
                    break;
                case 0: // Films mal notés (≤ 3 étoiles)
                    query = query.Where(x => x.Note <= 3);
                    break;
            }
            return query;
        }

        /// <summary>
        /// Convertion de la requête en une liste paginée de ModeleVueFilm.
        /// </summary>
        private async Task<IPagedList<ModeleVueFilm>> GetPagedFilmsListAsync(IQueryable<FilmUtilisateur> query, int pageNumber, int pageSize)
        {
            return await query.Select(x => new ModeleVueFilm
            {
                IdFilm = x.IdFilm,
                Titre = x.Film.Titre,
                Annee = x.Film.Annee,
                Vu = x.Vu,
                PresentDansListe = true,
                Note = x.Note,
                Durée = x.Film.Durée,
                Autheur = x.Film.Autheur,
                CouvertureUrl = x.Film.CouvertureUrl
            }).ToPagedListAsync(pageNumber, pageSize);
        }

        #endregion

        public async Task<IActionResult> Index(int? page = null, string? searchQuery = null, int? filter = null, int pageSize = 8)
        {
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            // Obtention de la requête filtrée des films de l'utilisateur
            var filmsQuery = GetUserFilmsQuery(idUtilisateur, searchQuery, filter);

            // Nombre total de films
            int totalFilms = await filmsQuery.CountAsync();
            ViewBag.TotalFilms = totalFilms;

            // Récupération la liste paginée des films
            int pageNumber = page ?? 1;
            var filmsPaged = await GetPagedFilmsListAsync(filmsQuery, pageNumber, pageSize);

            // Déterminer la plage d'éléments affichés
            ViewBag.StartItem = (pageNumber - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(ViewBag.StartItem + pageSize - 1, totalFilms);

            return View(filmsPaged);
        }



        public async Task<IActionResult> Edit(int id)
        {
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            if (idUtilisateur == null || id == 0)
            {
                return NotFound();
            }

            //var filmUtilisateur = await _contexte.FilmsUtilisateur.FindAsync(id);
            var filmUtilisateur = await _contexte.FilmsUtilisateur
                .Include(fu => fu.Film)
                .FirstOrDefaultAsync(x => x.IdUtilisateur == idUtilisateur && x.IdFilm == id);

            if (filmUtilisateur == null)
            {
                return NotFound();
            }
            //ViewData["IdFilm"] = new SelectList(_contexte.Films, "IdFilm", "IdFilm", filmUtilisateur.IdFilm);
            //ViewData["IdUtilisateur"] = new SelectList(_contexte.Users, "Id", "Id", filmUtilisateur.IdUtilisateur);

            var modeleVueFilm = new ModeleVueFilm
            {
                IdFilm = filmUtilisateur.IdFilm,
                Titre = filmUtilisateur.Film.Titre,
                Annee = filmUtilisateur.Film.Annee,
                Vu = filmUtilisateur.Vu,
                Note = filmUtilisateur.Note,
                PresentDansListe = true // ou en fonction de votre logique
            };

            return View(modeleVueFilm);
        }

        // POST: FilmUtilisateurs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUtilisateur,IdFilm,Vu,Note")] ModeleVueFilm modeleVueFilm)
        {
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            if (idUtilisateur == null || id != modeleVueFilm.IdFilm)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(modeleVueFilm); // Retourne la vue avec le modèle pour afficher les erreurs
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var filmUtilisateur = await _contexte.FilmsUtilisateur
                    .FirstOrDefaultAsync(x => x.IdUtilisateur == idUtilisateur && x.IdFilm == id);

                    if (filmUtilisateur == null)
                    {
                        return NotFound();
                    }

                    // Mise à jour des propriétés
                    filmUtilisateur.Vu = modeleVueFilm.Vu;
                    filmUtilisateur.Note = (int)modeleVueFilm.Note;

                    _contexte.Update(filmUtilisateur);
                    await _contexte.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmUtilisateurExists(idUtilisateur, id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            //ViewData["IdFilm"] = new SelectList(_contexte.Films, "IdFilm", "IdFilm", filmUtilisateur.IdFilm);
            //ViewData["IdUtilisateur"] = new SelectList(_contexte.Users, "Id", "Id", filmUtilisateur.IdUtilisateur);
            
            return View(modeleVueFilm);
        }

        private bool FilmUtilisateurExists(string idUtilisateur, int idFilm)
        {
            return _contexte.FilmsUtilisateur.Any(e => e.IdUtilisateur == idUtilisateur && e.IdFilm == idFilm);

        }

    }
}
