// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Implants.Components;
using Content.Shared.Implants;

namespace Content.Server._Goobstation.Implants.Systems;

public sealed class ComponentsImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentsImplantComponent, ImplantImplantedEvent>(OnImplanted);
        SubscribeLocalEvent<ComponentsImplantComponent, ImplantRemovedEvent>(OnRemoved);
    }

    public void OnImplanted(Entity<ComponentsImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted == null)
            return;

        var target = args.Implanted.Value;

        if (ent.Comp.Added is {} added)
        {
            foreach (var name in added.Keys)
            {
                var type = EntityManager.ComponentFactory.GetRegistration(name).Type;
                var newComp = (Component) EntityManager.ComponentFactory.GetComponent(type);
                EntityManager.AddComponent(target, newComp);
            }
        }

        if (ent.Comp.Removed is {} removed)
        {
            foreach (var name in removed.Keys)
            {
                var type = EntityManager.ComponentFactory.GetRegistration(name).Type;
                EntityManager.RemoveComponent(target, type);
            }
        }
    }

    public void OnRemoved(Entity<ComponentsImplantComponent> ent, ref ImplantRemovedEvent args)
    {
        var target = args.Implanted;  // vega
        if (ent.Comp.Removed is {} removed)
        {
            foreach (var name in removed.Keys)
            {
                var type = EntityManager.ComponentFactory.GetRegistration(name).Type;
                var newComp = (Component) EntityManager.ComponentFactory.GetComponent(type);
                EntityManager.AddComponent(target, newComp);
            }
        }

        if (ent.Comp.Added is {} added)
        {
            foreach (var name in added.Keys)
            {
                var type = EntityManager.ComponentFactory.GetRegistration(name).Type;
                EntityManager.RemoveComponent(target, type);
            }
        }
    }
}
