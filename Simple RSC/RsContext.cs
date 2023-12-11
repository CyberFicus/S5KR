using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Simple_RSC;

public partial class RsContext : DbContext
{
    public RsContext()
    {
    }

    public RsContext(DbContextOptions<RsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exception> Exceptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string path = Path.GetFullPath("config.json");
        string readstring = "";
        try
        {
            readstring = File.ReadAllText(path);
        }
        catch
        {
            var config = new Config { ConnectionString = "Server=SERVERNAME;Database=DBNAME;Trusted_Connection=True;" };
            var str = JsonSerializer.Serialize(config);
            File.WriteAllText(path, str);
        }
        var deser = JsonSerializer.Deserialize<Config>(readstring);
        optionsBuilder.UseSqlServer(deser.ConnectionString);

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exception>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Exception");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.InputString).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(250);
            entity.Property(e => e.StackTrace).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

class Config
{
    public string ConnectionString { get; set; }
}