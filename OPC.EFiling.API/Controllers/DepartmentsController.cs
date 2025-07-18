using Microsoft.AspNetCore.Mvc;
using OPC.EFiling.Infrastructure.Data;
using OPC.EFiling.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace OPC.EFiling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;


        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]

        public IActionResult GetDepartments()
        {
            var departments = _context.Departments.ToList();
            return Ok(departments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public IActionResult CreateDepartment(Department department)
        {
            _context.Departments.Add(department);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetDepartments), new { id = department.DepartmentID }, department);
        }
    }
}
