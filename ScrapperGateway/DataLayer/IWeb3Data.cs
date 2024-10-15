using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataLayer.Web3Data;


namespace DataLayer
{
    public interface IWeb3Data
    {

        public Task<String> MakeRequestAsync(string keywords, int pagestoscrap, string? latitude, string? longitude, int? minprice, int? maxprice);
    }
}
