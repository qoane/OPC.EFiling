using Microsoft.AspNetCore.Mvc;

public class CreateInstructionDto
{
    [FromForm(Name = "Title")]
    public string Title { get; set; } = null!;

    // ── NEW: the actual instruction body
    [FromForm(Name = "Description")]
    public string? Description { get; set; }

    [FromForm(Name = "DepartmentID")]
    public int DepartmentID { get; set; }

    [FromForm(Name = "AssignedDrafterID")]
    public int AssignedDrafterID { get; set; }

    [FromForm(Name = "Status")]
    public string? Status { get; set; }

    [FromForm(Name = "Priority")]
    public string? Priority { get; set; }

    [FromForm(Name = "ReceivedDate")]
    public DateTime ReceivedDate { get; set; }

    [FromForm(Name = "Files")]
    public Microsoft.AspNetCore.Http.IFormFileCollection? Files { get; set; }
}
