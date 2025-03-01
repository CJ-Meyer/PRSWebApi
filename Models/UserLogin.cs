using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PRSWebApi.Models
{
    public class UserLogin
    {
        [StringLength(20)]
        [Unicode(false)]
        public string Username { get; set; } = null!;

        [StringLength(10)]
        [Unicode(false)]
        public string Password { get; set; } = null!;
    }
}
