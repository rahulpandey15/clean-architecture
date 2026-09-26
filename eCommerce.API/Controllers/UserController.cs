using eCommerce.Application.Contracts;
using eCommerce.Application.DTO.Request;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            this._userService = userService;
        }


        [HttpPost]
        public async Task<IActionResult> Post(CreateUserDto createUserDto)
        {
            var response = await _userService.RegisterUserAsync(createUserDto);
            return Created("/user", true);
        }


    }
}
