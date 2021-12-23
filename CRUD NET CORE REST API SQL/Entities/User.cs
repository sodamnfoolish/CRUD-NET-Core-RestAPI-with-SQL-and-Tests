using System.ComponentModel.DataAnnotations;

namespace RestApi.Entities
{
    public class User
    {
        [Key]
        public Guid id { get; set; }

        public string name { get; set; }

        public string password { get; set; }

        public User() { }
        public User(Guid id, string name, string password)
        {
            this.id = id;
            this.name = name;
            this.password = password;
        }
        public User(User user)
        {
            this.id = user.id;
            this.name = user.name;
            this.password = user.password;
        }
    }
}
