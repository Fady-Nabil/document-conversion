using DocumentConversion.Api.Contracts;
using DocumentConversion.Application.Dtos;
using DocumentConversion.Application.Jobs.Commands;
using DocumentConversion.Application.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DocumentConversion.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitJobResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubmitJobResponse>> Submit(IFormFile? file, [FromForm] string outputFormat = "Docx", CancellationToken cancellationToken = default)
    {
        await using var memory = new MemoryStream();
        if (file is { Length: > 0 }) await file.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        var command = new SubmitConversionJobCommand(memory, file?.FileName ?? string.Empty, file?.ContentType ?? string.Empty, outputFormat);

        var result = await mediator.Send(command, cancellationToken);
        var response = new SubmitJobResponse(result.JobId);
        return AcceptedAtAction(nameof(GetById), new { id = result.JobId }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<JobDetailDto> GetById(Guid id, CancellationToken cancellationToken) =>
        mediator.Send(new GetConversionJobQuery(id), cancellationToken);

    [HttpGet]
    [ProducesResponseType(typeof(PagedJobsDto), StatusCodes.Status200OK)]
    public Task<PagedJobsDto> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        mediator.Send(new ListConversionJobsQuery(page, pageSize), cancellationToken);

    [HttpGet("{id:guid}/artifacts/{partIndex:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadArtifact(Guid id, int partIndex, CancellationToken cancellationToken)
    {
        var download = await mediator.Send(new GetJobArtifactDownloadQuery(id, partIndex), cancellationToken);
        return File(download.Content, download.ContentType, download.FileName);
    }
}
