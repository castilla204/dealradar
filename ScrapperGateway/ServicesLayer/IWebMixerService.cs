using DataLayer.Models;
using DataLayer.Models.Wallapop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLayer
{
    public interface IWebMixerService
    {
        public Task<List<AdModel>> AnalyzeAds(string keywords, string userSearch, int pagestoscrape, int? category,  string? latitude, string? longitude, int? minprice, int? maxprice, int? brandId, int? modelId);


    }
}