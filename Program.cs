using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReadMeApp.Data;
using ReadMeApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Authentication Configuration (Cookie Auth for Kakao OAuth)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.Name = "Readme_Auth";
    });

// Database Configuration (Supports MS SQL Server & SQLite fallback)
var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
    ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=readme.db";

// Primary DB Context registration
builder.Services.AddDbContext<ReadmeDbContext>(options =>
{
    var isSqlServer = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && 
                      !string.IsNullOrWhiteSpace(sqlServerConn) &&
                      (builder.Environment.IsDevelopment() || !sqlServerConn.Contains("(localdb)", StringComparison.OrdinalIgnoreCase));

    if (isSqlServer)
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
builder.Services.AddHttpClient<IKakaoAuthService, KakaoAuthService>();
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
        
        // 기존 더미 데이터 정리
        var dummyReviewers = new[] { "지혜로운산책자", "따뜻한라떼", "새벽네시", "기록하는개발자" };
        var dummyReviews = context.UserBooks.Where(ub => dummyReviewers.Contains(ub.ReviewerName)).ToList();
        if (dummyReviews.Any())
        {
            context.UserBooks.RemoveRange(dummyReviews);
            context.SaveChanges();
            logger.LogInformation("기존 더미 독서록 {Count}건 삭제 완료.", dummyReviews.Count);
        }

        ReadmeDbContext.SeedSampleData(context);
        logger.LogInformation("데이터베이스 초기화 준비 완료.");
    }
    catch (SqlException ex)
    {
        logger.LogWarning("MS SQL Server 연결 실패({Message}).", ex.Message);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
