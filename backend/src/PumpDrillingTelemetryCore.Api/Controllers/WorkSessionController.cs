using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;
using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;
using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Api.Controllers;

[ApiController]
[Route("api/pump/work-session")]
public sealed class WorkSessionController(ISender sender, IValidator<CreateWorkSessionRequest> validator) : ControllerBase
{
    private const int MaxFileSize = 10 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost]
    public async Task<ActionResult<WorkSessionSummaryResponse>> Create(
        CreateWorkSessionRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var measures = request.Samples.Select(sample => new Measure(
            sample.TimeInSeconds, sample.Pressure, sample.Rpm, sample.IsValid)).ToArray();
        
        return Ok(await sender.Send(new CreateWorkSessionCommand(measures), cancellationToken));
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    public async Task<ActionResult<WorkSessionSummaryResponse>> Import(
        [Required] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length is 0 or > MaxFileSize)
            return BadRequest("File must be non-empty and no larger than 10 MiB.");
        if (!string.Equals(Path.GetExtension(file.FileName), ".json", StringComparison.OrdinalIgnoreCase))
            return BadRequest("File must have a .json extension.");

        await using var stream = file.OpenReadStream();
        var request = await JsonSerializer.DeserializeAsync<CreateWorkSessionRequest>(stream, JsonOptions, cancellationToken);

        if (request is null)
            return BadRequest("File must contain a work session object.");
        if (!TryValidateModel(request))
            return ValidationProblem(ModelState);

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var measures = request.Samples.Select(sample => new Measure(
            sample.TimeInSeconds, sample.Pressure, sample.Rpm, sample.IsValid)).ToArray();

        return Ok(await sender.Send(new CreateWorkSessionCommand(measures), cancellationToken));
    }
}





