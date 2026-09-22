using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace DocumentConversion.Api.Tests;

public sealed class JobsApiTests : IClassFixture<ConversionApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;
    private readonly ConversionApiWebApplicationFactory _factory;

    public JobsApiTests(ConversionApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetJob_WhenMissing_Returns404()
    {
        var response = await _client.GetAsync($"/api/jobs/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
    }

    [Fact]
    public async Task Submit_WhenNotPdf_Returns400()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent("not a pdf"u8.ToArray()), "file", "notes.txt");
        content.Add(new StringContent("Docx"), "outputFormat");

        var response = await _client.PostAsync("/api/jobs", content);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Validation failed");
    }

    [Fact]
    public async Task Submit_TextPdf_CompletesWithArtifact()
    {
        var jobId = await SubmitSampleAsync("text-based.pdf");
        var detail = await GetJobDetailAsync(jobId);

        detail.Status.Should().Be("Completed");
        detail.Artifacts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Submit_ScannedOnlyPdf_FailsWithCode()
    {
        var jobId = await SubmitSampleAsync("scanned-image-only.pdf");
        var detail = await GetJobDetailAsync(jobId);

        detail.Status.Should().Be("Failed");
        detail.ErrorCode.Should().Be("ScannedDocumentNotSupported");
    }

    private async Task<Guid> SubmitSampleAsync(string sampleFileName)
    {
        var samplePath = Path.Combine(AppContext.BaseDirectory, "Samples", sampleFileName);
        await using var stream = File.OpenRead(samplePath);
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", sampleFileName);
        content.Add(new StringContent("Docx"), "outputFormat");

        var response = await _client.PostAsync("/api/jobs", content);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var body = await response.Content.ReadFromJsonAsync<SubmitJobResponse>(JsonOptions);
        body.Should().NotBeNull();
        await _factory.Scheduler.DrainPendingAsync();
        return body!.JobId;
    }

    private async Task<JobDetailResponse> GetJobDetailAsync(Guid jobId)
    {
        var response = await _client.GetAsync($"/api/jobs/{jobId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<JobDetailResponse>(JsonOptions);
        detail.Should().NotBeNull();
        return detail!;
    }

    private sealed record SubmitJobResponse(Guid JobId);

    private sealed record JobDetailResponse(
        Guid Id,
        string Status,
        string? ErrorCode,
        string? ErrorMessage,
        IReadOnlyList<ArtifactResponse> Artifacts);

    private sealed record ArtifactResponse(string FileName);
}
