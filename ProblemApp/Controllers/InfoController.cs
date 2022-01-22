using Microsoft.AspNetCore.Mvc;

namespace ProblemApp.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    /// <summary>
    /// Get application info.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => Ok(new
        {
            ProcessId = Environment.ProcessId,
            OSVersion = Environment.OSVersion.ToString(),
            DotnetRuntimeVersion = Environment.Version.ToString()
        });
}