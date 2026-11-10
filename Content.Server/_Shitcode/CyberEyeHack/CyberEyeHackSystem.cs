// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Explosion.EntitySystems;
using Content.Shared._Shitcode.CyberEyeHack;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared.NodeContainer;
using Content.Shared.Popups;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Shared.Timing;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Interaction;

namespace Content.Server._Shitcode.CyberEyeHack;

public sealed class CyberEyeHackSystem : EntitySystem
{
    [Dependency] private readonly ExplosionSystem _explosion = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly ISharedAdminLogManager _log = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = null!;
    [Dependency] private readonly SharedTransformSystem _xform = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;
    [Dependency] private readonly SharedInteractionSystem _interaction = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnchorableComponent,GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<CyberEyeHackComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CyberEyeHackComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CyberEyeHackComponent, CyberEyeHackOpenUiEvent>(OnOpenUiAction);
        SubscribeLocalEvent<CyberEyeHackComponent, CyberEyeHackOverloadEvent>(OnOverloadAction);
    }

    private void OnInit(Entity<CyberEyeHackComponent> ent, ref ComponentInit args)
    {
        ent.Comp.OpenUiActionUid   = _actions.AddAction(ent.Owner, ent.Comp.OpenUiAction,   ent);
        ent.Comp.OverloadActionUid = _actions.AddAction(ent.Owner, ent.Comp.OverloadAction, ent);
    }

    private void OnShutdown(Entity<CyberEyeHackComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.OpenUiActionUid);
        _actions.RemoveAction(ent.Owner, ent.Comp.OverloadActionUid);
        ent.Comp.OpenUiActionUid   = null;
        ent.Comp.OverloadActionUid = null;
    }

    private void OnOpenUiAction(Entity<CyberEyeHackComponent> ent, ref CyberEyeHackOpenUiEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        var target = args.Target;

        if (!HasComp<StationAiWhitelistComponent>(target))
            return;
        if (!_interaction.InRangeUnobstructed(user, target, ent.Comp.Range))
            return;
        if (!_ui.HasUi(target, AiUi.Key))
            return;

        _ui.OpenUi(target, AiUi.Key, user);
        args.Handled = true;
    }

    private void OnOverloadAction(Entity<CyberEyeHackComponent> ent, ref CyberEyeHackOverloadEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        var target = args.Target;

        if (!HasComp<StationAiWhitelistComponent>(target))
            return;
        if (!_interaction.InRangeUnobstructed(user, target, ent.Comp.Range))
            return;

        var hasApc      = TryComp<SharedApcPowerReceiverComponent>(target, out var apc);
        var hasBattery = TryComp<BatteryComponent>(target, out var battery);
        var hasNodes   = HasComp<NodeContainerComponent>(target);

        if (!hasApc && !hasBattery && !hasNodes)
            return;

        var isPowered = false;
        if (hasApc && _power.IsPowered(new Entity<SharedApcPowerReceiverComponent?>(target, apc)))
            isPowered = true;
        else if (hasBattery && battery!.CurrentCharge > 0f)
            isPowered = true;
        else if (hasNodes && !hasApc && !hasBattery)
            isPowered = true;

        if (!isPowered)
        {
            _popup.PopupEntity(Loc.GetString("cyber-eye-hack-unpowered"), user, user);
            args.Handled = true;
            return;
        }

        if (_timing.CurTime < ent.Comp.LastOverload + ent.Comp.OverloadCooldown)
        {
            _popup.PopupEntity(Loc.GetString("cyber-eye-hack-overload-cooldown"), user, user);
            args.Handled = true;
            return;
        }

        Overload(target, user, ent.Comp);
        args.Handled = true;
    }

    private void OnGetVerbs(Entity<AnchorableComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!HasComp<StationAiWhitelistComponent>(ent.Owner))
            return;

        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        if (!TryComp<CyberEyeHackComponent>(user, out var hack))
            return;

        var target = ent.Owner;

        if (!_interaction.InRangeUnobstructed(user, target, hack.Range))
            return;

        var hasApc     = TryComp<ApcPowerReceiverComponent>(target, out var apc);
        var hasBattery = TryComp<BatteryComponent>(target, out var battery);
        var hasNodes   = HasComp<NodeContainerComponent>(target);

        if (!hasApc && !hasBattery && !hasNodes)
            return;

        var isPowered = false;
        if (hasApc && _power.IsPowered((target, apc)))
            isPowered = true;
        else if (hasBattery && battery!.CurrentCharge > 0f)
            isPowered = true;
        else if (hasNodes && !hasApc && !hasBattery)
            isPowered = true;

        if (!isPowered)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("cyber-eye-hack-unpowered"),
                           Disabled = true,
            });
            return;
        }

        if (_ui.HasUi(target, AiUi.Key))
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("cyber-eye-hack-open-interface"),
                           Act = () =>
                           {
                               if (!_interaction.InRangeUnobstructed(user, target, hack.Range))
                                   return;
                               _ui.OpenUi(target, AiUi.Key, user);
                           },
                           Impact = LogImpact.Low,
            });
        }

        var onCooldown = _timing.CurTime < hack.LastOverload + hack.OverloadCooldown;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = onCooldown
            ? Loc.GetString("cyber-eye-hack-overload-cooldown")
            : Loc.GetString("cyber-eye-hack-overload"),
                       Disabled = onCooldown,
                       Act = () =>
                       {
                           if (_timing.CurTime < hack.LastOverload + hack.OverloadCooldown)
                               return;
                           if (!_interaction.InRangeUnobstructed(user, target, hack.Range))
                               return;
                           Overload(target, user, hack);
                       },
                       Impact = LogImpact.High,
        });
    }

    private void Overload(EntityUid target, EntityUid user, CyberEyeHackComponent hack)
    {
        hack.LastOverload = _timing.CurTime;

        var intensity = 2f;
        if (TryComp<BatteryComponent>(target, out var battery))
        {
            intensity = Math.Clamp(battery.CurrentCharge / 5000f, 1f, 6f);
        }
        else if (TryComp<ApcPowerReceiverComponent>(target, out var apc))
        {
            intensity = Math.Clamp(apc.Load / 500f, 1f, 6f);
        }

        intensity *= hack.ExplosionMultiplier;

        var coords = Transform(target).Coordinates;
        var debris = hack.DebrisProto;

        _popup.PopupEntity(Loc.GetString("cyber-eye-hack-overload-popup"), user, user, PopupType.MediumCaution);

        Spawn(debris, coords);

        _explosion.QueueExplosion(
            target,
            typeId: "Default",
            totalIntensity: intensity,
            slope: 1,
            maxTileIntensity: intensity,
            maxTileBreak: (int)intensity,
            addLog: true);

        _log.Add(LogType.Explosion, LogImpact.High,
            $"{ToPrettyString(user):player} overloaded {ToPrettyString(target):machine} via cyber eye implant.");
    }
}
