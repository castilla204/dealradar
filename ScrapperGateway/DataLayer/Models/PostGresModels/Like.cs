using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.PostGresModels
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Relación con usuario
        public int AdId { get; set; } // Relación con anuncio
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } // Propiedad de navegación
        public Ad Ad { get; set; } // Propiedad de navegación
    }
}
