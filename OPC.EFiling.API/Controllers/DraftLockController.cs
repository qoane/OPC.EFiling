using Microsoft.AspNetCore.Mvc;
using OPC.EFiling.Application.Services;
using System.Security.Claims;

namespace OPC.EFiling.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DraftLockController : ControllerBase
{
    private readonly DraftLockService _lockService;

    public DraftLockController(DraftLockService lockService)
    {
        _lockService = lockService;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim is missing or null.");
        }
        return int.Parse(userIdClaim);
    }

    /// <summary>
    /// Try to acquire a lock on an instruction
    /// </summary>
    [HttpPost("acquire/{instructionId}")]
    public async Task<IActionResult> Acquire(int instructionId)
    {
        var success = await _lockService.AcquireLockAsync(instructionId, GetUserId());
        return success ? Ok() : Conflict("Instruction is currently locked by another user.");
    }

    /// <summary>
    /// Release the lock when done
    /// </summary>
    [HttpPost("release/{instructionId}")]
    public async Task<IActionResult> Release(int instructionId)
    {
        await _lockService.ReleaseLockAsync(instructionId, GetUserId());
        return Ok();
    }

    /// <summary>
    /// Used to check lock status
    /// </summary>
    [HttpGet("is-locked/{instructionId}")]
    public async Task<IActionResult> IsLocked(int instructionId)
    {
        var isLocked = await _lockService.IsLockedByAnotherAsync(instructionId, GetUserId());
        return Ok(isLocked);
    }

    /// <summary>
    /// Heartbeat to extend lock expiration
    /// </summary>
    [HttpPost("heartbeat/{instructionId}")]
    public async Task<IActionResult> Heartbeat(int instructionId)
    {
        var renewed = await _lockService.RenewLockAsync(instructionId, GetUserId());
        return renewed ? Ok() : Unauthorized("Lock not held or expired.");
    }
}
