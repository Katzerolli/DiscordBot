using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.FishClasses
{
    public class Item
    {
        public int id { get; set; }
        public int? amount { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? itemTypeId { get; set; }
        public int? weight { get; set; }
        public int? worth { get; set; }
        public int? currencyId { get; set; }
        public int? baitId { get; set; }
        public int? regionId { get; set; }
        public string? imageURL { get; set; }
        public decimal? percentage { get; set; }
        public string? emoji { get; set; }
        public int? xp { get; set; }
    }
}
