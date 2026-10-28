// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._NapawlmSyndicateContractor;

/// <summary>
/// Marks an entity as a Syndicate contractor. Contractors hand out contracts,
/// cannot sign them, and can claim their payout once the contractee finishes the objective.
/// </summary>
[RegisterComponent]
public sealed partial class SyndicateContractorComponent : Component
{
}
