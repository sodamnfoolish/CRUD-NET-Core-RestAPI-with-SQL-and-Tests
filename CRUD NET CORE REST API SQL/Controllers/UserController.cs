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
            UserDto MappedUser = Mapper.Map<UserDto>(User);
            return MappedUser != null ? Ok(MappedUser) : BadRequest();
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserForCreateDto User)
        {
            User CreatedUser = await UserDbService.Create(Mapper.Map<User>(User));
            UserDto MappedCreatedUser = Mapper.Map<UserDto>(CreatedUser);
            return MappedCreatedUser != null ? CreatedAtRoute("GetById", new { id = MappedCreatedUser.id }, MappedCreatedUser) : BadRequest();
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            bool IsDeleted = await UserDbService.Delete(id);
            return IsDeleted == true ? Ok() : BadRequest();
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserForUpdateDto User)
        {
            User MappedUser = Mapper.Map<User>(User);
            bool IsUpdated = await UserDbService.Update(id, MappedUser);
            return IsUpdated == true ? Ok() : BadRequest();
        }
    }
}
