// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Shared._Napawlm.SyndicateContractor;

[DataDefinition]
public sealed partial class ContractObjectiveEntry
{
    [DataField(required: true)]
    public EntProtoId Objective;

    [DataField]
    public EntProtoId PayoutPrototype = "Telecrystal50";

    [DataField]
    public int PayoutAmount = 1;

    /// <summary>
    /// Relative weight when picking from the pool. Higher = more likely.
    /// </summary>
    [DataField]
    public float Weight = 1f;
}

/// <summary>
/// A single objective assigned by a contract, plus how much it pays out.
/// Stored on the contract after signing so we don't have to re-look up the pool.
/// </summary>
public sealed class AssignedContractObjective
{
    public EntityUid ObjectiveUid;
    public EntProtoId PayoutPrototype = default!;
    public int PayoutAmount;
}

[RegisterComponent]
public sealed partial class SyndicateContractorContractComponent : Component
{
    [DataField]
    public EntityUid? Signer;

    [DataField]
    public bool IsSigned;

    [DataField]
    public bool PayoutClaimed;

    /// <summary>
    /// How many objectives this contract assigns on signing.
    /// Each one is picked (weighted) from <see cref="ObjectivePool"/>.
    /// </summary>
    [DataField]
    public int ObjectiveCount = 1;

    /// <summary>
    /// Pool of possible objectives.
    /// </summary>
    [DataField]
    public List<ContractObjectiveEntry> ObjectivePool = new();

    // Legacy single-payout fallback (used only if ObjectivePool is empty)
    [DataField]
    public EntProtoId PayoutPrototype = "Telecrystal50";

    [DataField]
    public int PayoutAmount = 1;

    [DataField]
    public EntProtoId AntagRolePrototype = "MindRoleTraitor";

    /// <summary>
    /// Objective entries that were actually assigned by this contract.
    /// </summary>
    public List<AssignedContractObjective> AssignedObjectives = new();
}
