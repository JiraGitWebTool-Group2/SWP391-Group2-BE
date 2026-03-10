using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Group2.Application.Features.Users.Dtos
{
    public class CreateUserRequestDto
    {
        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = default!;

        [Required, StringLength(200)]
        public string FullName { get; set; } = default!;

        // LOCAL thì bắt buộc có Password
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        // LOCAL/GOOGLE/GITHUB
        [StringLength(30)]
        public string? Provider { get; set; }

        // GOOGLE: sub, GITHUB: id/login (tuỳ bạn lưu gì)
        [StringLength(255)]
        public string? ProviderUserId { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
