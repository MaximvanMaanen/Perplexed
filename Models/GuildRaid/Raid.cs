using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace guild_instinct.Models.GuildRaid
{
    public class Raid
    {
        public DateTime raidTime;

        public int MeleeClasses;
        public int RangedClasses;

        public int DamageDealers;
        public int Healers;
        public int Tanks;
    }
}
