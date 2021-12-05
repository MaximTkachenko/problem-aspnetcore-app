using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ProblemApp.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    /// <summary>
    /// Get application info.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            ProcessId = Process.GetCurrentProcess().Id,
            OSVersion = Environment.OSVersion.ToString(),
            DotnetRuntimeVersion = Environment.Version.ToString()
        });
    }
}