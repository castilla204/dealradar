using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public interface IWeb2Data
    {

        public Task<string> MakeRequestAsync(string searchKey);
    }
}
