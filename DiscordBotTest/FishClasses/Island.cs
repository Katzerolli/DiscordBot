using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.FishClasses
{
    public class Island
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int? regionId { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public int? earth { get; set; }
        public int? sand { get; set; }
        public int? shore { get; set; }
    }
}
