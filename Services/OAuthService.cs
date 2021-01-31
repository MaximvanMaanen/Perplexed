using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using guild_instinct.Models;
using Newtonsoft.Json;
using guild_instinct.Models.BlizzAPI;
using CookieManager;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

// https://flurl.dev/docs/fluent-http/
using Flurl.Http;
using ArgentPonyWarcraftClient;

namespace guild_instinct.Services
{
    public class OAuthService
    {
        public async void SignOut()
        {
            string logoutRedirect = "https://battle.net/login/logout?redirect_uri=https://localhost:44393/Home";
            await logoutRedirect.GetAsync();
        }

        /// <summary>
        /// Handles the OAuth 2.0 process of the Blizzard login using ArgentPonyWarcraftClient and Flurl.
        /// </summary>
        /// <param name="authCode">The Authentication Code from Blizzard's OAuth 2.0</param>
        /// <param name="configuration">Allows us to call the appsettings.json configurations.</param>
        /// <returns></returns>
        public async Task<AuthResponseType> LoginAsync(string authCode, IConfiguration configuration, ICookieManager cookieManager, IWarcraftClient warcraftClient)
        {
            if (authCode == null)
            {
                return IsCookiePresent(cookieManager);
            }
            else
            {
                // Get token || No idea what happens here tbh.
                var values = new Dictionary<string, string>
                {
                    { "region", configuration["BattleNet:Region"] },
                    { "grant_type", "authorization_code" },
                    { "code", authCode },
                    { "redirect_uri", "https://localhost:44393/OAuth/Login" },
                    { "scope", "wow.profile" }
                };

                var tokenReq = await @"https://eu.battle.net/oauth/token"
                    .WithBasicAuth(configuration["BattleNet:ClientId"], configuration["BattleNet:ClientSecret"])
                    .PostUrlEncodedAsync(values)
                    .ReceiveString();

                var token = JsonConvert.DeserializeObject<Token>(tokenReq);

                if(token.scope == null)
                {
                    // OAuth failed on Blizzard's side.
                    return AuthResponseType.Declined;
                }

                var userInfo = await @"https://eu.battle.net/oauth/userinfo"
                    .WithOAuthBearerToken(token.access_token).GetJsonAsync<UserInfo>();

                //Create cookie
                OAuthCookie cookie = cookieManager.GetOrSet("BNetProfile", () =>
                {
                    return new OAuthCookie()
                    {
                        Id = Guid.NewGuid().ToString(),
                        AccessToken = token.access_token,
                        Battletag = userInfo.BattleTag,
                        Rank = AssignGuildMemberRank(warcraftClient, token).Result
                    };

                }, new CookieOptions() { HttpOnly = true, Expires = DateTime.Now.AddDays(1), SameSite = SameSiteMode.Strict, Secure = true });

                // All is well, return the user to the LoginSuccesful View.
                return AuthResponseType.Success;
            }
        }

        // TODO Refactor this method.
        private async Task<string> AssignGuildMemberRank(IWarcraftClient warcraftClient, Token token)
        {
            // We retrieve a list of all our Guild Members.
            List<GuildMember> warcraftGuildMembers = warcraftClient.GetGuildRosterAsync("draenor", "perplexed", "profile-eu")
                        .Result.Value.Members.OrderBy(members => members.Rank)
                        .ThenBy(members => members.Character.Name)
                        .ToList();

            Dictionary<int, int> keyValueForRanks = new Dictionary<int, int>();
            
            for (int i = 0; i < warcraftGuildMembers.Count; i++)
            {
                keyValueForRanks.Add(warcraftGuildMembers[i].Character.Id, warcraftGuildMembers[i].Rank);
            }

            RequestResult<AccountProfileSummary> result = await warcraftClient.GetAccountProfileSummaryAsync(token.access_token, "profile-eu");

            bool Member = false;

            if (result.Success)
            {
                var wowAccounts = result.Value.WowAccounts;
                foreach (ArgentPonyWarcraftClient.WowAccount account in wowAccounts)
                {
                    foreach (ArgentPonyWarcraftClient.AccountCharacter character in account.Characters)
                    {
                        // The 2 refers to the ranks that exist in the guild. The API returns numbers. 0, 1 and 2 are officer ranks.
                        if (keyValueForRanks.TryGetValue(character.Id, out int rankNumberOfficer) && rankNumberOfficer <= 2)
                        {
                            return "Officer";
                        }

                        if (keyValueForRanks.TryGetValue(character.Id, out int rankNumberMember) && rankNumberMember >= 3)
                        {
                            Member = true;
                        }
                    }
                }

                if(Member)
                {
                    return "Member";
                }
                return "Guest";
            }

            // User does not have a BattleNet account.
            return "Failure Profile";
        }

        /// <summary>
        /// Check if the Cookie is present for our user.
        /// </summary>
        /// <param name="_cookieManager">The ICookieManager</param>
        /// <returns>A Response Type from the Blizzard API.</returns>
        public AuthResponseType IsCookiePresent(ICookieManager _cookieManager)
        {
            // Check if the token is present in the cookie.
            OAuthCookie cookie = _cookieManager.Get<OAuthCookie>("BNetProfile");
            if (cookie != null)
            {
                if (cookie.AccessToken != null && cookie.AccessToken != "")
                {
                    return AuthResponseType.Success;
                }
            }

            // If it does not exist, send the user to the Require Authentication page from Blizzard.
            return AuthResponseType.RequiresAuth;
        }
    }
}
