using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.FishClasses
{
    public class Fish
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int variantId { get; set; }
        public int regionId { get; set; }
        public int islandId { get; set; }
        public float weight { get; set; }
        public int worth { get; set; }
        public decimal percentage { get; set; }
        public int cookedness { get; set; }
        public string imageURL { get; set; }
        public int baitId { get; set; }
    }
}
