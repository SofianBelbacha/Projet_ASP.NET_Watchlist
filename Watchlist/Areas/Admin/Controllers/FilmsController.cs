using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Watchlist.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Watchlist.Data;
using X.PagedList.EF;
using Watchlist.Services;
using X.PagedList.Extensions;


namespace Watchlist.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FilmsController : Controller
    {
        private readonly UserManager<Utilisateur> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<Utilisateur> _gestionnaire;
        private readonly FilmService _filmService;

        public FilmsController(UserManager<Utilisateur> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, UserManager<Utilisateur> gestionnaire, FilmService filmService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
			_context = context;
			_gestionnaire = gestionnaire;
            _filmService = filmService;


        }

        private Task<Utilisateur> GetCurrentUserAsync() =>
		_gestionnaire.GetUserAsync(HttpContext.User);

		[HttpGet]
		public async Task<string> RecupererIdUtilisateurCourant()
		{
			Utilisateur utilisateur = await GetCurrentUserAsync();
			return utilisateur?.Id;
		}


        // GET: Films
        public async Task<IActionResult> Index(int? page = null, string? searchQuery = null, int? sortOption = null, int pageSize = 8)
        {
            var idUtilisateur = await RecupererIdUtilisateurCourant();

            // Récupération des moyennes des notes de films
            var moyenneNotes = await _filmService.GetMoyennesNotes();

            // Récupération des liste des films avec la recherche et le tri appliqués
            var filmsQuery = _filmService.GetFilmsQuery(searchQuery, sortOption);

            // Pagination
            int pageNumber = page ?? 1; // Page actuelle
            int totalFilms = await filmsQuery.CountAsync(); // Récupère le nombre total de films
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

            // Compléter les informations pour chaque film
            foreach (var film in films)
            {
                var filmWithDetails = await _filmService.GetFilmDetailsForUser(idUtilisateur, film);

                filmWithDetails.MoyenneNote = Math.Round(moyenneNotes.FirstOrDefault(m => m.IdFilm == filmWithDetails.IdFilm)?.MoyenneNote ?? 0.0, 1);

                updatedFilms.Add(filmWithDetails);
            }
            var pagedFilms = updatedFilms.ToPagedList(pageNumber, pageSize); // Convertir la liste en IPagedList

            return View(pagedFilms);
        }


        // GET: Films/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Films/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFilm,Titre,Annee,Autheur,Durée,CouvertureImage")] FilmImageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? relativePath = null;

                if (model.CouvertureImage != null)
                {
                    // Définir le chemin d'enregistrement
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Covers");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CouvertureImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Sauvegarder le fichier
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CouvertureImage.CopyToAsync(fileStream);
                    }

                    // Définir le chemin relatif
                    relativePath = $"/img/Covers/{uniqueFileName}";
                }

                var film = new Film
                {
                    Titre = model.Titre,
                    Annee = model.Annee,
                    Autheur = model.Autheur,
                    Durée = model.Durée,
                    CouvertureUrl = relativePath
                };

                _context.Add(film);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films.FindAsync(id);
            if (film == null)
            {
                return NotFound();
            }
            var model = new FilmImageViewModel
            {
                IdFilm = film.IdFilm,
                Titre = film.Titre,
                Annee = film.Annee,
                Autheur = film.Autheur,
                Durée = film.Durée,
                CouvertureUrl = film.CouvertureUrl
            };

            return View(model);
        }

        // POST: Films/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFilm,Titre,Annee,Autheur,Durée,CouvertureImage")] FilmImageViewModel model)
        {
            if (id != model.IdFilm)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var film = await _context.Films.FindAsync(id);

                    if (film == null)
                    {
                        return NotFound();
                    }

                    // Mise à jour des champs non liés à l'image
                    film.Titre = model.Titre;
                    film.Annee = model.Annee;
                    film.Autheur = model.Autheur;
                    film.Durée = model.Durée;

                    if (model.CouvertureImage != null)
                    {
                        // Supprimez l'ancienne image si nécessaire
                        if (!string.IsNullOrEmpty(film.CouvertureUrl))
                        {
                            string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", film.CouvertureUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Téléchargez la nouvelle image
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Covers");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CouvertureImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.CouvertureImage.CopyToAsync(fileStream);
                        }

                        // Mettez à jour le chemin de la couverture
                        film.CouvertureUrl = $"/img/Covers/{uniqueFileName}";
                    }

                    _context.Update(film);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmExists(model.IdFilm))
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
            return View(model);
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.IdFilm == id);
        }

        // GET: Films/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Films/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film != null)
            {
                // Supprimez l'ancienne image si nécessaire
                if (!string.IsNullOrEmpty(film.CouvertureUrl))
                {
                    string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", film.CouvertureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                _context.Films.Remove(film);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
