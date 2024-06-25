using Lab7;
using Polly;
using System.Net.Http;

var clientId = "oauth-client";
var clientSecret = "oauth-secret";

//1-2
var api = new API(clientId, clientSecret);
var articles = await api.GetArticles();
Console.WriteLine(string.Join('\n', articles));

//3
for (int i = 0; i < 3; i++)
{
    try
    {
        await api.GetCommonTime();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
