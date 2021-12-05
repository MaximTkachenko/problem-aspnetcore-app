using Microsoft.AspNetCore.Mvc;

namespace ProblemApp.Controllers;

[ApiController]
[Route("[controller]")]
public partial class ScriptsController : ControllerBase
{
    private readonly ILogger<ScriptsController> _logger;

    public ScriptsController(ILogger<ScriptsController> logger)
    {
        _logger = logger;
    }
}