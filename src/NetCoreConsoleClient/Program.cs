using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetCoreConsoleClient
{
    public class Program
    {
        private readonly HttpClient _client;
        private static Logger _logger;
        private static IConfiguration _configuration;
        static string _authority = "https://demo.identityserver.io";
        static string _api = "https://demo.identityserver.io/api/test";

        static HttpClient _apiClient = new HttpClient { BaseAddress = new Uri(_api) };

        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|  Sign in with OIDC    |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");
            Console.WriteLine("Press any key to sign in...");
            Console.ReadKey();
            _configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();

            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                .CreateLogger();
            await Login();
        }

        private static async Task Login()
        {
            using (var httpClient = new HttpClient())
            {
                var baseAuthUrl = "https://localhost:44300/";
                var clientSecrect = "BiOiJKV1QiLCJ4NXQiOiKhM";
                var clientId = "PowerBIRS";
                var grantType = "password";
                var scope = "openid profile";

                httpClient.BaseAddress = new Uri(baseAuthUrl);
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", grantType),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecrect),
                    new KeyValuePair<string, string>("scope", scope),
                    new KeyValuePair<string, string>("username", "david.nguyen@conexus.net"),
                    new KeyValuePair<string, string>("password", "123456")
                });
                var result = await httpClient.PostAsync("/connect/token", content);
                string resultContent = await result.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(resultContent);
                ShowResult(tokenResponse);
            }
        }

        private static void ShowResult(TokenResponse result)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.AccessToken) as JwtSecurityToken;

            var id = jsonToken.Claims
                .Where(x => x.Type == "sub")
                .Select(x => x.Value).FirstOrDefault();
            var userName = jsonToken.Claims
                .Where(x => x.Type == "preferred_username")
                .Select(x => x.Value)
                .FirstOrDefault();
            var roles = jsonToken.Claims
                .Where(x => x.Type == "role")
                .Select(x => x.Value).ToArray();
            var expirationUnix = long.Parse(jsonToken.Claims
                .Where(x => x.Type == "exp")
                .Select(x => x.Value)
                .FirstOrDefault());
            var expiration = (expirationUnix * 1000).ToDateTime().ToLocalTime();
            _logger.Information($"\nid: {id}");
            _logger.Information($"\nuserName: {userName}");
            _logger.Information($"\nroles: {JsonSerializer.Serialize(roles)}");
            _logger.Information($"\nexpiration: {expiration}");
        }
    }
}
