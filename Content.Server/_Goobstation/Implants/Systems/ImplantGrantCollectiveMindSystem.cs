// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Implants.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Implants;

namespace Content.Server._Goobstation.Implants.Systems;

public sealed class ImplantGrantCollectiveMindSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImplantGrantCollectiveMindComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ImplantGrantCollectiveMindComponent, ImplantRemovedEvent>(OnRemoved);
    }

    public void OnImplanted(Entity<ImplantGrantCollectiveMindComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted == null)
            return;

        var target = args.Implanted.Value;
        var mind = EnsureComp<CollectiveMindComponent>(target);
        mind.Channels.Add(ent.Comp.CollectiveMind);
    }

    public void OnRemoved(Entity<ImplantGrantCollectiveMindComponent> ent, ref ImplantRemovedEvent args)
    {
        var target = args.Implanted;

        if (!TryComp<CollectiveMindComponent>(target, out var comp))
            return;

        comp.Channels.Remove(ent.Comp.CollectiveMind);
        if (comp.Channels.Count == 0)
            RemComp(target, comp);
    }
}
