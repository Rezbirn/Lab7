using System.Net.Http.Headers;
using System.Text;
using Lab7.Models;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;

namespace Lab7
{
    public class API
    {
        private readonly string _token;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        public API(string clientId, string clientSecret) 
        {
            var token = GetToken(clientId, clientSecret);
            _token = token.Result;

             _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

        }
        private async Task<string> GetToken(string clientId, string clientSecret)
        {
            using (var httpClient = new HttpClient())
            {
                var authenticationString = $"{clientId}:{clientSecret}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                var jsonPayload = "{\"key\":\"value\"}";
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var resultRequest = await httpClient.PostAsync($"http://localhost:9090/oauth2/token?grant_type=client_credentials", content);
                if (resultRequest.IsSuccessStatusCode)
                {
                    var json = await resultRequest.Content.ReadAsStringAsync();
                    var tokenResult = JsonConvert.DeserializeObject<TokenResult>(json);
                    return tokenResult.access_token;
                }
                throw new HttpRequestException(resultRequest.StatusCode.ToString());
            }
            
        }

        public async Task<List<string>> GetArticles()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
                var resultRequest = await httpClient.GetAsync("http://localhost:9090/articles");
                if (resultRequest.IsSuccessStatusCode)
                {
                    var json = await resultRequest.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<string>>(json);
                    return result;
                }
                throw new HttpRequestException(resultRequest.StatusCode.ToString());
            }
        }
        public async Task GetCommonTime()
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("http://localhost:9090/api/v1/common/time");
                response.EnsureSuccessStatusCode();
            });
        }
    }
}
