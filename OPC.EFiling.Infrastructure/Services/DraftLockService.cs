using Microsoft.EntityFrameworkCore;
using OPC.EFiling.Domain.Entities;
using OPC.EFiling.Infrastructure.Data;

namespace OPC.EFiling.Application.Services
{
    /// <summary>
    /// A service for acquiring and releasing edit locks on drafting instructions.
    /// Ensures that only one drafter may be editing an instruction at a time.
    /// Supports expiration of stale locks.
    /// </summary>
    public class DraftLockService
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _lockDuration = TimeSpan.FromMinutes(1); // configurable duration

        public DraftLockService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Attempts to acquire an edit lock for the given instruction on behalf of a user.
        /// If another user already holds the lock and it hasn't expired, this method returns false.
        /// If the same user holds the lock or the lock is expired, it acquires a new lock.
        /// </summary>
        public async Task<bool> AcquireLockAsync(int instructionId, int userId)
        {
            var existing = await _context.InstructionLocks
                .FirstOrDefaultAsync(l => l.DraftingInstructionID == instructionId);

            if (existing != null)
            {
                if (existing.ExpiresAt <= DateTime.UtcNow)
                {
                    _context.InstructionLocks.Remove(existing);
                    await _context.SaveChangesAsync();
                    existing = null;
                }
                else if (existing.LockedByUserID != userId)
                {
                    return false;
                }
                else
                {
                    return true; // already held by same user
                }
            }

            var newLock = new InstructionLock
            {
                DraftingInstructionID = instructionId,
                LockedByUserID = userId,
                LockedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(_lockDuration)
            };

            _context.InstructionLocks.Add(newLock);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Releases the edit lock if held by the specified user.
        /// </summary>
        public async Task ReleaseLockAsync(int instructionId, int userId)
        {
            var existing = await _context.InstructionLocks
                .FirstOrDefaultAsync(l => l.DraftingInstructionID == instructionId
                                          && l.LockedByUserID == userId);

            if (existing != null)
            {
                _context.InstructionLocks.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Determines whether the instruction is currently locked by someone else (and not expired).
        /// </summary>
        public async Task<bool> IsLockedByAnotherAsync(int instructionId, int userId)
        {
            var existing = await _context.InstructionLocks
                .FirstOrDefaultAsync(l => l.DraftingInstructionID == instructionId);

            if (existing == null || existing.ExpiresAt <= DateTime.UtcNow)
                return false;

            return existing.LockedByUserID != userId;
        }

        /// <summary>
        /// Extends the expiration time of the lock if it's held by the specified user.
        /// Useful for heartbeats or autosave mechanisms.
        /// </summary>
        public async Task ExtendLockAsync(int instructionId, int userId)
        {
            var existing = await _context.InstructionLocks
                .FirstOrDefaultAsync(l => l.DraftingInstructionID == instructionId
                                          && l.LockedByUserID == userId);

            if (existing != null)
            {
                existing.ExpiresAt = DateTime.UtcNow.Add(_lockDuration);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Attempts to renew the lock by extending the expiration time.
        /// Only succeeds if the user currently holds the lock.
        /// </summary>
        public async Task<bool> RenewLockAsync(int instructionId, int userId)
        {
            var existing = await _context.InstructionLocks
                .FirstOrDefaultAsync(l => l.DraftingInstructionID == instructionId
                                          && l.LockedByUserID == userId);

            if (existing == null)
                return false;

            existing.ExpiresAt = DateTime.UtcNow.Add(_lockDuration);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
