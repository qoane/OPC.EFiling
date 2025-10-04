using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC.EFiling.Domain.Entities
{
    public class ResetPasswordWithOtpModel
    {
        public string? Email { get; set; }
        public string? Otp { get; set; }
        public string? NewPassword { get; set; }
    }
}
