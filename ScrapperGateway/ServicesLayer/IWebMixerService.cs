using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLayer
{
    public interface IWebMixerService
    {
        public Task<string> GetAllAds(string keywords, int pagestoscrape, string? latitude, string? longitude, int? minprice, int? maxprice, int? brandId, int? modelId);
    }
}
