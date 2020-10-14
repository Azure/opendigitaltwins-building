using Azure.Core;
using System.Threading;
using System.Threading.Tasks;

namespace AdtModelVisualizer.Services
{
    public class AdtApiTokenCredential : TokenCredential
    {
        private readonly ITokenService _tokenService;

        public AdtApiTokenCredential(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public async override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var accessToken = await _tokenService.GetAccessToken();
            return accessToken; 
        }
    }
}
