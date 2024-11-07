using DataLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLayer
{
    public interface IWeb4Service
    {
        public Task<List<AdModel>> GetWallapop(string keywords);
    }
}
