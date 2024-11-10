using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.PostGresModels
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string GoogleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Like> Likes { get; set; } // Relación con los likes
    }

}
