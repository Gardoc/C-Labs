using Microsoft.AspNetCore.Mvc;
using Truant.Contracts.History;
using Truant.History.Service.Data;

namespace Truant.History.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly HistoryRepository repository;

    public HistoryController(HistoryRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("day")]
    public async Task<IActionResult> SaveDay([FromBody] SaveDayHistoryRequest request, CancellationToken cancellationToken)
    {
        if (request.SemesterId <= 0 || request.DayNumber <= 0)
            return BadRequest("Некорректный ID семестра или номер дня.");

        if (request.Results == null || request.Results.Count == 0)
            return BadRequest("Результаты дня отсутствуют.");

        await repository.SaveDayAsync(request, cancellationToken);

        return Ok(new { Message = "Результаты дня сохранены." });
    }

    [HttpGet("{semesterId:long}/yesterday/{subject}")]
    public async Task<ActionResult<YesterdayHistoryResponse>> GetYesterday(long semesterId, string subject, [FromQuery] int dayNumber, CancellationToken cancellationToken)
    {
        if (semesterId <= 0 || dayNumber <= 0)
            return BadRequest("Некорректный ID семестра или номер дня.");

        var result = await repository.GetYesterdayAsync(semesterId, dayNumber, subject, cancellationToken);

        return Ok(result);
    }
}