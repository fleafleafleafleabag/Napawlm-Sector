// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Napawlm.SyndicateContractor;

/// <summary>
/// Marks an entity as unable to sign Syndicate contracts.
/// Use this instead of AntagImmuneComponent if you want to block contracts
/// without blocking the entity from becoming a regular traitor via other means.
/// </summary>
[RegisterComponent]
public sealed partial class ContracteeImmuneComponent : Component
{
}
