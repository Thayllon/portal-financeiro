using DbUp;

var connectionString = args.FirstOrDefault()
    ?? "Server=(localdb)\\mssqllocaldb;Database=PortalFinanceiro;Trusted_Connection=True;TrustServerCertificate=True";

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
