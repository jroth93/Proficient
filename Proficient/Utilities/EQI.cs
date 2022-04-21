using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.EntityFramework;
using System.Data.Common;
using System.Data.Entity;
using MySql.Data.MySqlClient;


namespace Proficient.Utilities
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class EQI : DbContext
    {
        public DbSet<DesignNote> DesignNotes { get; set; }

        public EQI() : base()
        {
        }

        // Constructor to use on a DbConnection that is already opened
        public EQI(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class DesignNote
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Markdown { get; set; }
    }

    class EQIConnection
    {
        public static DesignNote dn { get; set; }
        public const string connectionString = "server=10.10.0.17;port=3306;database=eqi;uid=jroth;password=ahuskynamedscout";
        public static string GetDesignNoteEntry()
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (EQI context = new EQI(connection, false))
                {
                    dn = context.DesignNotes.First();
                    return dn.Markdown;
                }
            }
        }
        public static void SetDesignNoteEntry(string md)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                dn.Markdown = md;
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (EQI context = new EQI(connection, false))
                    {
                        context.Database.UseTransaction(transaction);

                        context.DesignNotes.Attach(dn);
                        context.Entry(dn).Property(x => x.Markdown).IsModified = true;

                        context.SaveChanges();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }
    }
}
