using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using guild_instinct.Services;
using guild_instinct.Models;
using Microsoft.Extensions.Configuration;
using CookieManager;
using guild_instinct.Models.BlizzAPI;
using ArgentPonyWarcraftClient;

namespace guild_instinct.Controllers
{
    public class OAuthController : Controller
    {
        private readonly OAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ICookieManager _cookieManager;
        private readonly IWarcraftClient _warcraftClient;

        public OAuthController(OAuthService authService, IConfiguration configuration, ICookieManager cookieManager, IWarcraftClient warcraftClient)
        {
            _authService = authService;
            _configuration = configuration;
            _cookieManager = cookieManager;
            _warcraftClient = warcraftClient;
        }

        public async Task<IActionResult> Login([FromQuery]string code)
        {
            // TODO 
            // fill in the right url.
            string redirectURL = "https://eu.battle.net/oauth/authorize?client_id=8e63e6eccd604eab926424739f396985&scope=wow.profile&redirect_uri=https%3A%2F%2Flocalhost%3A44393%2FOAuth%2FLogin&response_type=code&state=";
            
            switch (await _authService.LoginAsync(code, _configuration, _cookieManager, _warcraftClient))
            {
                case AuthResponseType.Success:
                    return RedirectToAction("LoginSuccesful");
                case AuthResponseType.RequiresAuth:
                    return Redirect("https://eu.battle.net/oauth/authorize?client_id=8e63e6eccd604eab926424739f396985&scope=wow.profile&redirect_uri=https%3A%2F%2Flocalhost%3A44393%2FOAuth%2FLogin&response_type=code&state=");
                case AuthResponseType.Declined:
                    return RedirectToAction("LoginDeclined");

                default:
                    return Redirect(redirectURL);
            }
        }

        public IActionResult LoginSuccesful()
        {
            OAuthCookie cookie = _cookieManager.Get<OAuthCookie>("BNetProfile");

            return View("../Instinct/LoginSucceeded", cookie);
        }

        public IActionResult LoginDeclined()
        {
            return View("../Home/Index");
        }
    }
}