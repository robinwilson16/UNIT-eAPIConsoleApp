using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models;

public partial class NetsuiteContext : DbContext
{
    public NetsuiteContext()
    {
    }

    public NetsuiteContext(DbContextOptions<NetsuiteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LookupCampus> LookupCampus { get; set; }
    public virtual DbSet<Setting> Settings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=uk-btn-sql8;Initial Catalog=Netsuite;persist security info=true;TrustServerCertificate=True;Integrated Security=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.Property(e => e.Enviroment)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteAccountID)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteConsumerKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteConsumerSecret)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteTokenID)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteTokenSecret)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NetSuiteURL)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UniteAPIKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UniteBaseURL)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UniteTokenURL)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
