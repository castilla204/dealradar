using DataLayer;

namespace ServicesLayer
{
    public class Web3Service : IWeb3Service
    {
        private readonly IWeb3Data _web3Data = new Web3Data();
        public Web3Service(IWeb3Data web3Data) {
        _web3Data = web3Data;
        }



        public async Task <String> GetWallapop(string keywords, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            return await _web3Data.MakeRequestAsync(keywords, latitude, longitude, minprice, maxprice);
        }

    }
}
