using ArgentPonyWarcraftClient;
using guild_instinct.Models.GuildData;
using guild_instinct.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace guild_instinct.Controllers
{
    public class InstinctController : Controller
    {
        private readonly IWarcraftClient _warcraftClient;
        private readonly InstinctWarcraftClientService _instinctService;

        public InstinctController(IWarcraftClient warcraftClient, InstinctWarcraftClientService instinctService)
        {
            _warcraftClient = warcraftClient;
            _instinctService = instinctService;
        }

        public IActionResult Index()
        {
            List<InstinctGuildMember> guildMembers = _instinctService.PrepareGuildMembersList();


            return View(guildMembers);
        }
    }
}
