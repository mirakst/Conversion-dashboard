﻿using System.Data.Common;
using System.Text.Json.Serialization;

namespace DashboardFrontend.Settings
{
    public class Profile
    {
        public Profile(string name, string conversion, string dataSource, string database, int timeout)
        {
            Id = _nextId++;
            Name = name;
            Conversion = conversion;
            DataSource = dataSource;
            Database = database;
            Timeout = timeout;
        }

        [JsonConstructor]
        public Profile(int id, string name, string conversion, string dataSource, string database, int timeout)
            : this(name, conversion, dataSource, database, timeout)
        {
            Id = id;
            _nextId = id + 1;
        }

        public Profile() : this("", "", "", "", 30)
        {
        }

        private static int _nextId = 1;

        public int Id { get; }
        public string Name { get; set; }
        public string Conversion { get; set; }
        public string DataSource { get; set; }
        public string Database { get; set; }
        public int Timeout { get; set; } = 30;

        public string GetConnectionString(string userId, string password)
        {
            DbConnectionStringBuilder builder = new();
            builder.Add("Data Source", DataSource);
            builder.Add("Initial Catalog", Database);
            builder.Add("Connect Timeout", Timeout);
            builder.Add("User Id", userId);
            builder.Add("Password", password);
            // TODO: get provider type (Oracle/SQL) and use it to build the specific connection string
            return builder.ConnectionString;
        }

        public override bool Equals(object? obj)
        {
            return (obj as Profile)?.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}