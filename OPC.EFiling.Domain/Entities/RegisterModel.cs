using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC.EFiling.Domain.Entities
{
    public class RegisterModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int DepartmentID { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
