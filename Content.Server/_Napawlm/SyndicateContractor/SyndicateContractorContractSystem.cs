// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Antag.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Server.Roles;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Objectives.Systems;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared._Napawlm.SyndicateContractor;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Content.Shared.NPC.Systems;
using Content.Server.Antag;
using Content.Shared.Roles.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Audio;
using System.Linq;

namespace Content.Server._Napawlm.SyndicateContractor;

public sealed class SyndicateContractorContractSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _npcFaction = null!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = null!;
    [Dependency] private readonly MindSystem _mind = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly HandsSystem _hands = null!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = null!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = null!;
    [Dependency] private readonly AntagSelectionSystem _antag = null!;
    [Dependency] private readonly ISharedPlayerManager _player = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    private static readonly Color ContracteeBriefingColor = Color.FromHex("#cc3b3b");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SyndicateContractorContractComponent, BeingSignedAttemptEvent>(OnSignAttempt);
        SubscribeLocalEvent<SyndicateContractorContractComponent, SignSuccessfulEvent>(OnSigned);
        SubscribeLocalEvent<SyndicateContractorContractComponent, ExaminedEvent>(OnExamined);
    }

    private void OnSignAttempt(Entity<SyndicateContractorContractComponent> ent, ref BeingSignedAttemptEvent args)
    {
        var contract = ent.Comp;

        if (HasComp<SyndicateContractorComponent>(args.Signer))
        {
            args.Cancelled = true;
            TryClaimFromPen(ent, args.Signer);
            return;
        }

        if (contract.IsSigned)
        {
            args.Cancelled = true;
            args.FailReason = "syndicate-contract-already-signed";
            return;
        }

        if (HasComp<AntagImmuneComponent>(args.Signer))
        {
            args.Cancelled = true;
            args.FailReason = "syndicate-contract-immune-fail";
        }
    }

    private void TryClaimFromPen(Entity<SyndicateContractorContractComponent> ent, EntityUid contractor)
    {
        var contract = ent.Comp;

        if (!contract.IsSigned)
        {
            _popupSystem.PopupEntity(Loc.GetString("syndicate-contract-claim-fail"),
                                     ent, contractor, PopupType.MediumCaution);
            return;
        }

        if (contract.PayoutClaimed)
            return;

        if (!AreContractObjectivesComplete(contract))
        {
            _popupSystem.PopupEntity(Loc.GetString("syndicate-contract-not-completed"),
                                     ent, contractor, PopupType.MediumCaution);
            return;
        }

        ClaimPayout(ent, contractor);
    }

    private void OnSigned(Entity<SyndicateContractorContractComponent> ent, ref SignSuccessfulEvent args)
    {
        var contract = ent.Comp;
        if (contract.IsSigned)
            return;

        contract.IsSigned = true;
        contract.Signer = args.User;

        if (TryComp<PaperComponent>(ent, out var paper))
            paper.EditingDisabled = true;

        MakeContracteeAntag(ent, args.User);

        _popupSystem.PopupEntity(Loc.GetString("syndicate-contract-signed-success"), ent, args.User);
    }

    private void MakeContracteeAntag(Entity<SyndicateContractorContractComponent> ent, EntityUid contractee)
    {
        var contract = ent.Comp;

        if (!_mind.TryGetMind(contractee, out var mindId, out var mind))
            return;

        if (!_roleSystem.MindHasRole<TraitorRoleComponent>(mindId, out _))
        {
            _roleSystem.MindAddRole(mindId, "MindRoleTraitor", mind, silent: true);

            if (_roleSystem.MindHasRole<TraitorRoleComponent>(mindId, out var roleEnt))
            {
                var briefing = EnsureComp<RoleBriefingComponent>(roleEnt.Value.Owner);
                briefing.Briefing = Loc.GetString("syndicate-contractee-briefing");
            }

            if (_player.TryGetSessionById(mind.UserId, out var session) && session != null)
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("syndicate-contractee-popup"),
                                         contractee,
                                         PopupType.LargeCaution);

                _antag.SendBriefing(
                    session,
                    Loc.GetString("syndicate-contractee-briefing-start"),
                                    ContracteeBriefingColor,
                                    new SoundPathSpecifier("/Audio/Ambience/Antag/traitor_start.ogg"));
            }

            if (mind.RoleType == "Neutral")
                mind.RoleType = "Traitor";

            _npcFaction.RemoveFaction(contractee, "NanoTrasen", false);
            _npcFaction.AddFaction(contractee, "Syndicate");
        }

        AddContractObjectives(mindId, mind, contract);
    }

    private void AddContractObjectives(EntityUid mindId, MindComponent mind,
                                       SyndicateContractorContractComponent contract)
    {
        List<ContractObjectiveEntry> pool;
        if (contract.ObjectivePool.Count > 0)
            pool = new List<ContractObjectiveEntry>(contract.ObjectivePool);
        else
            pool = new List<ContractObjectiveEntry>
            {
                new()
                {
                    Objective = "KillRandomPersonObjective",
                    PayoutPrototype = contract.PayoutPrototype,
                    PayoutAmount = contract.PayoutAmount,
                    Weight = 1f,
                }
            };

        var added = 0;
        while (added < contract.ObjectiveCount && pool.Count > 0)
        {
            var entry = PickWeighted(pool);
            if (entry == null)
                break;
            pool.Remove(entry);

            if (!_mind.TryAddObjective(mindId, mind, entry.Objective))
                continue;

                added++;
            contract.AssignedObjectives.Add(new AssignedContractObjective
            {
                ObjectiveUid = mind.Objectives[^1],
                PayoutPrototype = entry.PayoutPrototype,
                PayoutAmount = entry.PayoutAmount,
            });
        }

        if (added < contract.ObjectiveCount)
            Log.Warning($"[Contract] Only added {added}/{contract.ObjectiveCount} objectives for {ToPrettyString(mindId)}");
    }

    private ContractObjectiveEntry? PickWeighted(List<ContractObjectiveEntry> pool)
    {
        var total = pool.Sum(e => e.Weight);
        if (total <= 0f)
            return pool.Count > 0 ? pool[0] : null;

        var roll = _random.NextFloat() * total;
        var acc = 0f;
        foreach (var e in pool)
        {
            acc += e.Weight;
            if (roll <= acc)
                return e;
        }
        return pool[^1];
    }

    private bool AreContractObjectivesComplete(SyndicateContractorContractComponent contract)
    {
        if (contract.Signer == null)
            return false;
        if (contract.AssignedObjectives.Count == 0)
            return false;
        if (!_mind.TryGetMind(contract.Signer.Value, out var mindId, out var mind))
            return false;

        foreach (var obj in contract.AssignedObjectives)
        {
            if (!_objectives.IsCompleted(obj.ObjectiveUid, (mindId, mind)))
                return false;
        }
        return true;
    }

    private void ClaimPayout(Entity<SyndicateContractorContractComponent> ent, EntityUid contractor)
    {
        var contract = ent.Comp;
        contract.PayoutClaimed = true;

        var coords = Transform(ent).Coordinates;
        foreach (var obj in contract.AssignedObjectives)
        {
            for (var i = 0; i < obj.PayoutAmount; i++)
            {
                var spawned = Spawn(obj.PayoutPrototype, coords);
                _hands.TryPickupAnyHand(contractor, spawned, false, false, false);
            }
        }
        }

        RemoveContractObjectives(contract);
        Spawn("Ash", coords);
        QueueDel(ent);

        _popupSystem.PopupEntity(Loc.GetString("syndicate-contract-claimed-success"), contractor, contractor);
        _audio.PlayPvs(new SoundCollectionSpecifier("Fire"), coords);
    }

    private void RemoveContractObjectives(SyndicateContractorContractComponent contract)
    {
        if (contract.Signer == null)
            return;

        if (!_mind.TryGetMind(contract.Signer.Value, out var mindId, out var mind))
            return;

        foreach (var obj in contract.AssignedObjectives)
        {
            var index = mind.Objectives.IndexOf(obj.ObjectiveUid);
            if (index >= 0)
                _mind.TryRemoveObjective(mindId, mind, index);
        }
    }


    private void OnExamined(Entity<SyndicateContractorContractComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var contract = ent.Comp;

        args.PushMarkup(Loc.GetString("syndicate-contract-examine-twisted"));

        if (contract.PayoutClaimed)
        {
            args.PushMarkup(Loc.GetString("syndicate-contract-examine-claimed"));
            return;
        }

        if (!contract.IsSigned)
        {
            args.PushMarkup(Loc.GetString("syndicate-contract-examine-unsigned"));
            return;
        }

        if (!AreContractObjectivesComplete(contract))
        {
            args.PushMarkup(Loc.GetString("syndicate-contract-examine-in-progress"));
            return;
        }

        if (HasComp<SyndicateContractorComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("syndicate-contract-examine-claim-ready-contractor"));
        else
            args.PushMarkup(Loc.GetString("syndicate-contract-examine-claim-ready"));
    }

}
