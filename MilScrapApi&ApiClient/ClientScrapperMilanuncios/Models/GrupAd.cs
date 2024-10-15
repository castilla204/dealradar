namespace ClientScrapperMilanuncios.Models
{
    public class Coordinates
    {
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class Dates
    {
        public string createdAt { get; set; }
        public string publishedAt { get; set; }
    }

    public class Details
    {
        public string year { get; set; }
        public string kilometers { get; set; }
        public string cubicCapacity { get; set; }
        public string fuelType { get; set; }
    }

    public class GrupLocation
    {
        public string province { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public Coordinates coordinates { get; set; }
    }

    public class Media
    {
        public List<string> images { get; set; }
        public List<string> videos { get; set; }
    }

    public class GrupPrice
    {
        public string amount { get; set; }
        public string hasTaxes { get; set; }
    }

    public class Rating
    {
        public string score { get; set; }
        public string totalReviews { get; set; }
    }


        public class GrupAd
        {
            public string id { get; set; }
            public string source { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public GrupPrice GrupPrice { get; set; }
            public Seller seller { get; set; }
            public GrupLocation GrupLocation { get; set; }
            public Details details { get; set; }
            public Media media { get; set; }
            public Dates dates { get; set; }
            public string url { get; set; }
            public string category { get; set; }
            public List<string> positiveAspects { get; set; }
            public List<string> negativeAspects { get; set; }
        }

    



    public class Seller
    {
        public string name { get; set; }
        public string isProfessional { get; set; }
        public string phone { get; set; }
        public Rating rating { get; set; }
    }

}
