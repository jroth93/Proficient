using MySql.Data.EntityFramework;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;

namespace Proficient.Utilities;

[DbConfigurationType(typeof(MySqlEFConfiguration))]
public class MeiDb : DbContext
{
    public DbSet<EqiNote> EqiNotes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserRevitVersion> UserRevitVersions { get; set; } = null!;
    public MeiDb() {}
    public MeiDb(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection){}
}

public class EqiNote
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Markdown { get; set; } = string.Empty;
}

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AutodeskUser { get; set; } = string.Empty;
    public bool GlobalNoteEditor { get; set; }
    public bool GlobalNoteManager { get; set; }
    public string ProficientVersion { get; set; } = string.Empty;
}

public class UserRevitVersion
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }
    [Key]
    [Column(Order = 2)]
    public int VersionNumber { get; set; }
    public string VersionBuild { get; set; } = string.Empty;
}

internal class MeiDbConn
{
    private const string ConnectionString = "server=10.10.0.17;port=3306;database=mei;uid=jroth;password=ahuskynamedscout";
    public static EqiNote? GetEqiNote(int id)
    {
        using MySqlConnection connection = new (ConnectionString);
        connection.Open();

        using MeiDb context = new (connection, false);
        var notes = context.EqiNotes.Where(n => n.Id == id);
        return notes.Any() ? notes.First() : null;
    }

    public static List<EqiNote> GetEqiNotes()
    {
        using MySqlConnection connection = new(ConnectionString);
        connection.Open();

        using MeiDb context = new (connection, false);
        return [.. context.EqiNotes];
    }

    public static int CreateEqiNote(string desc)
    {
        EqiNote dn = new()
        {
            Description = desc,
            Markdown = string.Empty
        };

        using MySqlConnection connection = new(ConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            using MeiDb context = new (connection, false);
            context.Database.UseTransaction(transaction);
            context.EqiNotes.Add(dn);
            context.SaveChanges();

            transaction.Commit();

            return dn.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    public static void SetEqiNote(int id, string md)
    {
        using MySqlConnection connection = new (ConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            using MeiDb context = new(connection, false);
            var dn = context.EqiNotes.FirstOrDefault(n => n.Id == id);
            if (dn == null)
            {
                transaction.Rollback();
                return;
            }
            dn.Markdown = md;

            context.Database.UseTransaction(transaction);
            context.EqiNotes.Attach(dn);
            context.Entry(dn).Property(x => x.Markdown).IsModified = true;
            context.SaveChanges();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static List<User> GetUsers()
    {
        using MySqlConnection connection = new (ConnectionString);
        using MeiDb context = new (connection, false);
        return context.Users.ToList();
    }

    public static User? GetUserByAdId(string adUser)
    {
        using MySqlConnection connection = new (ConnectionString);
        using MeiDb context = new (connection, false);
        return context.Users.FirstOrDefault(u => u.AutodeskUser == adUser);
    }

    public static User CreateUser(string autodeskUser, string proficientVersion, string firstName = "", string lastName = "",  bool globalNoteEditor = false, bool globalNoteManager = false)
    {
        User user = new()
        {
            FirstName = firstName,
            LastName = lastName,
            AutodeskUser = autodeskUser,
            GlobalNoteEditor = globalNoteEditor,
            GlobalNoteManager = globalNoteManager,
            ProficientVersion = proficientVersion
        };

        using MySqlConnection connection = new(ConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            using MeiDb context = new (connection, false);
            context.Database.UseTransaction(transaction);
            context.Users.Add(user);
            context.SaveChanges();
            transaction.Commit();

            return user;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static void SetUserProficientVersion(User user)
    {
        if (user == null) return;
        using MySqlConnection connection = new(ConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            using MeiDb context = new(connection, false);
            var dbUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (dbUser == null)
            {
                transaction.Rollback();
                return;
            }

            dbUser.ProficientVersion = user.ProficientVersion;
            
            context.Database.UseTransaction(transaction);
            context.Users.Attach(dbUser);
            context.Entry(dbUser).Property(x => x.ProficientVersion).IsModified = true;
            context.SaveChanges();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public static void SetUserRevitVersion(int userId, int version, string build)
    {
        using MySqlConnection connection = new (ConnectionString);
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            using MeiDb context = new(connection, false);
            var urv = context.UserRevitVersions
                .FirstOrDefault(x => x.UserId == userId && x.VersionNumber == version);

            if(urv is null)
            {
                context.UserRevitVersions.Add(new UserRevitVersion
                {
                    UserId = userId,
                    VersionNumber = version,
                    VersionBuild = build
                });
            }
            else
            {
                context.UserRevitVersions.Attach(urv);
                urv.VersionBuild = build;
                context.Entry(urv).Property(x => x.VersionBuild).IsModified = true;
            }

            context.Database.UseTransaction(transaction);
            context.SaveChanges();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}