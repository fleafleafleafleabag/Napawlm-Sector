using Content.Server.Mind;
using Content.Server._Napawlm.Objectives;
using Content.Shared.Mind;
using Robust.Shared.GameObjects;

namespace Content.Server._Napawlm.SyndicateContractor.Objectives;

public sealed class ContrabandPosterConditionSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ContrabandPosterBuiltEvent>(OnPosterBuilt);
        SubscribeLocalEvent<ContrabandPosterConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnPosterBuilt(ContrabandPosterBuiltEvent args)
    {
        if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
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
        if (!TryComp<NumberObjectiveComponent>(ent, out var number))
            return;

        args.Progress = number.Target <= 0
            ? 1f
            : Math.Clamp((float) ent.Comp.Count / number.Target, 0f, 1f);
    }
}
