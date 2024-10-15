using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLayer
{
    public interface IWeb3Service
    {
        public Task<String> GetWallapop(string keywords, int pagestoscrap, string? latitude, string? longitude, int? minprice, int? maxprice);
    }
}
