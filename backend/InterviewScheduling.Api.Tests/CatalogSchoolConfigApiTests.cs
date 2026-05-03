using System.Net;
using System.Text.Json;
using Xunit;

namespace InterviewScheduling.Api.Tests;

public sealed class CatalogSchoolConfigApiTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public CatalogSchoolConfigApiTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetSchoolConfig_anonymous_returns_ok_with_branding_fields()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/v1/catalog/school-config");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("schoolTimeZoneId", out _));
        Assert.True(root.TryGetProperty("uiLanguage", out _));
        Assert.True(root.TryGetProperty("themePreset", out var theme));
        Assert.False(string.IsNullOrWhiteSpace(theme.GetString()));
        Assert.True(root.TryGetProperty("hasCustomLogo", out _));
        Assert.True(root.TryGetProperty("brandingVersion", out _));
    }
}
