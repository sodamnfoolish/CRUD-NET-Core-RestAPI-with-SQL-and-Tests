using System.ComponentModel.DataAnnotations;

namespace RestApi.Dtos
{
    public class UserForCreateDto : IValidatableObject
    {
        [Required(ErrorMessage = "Name is required.")]
        public string name { get; set; }

        [Required(ErrorMessage = "Password is required."), MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!password.Any(char.IsUpper)) yield return new ValidationResult("Password must contain an uppercase letter.", new[] { nameof(password) });
            if (!password.Any(char.IsLower)) yield return new ValidationResult("Password must contain an lowercase letter.", new[] { nameof(password) });
            if (!password.Any(char.IsNumber)) yield return new ValidationResult("Password must contain a digit.", new[] { nameof(password) });
        }
    }
}
