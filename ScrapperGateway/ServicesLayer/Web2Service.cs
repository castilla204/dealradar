using DataLayer;

namespace ServicesLayer
{
    public class Web2Service : IWeb2Service
    {
        private readonly IWeb2Data _web2Data = new Web2Data();
        public Web2Service(IWeb2Data web2Data) {
        _web2Data = web2Data;
        }



        public async Task<string> GetVintedList(string searchKey)
        {
            return await _web2Data.MakeRequestAsync(searchKey);
        }

    }
}
