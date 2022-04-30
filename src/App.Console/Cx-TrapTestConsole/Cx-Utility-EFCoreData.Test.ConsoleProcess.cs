using Microsoft.EntityFrameworkCore;
using CxUtility.EFCoreData;


namespace Cx_TrapTestConsole;

[CxConsoleInfo("efcore", typeof(Cx_Utility_EFCoreData), CxRegisterTypes.Preview, "Live Inplace Testing Ground for Cx-Utility-EFCoreData Project")]
internal class Cx_Utility_EFCoreData : ConsoleBaseProcess
{
    //CxEFCoreContext context { get; }

    public Cx_Utility_EFCoreData(CxCommandService CxProcess, IConfiguration config) :
            base(CxProcess, config)
    {
        //context = _context;
    }

    [CxConsoleAction(test_SqlContext_Args.Name, test_SqlContext_Args.Description, test_SqlContext_Args.RegisterType)]
    public async Task test_SqlContext(CancellationToken cancellationToken)
    {

        using CxEFCoreContext context = new CxEFCoreContext(_Config);

        await context.Database.EnsureCreatedAsync();

        var t1 = new Q1Test { name = "Test 1" };
        await context.QTests.AddUpdateEntityAsync(t1, x => x.id.Equals(t1.id));

        var t2 = new Q1Test { name = "Test 2" };
        await context.QTests.AddUpdateEntityAsync(t2, x => x.id.Equals(t2.id));

        try
        {
            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesCleanTrackerAsync();

            var tResults = await context.QTests.ToArrayAsync();

            var tResults_2 = context.BuildScriptCommand($"select * from [{nameof(Q1Test)}]")
                .Exec_Command<Q1Test>();

            foreach (var result in tResults)
            {

            }
        }
        catch (Exception Ex)
        {

        }



    }

}


public record test_SqlContext_Args(string id, string name) {

    internal const string Name = "q1-test";
    internal const string Description = "Clears any Image that is none existant";
    internal const CxRegisterTypes RegisterType = CxRegisterTypes.Preview;

    internal const string arg_id = "-id";
    internal const string arg_id_Name = "id";
    internal const string arg_id_Description = "The system Id to process for";
    internal const CxConsoleActionArgAttribute.arg_Types arg_idType = CxConsoleActionArgAttribute.arg_Types.Value;

    internal const string arg_name = "-run_procs";
    internal const string arg_nameName = "run_procs";
    internal const string arg_nameDescription = "The ammount of processes to run the code on. Default = Environment.ProcessorCount";
    internal const CxConsoleActionArgAttribute.arg_Types arg_nameType = CxConsoleActionArgAttribute.arg_Types.Number;


}



public class CxEFCoreContext : DbContext
{
    internal const string sqliteTest = "sql.test.db";

    public DbSet<Q1Test> QTests { get; set; }

    //public CxEFCoreContext(DbContextOptions<CxEFCoreSqliteContext> options) : base(options)
    //{
    //}

    private string connString { get; }

    public CxEFCoreContext(IConfiguration config)
    {
        connString = config.GetConnectionString(nameof(CxEFCoreContext))
                        .ErrorIfNull_Or_NotValid(v => v.hasCharacters(),
                            new InvalidOperationException("The Connction string is invalid at start-up"),
                            new ArgumentNullException("No connection string found"));
    }

    static string testString = Guid.NewGuid().ToString("N");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {

            options.UseSqlServer(connString);

            //ser.AddDbContext<CxEFCoreContext>(op => 

            //);



           // options.UseInMemoryDatabase(testString);
        }

        //base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Q1Test>()
            .ToTable(nameof(Q1Test))
            .HasKey(f => f.id);
    }


}


public record Q1Test
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public string name { get; set; }
}

public record Q2Test : Q1Test
{
    public Q2Test()
    {

    }

    public Q2Test(string _id, string _name)
    {
        id = _id;
        name = _name;

        /*
         public string id { get; set; } = Guid.NewGuid().ToString();

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public string name { get; set; }
         
         */
    }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public string referenceId { get; set; }
}