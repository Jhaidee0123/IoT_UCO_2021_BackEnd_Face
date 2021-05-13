using FaceId.Dto;
using FaceId.Infrastructure;
using FaceId.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FaceId.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FaceController : ControllerBase
    {
        private readonly ILogger<FaceController> _logger;
        private readonly IFaceService _faceService;

        public FaceController(ILogger<FaceController> logger, IFaceService faceService)
        {
            _faceService = faceService;
            _logger = logger;
        }

        [HttpPost("train")]
        public async Task<IActionResult> TrainPersonGroup([FromBody] UserInformationDto userInformation)
        {
            await _faceService.TrainPersonGroupAsync(userInformation.RegisterUserPhotoUrl, userInformation.Email);
            return Ok();
        }

        [HttpPost("validate-face")]
        public async Task<IActionResult> ValidateFaceIdentity([FromBody] ValidateUserDto userDto)
        {
            var validation = await _faceService.ValidatePerson(userDto.Url);
            if (validation)
            {
                return Ok(validation);
            }
            else
            {
                return Unauthorized("Usuario no autorizado");   
            }
        }
    }
}
