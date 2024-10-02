using DataLayer;

namespace ServicesLayer
{
    public class Web1Service : IWeb1Service
    {
        private readonly IWeb1Data _web1Data = new Web1Data();
        public Web1Service(IWeb1Data web1Data) {
        _web1Data = web1Data;
        }



        public async Task<string> GetCarList(int brandId, int modelId)
        {
            return await _web1Data.MakeRequestAsync(brandId, modelId);
        }

    }
}
