using AuthServer.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace AuthServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly OIDCService _oidcService;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;
    public AuthController(
        OIDCService oidcService,
        IConfiguration config,
        IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition)
    {
        _oidcService = oidcService;
        _config = config;
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
    }

    [HttpGet("oidc/signin", Name = nameof(SigninOIDCAsync))]
    public async Task<IActionResult> SigninOIDCAsync(string code, string state, string? error)
    {
        
        if (state != "12345GG")
        {
            return BadRequest();
        }

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        string idToken = await _oidcService.GetIdTokenAsync(code);

        var link = $"{_config["FrontEndUri"]}?idToken={idToken}";


        return Redirect($"{_config["FrontEndUri"]}?idToken={idToken}");
    }


    [HttpGet("oidcHDBIAM/HDBIAMsignin", Name = nameof(SigninOIDCHDBIAMAsync))]
    public async Task<IActionResult> SigninOIDCHDBIAMAsync(string code, string state, string? error)
    {
        if (state != "12345GG")
        {
            return BadRequest();
        }

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        string idToken = await _oidcService.GetHDBIAMIdTokenAsync(code);
       
        return Redirect($"{_config["FrontEndUri"]}?idToken={idToken}");
    }

    [HttpGet("oidcHDBIAMAuthorizeCall/HDBIAMAuthorize", Name = nameof(HDBIAMAuthorizeAsync))]
    public IActionResult HDBIAMAuthorizeAsync()
    {
        //var link = $"{_config["FrontEndUri"]}?idToken=asdsasddsadsadsadadsadasdsadsdsadsadd";
        var url = $"https://iam.hdb.gov.sg/oidc/authorize?response_type=code&scope=openid&client_id={_config["Authentication:IAM:client_id"]}&nonce=a2ghskf1234las&state=af0ifjsldkj&redirect_nuri=https%3A%2F%2Fclient.example.org%2Fcb";
        return Ok(url);
    }



    /// <summary>
    /// AAD Authentication
    /// </summary>
    /// <param name="code"></param>
    /// <param name="state"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>


    [HttpGet("oidc/signinaad", Name = nameof(SigninAADAsync))]
    public async Task<IActionResult> SigninAADAsync(string code, string state, string? error)
    {
        if (state != "12345GG")
        {
            return BadRequest();
        }

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        string idToken = await _oidcService.GetAADTokenAsync(code);

        var link = $"{_config["FrontEndUri"]}?idToken={idToken}";


        return Redirect($"{_config["FrontEndUri"]}?idToken={idToken}");
    }


    [HttpGet("oidcAADAuthorizeCall/AADAuthorize", Name = nameof(AADAuthorizeAsync))]
    public IActionResult AADAuthorizeAsync()
    {


        var values = new Dictionary<string, string>
          {
              { "client_id", _config["AzureAd:ClientId"] },
              { "response_type",_config["AzureAd:response_type"] },
              { "redirect_uri", _config["AzureAd:redirect_uri"] },
              { "response_mode", _config["AzureAd:response_mode"]},
              { "scope", _config["AzureAd:scope"]},
              { "state", _config["AzureAd:state"]},
              { "nonce", _config["AzureAd:nonce"]},
          };
        var query = new QueryBuilder(values);


        var url = _config["AzureAd:Instance"] + "/" + _config["AzureAd:TenantId"] + _config["AzureAd:authorize"] + query.ToQueryString();



        return Ok(url);
    }







}

