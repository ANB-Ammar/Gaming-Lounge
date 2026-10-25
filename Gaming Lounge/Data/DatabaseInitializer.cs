using Gaming_Lounge.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedCatalogAsync(context);
        await SeedAdminAsync(userManager, configuration, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Customer", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        if (await context.GamingCategories.AnyAsync())
        {
            return;
        }

        var pc = new GamingCategory
        {
            Name = "Elite PC Arena",
            Slug = "gaming-pcs",
            Description = "Tournament-ready PCs built for high refresh-rate competitive play.",
            IconClass = "bi-pc-display",
            AccentColor = "cyan",
            SortOrder = 1
        };
        var console = new GamingCategory
        {
            Name = "Console Lounges",
            Slug = "console-booths",
            Description = "Private big-screen booths with premium seating and current consoles.",
            IconClass = "bi-controller",
            AccentColor = "pink",
            SortOrder = 2
        };
        var vr = new GamingCategory
        {
            Name = "VR Dimensions",
            Slug = "vr-pods",
            Description = "Room-scale virtual reality pods with immersive audio and tracking.",
            IconClass = "bi-headset-vr",
            AccentColor = "purple",
            SortOrder = 3
        };

        context.GamingCategories.AddRange(pc, console, vr);
        await context.SaveChangesAsync();

        context.GamingStations.AddRange(
            CreateStation("RTX Overdrive 01", "rtx-overdrive-01", "Our flagship competitive rig for esports, streaming, and ultra settings.", "RTX 5090 • Ryzen 9 • 64GB DDR5 • 360Hz OLED • Pro peripherals", 220, "/images/stations/pc-arena.svg", pc.Id, true),
            CreateStation("Neon Striker 02", "neon-striker-02", "Fast, balanced, and tuned for ranked sessions with zero distractions.", "RTX 5080 • Intel Core Ultra 9 • 32GB DDR5 • 240Hz display", 180, "/images/stations/pc-arena.svg", pc.Id),
            CreateStation("PlayStation Pulse", "playstation-pulse", "A private social booth for couch co-op, sports, and cinematic adventures.", "PlayStation 5 Pro • 65-inch 4K OLED • Four DualSense controllers", 160, "/images/stations/console-booth.svg", console.Id, true),
            CreateStation("Xbox Nova Booth", "xbox-nova-booth", "A comfortable 4K console booth with instant access to a huge game library.", "Xbox Series X • 65-inch 4K QLED • Four wireless controllers", 145, "/images/stations/console-booth.svg", console.Id),
            CreateStation("VR Quantum Pod", "vr-quantum-pod", "Step into a dedicated room-scale arena with precise full-body movement.", "Meta Quest 3 • PC VR link • 3m x 3m play area • Spatial audio", 250, "/images/stations/vr-pod.svg", vr.Id, true),
            CreateStation("VR Horizon Pod", "vr-horizon-pod", "A guided VR pod ideal for first-time explorers and multiplayer adventures.", "HTC Vive Pro 2 • Wireless adapter • Precision tracking • Haptics", 275, "/images/stations/vr-pod.svg", vr.Id));

        await context.SaveChangesAsync();
    }

    private static GamingStation CreateStation(
        string name,
        string slug,
        string description,
        string specifications,
        decimal hourlyRate,
        string imagePath,
        int categoryId,
        bool featured = false) => new()
        {
            Name = name,
            Slug = slug,
            Description = description,
            Specifications = specifications,
            HourlyRate = hourlyRate,
            ImagePath = imagePath,
            GamingCategoryId = categoryId,
            IsFeatured = featured
        };

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("No admin bootstrap credentials were supplied. Admin account creation was skipped.");
            return;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = "Neon Nexus Admin"
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Admin bootstrap failed: {Errors}", string.Join("; ", createResult.Errors.Select(x => x.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
