using Base.IntegrationTests.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.IntegrationTests;
public static class JwtTestHelper
{
    public static async Task<Token> DoLoginAsync(HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        const string loginUrl = "/api/v1/auth/login";
        var user = new { Username = "Admin", Password = "1234" };
        using var stringContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, loginUrl)
        {
            Content = stringContent
        };
        var response = await client.SendAsync(httpRequestMessage);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        responseString.Should().NotBeNullOrEmpty();
        return JsonSerializer.Deserialize<Token>(responseString);
    }
}