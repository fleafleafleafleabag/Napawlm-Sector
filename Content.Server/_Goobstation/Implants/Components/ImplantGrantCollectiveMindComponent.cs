// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Implants.Components;

[RegisterComponent]
public sealed partial class ImplantGrantCollectiveMindComponent : Component
{
    [DataField]
    public ProtoId<CollectiveMindPrototype> CollectiveMind;
}
