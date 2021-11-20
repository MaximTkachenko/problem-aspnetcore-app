using Microsoft.AspNetCore.Mvc;
using ProblemApp.Scripts;

namespace ProblemApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ScriptsController : ControllerBase
{
    private readonly ILogger<ScriptsController> _logger;

    public ScriptsController(ILogger<ScriptsController> logger)
    {
        _logger = logger;
    }

    [HttpPost("gc-stress")]
    public async Task<IActionResult> StartGarbageCollectionStress(GarbageCollectionStressRequest request) =>
        Ok(await GarbageCollectionStressScript.Instance.StartAsync(request) ? "Started" : "Already started");

    [HttpDelete("gc-stress")]
    public async Task<IActionResult> StopGarbageCollectionStress() =>
        Ok(await GarbageCollectionStressScript.Instance.StopAsync() ? "Stopped" : "Already stopped");

    [HttpPost("deadlock-with-threads")]
    public async Task<IActionResult> ExecuteDeadlockWithThreads(DeadlockedWithThreadsRequest request)
    {
        await DeadlockedWithThreadsScript.Instance.StartAsync(request);
        return Ok("Started");
    }

    [HttpPost("deadlock-with-tasks")]
    public async Task<IActionResult> ExecuteDeadlockWithTasks(DeadlockedWithTasksRequest request)
    {
        await DeadlockedWithTasksScript.Instance.StartAsync(request);
        return Ok("Done");
    }
}