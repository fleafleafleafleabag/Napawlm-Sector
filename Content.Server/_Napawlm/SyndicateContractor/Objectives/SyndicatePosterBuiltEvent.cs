using Robust.Shared.GameObjects;

namespace Content.Server._Napawlm.SyndicateContractor.Objectives;

/// <summary>
/// Raised (broadcast) when someone completes the RandomContrabandPoster construction.
/// </summary>
public readonly record struct ContrabandPosterBuiltEvent(EntityUid User);
