using Microsoft.AspNetCore.Mvc;
using Truant.Contracts.Teacher;
using Truant.Teacher.Service.Models;
using Truant.Teacher.Service.Services;

namespace Truant.Teacher.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeacherController : ControllerBase
{
    private readonly TeacherLogicService teacherService;

    public TeacherController(TeacherLogicService teacherService)
    {
        this.teacherService = teacherService;
    }

    [HttpPost("{semesterId:long}/configurations")]
    public async Task<IActionResult> SaveConfigurations(long semesterId, [FromBody] List<TeacherConfiguration> configurations, CancellationToken cancellationToken)
    {
        if (semesterId <= 0)
            return BadRequest("Некорректный ID семестра.");

        if (configurations == null || configurations.Count != 6)
            return BadRequest("Необходимо передать ровно 6 преподавателей.");

        if (configurations.Any(t => string.IsNullOrWhiteSpace(t.Subject)))
            return BadRequest("У каждого преподавателя должен быть указан предмет.");

        if (configurations.Select(t => t.Subject).Distinct().Count() != 6)
            return BadRequest("Предметы преподавателей должны быть уникальными.");

        if (configurations.Any(t => t.Rule is < 0 or > 2))
            return BadRequest("Неизвестное правило преподавателя.");

        if (configurations.Any(t =>
            (t.Rule == 0 && (t.SubjectA != null || t.SubjectB != null)) ||
            (t.Rule == 1 && (string.IsNullOrWhiteSpace(t.SubjectA) || t.SubjectB != null)) ||
            (t.Rule == 2 && (string.IsNullOrWhiteSpace(t.SubjectA) ||
            string.IsNullOrWhiteSpace(t.SubjectB) ||
            t.SubjectA == t.SubjectB))))
        {
            return BadRequest("Параметры предметов A и B не соответствуют правилу преподавателя.");
        }

        await teacherService.SaveConfigurationsAsync(semesterId, configurations, cancellationToken);

        return Ok(new { Message = "Конфигурации преподавателей сохранены." });
    }

    [HttpPost("{subject}/ask")]
    public async Task<ActionResult<AskTeacherResponse>> Ask(string subject, [FromBody] AskTeacherRequest request, CancellationToken cancellationToken)
    {
        if (request.SemesterId <= 0 || request.DayNumber <= 0)
            return BadRequest("Некорректный ID семестра или номер дня.");

        var result = await teacherService.AskAsync(request, subject, cancellationToken);

        if (result == null)
            return NotFound($"Преподаватель по предмету {subject} не найден для этого семестра.");

        return Ok(result);
    }
}