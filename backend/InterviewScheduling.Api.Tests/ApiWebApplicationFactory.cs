using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InterviewScheduling.Api.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Loads appsettings.Testing.json from the API project (Jwt signing key, etc.).
        builder.UseEnvironment("Testing");
    }
}
