using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utiles
{
    public static class ApiHelper
    {
        //public static async Task<T> GetAsync<T>(string url, string apiKey)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        //        var response = await client.GetAsync(url);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var json = await response.Content.ReadAsStringAsync();
        //            return JsonConvert.DeserializeObject<T>(json);
        //        }
        //        return default(T);
        //    }
        //}
    }
}
