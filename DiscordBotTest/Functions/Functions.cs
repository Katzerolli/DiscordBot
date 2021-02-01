using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DiscordBotTest.Functions
{
    public static class Functions
    {
        private static readonly string dblocation = $@"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Database\ClanDatabase.db";

        public static ConfigJson ReadConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead($@"{Environment.CurrentDirectory}\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }


        #region SQL User Commands

        public static void AddUser(long userIdInsert, long userId, bool admin, long clanLid = 0, long roleLid = 0)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO DSUSER (DTINSERT, LUSERIDINSERT, USERID, ADMIN, REF_CLANID, REF_ROLE) VALUES ($DTINSERT, $LUSERIDINSERT, $USERID, $ADMIN, $REF_CLANID, $REF_ROLE);";
                command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                command.Parameters.AddWithValue("$USERID", userId);
                command.Parameters.AddWithValue("$ADMIN", admin);
                if (clanLid == 0)
                {
                    command.Parameters.AddWithValue("$REF_CLANID", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("$REF_CLANID", clanLid);
                }
                if (roleLid == 0)
                {
                    command.Parameters.AddWithValue("$REF_ROLE", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("$REF_ROLE", roleLid);
                }
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void UpdateUser(long lUserId, long userId, bool admin, long clanLid, long roleLid)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE DSUSER SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, ADMIN = $ADMIN, REF_CLANID = $REF_CLANID, REF_ROLE = $REF_ROLE WHERE USERID = $USERID;";
                command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERID", lUserId);
                command.Parameters.AddWithValue("$ADMIN", admin);
                if (clanLid == 0)
                {
                    command.Parameters.AddWithValue("$REF_CLANID", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("$REF_CLANID", clanLid);
                }
                if (roleLid == 0)
                {
                    command.Parameters.AddWithValue("$REF_ROLE", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("$REF_ROLE", roleLid);
                }
                command.Parameters.AddWithValue("$USERID", userId);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static userResult SelectUser(long userId)
        {
            userResult result = new userResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSUSER WHERE USERID = $USERID";
                command.Parameters.AddWithValue("$USERID", userId);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = (long)r["LID"];
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = (long)r["LUSERIDINSERT"];
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : (long)r["LUSERID"];
                    result.USERID = (long)r["USERID"];
                    result.ADMIN = Convert.ToBoolean(r["ADMIN"]);
                    result.REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : (long)r["REF_CLANID"];
                    result.REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?)null : (long)r["REF_ROLE"];
                }
                connection.Close();
            }
            return result;
        }

        public static List<userResult> SelectClanMember(long clanId, List<long> roleId)
        {
            var result = new List<userResult>();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var roles = string.Join(", ", roleId);
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSUSER WHERE REF_CLAN = $CLANID AND REF_ROLE = $ROLEID";
                command.Parameters.AddWithValue("$CLANID", clanId);
                command.Parameters.AddWithValue("$ROLEID", roles);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    var tmp = new userResult()
                    {
                        LID = (long) r["LID"],
                        DTINSERT = Convert.ToDateTime(r["DTINSERT"]),
                        LUSERIDINSERT = (long) r["LUSERIDINSERT"],
                        DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?) null : Convert.ToDateTime(r["DTEDIT"]),
                        LUSERID = r["LUSERID"] == DBNull.Value ? (long?) null : (long) r["LUSERID"],
                        USERID = (long) r["USERID"],
                        ADMIN = Convert.ToBoolean(r["ADMIN"]),
                        REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?) null : (long) r["REF_CLANID"],
                        REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?) null : (long) r["REF_ROLE"]
                    };
                    result.Add(tmp);
                }
                connection.Close();
            }
            return result;
        }

        public static userResult SelectAllUser()
        {
            userResult result = new userResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSUSER";

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = (long)r["LID"];
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = (long)r["LUSERIDINSERT"];
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : (long)r["LUSERID"];
                    result.USERID = (long)r["USERID"];
                    result.ADMIN = Convert.ToBoolean(r["ADMIN"]);
                    result.REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : (long)r["REF_CLANID"];
                    result.REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?)null : (long)r["REF_ROLE"];
                }
                connection.Close();
            }
            return result;
        }

        public static List<userResult> SelectClanUser([Description("Die Datenbank LID des Clans")]long clanId)
        {
            List<userResult> result = new List<userResult>();

            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSUSER WHERE REF_CLANID = $CLANID";
                command.Parameters.AddWithValue("$CLANID", clanId);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    var tmp = new userResult()
                    {
                        LID = Convert.ToInt64(r["LID"]),
                        DTINSERT = Convert.ToDateTime(r["DTINSERT"]),
                        LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]),
                        DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]),
                        LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]),
                        USERID = Convert.ToInt64(r["USERID"]),
                        ADMIN = Convert.ToBoolean(r["ADMIN"]),
                        REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_CLANID"]),
                        REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_ROLE"])
                    };
                    result.Add(tmp);
                }
                connection.Close();
            }
            return result;
        }

        public static List<Tuple<long, string>> CountClanMember(long clanId, List<long> roleId)
        {
            var result = new List<Tuple<long, string>>();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                foreach (var role in roleId)
                {
                    var tmp = new Tuple<long, string>(0,string.Empty);

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT COUNT(*) FROM DSUSER WHERE REF_CLANID = $CLANID AND REF_ROLE = $ROLEID";
                    command.Parameters.AddWithValue("$CLANID", clanId);
                    command.Parameters.AddWithValue("$ROLEID", role);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        tmp = new Tuple<long, string>(role, r.ToString());
                    }
                    result.Add(tmp);
                }

                connection.Close();
            }
            return result;
        }

        public static void DeleteUser(long userId)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"DELETE FROM DSUSER WHERE USERID = $USERID";
                command.Parameters.AddWithValue("$USERID", userId);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        #endregion

        public static void AddClan(long userIdInsert, long clanId, string clanName, string clanColor)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO DSCLAN (DTINSERT, LUSERIDINSERT, CLANID, CLANNAME, CLANCOLOR) VALUES ($DTINSERT, $LUSERIDINSERT, $CLANID, $CLANNAME, $CLANCOLOR);";
                command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                command.Parameters.AddWithValue("$CLANID", clanId);
                command.Parameters.AddWithValue("$CLANNAME", clanName);
                command.Parameters.AddWithValue("$CLANCOLOR", clanColor);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void UpdateClan(long userIdInsert, long clanId, string clanName, string clanColor)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE DSCLAN SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, CLANNAME = $CLANNAME, CLANCOLOR = $CLANCOLOR WHERE CLANID = $CLANID);";
                command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERID", userIdInsert);
                command.Parameters.AddWithValue("$CLANID", clanId);
                command.Parameters.AddWithValue("$CLANNAME", clanName);
                command.Parameters.AddWithValue("$CLANCOLOR", clanColor);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static clanResult SelectClanById(long clanId)
        {
            clanResult result = new clanResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANID = $CLANID";
                command.Parameters.AddWithValue("$CLANID", clanId);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = Convert.ToInt64(r["LID"]);
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                    result.CLANID = Convert.ToInt64(r["CLANID"]);
                    result.CLANNAME = r["CLANNAME"].ToString();
                    result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                }
                connection.Close();
            }
            return result;
        }

        public static clanResult SelectClanByName(string clanName)
        {
            clanResult result = new clanResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANNAME = $CLANNAME";
                command.Parameters.AddWithValue("$CLANNAME", clanName);

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = Convert.ToInt64(r["LID"]);
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                    result.CLANID = Convert.ToInt64(r["CLANID"]);
                    result.CLANNAME = r["CLANNAME"].ToString();
                    result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                }
                connection.Close();
            }
            return result;
        }

        public static clanResult SelectAllClan()
        {
            clanResult result = new clanResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSCLAN";

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = Convert.ToInt64(r["LID"]);
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                    result.CLANID = Convert.ToInt64(r["CLANID"]);
                    result.CLANNAME = r["CLANNAME"].ToString();
                    result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                }
                connection.Close();
            }
            return result;
        }

        [Description("Entfernt Eintrag aus DSCLAN. Damit ein Clan gelöscht werden kann müssen erst Benutzer entfernt werden!")]
        public static void DeleteClan(long clanId)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"DELETE FROM DSCLAN WHERE CLANID = $CLANID);";
                command.Parameters.AddWithValue("$CLANID", clanId);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }


        public static void AddRole(long userIdInsert, long roleId, string roleName)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO DSROLE (DTINSERT, LUSERIDINSERT, ROLEID, ROLENAME) VALUES ($DTINSERT, $LUSERIDINSERT, $ROLEID, $ROLENAME);";
                command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                command.Parameters.AddWithValue("$ROLEID", roleId);
                command.Parameters.AddWithValue("$ROLENAME", roleName);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void UpdateRole(long userIdEdit, long roleId, string roleName)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE DSROLE SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, ROLENAME = $ROLENAME WHERE ROLEID = $ROLEID);";
                command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                command.Parameters.AddWithValue("$LUSERID", userIdEdit);
                command.Parameters.AddWithValue("$ROLENAME", roleName);
                command.Parameters.AddWithValue("$ROLEID", roleId);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static roleResult SelectRoleByName(string roleName)
        {
            roleResult result = new roleResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSROLE WHERE ROLENAME = $ROLENAME";
                command.Parameters.AddWithValue("$ROLENAME", roleName);
                 
                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = Convert.ToInt64(r["LID"]);
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                    result.ROLEID = Convert.ToInt64(r["ROLEID"]);
                    result.ROLENAME = r["ROLENAME"].ToString();
                }
                connection.Close();
            }
            return result;
        }

        public static roleResult SelectAllRole()
        {
            roleResult result = new roleResult();
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT * FROM DSROLE";

                using var r = command.ExecuteReader();
                while (r.HasRows && r.Read())
                {
                    result.LID = Convert.ToInt64(r["LID"]);
                    result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                    result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                    result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                    result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                    result.ROLEID = Convert.ToInt64(r["ROLEID"]);
                    result.ROLENAME = r["ROLENAME"].ToString();
                }
                connection.Close();
            }
            return result;
        }

        public static void DeleteRole(long roleId)
        {
            using (var connection = new SqliteConnection($"Data Source={dblocation}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"DELETE FROM DSROLE WHERE ROLEID = $ROLEID);";
                command.Parameters.AddWithValue("$ROLEID", roleId);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public class clanResult
        {
            public long LID { get; set; }
            public DateTime DTINSERT { get; set; }
            public long LUSERIDINSERT { get; set; }
            public DateTime? DTEDIT { get; set; }
            public long? LUSERID { get; set; }
            public long CLANID { get; set; }
            public string CLANNAME { get; set; }
            public string? CLANCOLOR { get; set; }
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
            public long LID { get; set; }
            public DateTime DTINSERT { get; set; }
            public long LUSERIDINSERT { get; set; }
            public DateTime? DTEDIT { get; set; }
            public long? LUSERID { get; set; }
            public long ROLEID { get; set; }
            public string ROLENAME { get; set; }
        }
    }
}
