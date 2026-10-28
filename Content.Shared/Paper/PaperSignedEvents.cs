using Robust.Shared.GameObjects;

namespace Content.Shared.Paper; //ported from goob (goop) for the contractor

/// <summary>
/// Raised on a paper entity when a sign attempt is about to happen. Cancellable.
/// </summary>
[ByRefEvent]
public record struct BeingSignedAttemptEvent(
    EntityUid Paper,
    EntityUid Signer,
    EntityUid Pen,
    bool Cancelled = false,
    string? FailReason = null);

/// <summary>
/// Raised on a paper entity after a signature was successfully applied.
/// </summary>
[ByRefEvent]
public record struct SignSuccessfulEvent(
    EntityUid Paper,
    EntityUid User,
    EntityUid Pen);