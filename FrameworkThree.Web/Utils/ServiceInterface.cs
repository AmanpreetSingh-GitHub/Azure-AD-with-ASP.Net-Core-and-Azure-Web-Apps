using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkThree.Web.Utils
{
    public class ServiceInterface
    {
        private string baseServiceURL = "https://localhost:44344";

        static ServiceInterface instance = null;

        public static ServiceInterface Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceInterface();
                }
                return instance;
            }
        }

        public static void Reset()
        {
            instance = null;
        }

        private ServiceInterface()
        {
        }

        public async Task<RestMessage<T>> GetDataAsync<T>(AuthenticationResult authenticationResult, string controller) where T : class
        {
            RestMessage<T> output = new RestMessage<T>();

            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseServiceURL + "/api/" + controller);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    output = JsonConvert.DeserializeObject<RestMessage<T>>(responseString);

                    output.SetAsGoodRequest();
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        output.SetAsBadRequest();
                        output.StatusCode = HttpStatusCode.Unauthorized;
                        output.Exception = new Exception("Unauthorized");
                    }
                    else
                    {
                        output.SetAsBadRequest();
                        output.Exception = new Exception("Error occured during processing");
                    }
                }
            }
            catch (Exception e)
            {
                output.Exception = e;
                output.SetAsBadRequest();
                output.StatusText = "Error during processing";
            }

            return output;
        }

        public async Task<RestMessage<T>> PostDataToAPI<T>(AuthenticationResult authenticationResult, string controller, object content) where T : class
        {
            RestMessage<T> output = new RestMessage<T>();

            try
            {
                string contentData = JsonConvert.SerializeObject(content);

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, baseServiceURL + "/api/" + controller);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
                request.Content = new StringContent(contentData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    output = JsonConvert.DeserializeObject<RestMessage<T>>(responseString);

                    output.SetAsGoodRequest();
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        output.SetAsBadRequest();
                        output.StatusCode = HttpStatusCode.Unauthorized;
                        output.Exception = new Exception("Unauthorized");
                    }
                    else
                    {
                        output.SetAsBadRequest();
                        output.Exception = new Exception("Error occured during processing");
                    }
                }
            }
            catch (Exception e)
            {
                output.Exception = e;
                output.SetAsBadRequest();
                output.StatusText = "Error occured during processing";
            }

            return output;
        }
    }
}
