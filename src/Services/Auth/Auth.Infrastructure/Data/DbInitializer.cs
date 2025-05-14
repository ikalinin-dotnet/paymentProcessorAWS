using Microsoft.AspNetCore.Identity;
using PaymentProcessor.Services.Auth.Infrastructure.Identity;

namespace PaymentProcessor.Services.Auth.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Create roles if they don't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Create an admin user if one doesn't exist
        if (await userManager.FindByEmailAsync("admin@payment-processor.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@payment-processor.com",
                Email = "admin@payment-processor.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}