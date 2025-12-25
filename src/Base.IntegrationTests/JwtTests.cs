using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using Base.Common.Helpers;
using Base.IntegrationTests.Base;
using Base.IntegrationTests.Models;
using FluentAssertions;
using Xunit;
using Base.ViewModels;

namespace Base.IntegrationTests;

public class JwtTests
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    [Fact]
    public async Task TestLoginWorks()
    {
        // Arrange
        var client = TestsHttpClient.Instance;

        // Act
        var token = await JwtTestHelper.DoLoginAsync(client);

        // Assert
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TestCallProtectedApiWorks()
    {
        // Arrange
        var client = TestsHttpClient.Instance;

        // Act
        var token = await JwtTestHelper.DoLoginAsync(client);

        // Assert
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();

        // Act
        const string protectedApiUrl = "api/v1/account/GetUserInfo";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var response = await client.GetAsync(protectedApiUrl);
        response.EnsureSuccessStatusCode();

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        var apiResponse = JsonSerializer.Deserialize<Response<UserViewModel>>(responseString, JsonSerializerOptions);
        apiResponse.Data.Username.Should().NotBeNullOrEmpty();
        apiResponse.Data.Username.Should().Be("admin");
    }

}
