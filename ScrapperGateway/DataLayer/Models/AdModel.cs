using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class AdModel
    {
        public string id { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public double price { get; set; }
        public List<string> images { get; set; }
        public int Adscore { get; set; }
        public int finalScore { get; set; }
        public List<string> goodThings { get; set; }
        public List<string> badThings { get; set; }
        public DateTime publishDate { get; set; }
        //categoria
        public string category { get; set; }
        public int categoryId { get; set; }
        //location
        public string province { get; set; }
        public int provinceId { get; set; }
        public string city { get; set; }
        public int cityId { get; set; }
        public bool highlighted { get; set; }
        public bool isNew { get; set; }
        public string isReserved { get; set; }
        public string slug { get; set; }
        public string sellerType { get; set; }
        public List<string> tags { get; set; }
        public int userId { get; set; }
        public DateTime updateDate { get; set; }
        public DateTime ScrappedDate { get; set; }

    }
    }


