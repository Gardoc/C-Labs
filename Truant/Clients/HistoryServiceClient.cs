using System.Net.Http.Json;
using Truant.Contracts.History;

namespace Truant.Clients;
public class HistoryServiceClient
{
    private readonly HttpClient httpClient;
    public HistoryServiceClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task SaveDayAsync(SaveDayHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/History/day", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}