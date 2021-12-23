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
        private readonly IUserDbService UserDbService;
        private readonly IMapper Mapper;



        public UserController(IUserDbService UserDbService, IMapper Mapper)
        {
            this.UserDbService = UserDbService;
            this.Mapper = Mapper;
        }

        

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<User> UserList = await UserDbService.GetAll();

            List<UserDto> MappedUserList = Mapper.Map<List<UserDto>>(UserList);

            return Ok(MappedUserList);
        }

     

        [HttpGet("{id}", Name = "GetById")]
        public async Task<IActionResult> GetById(Guid id)
        {
            User User = await UserDbService.GetById(id);

            if (User == null) return NotFound();

            UserDto MappedUser = Mapper.Map<UserDto>(User);

            return Ok(MappedUser);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserForCreateDto User)
        {
            User CreatedUser = await UserDbService.Create(Mapper.Map<User>(User));

            if (CreatedUser == null) return BadRequest();

            UserDto MappedCreatedUser = Mapper.Map<UserDto>(CreatedUser);

            return CreatedAtRoute("GetById", new { id = MappedCreatedUser.id }, MappedCreatedUser);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            User User = await UserDbService.GetById(id);

            if (User == null) return NotFound();

            bool IsDeleted = await UserDbService.Delete(User);

            return IsDeleted == true ? Ok() : BadRequest();
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserForUpdateDto NewUser)
        {
            User User = await UserDbService.GetById(id);

            if (User == null) return NotFound();

            User MappedNewUser = Mapper.Map<User>(NewUser);

            bool IsUpdated = await UserDbService.Update(id, MappedNewUser);

            return IsUpdated == true ? Ok() : BadRequest();
        }
    }
}
