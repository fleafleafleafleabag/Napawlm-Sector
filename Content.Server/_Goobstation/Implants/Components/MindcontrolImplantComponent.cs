// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Implants.Components;

[RegisterComponent]
public sealed partial class MindcontrolImplantComponent : Component
{
    [DataField] public EntityUid? HolderUid = null; //who holds the implanter
    [DataField] public EntityUid? ImplanterUid = null; // the implanter carrying the implant
}
