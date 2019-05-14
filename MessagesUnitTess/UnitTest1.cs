using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessagesUnitTess
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1Async()
        {
            var sendrequest = "http://localhost:50256/api/message/UserSignup";
            var getRequest = "http://localhost:50256/api/message/UserSignupResponse";
            string baseURL = "http://localhost:50256";


            var User = new
            {
                UserName = "TestUser"
            };


            var sendRequest = GetHttpClient(baseURL).PostAsync(sendrequest, new StringContent(JsonConvert.SerializeObject(User), Encoding.UTF8, "application/json"));
            Thread.Sleep(10000);


            var response =  GetHttpClient(baseURL).GetAsync(getRequest).Result;
            string content = await response.Content.ReadAsStringAsync();
            
            List<string> messageList = JsonConvert.DeserializeObject<List<string>>(content);

            Assert.IsTrue(messageList.Contains(User.UserName));

        }

        public static HttpClient GetHttpClient(string baseURI)
        {
            var result = new HttpClient(new HttpClientHandler
            {
                PreAuthenticate = true,
                // Assign the credentials of the logged in user or the user being impersonated.
               UseDefaultCredentials = true
            })
            {
                BaseAddress = new Uri(baseURI)
            };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return result;
        }
    }
}
