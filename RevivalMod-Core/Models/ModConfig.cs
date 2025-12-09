namespace RevivalMod.Models;

public class ModConfig
{
    public bool Enabled { get; set; }
    public bool EnabledSelfRevival { get; set; }
    public float SelfRevivalHoldDuration { get; set; }
    public float TeamRevivalHoldDuration { get; set; }
    public float CriticalStateDuration { get; set; }
    public float InvulnerabilityDuration { get; set; }
    public float RevivalCooldown { get; set; }
    public bool RestoreDestroyedBodyParts { get; set; }
    public float RestoreDestroyedBodyPartsPercentage { get; set; }
    public bool ContusionEffect { get; set; }
    public bool StunEffect { get; set; }
    public float HitboxXDimension { get; set; }
    public float HitboxYDimension { get; set; }
    public float HitboxZDimension { get; set; }
    public bool GhostMode { get; set; }
    public bool GodMode { get; set; }
    public bool HeadshotsAreFatal { get; set; }
    public float CriticalStateChance { get; set; }
    public string ItemId { get; set; }
}