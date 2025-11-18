using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRoleAuth.Core.DTOs
{
    internal class AuthDtos
    {
        public class CreateUserDto
        {
            public string UserName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string? DisplayName { get; set; }
        }

        public class LoginDto
        {
            public string UserName { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        public class TokenResponseDto
        {
            public string AccessToken { get; set; } = null!;
            public string RefreshToken { get; set; } = null!;
            public DateTime ExpiresAt { get; set; }
        }
    }
}
