using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Configuration (Supports MS SQL Server & SQLite fallback)
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=readme.db";

// Primary DB Context registration
builder.Services.AddDbContext<ReadmeDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(sqlServerConn))
    {
        options.UseSqlServer(sqlServerConn);
    }
    else
    {
        options.UseSqlite(sqliteConn);
    }
});

// Register Application Services
builder.Services.AddHttpClient<IBookSearchService, KakaoBookSearchService>();
builder.Services.AddScoped<IReadmeExportService, ReadmeExportService>();

var app = builder.Build();

// Ensure Database is Created & Seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ReadmeDbContext>();
        context.Database.EnsureCreated();
        ReadmeDbContext.SeedSampleData(context);
        logger.LogInformation("데이터베이스 초기화 및 샘플 시드 데이터 준비 완료.");
    }
    catch (SqlException ex)
    {
        logger.LogWarning("MS SQL Server 연결 실패({Message}). 로컬 SQLite로 대체 시도를 권장합니다.", ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "데이터베이스 초기화 중 오류 발생.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
