using Robust.Shared.GameObjects;

namespace Content.Server._Napawlm.SyndicateContractor.Objectives;

/// <summary>
/// Tracks how many contraband posters the objective owner has constructed.
/// Progress is compared against NumberObjectiveComponent.Target.
/// </summary>
[RegisterComponent]
public sealed partial class ContrabandPosterConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Count;
}
