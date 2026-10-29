using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Robust.Shared.GameObjects;

namespace Content.Server._Napawlm.SyndicateContractor.Objectives;

public sealed class ContrabandPosterConditionSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = null!;
    [Dependency] private readonly NumberObjectiveSystem _numberObjectives = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ContrabandPosterBuiltEvent>(OnPosterBuilt);
        SubscribeLocalEvent<ContrabandPosterConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnPosterBuilt(ContrabandPosterBuiltEvent args)
    {
        if (!_mind.TryGetMind(args.User, out _, out var mind))
            return;

        foreach (var objId in mind.Objectives)
        {
            if (!TryComp<ContrabandPosterConditionComponent>(objId, out var cond))
                continue;

            cond.Count++;
            Dirty(objId, cond);
        }
    }

    private void OnGetProgress(Entity<ContrabandPosterConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var target = _numberObjectives.GetTarget(ent);
        args.Progress = target <= 0
        ? 1f
        : Math.Clamp((float) ent.Comp.Count / target, 0f, 1f);
    }
}
