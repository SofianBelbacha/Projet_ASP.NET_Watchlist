using System.ComponentModel.DataAnnotations;

namespace Watchlist.Data
{
    public class Film
    {
        [Key]
        public int IdFilm { get; set; }
        public string Titre { get; set; }
        public int Annee { get; set; }
        public string Autheur { get; set; }
        public TimeOnly Durée { get; set; }
        public string? CouvertureUrl { get; set; }

    }
}
