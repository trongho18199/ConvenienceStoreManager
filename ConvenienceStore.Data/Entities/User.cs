using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceStore.Data.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Index(IsUnique = true)]
        public string Username { get; set; }

        [Required, MaxLength(200)]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
