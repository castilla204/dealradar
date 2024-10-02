using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public interface IWeb1Data
    {

        public Task<string> MakeRequestAsync(int brandId, int modelId);
    }
}
