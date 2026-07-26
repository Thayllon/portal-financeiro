using DbUp;
using Microsoft.Data.SqlClient;

var connectionString = args.FirstOrDefault()
    ?? "Server=(localdb)\\mssqllocaldb;Database=PortalFinanceiro;Trusted_Connection=True;TrustServerCertificate=True";

var builder = new SqlConnectionStringBuilder(connectionString);
var databaseName = builder.InitialCatalog;
builder.InitialCatalog = "master";

using var masterConn = new SqlConnection(builder.ConnectionString);
masterConn.Open();

var checkDbCmd = masterConn.CreateCommand();
checkDbCmd.CommandText = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @db) CREATE DATABASE [{databaseName}]";
checkDbCmd.Parameters.AddWithValue("@db", databaseName);
checkDbCmd.ExecuteNonQuery();

masterConn.Close();

var scriptsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "sql");

var result = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsFromFileSystem(scriptsPath)
    .LogToConsole()
    .Build()
    .PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Migrações executadas com sucesso!");
Console.ResetColor();
return 0;
