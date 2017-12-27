using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Connectors
{
    public class CTMSWebApiConnector : ServiceBase
    {
        private const string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdXRoLnN1YmRvbWFpbiI6InRwcyIsImF1dGgudXNlcmlkIjoibm9sb2dpbi1hZG1pbiIsInRzIjoxNDk4MDk0MDAyLCJ1c2VyX25hbWUiOiJhZG1pbiJ9.D3sCMpBVnxoC7FOx32t2rz8HfbG4hF1L81e1K4dKgeo";

        private HttpClient _client;

        public CTMSWebApiConnector()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(ZKTecoConfig.ApiRootUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", Token));
            //_client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            // It fixes the following error:
            // The underlying connection was closed: An unexpected error occurred on a send.
            // https://stackoverflow.com/questions/22627977/the-underlying-connection-was-closed-an-unexpected-error-occurred-on-a-send
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        
        public async Task<string> FindProjectWorkerByFaceId(string projectId, string faceId)
        {
            var url = string.Format("projects/{0}/workers/{1}", projectId, faceId);
            try
            {
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var worker = await response.Content.ReadAsStringAsync();
                    return worker;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return await Task.FromResult("None");
        }

        public string Get(Dictionary<string, string> parameters)
        {
            return null;
        }

        public void Post(string json)
        {

        }

        public void Put(string json)
        {

        }
    }
}
