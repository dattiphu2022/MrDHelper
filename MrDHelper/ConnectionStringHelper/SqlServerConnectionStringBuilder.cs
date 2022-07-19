namespace MrDHelper
{
    /// <summary>
    /// <b>[Standard Security]</b><br/>
    /// Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
    /// <br/><br/>
    /// <b>[Trusted Connection]</b><br/>
    /// Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
    /// <br/><br/>
    /// <b>[Connection to a SQL Server instance]</b><br/>
    /// Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
    /// <br/><br/>
    /// <b>[Using a non-standard port]</b><br/>
    /// Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;
    /// <br/><br/>
    /// <b>[Connect via an IP address]</b><br/>
    /// Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
    /// <br/><br/>
    /// If <see cref="SqlServerConnectionStringBuilder.Trusted_Connection"/> then do NOT set <see cref="SqlServerConnectionStringBuilder.UserId"/><see cref="SqlServerConnectionStringBuilder.Password"/>
    /// </summary>
    public class SqlServerConnectionStringBuilder : IConnectionString
    {
        public string FinalConnectionString
        {
            get => new ConnectionStringBuilder()
            .AddProperty($"Data Source={Server}", !isLocal && Server.NotNullOrWhiteSpace())
            .AddProperty($"Server={Server}", isLocal && Server.NotNullOrWhiteSpace())

            .AddProperty($"Network Library=DBMSSOCN;Initial Catalog={Database}", !isLocal && Database.NotNullOrWhiteSpace())
            .AddProperty($"Database={Database}", isLocal && Database.NotNullOrWhiteSpace())

            .AddProperty($"Trusted_Connection=True", isLocal && Trusted_Connection.IsTrue())

            .AddProperty($"User Id={UserId}", Trusted_Connection.IsFalse() && UserId.NotNullOrWhiteSpace())
            .AddProperty($"Password={Password}", Trusted_Connection.IsFalse() && Password.NotNullOrWhiteSpace())
            .Build();
        }
        /// <summary>
        /// <b>[Standard Security]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Trusted Connection]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
        /// <br/><br/>
        /// <b>[Connection to a SQL Server instance]</b><br/>
        /// Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Using a non-standard port]</b><br/>
        /// Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Connect via an IP address]</b><br/>
        /// Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
        /// <br/><br/>
        /// </summary>
        public string? Server { get; set; }

        /// <summary>
        /// <b>[Standard Security]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Trusted Connection]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
        /// <br/><br/>
        /// <b>[Connection to a SQL Server instance]</b><br/>
        /// Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Using a non-standard port]</b><br/>
        /// Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Connect via an IP address]</b><br/>
        /// Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
        /// <br/><br/>
        /// </summary>
        public string? Database { get; set; }

        /// <summary>
        /// <b>[Standard Security]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Trusted Connection]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
        /// <br/><br/>
        /// <b>[Connection to a SQL Server instance]</b><br/>
        /// Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Using a non-standard port]</b><br/>
        /// Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Connect via an IP address]</b><br/>
        /// Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
        /// <br/><br/>
        /// </summary>
        public string? UserId { get; set; }


        /// <summary>
        /// <b>[Standard Security]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Trusted Connection]</b><br/>
        /// Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
        /// <br/><br/>
        /// <b>[Connection to a SQL Server instance]</b><br/>
        /// Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Using a non-standard port]</b><br/>
        /// Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;
        /// <br/><br/>
        /// <b>[Connect via an IP address]</b><br/>
        /// Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;
        /// <br/><br/>
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// If true, this will causes the <see cref="SqlServerConnectionStringBuilder.UserId"/> and <see cref="SqlServerConnectionStringBuilder.Password"/> builds skipped.
        /// </summary>
        public bool Trusted_Connection { get; set; }

        /// <summary>
        /// If true, this will causes the <see cref="SqlServerConnectionStringBuilder.Server"/> and <see cref="SqlServerConnectionStringBuilder.Database"/> have difference way of generating.
        /// </summary>
        public bool ConnectViaIP { get; set; }
        private bool isLocal => ConnectViaIP.NotTrue();
    }

    public interface IConnectionString
    {
        public string FinalConnectionString { get; }
    }
}
