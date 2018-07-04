using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyDapper
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Age { get; set; }

        /// <summary>
        /// 1对1
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// 1对多
        /// </summary>
        public List<Role> Roles { get; set; }
    }
}