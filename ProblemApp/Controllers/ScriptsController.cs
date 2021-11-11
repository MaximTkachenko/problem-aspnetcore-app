using Microsoft.AspNetCore.Mvc;
using ProblemApp.Scripts;
using static ProblemApp.Scripts.GarbageCollectionStressScript;

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
    public async Task<IActionResult> StartGarbageCollectionStress(StartGarbageCollectionStressRequest request) =>
        Ok(await GarbageCollectionStressScript.Instance.StartAsync(request) ? "Started" : "Already started");

    [HttpDelete("gc-stress")]
    public async Task<IActionResult> StopGarbageCollectionStress() =>
        Ok(await GarbageCollectionStressScript.Instance.StopAsync() ? "Stopped" : "Already stopped");
}