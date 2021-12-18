using System.ComponentModel.DataAnnotations;

namespace ProjectRestApi.Dtos
{
    public class UserForCreateDto : IValidatableObject
    {
        [Required, MinLength(1)]
        public string name { get; set; }

        [Required, MinLength(8), MaxLength(32)]
        public string password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!password.Any(char.IsUpper)) yield return new ValidationResult("Password must contain an uppercase letter", new[] { nameof(password) });
            if (!password.Any(char.IsLower)) yield return new ValidationResult("Password must contain an lowercase letter", new[] { nameof(password) });
            if (!password.Any(char.IsNumber)) yield return new ValidationResult("Password must contain a digit", new[] { nameof(password) });
        }
    }
}
