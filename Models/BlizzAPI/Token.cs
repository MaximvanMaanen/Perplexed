﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace guild_instinct.Models.BlizzAPI
{
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string bearer { get; set; }
        public string scope { get; set; }
    }
}
