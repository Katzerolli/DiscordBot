using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.FishClasses
{
    public class Player
    {
        public int id { get; set; }
        public long? userId { get; set; }
        public decimal? encumbrance { get; set; }
        public int? level { get; set; }
        public int? gold { get; set; }
        public int? doubloon { get; set; }
        public int? equippedBaitId { get; set; }
        public int? equippedRodId { get; set; }
        public int? currentIslandId { get; set; }
        public int? currentRegionId { get; set; }
        public bool? locked { get; set; }
    }
}
