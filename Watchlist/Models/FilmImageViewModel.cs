namespace Watchlist.Models
{
    public class FilmImageViewModel
    {
        public int IdFilm { get; set; }
        public string? Titre { get; set; }
        public int Annee { get; set; }
        public string? Autheur { get; set; }
        public TimeOnly Durée { get; set; }
        public IFormFile? CouvertureImage { get; set; } // Fichier téléchargé
        public string? CouvertureUrl { get; set; }


    }
}
