using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleClientWithBrowser
{
    public class Program
    {
        private readonly HttpClient _client;
        private static Logger _logger;
        private static IConfiguration _configuration;
        static string _authority = "https://demo.identityserver.io";
        static string _api = "https://demo.identityserver.io/api/test";

        static OidcClient _oidcClient;
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
                var response = httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest()
                {
                    Address = $"{_configuration["Authority"]}/connect/token",
                    ClientId = _configuration["ClientId"],
                    ClientSecret = _configuration["ClientSecret"],
                    GrantType = "passwordd",
                    Scope = _configuration["Scope"],
                    UserName = "david.nguyen@conexus.net",
                    Password = "123456"
                }).GetAwaiter().GetResult();
                ShowResult(response);
            }
        }

        private static void ShowResult(TokenResponse result)
        {
            if (result.IsError)
            {
                _logger.Error("\n\nError:\n{0}", result.Error);
                return;
            }
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.AccessToken) as JwtSecurityToken;
            //_logger.Information("\n\nClaims:");
            //foreach (var claim in jsonToken.Claims)
            //{
            //    _logger.Information("{0}: {1}", claim.Type, claim.Value);
            //}
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
            _logger.Information($"\nroles: {JsonConvert.SerializeObject(roles)}");
            _logger.Information($"\nexpiration: {expiration}");
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "  x...exit  c...call api   ";
            if (currentRefreshToken != null) menu += "r...refresh token   ";

            while (true)
            {
                Console.WriteLine("\n\n");

                Console.Write(menu);
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.X) return;
                if (key.Key == ConsoleKey.C) await CallApi(currentAccessToken);
                if (key.Key == ConsoleKey.R)
                {
                    var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
                    if (refreshResult.IsError)
                    {
                        Console.WriteLine($"Error: {refreshResult.Error}");
                    }
                    else
                    {
                        currentRefreshToken = refreshResult.RefreshToken;
                        currentAccessToken = refreshResult.AccessToken;

                        Console.WriteLine("\n\n");
                        Console.WriteLine($"access token:   {refreshResult.AccessToken}");
                        Console.WriteLine($"refresh token:  {refreshResult?.RefreshToken ?? "none"}");
                    }
                }
            }
        }

        private static async Task CallApi(string currentAccessToken)
        {
            _apiClient.SetBearerToken(currentAccessToken);
            var response = await _apiClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                var json = JArray.Parse(await response.Content.ReadAsStringAsync());
                Console.WriteLine("\n\n");
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }
    }
}
