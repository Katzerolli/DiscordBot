using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotTest
{
    class ApplicationDbContext : DbContext
    {

        //public Object GetAllMebmers()
        //{
        //    return ExecuteQuery("SELECT * FROM DSUSER");
        //}

        //public Object GetAllClans()
        //{
        //    return ExecuteQuery("SELECT * FROM DSCLAN");
        //}

        //public Object GetAllRoles()
        //{
        //    return ExecuteQuery("SELECT * FROM DSROLES");
        //}

        //public static Object ExecuteQuery(string query)
        //{
        //    var cs = @$"Data Source = {Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Dummy.db";
        //    using var con = new SqliteConnection(cs);

        //    con.Open();

        //    var cmd = new SqliteCommand(query, con);
        //    var result = cmd.ExecuteScalar();

        //    con.Close();

        //    return result;
        //}
    }
}
