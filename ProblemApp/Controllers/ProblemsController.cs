using Microsoft.AspNetCore.Mvc;

namespace ProblemApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    // based on https://github.com/fremag/MemoScope.Net/tree/master/MemoDummy
    public class ProblemsController : ControllerBase
    {
        private readonly ILogger<ProblemsController> _logger;

        public ProblemsController(ILogger<ProblemsController> logger)
        {
            _logger = logger;
        }

        [HttpPost("FragmentedMemory")]
        public IActionResult ExecFragmentedMemory()
        {
            return Ok();
        }
    }
}