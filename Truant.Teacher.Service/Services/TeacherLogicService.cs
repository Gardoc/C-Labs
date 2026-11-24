using Truant.Contracts.History;
using Truant.Contracts.Teacher;
using Truant.Teacher.Service.Data;

namespace Truant.Teacher.Service.Services;

public class TeacherLogicService
{
    private readonly TeacherRepository repository;
    private readonly HttpClient httpClient;

    public TeacherLogicService(TeacherRepository repository, HttpClient httpClient)
    {
        this.repository = repository;
        this.httpClient = httpClient;
    }

    public async Task SaveConfigurationsAsync(long semesterId, List<Models.TeacherConfiguration> configurations, CancellationToken cancellationToken = default)
    {
        await repository.SaveConfigurationsAsync(semesterId, configurations, cancellationToken);
    }

    public async Task<AskTeacherResponse?> AskAsync(AskTeacherRequest request, string subject, CancellationToken cancellationToken = default)
    {
        var teacher = await repository.GetConfigurationAsync(request.SemesterId, subject, cancellationToken);

        if (teacher == null)
            return null;

        bool wasAsked;

        switch (teacher.Rule)
        {
            case 0:
                if (request.RandomValue == null ||
                    request.RandomValue < 0 ||
                    request.RandomValue >= 1)
                {
                    throw new ArgumentException("Для случайного правила необходимо передать RandomValue от 0 до 1.");
                }

                wasAsked = request.RandomValue.Value < 0.5;
                break;

            case 1:
                wasAsked = await WasAskedYesterdayAsync(request, teacher.SubjectA!, cancellationToken);
                break;

            case 2:
                bool askedA = await WasAskedYesterdayAsync(request, teacher.SubjectA!, cancellationToken);

                bool askedB = await WasAskedYesterdayAsync(request, teacher.SubjectB!, cancellationToken);

                wasAsked = askedA != askedB;
                break;

            default:
                throw new InvalidOperationException($"Неизвестное правило преподавателя: {teacher.Rule}");
        }

        return new AskTeacherResponse
        {
            Subject = teacher.Subject,
            WasAsked = wasAsked
        };
    }

    private async Task<bool> WasAskedYesterdayAsync(AskTeacherRequest request, string subject, CancellationToken cancellationToken)
    {
        if (request.DayNumber <= 1)
            return false;

        string url = $"api/History/{request.SemesterId}/yesterday/{Uri.EscapeDataString(subject)}?dayNumber={request.DayNumber}";

        var response = await httpClient.GetFromJsonAsync<YesterdayHistoryResponse>(url, cancellationToken);

        return response?.WasAsked ?? false;
    }
}