using System;
using System.ComponentModel.DataAnnotations;

namespace Entities.DTOs
{
    // Custom validation attribute để kiểm tra Role chỉ là Admin hoặc Assistant
    public class ValidRoleAttribute : ValidationAttribute
    {
        private static readonly string[] AllowedRoles = { "Admin", "Assistant" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // Role là optional, cho phép null

            string role = value.ToString();
            if (!AllowedRoles.Contains(role))
            {
                return new ValidationResult($"Role phải là một trong các giá trị: {string.Join(", ", AllowedRoles)}");
            }

            return ValidationResult.Success;
        }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public string? FullName { get; set; }

        // Optional role; only Admin or Assistant allowed
        [ValidRole]
        public string? Role { get; set; }
    }

    public class UpdateUserDto
    {
        [Required]
        public Guid Id { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        // Optional: Admin can update role, only Admin or Assistant allowed
        [ValidRole]
        public string? Role { get; set; }
    }
}