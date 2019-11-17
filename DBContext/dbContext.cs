using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using lighthouse.Models;

namespace lighthouse.DBContext
{
    public partial class dbContext : DbContext
    {
        private string db_path;
        public dbContext(string source_db_path)
        {
            db_path = source_db_path;
        }

        public dbContext(DbContextOptions<dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Link> Link { get; set; }
        public virtual DbSet<LolNode> LolNode { get; set; }
        public virtual DbSet<LolTag> LolTag { get; set; }
        public virtual DbSet<OsmNode> OsmNode { get; set; }
        public virtual DbSet<OsmTag> OsmTag { get; set; }
        public virtual DbSet<TagKey> TagKey { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={db_path}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Link>(entity =>
            {
                entity.ToTable("link");

                entity.HasIndex(e => e.LolNodeId)
                    .IsUnique();

                entity.HasIndex(e => e.OsmNodeId)
                    .IsUnique();

                entity.Property(e => e.LinkId)
                    .HasColumnName("link_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LolNodeId).HasColumnName("lol_node_id");

                entity.Property(e => e.OsmNodeId).HasColumnName("osm_node_id");

                entity.HasOne(d => d.LolNode)
                    .WithOne(p => p.Link)
                    .HasForeignKey<Link>(d => d.LolNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.OsmNode)
                    .WithOne(p => p.Link)
                    .HasForeignKey<Link>(d => d.OsmNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<LolNode>(entity =>
            {
                entity.ToTable("lol_node");

                entity.Property(e => e.LolNodeId)
                    .HasColumnName("lol_node_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Lat).HasColumnName("lat");

                entity.Property(e => e.Lon).HasColumnName("lon");
            });

            modelBuilder.Entity<LolTag>(entity =>
            {
                entity.ToTable("lol_tag");

                entity.HasIndex(e => new { e.LolNodeId, e.TagKeyId })
                    .IsUnique();

                entity.Property(e => e.LolTagId)
                    .HasColumnName("lol_tag_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LolNodeId).HasColumnName("lol_node_id");

                entity.Property(e => e.TagKeyId).HasColumnName("tag_key_id");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.HasOne(d => d.LolNode)
                    .WithMany(p => p.LolTag)
                    .HasForeignKey(d => d.LolNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.TagKey)
                    .WithMany(p => p.LolTag)
                    .HasForeignKey(d => d.TagKeyId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<OsmNode>(entity =>
            {
                entity.ToTable("osm_node");

                entity.HasIndex(e => e.OsmId)
                    .IsUnique();

                entity.Property(e => e.OsmNodeId)
                    .HasColumnName("osm_node_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Lat).HasColumnName("lat");

                entity.Property(e => e.Lon).HasColumnName("lon");

                entity.Property(e => e.OsmId).HasColumnName("osm_id");

                entity.Property(e => e.Version).HasColumnName("version");
            });

            modelBuilder.Entity<OsmTag>(entity =>
            {
                entity.ToTable("osm_tag");

                entity.HasIndex(e => new { e.OsmNodeId, e.TagKeyId })
                    .IsUnique();

                entity.Property(e => e.OsmTagId)
                    .HasColumnName("osm_tag_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.OsmNodeId).HasColumnName("osm_node_id");

                entity.Property(e => e.TagKeyId).HasColumnName("tag_key_id");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.HasOne(d => d.OsmNode)
                    .WithMany(p => p.OsmTag)
                    .HasForeignKey(d => d.OsmNodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.TagKey)
                    .WithMany(p => p.OsmTag)
                    .HasForeignKey(d => d.TagKeyId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TagKey>(entity =>
            {
                entity.ToTable("tag_key");

                entity.HasIndex(e => e.TagKey1)
                    .IsUnique();

                entity.Property(e => e.TagKeyId)
                    .HasColumnName("tag_key_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.TagKey1)
                    .IsRequired()
                    .HasColumnName("tag_key");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
