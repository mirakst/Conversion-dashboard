﻿using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

#nullable disable

namespace DashboardBackend.Database.Models
{
    public partial class NetcompanyDbContext : DbContext, INetcompanyDbContext
    {
        public NetcompanyDbContext()
        {
        }

        public NetcompanyDbContext(DbContextOptions<NetcompanyDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Afstemning> Afstemnings { get; set; }
        public virtual DbSet<AuditFkError> AuditFkErrors { get; set; }
        public virtual DbSet<AuditLogerror> AuditLogerrors { get; set; }
        public virtual DbSet<AuditLoginfo> AuditLoginfos { get; set; }
        public virtual DbSet<AuditLoginfoType> AuditLoginfoTypes { get; set; }
        public virtual DbSet<ColumnValue> ColumnValues { get; set; }
        public virtual DbSet<DestTable> DestTables { get; set; }
        public virtual DbSet<EngineProperty> EngineProperties { get; set; }
        public virtual DbSet<Execution> Executions { get; set; }
        public virtual DbSet<HealthReport> HealthReports { get; set; }
        public virtual DbSet<Logging> Loggings { get; set; }
        public virtual DbSet<LoggingContext> LoggingContexts { get; set; }
        public virtual DbSet<Manager> Managers { get; set; }
        public virtual DbSet<ManagerTracking> ManagerTrackings { get; set; }
        public virtual DbSet<MigrationFile> MigrationFiles { get; set; }
        public virtual DbSet<SequenceTracking> SequenceTrackings { get; set; }
        public virtual DbSet<StatementColumn> StatementColumns { get; set; }
        public virtual DbSet<StatementJoin> StatementJoins { get; set; }
        public virtual DbSet<StatementTable> StatementTables { get; set; }
        public virtual DbSet<SysHousekeeping> SysHousekeepings { get; set; }
        public virtual DbSet<SysHousekeepingUuid> SysHousekeepingUuids { get; set; }
        public virtual DbSet<TableLog> TableLogs { get; set; }
        public virtual DbSet<VEngineProperty> VEngineProperties { get; set; }
        public virtual DbSet<VoteCombination> VoteCombinations { get; set; }
        public virtual DbSet<VoteResult> VoteResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Connection string should be added to a ConfigurationManager at a later date (keep it out of the source code)
                string connectionString = ReadConnectionStringFromFile();
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        /// <summary>
        /// Temporary method that reads a server name and database from a local file to generate a connection string.
        /// </summary>
        /// <returns>A database connection string</returns>
        private string ReadConnectionStringFromFile()
        {
            string[] parts = File.ReadLines(@"Database/hostinfo.meta").First().Split(';');
            string serverName = parts[0];
            string dbName = parts[1]; 
            return $"Data Source={serverName};initial catalog={dbName};Integrated Security=True;ConnectRetryCount=0";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AuditFkError>(entity =>
            {
                entity.Property(e => e.Rowdata).IsUnicode(false);
            });

            modelBuilder.Entity<AuditLoginfoType>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<HealthReport>(entity =>
            {
                entity.Property(e => e.ReportValueHuman).IsUnicode(false);
            });

            modelBuilder.Entity<MigrationFile>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<VEngineProperty>(entity =>
            {
                entity.ToView("V_ENGINE_PROPERTIES");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}