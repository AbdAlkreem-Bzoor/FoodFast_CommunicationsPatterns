using FoodFast.API.Data;
using FoodFast.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodFast.API.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly FoodFastDbContext _database;
    private readonly IWebHostEnvironment _environment;

    public UploadsController(FoodFastDbContext database,
                             IWebHostEnvironment env)
    {
        _database = database;
        _environment = env;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("file missing");
        }

        if (file.Length > 12 * 1024 * 1024)
        {
            return BadRequest("file too large (>12MB)");
        }

        var owner = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var jobId = Guid.NewGuid();
        var uploadDir = Path.Combine(_environment.ContentRootPath, "uploads", "raw");
        Directory.CreateDirectory(uploadDir);
        var ext = Path.GetExtension(file.FileName);
        var savedPath = Path.Combine(uploadDir, $"{jobId}{ext}");

        using (var fs = new FileStream(savedPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        var job = new UploadJob
        {
            Id = jobId,
            FilePath = savedPath,
            OwnerUserId = owner,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _database.UploadJobs.Add(job);

        await _database.SaveChangesAsync();

        return Accepted(new { jobId = job.Id });
    }

    [HttpGet("{id}/status")]
    [Authorize]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var job = await _database.UploadJobs.FindAsync(id);
        if (job is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            jobId = job.Id,
            status = job.Status,
            result = job.ResultPath,
            error = job.Error,
            createdAt = job.CreatedAt,
            completedAt = job.CompletedAt
        });
    }
}