using PROG7312_POE_P1.Models;
using PROG7312_POE_P1.Services;
using Microsoft.AspNetCore.Mvc;

namespace PROG7312_POE_P1.Controllers.Api
{
    [ApiController]
    [Route("api/deployment")]
    public class DeploymentController : ControllerBase
    {
        [HttpPost("validate")]
        public IActionResult ValidateDeployment([FromBody] DeploymentNode rootNode)
        {
            DeploymentValidationResult result = DeploymentValidator.Validate(rootNode);
            return Ok(result);
        }
    }
}
