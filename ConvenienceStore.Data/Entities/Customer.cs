using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; }
        [MaxLength(30)] public string Phone { get; set; }
        public bool IsVip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
