using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;

namespace AuthServer.Services;

public class OIDCService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    private string RedirectUri { get; set; } = "http://localhost:7091/api/Auth/oidc/signin";
    private string ClientId { get; }
    private string ClientSecret { get; }

    public OIDCService(IHttpClientFactory httpClientFactory,IConfiguration config,string requestUrl)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;

        if (!string.IsNullOrEmpty(requestUrl))
            SetupRedirectUri(requestUrl);

        // Read redirectUri from configuration if it exists.
        RedirectUri =  !string.IsNullOrEmpty(_config["Authentication:Google:RedirectUri"])
            ? _config["Authentication:Google:RedirectUri"]
            : RedirectUri;
        ClientId = _config["Authentication:Google:ClientId"];
        ClientSecret = _config["Authentication:Google:ClientSecret"];
    }

    /// <summary>
    /// Request access token with authorization code.
    /// </summary>
    /// <param name="authorization_code"></param>
    /// <returns></returns>
    public async Task<string> GetIdTokenAsync(string authorization_code)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        AuthorizationCodeTokenRequest request = new()
        {
            Code = authorization_code,
            RedirectUri = RedirectUri,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            Scope = "openid profile email"
        };

        TokenResponse responce = await request.ExecuteAsync(client,
                                                            GoogleAuthConsts.OidcTokenUrl,
                                                            new(),
                                                            Google.Apis.Util.SystemClock.Default);

        return responce.IdToken;
    }



    public async Task<string> GetHDBIAMIdTokenAsync(string authorization_code)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        var values = new Dictionary<string, string>
          {
              { "grant_type", "authorization_code" },
              { "client_id", "client_id" },
              { "client_secret", "secret_code" },
              { "redirect_uri", RedirectUri },
              { "code", "authorization_code" },
          };

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(_config["Authentication:HDBIAM:ApiUrl"], content);
        var responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }



    public async Task<string> GetAADTokenAsync(string authorization_code)
    {
        using HttpClient client = _httpClientFactory.CreateClient();
        var values = new Dictionary<string, string>
          {
              { "grant_type", "authorization_code" },
              { "scope", "openid" },
              { "client_id", _config["AzureAd:ClientId"] },
              { "client_secret", _config["AzureAd:Secret"]},
              { "redirect_uri", "https://localhost:7091/api/Auth/oidc/signinaad" },
              { "code", authorization_code},
          };

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync("https://login.microsoftonline.com/902c3174-006f-48cb-aa59-69e528715f63/oauth2/v2.0/token", content);
        var responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }





    internal void SetupRedirectUri(string requestUrl, string? route = "api/Auth/oidc/signin")
    {
        Uri uri = new(requestUrl);
        string port = uri.Scheme == "https" && uri.Port == 443
                      || uri.Scheme == "http" && uri.Port == 80
                      ? ""
                      : $":{uri.Port}";
        RedirectUri = $"{uri.Scheme}://{uri.Host}{port}/{route}";
    }
    
}
