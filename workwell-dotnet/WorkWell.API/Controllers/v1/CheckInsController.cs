using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkWell.API.Helpers;
using WorkWell.Application.DTOs;
using WorkWell.Application.Services;

namespace WorkWell.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class CheckInsController : ControllerBase
{
    private readonly ICheckinService _checkinService;
    private readonly ILogger<CheckInsController> _logger;

    public CheckInsController(ICheckinService checkinService, ILogger<CheckInsController> logger)
    {
        _checkinService = checkinService;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo check-in diário
    /// </summary>
    [HttpPost(Name = "CreateCheckin")]
    [ProducesResponseType(typeof(ResourceResponse<CheckinDiarioDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCheckin([FromBody] CreateCheckinRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var checkin = await _checkinService.CreateCheckinAsync(userId, request);

            var response = new ResourceResponse<CheckinDiarioDto>
            {
                Data = checkin,
                Message = "Check-in criado com sucesso",
                Links = new List<Link>
                {
                    new() { Href = Url.Link("GetCheckinById", new { id = checkin.Id })!, Rel = "self", Method = "GET" },
                    new() { Href = Url.Link("GetMyCheckins", null)!, Rel = "list", Method = "GET" },
                    new() { Href = Url.Link("GetMyStatistics", null)!, Rel = "statistics", Method = "GET" }
                }
            };

            _logger.LogInformation("Check-in created for user {UserId}", userId);
            return CreatedAtRoute("GetCheckinById", new { id = checkin.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Busca check-in por ID
    /// </summary>
    [HttpGet("{id}", Name = "GetCheckinById")]
    [ProducesResponseType(typeof(ResourceResponse<CheckinDiarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var checkin = await _checkinService.GetCheckinByIdAsync(id);
        if (checkin == null)
        {
            return NotFound(new { message = "Check-in não encontrado" });
        }

        var response = new ResourceResponse<CheckinDiarioDto>
        {
            Data = checkin,
            Links = new List<Link>
            {
                new() { Href = Url.Link("GetCheckinById", new { id })!, Rel = "self", Method = "GET" },
                new() { Href = Url.Link("GetMyCheckins", null)!, Rel = "list", Method = "GET" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Lista check-ins do usuário autenticado
    /// </summary>
    [HttpGet("me", Name = "GetMyCheckins")]
    [ProducesResponseType(typeof(PagedResponse<CheckinDiarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCheckins(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var checkins = (await _checkinService.GetUserCheckinsAsync(userId, dataInicio, dataFim)).ToList();

        var pagedResponse = PaginationHelper.CreatePagedResponse(checkins, pageNumber, pageSize, checkins.Count);
        pagedResponse.Links = HateoasHelper.GeneratePaginationLinks(
            Url,
            "GetMyCheckins",
            pageNumber,
            pageSize,
            pagedResponse.TotalPages,
            new { dataInicio, dataFim });

        return Ok(pagedResponse);
    }

    /// <summary>
    /// Obtém estatísticas dos check-ins do usuário
    /// </summary>
    [HttpGet("me/statistics", Name = "GetMyStatistics")]
    [ProducesResponseType(typeof(CheckinStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStatistics(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var statistics = await _checkinService.GetUserStatisticsAsync(userId, dataInicio, dataFim);

        return Ok(statistics);
    }
}

