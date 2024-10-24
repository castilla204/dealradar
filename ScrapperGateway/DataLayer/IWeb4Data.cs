using DataLayer.Models.Wallapop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataLayer.Web3Data;


namespace DataLayer
{
    public interface IWeb4Data
    {

        public Task<String> MakeRequestAsync(string keywords);
        public Task<List<Root>> GetAnunciosAsync(string keyword);
    }
}
