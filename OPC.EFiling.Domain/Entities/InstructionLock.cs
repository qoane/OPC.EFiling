using System;

namespace OPC.EFiling.Domain.Entities
{
    /// <summary>
    /// Represents an exclusive edit lock for a drafting instruction.  When a drafter
    /// begins editing an instruction, a corresponding InstructionLock is created.
    /// Only the user identified by <see cref="LockedByUserID"/> is permitted to edit
    /// the instruction until the lock is removed (e.g., when the draft is
    /// submitted).  Locks can also be purged after a timeout by an administrator.
    /// </summary>
    public class InstructionLock
    {
        public int InstructionLockID { get; set; }

        /// <summary>
        /// The ID of the drafting instruction that is locked.
        /// </summary>
        public int DraftingInstructionID { get; set; }

        /// <summary>
        /// The user who currently holds the lock.
        /// </summary>
        public int LockedByUserID { get; set; }

        /// <summary>
        /// The UTC timestamp when the lock was acquired.
        /// </summary>
        public DateTime LockedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30); // adjustable timeout
    }
}