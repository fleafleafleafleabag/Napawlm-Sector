using Content.Shared.Construction;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Napawlm.SyndicateContractor.Objectives;

/// <summary>
/// Graph action: raises ContrabandPosterBuiltEvent on the local event bus
/// with the user who completed the construction.
/// </summary>
[DataDefinition]
public sealed partial class NotifyContrabandPosterBuilt : IGraphAction
{
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        if (userUid == null)
            return;

        var ev = new ContrabandPosterBuiltEvent(userUid.Value);
        entityManager.EventBus.RaiseEvent(EventSource.Local, ev);
    }
}
