// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Shitcode.CyberEyeHack;

[RegisterComponent]
public sealed partial class CyberEyeHackComponent : Component
{
    [DataField]
    public EntProtoId OpenUiAction = "ActionCyberEyeHackOpenUi";

    [DataField]
    public EntProtoId OverloadAction = "ActionCyberEyeHackOverload";

    [ViewVariables]
    public EntityUid? OpenUiActionUid;

    [ViewVariables]
    public EntityUid? OverloadActionUid;

    [DataField]
    public float Range = 4f;

    [DataField]
    public TimeSpan OverloadCooldown = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Server-side timestamp of the last overload. Prevents rapid repeat use.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastOverload = TimeSpan.Zero;

    /// <summary>
    /// Multiplier on the computed explosion intensity.
    /// </summary>
    [DataField]
    public float ExplosionMultiplier = 1f;

    /// <summary>
    /// What's left behind after a machine is overloaded.
    /// </summary>
    [DataField]
    public EntProtoId DebrisProto = "Ash";
}

public sealed partial class CyberEyeHackOpenUiEvent : EntityTargetActionEvent;
public sealed partial class CyberEyeHackOverloadEvent : EntityTargetActionEvent;
