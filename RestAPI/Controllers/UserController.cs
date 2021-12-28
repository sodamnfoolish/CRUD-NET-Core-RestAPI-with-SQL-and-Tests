using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestApi.Interfaces;
using RestApi.Entities;
using RestApi.Dtos;

namespace RestApi.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserDbService _userDbService;
        private readonly IMapper _mapper;



        public UserController(IUserDbService userDbService, IMapper mapper)
        {
            this._userDbService = userDbService;
            this._mapper = mapper;
        }

        

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userList = await _userDbService.GetAll();

            var mappedUserList = _mapper.Map<List<UserDto>>(userList);

            return Ok(mappedUserList);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserForCreateDto userForCreate)
        {
            var user = new User
            {
                id = Guid.NewGuid(),
                name = userForCreate.name,
                password = userForCreate.password,
            };

            var createdUser = await _userDbService.Create(user);

            if (createdUser == null) return StatusCode(StatusCodes.Status500InternalServerError);

            var mappedCreatedUser = _mapper.Map<UserDto>(createdUser);

            return CreatedAtAction(nameof(GetById), new { id = mappedCreatedUser.id }, mappedCreatedUser);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userDbService.GetById(id);

            if (user == null) return NotFound($"Cannot find User with {id} id.");

            var mappedUser = _mapper.Map<UserDto>(user);

            return Ok(mappedUser);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userDbService.GetById(id);

            if (user == null) return NotFound($"Cannot find User with {id} id.");

            var deletedUser = await _userDbService.Delete(user);

            if (deletedUser == null) return StatusCode(StatusCodes.Status500InternalServerError);

            var mappedDeletedUser = _mapper.Map<UserDto>(deletedUser);

            return Ok(mappedDeletedUser);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserForUpdateDto userForUpdate)
        {
            var user = await _userDbService.GetById(id);

            if (user == null) return NotFound($"Cannot find User with {id} id.");

            user.name = userForUpdate.name;
            user.password = userForUpdate.password;

            var updatedUser = await _userDbService.Update(user);

            if (updatedUser == null) return StatusCode(StatusCodes.Status500InternalServerError);

            var mappedUpdatedUser = _mapper.Map<UserDto>(updatedUser);

            return Ok(mappedUpdatedUser);
        }
    }
}
