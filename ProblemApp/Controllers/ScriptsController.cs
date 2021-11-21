using Microsoft.AspNetCore.Mvc;
using ProblemApp.Scripts;

namespace ProblemApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ScriptsController : ControllerBase
{
    private readonly ILogger<ScriptsController> _logger;
    private readonly GarbageCollectionStressScript _garbageCollectionStressScript;
    private readonly DeadlockedWithThreadsScript _deadlockedWithThreadsScript;
    private readonly DeadlockOnThreadPoolScript _deadlockOnThreadPoolScript;

    public ScriptsController(ILogger<ScriptsController> logger,
        GarbageCollectionStressScript garbageCollectionStressScript,
        DeadlockedWithThreadsScript deadlockedWithThreadsScript,
        DeadlockOnThreadPoolScript deadlockedWithTasksScript)
    {
        _logger = logger;
        _garbageCollectionStressScript = garbageCollectionStressScript;
        _deadlockedWithThreadsScript = deadlockedWithThreadsScript;
        _deadlockOnThreadPoolScript = deadlockedWithTasksScript;
    }

    [HttpPost("gc-stress")]
    public async Task<IActionResult> StartGarbageCollectionStress(GarbageCollectionStressRequest request) =>
        Ok(await _garbageCollectionStressScript.StartAsync(request) ? "Started" : "Already started");

    [HttpDelete("gc-stress")]
    public async Task<IActionResult> StopGarbageCollectionStress() =>
        Ok(await _garbageCollectionStressScript.StopAsync() ? "Stopped" : "Already stopped");

    [HttpPost("deadlock-on-threads")]
    public async Task<IActionResult> ExecuteDeadlockOnThreads(DeadlockedWithThreadsRequest request)
    {
        request.ThreadNamePrefix = HttpContext.TraceIdentifier;
        await _deadlockedWithThreadsScript.StartAsync(request);
        return Ok("Started");
    }

    [HttpPost("deadlock-on-threadpool")]
    public async Task<IActionResult> ExecuteDeadlockOnThreadPool(DeadlockOnThreadPoolRequest request)
    {
        await _deadlockOnThreadPoolScript.StartAsync(request);
        return Ok("Started");
    }
}