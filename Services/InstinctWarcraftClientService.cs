using ArgentPonyWarcraftClient;
using guild_instinct.Models.GuildData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace guild_instinct.Services
{
    public class InstinctWarcraftClientService
    {
        private readonly IWarcraftClient _warcraftClient;

        public InstinctWarcraftClientService(IWarcraftClient warcraftClient)
        {
            _warcraftClient = warcraftClient;
        }

        /// <returns>A list of all guild members.</returns>
        public List<InstinctGuildMember> PrepareGuildMembersList()
        {
            List<GuildMember> warcraftGuildMembers = _warcraftClient.GetGuildRosterAsync("draenor", "perplexed", "profile-eu")
                                                    .Result.Value.Members.OrderBy(members => members.Rank)
                                                    .ThenBy(members => members.Character.Name)
                                                    .ToList();

            // We call the raceIndex and the classIndex here to reduce the total amount of API calls.
            PlayableRacesIndex raceIndex = _warcraftClient.GetPlayableRacesIndexAsync("static-eu").Result.Value;
            PlayableClassesIndex classIndex = _warcraftClient.GetPlayableClassesIndexAsync("static-eu").Result.Value;

            // We create a new list with our own model of InstinctGuildMembers
            List<InstinctGuildMember> instinctGuildMembers = new List<InstinctGuildMember>();
            foreach (GuildMember member in warcraftGuildMembers)
            {
                InstinctGuildMember newModel = ConvertGuildMemberModel(member, classIndex, raceIndex);

                instinctGuildMembers.Add(newModel);
            }

            return instinctGuildMembers;
        }

        /// <summary>
        /// Sets the needed information for our new InstinctGuildMember Model.
        /// </summary>
        /// <param name="oldModel">The ArgentPonyWarcraftClient GuildMember Model</param>
        /// <returns>The new InstinctGuildMember model with all the set information</returns>
        public InstinctGuildMember ConvertGuildMemberModel(GuildMember oldModel, PlayableClassesIndex classIndex, PlayableRacesIndex raceIndex)
        {
            InstinctGuildMember instinctMember = new InstinctGuildMember();
            instinctMember.Level = oldModel.Character.Level;
            instinctMember.Name = oldModel.Character.Name;
            instinctMember.PlayableClass = SetCorrectPlayableClassName(oldModel, classIndex);
            instinctMember.PlayableRace = SetCorrectPlayableRaceName(oldModel, raceIndex);
            instinctMember.Realm = ToUpperFirst(oldModel.Character.Realm.Slug);
            instinctMember.RankName = SetCorrectRankName(oldModel);

            return instinctMember;
        }

        /// <summary>
        /// Returns the correct rank name of the guild member based on the GuildMember.Rank
        /// </summary>
        /// <param name="guildMember">The ArgentPonyWarcraftClient GuildMember Model</param>
        /// <returns>The rank name</returns>
        public string SetCorrectRankName(GuildMember guildMember)
        {
            switch (guildMember.Rank)
            {
                case 0:
                    return "Guild Leader";
                case 1:
                    return "Officer";
                case 2:
                    return "Officer (Alt)";
                case 3:
                    return "Raider";
                case 4:
                    return "Trial";
                case 5:
                    return "Social";
                case 6:
                    return "Alt";
                case 7:
                    return "Inactive";
                default:
                    return "Not Available";
            }
        }

        /// <summary>
        /// Returns the correct name of the played class based on the GuildMember.Character.PlayableClass.Id
        /// </summary>
        /// <param name="guildMember">The ArgentPonyWarcraftClient GuildMember Model</param>
        /// <returns>The class name</returns>
        public string SetCorrectPlayableClassName(GuildMember guildMember, PlayableClassesIndex classIndex)
        {
            foreach(var playableClass in classIndex.Classes)
            {
                if (guildMember.Character.PlayableClass.Id == playableClass.Id)
                {
                    return playableClass.Name;
                }
            }

            return "Class not found";
        }

        /// <summary>
        /// Returns the correct race of the played class based on the GuildMember.Character.PlayableRace.Id
        /// </summary>
        /// <param name="guildMember">The ArgentPonyWarcraftClient GuildMember Model</param>
        /// <returns>The race name</returns>
        public string SetCorrectPlayableRaceName(GuildMember guildMember, PlayableRacesIndex raceIndex)
        {
            foreach (var playableRace in raceIndex.Races)
            {
                if (guildMember.Character.PlayableRace.Id == playableRace.Id)
                {
                    return playableRace.Name;
                }
            }

            return "Race not found.";
        }

        /// <summary>
        /// Returns the input string with the first character converted to uppercase
        /// </summary>
        public string ToUpperFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("There is no first letter");

            Span<char> a = stackalloc char[s.Length];
            s.AsSpan(1).CopyTo(a.Slice(1));
            a[0] = char.ToUpper(s[0]);
            return new string(a);
        }
    }
}
