using System.ComponentModel.DataAnnotations;
namespace Watchlist.Models
{
    public class ModeleVueFilm
    {
        [Key]
        public int IdFilm { get; set; }
        public string? Titre { get; set; }
        public int Annee { get; set; }
        public bool PresentDansListe { get; set; }
        public bool Vu { get; set; }
        public int? Note { get; set; }
        public double MoyenneNote { get; set; }
        public string? Autheur { get; set; }
        public TimeOnly Durée { get; set; }
        public string? CouvertureUrl { get; set; }


    }
}
