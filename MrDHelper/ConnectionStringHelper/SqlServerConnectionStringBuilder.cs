namespace MrDHelper
{
    using System;
    using System.Data.SqlClient;
    using System.ServiceProcess;

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

        /// <summary>
        /// Validate the <see cref="SqlServerConnectionStringBuilder.FinalConnectionString"/> with given properties.
        /// </summary>
        /// <returns><see cref="Tuple{T1, T2}"/>
        /// <br/>
        /// (true, string.empty)||<br/>
        /// (false,"error message")</returns>
        public (bool validateResult, string message) ValidateConnectionString()
        {
            if (IsMSSQLServerServiceRunning().NotTrue())
            {
                return (false, "Không tìm thấy MSSQLSERVER service đang chạy.");
            }
            SqlConnection? connection = null;

            try
            {
                connection = new SqlConnection(FinalConnectionString);
                connection.Open();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                connection?.Close();
                connection = null;
                connection?.Dispose();
            }
        }

        private bool IsMSSQLServerServiceRunning()
        {
            ServiceController[] service = ServiceController.GetServices();
            bool findSQLServer = false;

            bool isrunning = false;

            for (int i = 0; i < service.Length; i++)
            {

                if (service[i].DisplayName.ToString() == CONSTANTS.MSSQLServerServiceName)
                {
                    findSQLServer = true;

                    if (service[i].Status == ServiceControllerStatus.Running)
                    {
                        isrunning = true;
                    }
                    else
                    {
                        isrunning = false;
                    }
                    break;

                }

            }
            if (findSQLServer && isrunning == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public interface IConnectionString
    {
        public string FinalConnectionString { get; }
        public (bool validateResult, string message) ValidateConnectionString();
    }
}
