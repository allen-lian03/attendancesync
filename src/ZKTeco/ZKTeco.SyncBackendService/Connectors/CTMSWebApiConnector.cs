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
        private HttpClient _client;

        public CTMSWebApiConnector()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(ZKTecoConfig.ApiRootUrl);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", ZKTecoConfig.ApiToken));

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
                    var worker = await response.Content.ReadAsAsync<WorkerInfo>();
                    if (worker != null)
                    {
                        return worker.UserId;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("FindProjectWorkerByFaceId error: {@ex}", ex);                
            }
            return await Task.FromResult("");
        }

        public async Task<bool> CheckIn(string projectId, string workerId, DateTime logDate, string location)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(
                    "attendances/in", 
                    new CheckInInfo(projectId, workerId, location, logDate));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CheckIn error: {@ex}", ex);
                return false;
            }
            return true;
        }

        public async Task<bool> CheckOut(string projectId, string workerId, DateTime logDate)
        {
            try
            {
                var response = await _client.PostAsJsonAsync(
                    "attendances/out",
                    new CheckOutInfo(projectId, workerId, logDate));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CheckOut error: {@ex}", ex);
                return false;
            }
            return true;
        }        
    }
}
