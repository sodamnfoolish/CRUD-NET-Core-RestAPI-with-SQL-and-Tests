using System.ComponentModel.DataAnnotations;

namespace RestApi.Entities
{
    public class User
    {
        [Key]
        public Guid id { get; set; }

        public string name { get; set; }

        public string password { get; set; }
    }
}
