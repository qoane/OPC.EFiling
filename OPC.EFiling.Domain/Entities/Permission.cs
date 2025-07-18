namespace OPC.EFiling.Domain.Entities
{
    public class Permission
    {
        public int PermissionID { get; set; }
        public int RoleID { get; set; }
        public string? PermissionName { get; set; }
    }
}