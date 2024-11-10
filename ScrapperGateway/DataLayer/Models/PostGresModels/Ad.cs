namespace DataLayer.Models.PostGresModels
{
    public class Ad
    {
        public int Id { get; set; }  // Cambiado de string a int para coincidir con SERIAL
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public decimal? Price { get; set; }
        public string[] Images { get; set; }  // Cambiado a array para coincidir con TEXT[]
        public int? AdScore { get; set; }     // Nullable para permitir valores nulos
        public int? FinalScore { get; set; }  // Nullable para permitir valores nulos
        public string[] GoodThings { get; set; }  // Cambiado a array
        public string[] BadThings { get; set; }   // Cambiado a array
        public DateTimeOffset? PublishDate { get; set; }  // Nullable
        public string Category { get; set; }
        public int? CategoryId { get; set; }     // Nullable
        public string Province { get; set; }
        public int? ProvinceId { get; set; }     // Nullable
        public string City { get; set; }
        public int? CityId { get; set; }         // Nullable
        public bool Highlighted { get; set; }
        public bool IsNew { get; set; }
        public bool IsReserved { get; set; }
        public string Slug { get; set; }
        public string SellerType { get; set; }
        public string[] Tags { get; set; }       // Cambiado a array
        public DateTimeOffset? UpdateDate { get; set; }
        public DateTimeOffset? ScrappedDate { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
    }
}