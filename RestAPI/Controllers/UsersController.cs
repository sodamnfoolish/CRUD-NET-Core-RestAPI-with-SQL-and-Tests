using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestApi.Interfaces;
using RestApi.Entities;
using RestApi.Dtos;

namespace RestApi.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserDbService userDbService;
        private readonly IMapper mapper;



        public UsersController(IUserDbService _userDbService, IMapper _mapper)
        {
            this.userDbService = _userDbService;
            this.mapper = _mapper;
        }

        

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<User> userList = await userDbService.GetAll();

            List<UserDto> mappedUserList = mapper.Map<List<UserDto>>(userList);

            return Ok(mappedUserList);
        }

     

        [HttpGet("{_id}", Name = "GetById")]
        public async Task<IActionResult> GetById(Guid _id)
        {
            User user = await userDbService.GetById(_id);

            if (user == null) return NotFound();

            UserDto mappedUser = mapper.Map<UserDto>(user);

            return Ok(mappedUser);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserForCreateDto _userForCreate)
        {
            User createdUser = await userDbService.Create(mapper.Map<User>(_userForCreate));

            if (createdUser == null) return BadRequest();

            UserDto mappedCreatedUser = mapper.Map<UserDto>(createdUser);

            return CreatedAtRoute("GetById", new { _id = mappedCreatedUser.id }, mappedCreatedUser);
        }



        [HttpDelete("{_id}")]
        public async Task<IActionResult> Delete(Guid _id)
        {
            User user = await userDbService.GetById(_id);

            if (user == null) return NotFound();

            bool isDeleted = await userDbService.Delete(user);

            return isDeleted == true ? Ok() : BadRequest();
        }



        [HttpPut("{_id}")]
        public async Task<IActionResult> Update(Guid _id, [FromBody] UserForUpdateDto _userForUpdate)
        {
            User user = await userDbService.GetById(_id);

            if (user == null) return NotFound();

            User mappedUserForUpdate = mapper.Map<User>(_userForUpdate);

            bool isUpdated = await userDbService.Update(_id, mappedUserForUpdate);

            return isUpdated == true ? Ok() : BadRequest();
        }
    }
}
