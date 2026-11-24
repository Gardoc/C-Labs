using System.Net.Http.Json;
using Truant.Contracts.Teacher;

namespace Truant.Clients;
public class TeacherServiceClient
{
    private readonly HttpClient httpClient;

    public TeacherServiceClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task SaveConfigurationsAsync(long semesterId, List<TeacherConfigurationDto> configurations, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/Teacher/{semesterId}/configurations", configurations, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<AskTeacherResponse> AskAsync(AskTeacherRequest request, string subject, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/Teacher/{Uri.EscapeDataString(subject)}/ask", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AskTeacherResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Teacher Service вернул пустой ответ.");
    }
}