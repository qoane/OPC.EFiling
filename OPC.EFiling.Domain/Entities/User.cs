using Microsoft.AspNetCore.Identity;

public class User : IdentityUser<int>
{
    public int UserID { get; set; }
    public string? FullName { get; set; }
    public int DepartmentID { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ResetOtp { get; set; }
    public DateTime? ResetOtpExpiry { get; set; }

}
