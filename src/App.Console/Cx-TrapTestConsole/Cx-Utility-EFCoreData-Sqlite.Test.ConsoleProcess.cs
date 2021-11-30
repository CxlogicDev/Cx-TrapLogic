using CxUtility.Cx_Console;
using CxUtility.EFCoreData.SQLite;
using Microsoft.EntityFrameworkCore;

namespace Cx_TrapTestConsole;

[CxConsoleInfo("efcore-sqlite", typeof(Cx_Utility_EFCoreData_Sqlite), CxRegisterTypes.Preview, "Live Inplace Testing Ground for Cx-Utility-EFCoreData-Sqlite Project")]
internal class Cx_Utility_EFCoreData_Sqlite : ConsoleBaseProcess
{

    CxEFCoreSqliteContext context { get; }

    public Cx_Utility_EFCoreData_Sqlite(CxEFCoreSqliteContext _context, CxCommandService CxProcess, IConfiguration config) :
            base(CxProcess, config)
    {
        context = _context;
    }

    [CxConsoleAction("test", "Test Ground for sqlite", true, CxRegisterTypes.Register)]
    public async Task test_Sqlite_Method(CancellationToken cancellationToken)
    {

        if (await context.Database.CanConnectAsync(cancellationToken))
            await context.Database.EnsureDeletedAsync(cancellationToken);


        bool isCreated = await context.ErrorIfNull().Database.EnsureCreatedAsync(cancellationToken);

        if(!isCreated)
            isCreated = await context.Database.CanConnectAsync(cancellationToken);


        if (!isCreated)
            throw new InvalidOperationException("The database cannot be connected to.");

        var name_referenceId = Guid.NewGuid().ToString();

        var entity_Obj = context.QTests.AddUpdate_SqliteEntity_Async(o => {
            o.Model = new QTest { id = 2, name = name_referenceId.ToUpper(), referenceId = name_referenceId.ToLower() };
            o.Search_Predicate = x => x.id == 2 || x.referenceId == name_referenceId.ToLower();
        });


        int result = await context.SaveLiteChangesCleanTrackerAsync();

        result.Error_IfNotValid(v => v == 1, new InvalidOperationException("The Value was not saved in the context"));

        //look-up the entity

        var check_Entity = await context.QTests.FirstOrDefaultAsync(w => w.id == 2);

        check_Entity.ErrorIfNull_Or_NotValid(f => f.id == 2 && f.referenceId == name_referenceId.ToLower());


        context.QTests.Remove(check_Entity);


        int result2 = await context.SaveLiteChangesCleanTrackerAsync();
        result.Error_IfNotValid(v => v == 1, new InvalidOperationException("The Value was not removed from the context"));
    }





}



public class CxEFCoreSqliteContext : DbContext
{
    internal const string sqliteTest = "sql.test.db";
    
    public DbSet<QTest>? QTests { get; set; }

    public CxEFCoreSqliteContext(DbContextOptions<CxEFCoreSqliteContext> options) : base(options)
    {

    } 


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QTest>().ToTable(nameof(QTest));
    }


}

public record QTest
{
    public int id { get; set; }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public string name { get; set; }

    [System.Diagnostics.CodeAnalysis.AllowNull]
    public string referenceId { get; set; }
}

