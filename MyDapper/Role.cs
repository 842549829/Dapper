using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyDapper
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}