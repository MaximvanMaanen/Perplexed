using ArgentPonyWarcraftClient;
using System;

namespace guild_instinct.Models.GuildData
{
    public class InstinctGuildMember : GuildMember
    {
        public int Level;
        public string Name;
        public string PlayableClass;
        public string PlayableRace;
        public string Realm;
        public string RankName { get; set; }
    }
}
