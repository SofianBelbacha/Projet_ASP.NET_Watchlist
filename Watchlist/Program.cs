using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Watchlist.Data;
using Watchlist.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Watchlist.Services;


namespace Watchlist
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));


            builder.Services.AddIdentity<Utilisateur, IdentityRole>(options => options.User.RequireUniqueEmail = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<FilmService>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login"; // Définit la bonne route de connexion
                options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Définit la page d'accès refusé
                options.SlidingExpiration = true; // Permet de prolonger la session active
            });

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<IdentitySeeder>>();
                await IdentitySeeder.SeedRolesAndAdmin(services, logger);
            }

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate(); // Applique toutes les migrations

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentitySeeder>>();
                await IdentitySeeder.SeedRolesAndAdmin(scope.ServiceProvider, logger);
            }

            app.Run();
        }

        public class DummyEmailSender : IEmailSender
        {
            public Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                Console.WriteLine($"Email fictif envoy� � {email} avec le sujet: {subject}");
                return Task.CompletedTask;
            }
        }

    }
}
