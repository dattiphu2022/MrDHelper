namespace MrDHelper
{
    using System;
    using System.ComponentModel;
    using System.Data.SqlClient;
    using System.ServiceProcess;
    using System.Threading.Tasks;

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
    /// [fails in real app]Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword; <br/>
    /// Data Source=192.168.10.234,1433;Initial Catalog=Farm;Integrated Security=False;User ID=sa;Password=123456$;
    /// <br/><br/>
    /// If <see cref="SqlServerConnectionStringBuilder.Trusted_Connection"/> then do NOT set <see cref="SqlServerConnectionStringBuilder.UserId"/><see cref="SqlServerConnectionStringBuilder.Password"/>
    /// </summary>
    public class SqlServerConnectionStringBuilder : IConnectionString, INotifyPropertyChanged, IDisposable
    {
        private string? server;
        private string? database;
        private string? userId;
        private string? password;
        private bool trusted_Connection;
        private bool connectViaIP;
        private bool disposedValue;

        public string FinalConnectionString
        {
            get => new ConnectionStringBuilder()
            .AddProperty($"Data Source={Server}", !IsLocal && Server.NotNullOrWhiteSpace())
            .AddProperty($"Server={Server}", IsLocal && Server.NotNullOrWhiteSpace())

            .AddProperty($"Initial Catalog={Database}", !IsLocal && Database.NotNullOrWhiteSpace())
            .AddProperty($"Database={Database}", IsLocal && Database.NotNullOrWhiteSpace())

            .AddProperty($"Integrated Security=False", !IsLocal && Trusted_Connection.IsFalse())
            .AddProperty($"Trusted_Connection=True", IsLocal && Trusted_Connection.IsTrue())

            .AddProperty($"User Id={UserId}", Trusted_Connection.IsFalse() && UserId.NotNullOrWhiteSpace())
            .AddProperty($"Password={Password}", Trusted_Connection.IsFalse() && Password.NotNullOrWhiteSpace())
            .Build();
        }
        public string ConnectionStringForNotExistedDatabase
        {
            get => new ConnectionStringBuilder()
            .AddProperty($"Data Source={Server}", !IsLocal && Server.NotNullOrWhiteSpace())
            .AddProperty($"Server={Server}", IsLocal && Server.NotNullOrWhiteSpace())

            .AddProperty($"Integrated Security=False", !IsLocal && Trusted_Connection.IsFalse())
            .AddProperty($"Trusted_Connection=True", IsLocal && Trusted_Connection.IsTrue())

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
        /// [fails in real app]Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword; <br/>
        /// Data Source=192.168.10.234,1433;Initial Catalog=Farm;Integrated Security=False;User ID=sa;Password=123456$;
        /// <br/><br/>
        /// </summary>
        public string? Server
        {
            get => server;
            set
            {
                if (server != value)
                {
                    server = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Server)));
                }
            }
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
        /// [fails in real app]Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword; <br/>
        /// Data Source=192.168.10.234,1433;Initial Catalog=Farm;Integrated Security=False;User ID=sa;Password=123456$;
        /// <br/><br/>
        /// </summary>
        public string? Database
        {
            get => database;
            set
            {
                if (database != value)
                {
                    database = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Database)));
                }
            }
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
        /// [fails in real app]Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword; <br/>
        /// Data Source=192.168.10.234,1433;Initial Catalog=Farm;Integrated Security=False;User ID=sa;Password=123456$;
        /// <br/><br/>
        /// </summary>
        public string? UserId
        {
            get => userId;
            set
            {
                if (userId != value)
                {
                    userId = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserId)));
                }
            }
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
        /// [fails in real app]Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword; <br/>
        /// Data Source=192.168.10.234,1433;Initial Catalog=Farm;Integrated Security=False;User ID=sa;Password=123456$;
        /// <br/><br/>
        /// </summary>
        public string? Password
        {
            get => password;
            set
            {
                if (password != value)
                {
                    password = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
                }
            }
        }

        /// <summary>
        /// If true, this will causes the <see cref="SqlServerConnectionStringBuilder.UserId"/> and <see cref="SqlServerConnectionStringBuilder.Password"/> builds skipped.
        /// </summary>
        public bool Trusted_Connection
        {
            get => trusted_Connection;
            set
            {
                if (trusted_Connection != value)
                {
                    trusted_Connection = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Trusted_Connection)));
                }
            }
        }

        /// <summary>
        /// If true, this will causes the <see cref="SqlServerConnectionStringBuilder.Server"/> and <see cref="SqlServerConnectionStringBuilder.Database"/> have difference way of generating.
        /// </summary>
        public bool ConnectViaIP
        {
            get => connectViaIP;
            set
            {
                if (connectViaIP != value)
                {
                    connectViaIP = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectViaIP)));
                }
            }
        }
        private bool IsLocal => ConnectViaIP.NotTrue();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Validate the <see cref="SqlServerConnectionStringBuilder.FinalConnectionString"/> with given properties.
        /// </summary>
        /// <returns><see cref="Tuple{T1, T2}"/>
        /// <br/>
        /// (true, string.empty)||<br/>
        /// (false,"error message")</returns>
        public async Task<(bool validateResult, string message)> ValidateConnectionString(bool newDatabaseName = false)
        {
            SqlConnection? connection = null;

            try
            {
                if (newDatabaseName.NotTrue())
                {
                    connection = new SqlConnection(FinalConnectionString);
                }
                else
                {
                    connection = new SqlConnection(ConnectionStringForNotExistedDatabase);
                }
                await connection.OpenAsync();
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
        public static async Task<(bool validateResult, string message)> ValidateConnectionString(string conString)
        {
            SqlConnection? connection = null;

            try
            {
                if (conString.NotNullOrWhiteSpace())
                {
                    connection = new SqlConnection(conString);
                    await connection.OpenAsync();
                    return (true, string.Empty);
                }
                return (false, "Connectionstring is null or white space.");
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
        //private async Task<bool> CheckMSSQLServerServiceRunning()
        //{
        //    return true;
            //ServiceController[] service = ServiceController.GetServices();
            //bool findSQLServer = false;

            //bool isrunning = false;

            //for (int i = 0; i < service.Length; i++)
            //{

            //    if (service[i].DisplayName.Contains(CONSTANTS.MSSQLServerServiceName) ||
            //        service[i].DisplayName.Contains(CONSTANTS.SQLExpressServiceName))
            //    {
            //        findSQLServer = true;

            //        if (service[i].Status == ServiceControllerStatus.Running)
            //        {
            //            isrunning = true;
            //        }
            //        else
            //        {
            //            isrunning = false;
            //        }
            //        break;

            //    }

            //}
            //if (findSQLServer && isrunning == true)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        //}
        ~SqlServerConnectionStringBuilder()
        {
            Dispose();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                PropertyChanged = null;
                server = null;
                database = null;
                userId = null;
                password = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SqlServerConnectionStringBuilder()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IConnectionString
    {
        public string FinalConnectionString { get; }
        public Task<(bool validateResult, string message)> ValidateConnectionString(bool newDatabaseName);
    }
}
