using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SDKExample.SDK
{
    public class TestApiClient
    {
        readonly HttpClient _client;

        public TestApiClient(HttpClient httpClient)
        {
            _client = httpClient;
        }

        public async Task<string> Test()
        {
            return await _client.GetAsync<string>("/limit");
        }
    }
}
