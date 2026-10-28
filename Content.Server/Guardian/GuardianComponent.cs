namespace Content.Server.Guardian
{
    /// <summary>
    /// Given to guardians to monitor their link with the host
    /// </summary>
    [RegisterComponent]
    public sealed partial class GuardianComponent : Component
    {
        /// <summary>
        /// The guardian host entity
        /// </summary>
        [DataField]
        public EntityUid? Host;

        /// <summary>
        /// Percentage of damage reflected from the guardian to the host
        /// </summary>
        [DataField]
        public float DamageShare { get; set; } = 0.5f;

        /// <summary>
        /// Maximum distance the guardian can travel away from its user, use YAML to set.
        /// </summary>
        [DataField]
        public float DistanceAllowed { get; set; } = 7f;

        /// <summary>
        /// When the guardian is farther than this distance, it is automatically recalled.
        /// </summary>
        [DataField]
        public float HardCapDistance { get; set; } = 12f;

        /// <summary>
        /// If the guardian is currently manifested
        /// </summary>
        [DataField]
        public bool GuardianLoose;

        /// <summary>
        /// If the guardian can be AI based
        /// </summary>
        [DataField("ai")]
        public bool Ai;
    }
}
