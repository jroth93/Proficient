using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using MySql.Data.EntityFramework;
using System.Data.Common;
using System.Data.Entity;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;


namespace Proficient.Utilities
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class MEIDB : DbContext
    {
        public DbSet<EQINote> EQINotes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRevitVersion> UserRevitVersions { get; set; }
        public MEIDB() : base(){}

        // Constructor to use on a DbConnection that is already opened
        public MEIDB(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection){}
        protected override void OnModelCreating(DbModelBuilder modelBuilder) => base.OnModelCreating(modelBuilder);
    }

    public class EQINote
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Markdown { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AutodeskUser { get; set; }
        public bool GlobalNoteEditor { get; set; }
        public bool GlobalNoteManager { get; set; }
        public string ProficientVersion { get; set; }
    }

    public class UserRevitVersion
    {
        [Key]
        [Column(Order = 1)]
        public int UserId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int VersionNumber { get; set; }
        public string VersionBuild { get; set; }
    }

    class MEIDBConn
    {
        public const string connectionString = "server=10.10.0.17;port=3306;database=mei;uid=jroth;password=ahuskynamedscout";
        public static EQINote GetEQINote(int id)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MEIDB context = new MEIDB(connection, false))
                {
                    var notes = context.EQINotes.Where(n => n.Id == id);
                    return notes.Any() ? notes.First() : null;
                }
            }
        }

        public static List<EQINote> GetEQINotes()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MEIDB context = new MEIDB(connection, false))
                {
                    return context.EQINotes.ToList();
                }
            }
        }

        public static int CreateEQINote(string desc)
        {
            EQINote dn = new EQINote()
            {
                Description = desc,
                Markdown = string.Empty
            };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (MEIDB context = new MEIDB(connection, false))
                    {
                        context.Database.UseTransaction(transaction);
                        context.EQINotes.Add(dn);
                        context.SaveChanges();
                    }

                    transaction.Commit();

                    return dn.Id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public static void SetEQINote(int id, string md)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (MEIDB context = new MEIDB(connection, false))
                    {
                        EQINote dn = context.EQINotes.Where(n => n.Id == id).FirstOrDefault();
                        dn.Markdown = md;

                        context.Database.UseTransaction(transaction);

                        context.EQINotes.Attach(dn);
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

        public static List<User> GetUsers()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MEIDB context = new MEIDB(connection, false))
                {
                    return context.Users.ToList();
                }
            }
        }

        public static User GetUserByAdId(string autodeskUser)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MEIDB context = new MEIDB(connection, false))
                {
                    return context.Users.Where(u => u.AutodeskUser == autodeskUser).FirstOrDefault();
                }
            }
        }

        public static User CreateUser(string autodeskUser, string proficientVersion, string firstName = "", string lastName = "",  bool globalNoteEditor = false, bool globalNoteManager = false)
        {
            User user = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                AutodeskUser = autodeskUser,
                GlobalNoteEditor = globalNoteEditor,
                GlobalNoteManager = globalNoteManager,
                ProficientVersion = proficientVersion
            };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (MEIDB context = new MEIDB(connection, false))
                    {
                        context.Database.UseTransaction(transaction);
                        context.Users.Add(user);
                        context.SaveChanges();
                    }

                    transaction.Commit();

                    return user;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void SetUserProficientVersion(User user)
        {
            if(user != null)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        using (MEIDB context = new MEIDB(connection, false))
                        {
                            User dbUser = context.Users.Where(u => u.Id == user.Id).FirstOrDefault();
                            dbUser.ProficientVersion = user.ProficientVersion;

                            context.Database.UseTransaction(transaction);

                            context.Users.Attach(dbUser);
                            context.Entry(dbUser).Property(x => x.ProficientVersion).IsModified = true;

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

        public static void SetUserRevitVersion(int userId, int version, string build)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    using (MEIDB context = new MEIDB(connection, false))
                    {
                        UserRevitVersion urv = context.UserRevitVersions
                            .Where(x => x.UserId == userId && x.VersionNumber == version)
                            .FirstOrDefault();

                        if(urv != null)
                        {
                            context.UserRevitVersions.Attach(urv);
                            urv.VersionBuild = build;
                            context.Entry(urv).Property(x => x.VersionBuild).IsModified = true;
                        }
                        else
                        {
                            context.UserRevitVersions.Add(new UserRevitVersion
                            {
                                UserId = userId,
                                VersionNumber = version,
                                VersionBuild = build
                            });
                        }

                        context.Database.UseTransaction(transaction);
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
