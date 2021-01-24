using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DiscordBotTest.Functions
{
    public static class Functions
    {
        private static readonly string dblocation = @"C:\Users\Nik19\Desktop\DiscordBotTestStep\DiscordBotTest\Database\Dummy.db"; //TODO Dynamic Path

        public static ConfigJson ReadConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead($@"{Environment.CurrentDirectory}\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        //public static Object ExecuteQuery(string query) //string type, string table, string param
        //{

        //    var connectionStringBuilder = new SqliteConnectionStringBuilder();
        //    connectionStringBuilder.DataSource = $@"{Environment.CurrentDirectory}\Dummy.db";
        //    var con = new SqliteConnection(connectionStringBuilder.ConnectionString))

        //}
        public static void AddUser(ulong userIdInsert, ulong userId, bool admin)
        {
            List<clanResult> result = new List<clanResult>();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO DSUSER VALUES ($LID, $DTINSERT, $LUSERIDINSERT, $DTEDIT, $LUSERID, $USERID, $ADMIN, $REF_CLANID, $REF_CLANROLE);";
                command.Parameters.AddWithValue("$LID", null);
                command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                command.Parameters.AddWithValue("$DTEDIT", null);
                command.Parameters.AddWithValue("$LUSERID", null);
                command.Parameters.AddWithValue("$USERID", userId);
                command.Parameters.AddWithValue("$ADMIN", admin);
                command.Parameters.AddWithValue("$REF_CLANID", null);
                command.Parameters.AddWithValue("$REF_CLANROLE", null);
                command.ExecuteNonQuery();
            }
        }

        public static List<userResult> GetUserById(ulong userId)
        {
            List<userResult> result = new List<userResult>();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSUSER WHERE USERID = $userId";
                command.Parameters.AddWithValue("$userId", userId);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    //Console.WriteLine($"{r["LID"]}, {r["DTINSERT"]}, {r["LUSERIDINSERT"]}, {r["DTEDIT"]}, {r["LUSERID"]}, {r["USERID"]}, {r["ADMIN"]}, {r["REF_CLANID"]}, {r["REF_CLANROLE"]}");

                    userResult tmp = new userResult()
                    {
                        LID = Convert.ToInt64(r["LID"]),
                        DTINSERT = Convert.ToDateTime(r["DTINSERT"]),
                        LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]),
                        DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]),
                        LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]),
                        USERID = Convert.ToInt64(r["USERID"]),
                        ADMIN = Convert.ToBoolean(r["ADMIN"]),
                        REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_CLANID"]),
                        REF_CLANROLE = r["REF_CLANROLE"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_CLANROLE"])
                    };
                    result.Add(tmp);
                }
            }
            if(result.Count == 0)
            {
                return null;
            }
            else
            {
                return result;
            }
        }



        public static List<clanResult> GetClanById(ulong clanId)
        {
            List<clanResult> result = new List<clanResult>();
            using (var connection = new SqliteConnection("Data Source=./DB/AdventCalendarDB.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANID = $clanId";
                command.Parameters.AddWithValue("$clanId", clanId);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    clanResult tmp = new clanResult()
                    {
                        Id = Convert.ToInt32(r["LID"]),
                        DtInsert = Convert.ToDateTime(r["DTINSERT"]),
                        UserIdInsert = Convert.ToInt32(r["LUSERIDINSERT"]),
                        DtEdit = Convert.ToDateTime(r["DTEDIT"]),
                        UserId = Convert.ToInt32(r["LUSERID"]),
                        ClanId = Convert.ToInt32(r["CLANID"]),
                        ClanName = r["CLANNAME"].ToString(),
                        ClanColor = r["CLANNAME"].ToString()
                    };
                    result.Add(tmp);
                }
            }
            return result;
        }

        public static List<clanResult> GetClanByName(string clanName)
        {
            List<clanResult> result = new List<clanResult>();
            using (var connection = new SqliteConnection("Data Source=./DB/AdventCalendarDB.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANNAME = $clanName";
                command.Parameters.AddWithValue("$clanName", clanName);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    clanResult tmp = new clanResult()
                    {
                        Id = Convert.ToInt32(r["LID"]),
                        DtInsert = Convert.ToDateTime(r["DTINSERT"]),
                        UserIdInsert = Convert.ToInt32(r["LUSERIDINSERT"]),
                        DtEdit = Convert.ToDateTime(r["DTEDIT"]),
                        UserId = Convert.ToInt32(r["LUSERID"]),
                        ClanId = Convert.ToInt32(r["CLANID"]),
                        ClanName = r["CLANNAME"].ToString(),
                        ClanColor = r["CLANNAME"].ToString()
                    };
                    result.Add(tmp);
                }
            }
            return result;
        }


    }

    public class clanResult
    {
        public int Id { get; set; }
        public DateTime DtInsert { get; set; }
        public int UserIdInsert { get; set; }
        public DateTime? DtEdit { get; set; }
        public int? UserId { get; set; }
        public int ClanId { get; set; }
        public string ClanName { get; set; }
        public string ClanColor { get; set; }
    }

    public class userResult
    {
        public long LID { get; set; }
        public DateTime DTINSERT { get; set; }
        public long LUSERIDINSERT { get; set; }
        public DateTime? DTEDIT { get; set; }
        public long? LUSERID { get; set; }
        public long USERID { get; set; }
        public bool ADMIN { get; set; }
        public long? REF_CLANID { get; set; }
        public long? REF_CLANROLE { get; set; }
    }

    public class roleResult
    {
        public int LID { get; set; }
        public DateTime DTINSERT { get; set; }
        public int LUSERIDINSERT { get; set; }
        public DateTime DTEDIT { get; set; }
        public int LUSERID { get; set; }
        public int ROLEID { get; set; }
        public string ROLENAME { get; set; }
    }



}

