using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class OrderEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostValidOrder_Returns201_AndGetByLocation_Returns200()
    {
        // Arrange
        var request = new SaveOrderRequest(
            CustomerId: "CUST-TEST-01",
            IsActive: true,
            Lines: [new SaveOrderLineRequest("PROD-10", 150m, 2)]
        );

        // Act: POST
        var postResponse = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert: 201 Created + Location
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var createdDto = await postResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.NotNull(createdDto);
        Assert.Equal("CUST-TEST-01", createdDto.CustomerId);
        Assert.Equal(300m, createdDto.Total);

        // Act: GET за отриманим Location
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);

        // Assert: 200 OK
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedDto = await getResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        Assert.NotNull(fetchedDto);
        Assert.Equal(createdDto.Id, fetchedDto.Id);
    }

    [Fact]
    public async Task PostInvalidOrder_Returns400ValidationProblem()
    {
        var invalidRequest = new SaveOrderRequest(
            CustomerId: "",
            IsActive: true,
            Lines: []
        );

        var response = await _client.PostAsJsonAsync("/api/orders", invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmAlreadyConfirmedOrder_Returns409Conflict()
    {
        var request = new SaveOrderRequest(
            CustomerId: "CUST-CONFLICT",
            IsActive: true,
            Lines: [new SaveOrderLineRequest("PROD-1", 100m, 1)]
        );

        var createResponse = await _client.PostAsJsonAsync("/api/orders", request);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponseDto>();

        // Перше підтвердження -> 200 OK
        var firstConfirm = await _client.PostAsync($"/api/orders/{created!.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, firstConfirm.StatusCode);

        // Повторне підтвердження -> 409 Conflict (DomainException)
        var secondConfirm = await _client.PostAsync($"/api/orders/{created.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, secondConfirm.StatusCode);
    }

    [Fact]
    public async Task GetOrdersWithPagination_ReturnsItemsAndTotal()
    {
        var response = await _client.GetAsync("/api/orders?page=1&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedResult<OrderResponseDto>>();

        Assert.NotNull(paged);
        Assert.True(paged.Total >= 20);
        Assert.Equal(5, paged.Items.Count);
    }
}