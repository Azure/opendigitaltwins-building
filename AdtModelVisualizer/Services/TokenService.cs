using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace AdtModelVisualizer.Services
{
    public interface ITokenService
    {
        Task<string> GetToken();
        Task<AccessToken> GetAccessToken();
    }

    public class TokenService : ITokenService
    {
        private const string AdtAppId = "https://digitaltwins.azure.net";
        private readonly string _clientId;
        private readonly string _tenantId;
        private readonly string _clientSecret;
        private readonly IMemoryCache _cache;

        public TokenService(IConfiguration configuration, IMemoryCache cache)
        {
            _clientId = configuration.GetValue<string>("ClientId");
            _tenantId = configuration.GetValue<string>("TenantId");
            _clientSecret = configuration.GetValue<string>("ClientSecret");
            _cache = cache;
        }

        private async Task<AuthenticationResult> GetTokenInternal()
        {
            string[] scopes = new[] { AdtAppId + "/.default" };
            var app = ConfidentialClientApplicationBuilder.Create(_clientId)
                                                          .WithClientSecret(_clientSecret)
                                                          .WithTenantId(_tenantId)
                                                          .WithRedirectUri("http://localhost")
                                                          .Build();
            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return authResult;
        }

        public async Task<string> GetToken()
        {
            var token = await GetAccessToken();
            return token.Token;
        }

        public async Task<AccessToken> GetAccessToken()
        {
            var token = await _cache.GetOrCreateAsync(
                "adt-token",
                async (entity) =>
                {
                    var authResult = await GetTokenInternal();
                    entity.AbsoluteExpiration = authResult.ExpiresOn;
                    return new AccessToken(authResult.AccessToken, authResult.ExpiresOn);
                }
            );
            return token;
        }
    }
}