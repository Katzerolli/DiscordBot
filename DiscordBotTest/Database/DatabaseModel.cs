using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotTest.DatabaseContext
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Clan> Clan { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(@$"Data Source = {Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Database\Dummy.db");
        }
    }


    public class User
    {
        public int LID { get; set; }
        public DateTime DTINSERT { get; set; }
        public int LUSERIDINSERT { get; set; }
        public DateTime DTEDIT { get; set; }
        public  int LUSERID { get; set; }


    }

    public class Clan
    {
        public int LID { get; set; }
        public DateTime DTINSERT { get; set; }
        public int LUSERIDINSERT { get; set; }
        public DateTime DTEDIT { get; set; }
        public int LUSERID { get; set; }


    }


}