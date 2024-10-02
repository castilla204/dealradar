using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesLayer
{
    public interface IWeb1Service
    {
        public Task<string> GetCarList(int brandId, int modelId);
    }
}
