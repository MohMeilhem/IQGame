using IQGame.Admin;
using IQGame.Admin.Services;
using IQGame.Application.Interfaces;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using IQGame.Infrastructure.Repositories;
using IQGame.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register Infrastructure DbContext
builder.Services.AddDbContext<IQGameDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Configure Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<IQGameDbContext>();

// Register repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddHttpClient();

// Configure image search settings
builder.Services.Configure<ImageSearchConfiguration>(builder.Configuration.GetSection("ImageSearch"));

// Register the image search service factory and create the service
builder.Services.AddScoped<ImageSearchServiceFactory>();
builder.Services.AddScoped<IImageSearchService>(provider =>
{
    var factory = provider.GetRequiredService<ImageSearchServiceFactory>();
    return factory.CreateImageSearchService();
});

// Register provider switcher for diagnostics
builder.Services.AddScoped<ImageSearchProviderSwitcher>();

var app = builder.Build();

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

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IQGameDbContext>();
    await db.Database.MigrateAsync(); // This is still needed after the checks


    try
    {
        await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Seeder Error] AdminSeeder failed:");
        Console.WriteLine(ex.ToString());
        throw;
    }
}




app.Run();
