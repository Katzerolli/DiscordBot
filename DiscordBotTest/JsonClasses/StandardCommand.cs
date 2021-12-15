using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotTest.JsonClasses
{
    public class hentaiV3
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Response response { get; set; }
        public Status status { get; set; }
    }

    public class Response
    {
        public string url { get; set; }
    }

    public class Status
    {
        public int code { get; set; }
        public object message { get; set; }
        public string rendered_in { get; set; }
        public bool success { get; set; }
    }

    public class hentaiV2
    {
        public string url { get; set; }
    }

    public class hentaiV1
    {
        public string url { get; set; }
    }

    public class xkcd
    {
        public string month { get; set; }
        public int num { get; set; }
        public string link { get; set; }
        public string year { get; set; }
        public string news { get; set; }
        public string safe_title { get; set; }
        public string transcript { get; set; }
        public string alt { get; set; }
        public string img { get; set; }
        public string title { get; set; }
        public string day { get; set; }
    }

    public class cat
    {
        public string id { get; set; }
        public string url { get; set; }
        public catcategorie[] categories { get; set; }
        public catbreed[] breeds { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class catbreed
    {
        public string id { get; set; }
        public string name { get; set; }
        public string temperament { get; set; }
        public string life_span { get; set; }
        public string alt_names { get; set; }
        public string wikipedia_url { get; set; }
        public string origin { get; set; }
        public string weight_imperial { get; set; }
        public int experimental { get; set; }
        public int hairless { get; set; }
        public int natural { get; set; }
        public int rare { get; set; }
        public int rex { get; set; }
        public int suppres_tail { get; set; }
        public int short_legs { get; set; }
        public int hypoallergenic { get; set; }
        public int adaptability { get; set; }
        public int affection_level { get; set; }
        public string country_code { get; set; }
        public int child_friendly { get; set; }
        public int dog_friendly { get; set; }
        public int energy_level { get; set; }
        public int grooming { get; set; }
        public int health_issues { get; set; }
        public int intelligence { get; set; }
        public int shedding_level { get; set; }
        public int social_needs { get; set; }
        public int stranger_friendly { get; set; }
        public int vocalisation { get; set; }
    }

    public class catcategorie
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class echse
    {
        public string url { get; set; }
    }


    public class Asset
    {
        public int appid { get; set; }
        public string contextid { get; set; }
        public string assetid { get; set; }
        public string classid { get; set; }
        public string instanceid { get; set; }
        public string amount { get; set; }
    }

    public class Description
    {
        public string type { get; set; }
        public string value { get; set; }
        public string color { get; set; }
        public int appid { get; set; }
        public string classid { get; set; }
        public string instanceid { get; set; }
        public int currency { get; set; }
        public string background_color { get; set; }
        public string icon_url { get; set; }
        public string icon_url_large { get; set; }
        public List<Description> descriptions { get; set; }
        public int tradable { get; set; }
        public List<Action> actions { get; set; }
        public string name { get; set; }
        public string name_color { get; set; }
        public string market_name { get; set; }
        public string market_hash_name { get; set; }
        public List<MarketAction> market_actions { get; set; }
        public int commodity { get; set; }
        public int market_tradable_restriction { get; set; }
        public int marketable { get; set; }
        public List<Tag> tags { get; set; }
        public string market_buy_country_restriction { get; set; }
    }

    public class Action
    {
        public string link { get; set; }
        public string name { get; set; }
    }

    public class MarketAction
    {
        public string link { get; set; }
        public string name { get; set; }
    }

    public class Tag
    {
        public string category { get; set; }
        public string internal_name { get; set; }
        public string localized_category_name { get; set; }
        public string localized_tag_name { get; set; }
        public string color { get; set; }
    }

    public class CSGOInv
    {
        public List<Asset> assets { get; set; }
        public List<Description> descriptions { get; set; }
        public int total_inventory_count { get; set; }
        public int success { get; set; }
        public int rwgrsn { get; set; }
    }

}
