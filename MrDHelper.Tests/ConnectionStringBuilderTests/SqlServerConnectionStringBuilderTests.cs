
namespace MrDHelper.Tests
{
    public class SqlServerConnectionStringBuilderTests
    {

        [Test]
        [TestCase("myServerAddress", "myDataBase", "myUsername", "myPassword", false,false, "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
        [TestCase("myServerAddress", "myDataBase", "myUsername", "myPassword", true,false, "Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;")]
        [TestCase(@"myServerName\myInstanceName", "myDataBase", "myUsername", "myPassword", false,false, @"Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
        [TestCase(@"myServerName,myPortNumber", "myDataBase", "myUsername", "myPassword", false,false, "Server=myServerName,myPortNumber;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
        [TestCase(@"190.190.200.100,1433", "myDataBase", "myUsername", "myPassword", false,true, "Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User Id=myUsername;Password=myPassword;")]
        public void ShouldMatchTestCase(string? server, string? database, string? userId, string? passWord, bool trusted, bool viaIp, string finalString)
        {
            //arrange
            var connectionStringToBuild = new SqlServerConnectionStringBuilder{
                Server = server,
                Database = database,
                UserId = userId,
                Password = passWord,
                Trusted_Connection = trusted,
                ConnectViaIP = viaIp
            };

            var caculatedFinalString = connectionStringToBuild.FinalConnectionString;

            //assert
            Assert.That(caculatedFinalString, Is.EqualTo(finalString));
        }

    }
}