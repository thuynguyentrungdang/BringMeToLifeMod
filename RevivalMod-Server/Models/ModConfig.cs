using System.Text.Json.Serialization;

namespace RevivalModServer.Models;

public class ModConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("enabledSelfRevival")]
    public bool EnabledSelfRevival { get; set; }
    [JsonPropertyName("selfRevivalHoldDuration")]
    public float SelfRevivalHoldDuration { get; set; }
    [JsonPropertyName("teamRevivalHoldDuration")]
    public float TeamRevivalHoldDuration { get; set; }
    [JsonPropertyName("criticalStateDuration")]
    public float CriticalStateDuration { get; set; }
    [JsonPropertyName("invulnerabilityDuration")]
    public float InvulnerabilityDuration { get; set; }
    [JsonPropertyName("revivalCooldown")]
    public float RevivalCooldown { get; set; }
    [JsonPropertyName("restoreDestroyedBodyParts")]
    public bool RestoreDestroyedBodyParts { get; set; }
    [JsonPropertyName("restoreDestroyedBodyPartsPercentage")]
    public float RestoreDestroyedBodyPartsPercentage { get; set; }
    [JsonPropertyName("contusionEffect")]
    public bool ContusionEffect { get; set; }
    [JsonPropertyName("stunEffect")]
    public bool StunEffect { get; set; }
    [JsonPropertyName("hitboxXDimension")]
    public float HitboxXDimension { get; set; }
    [JsonPropertyName("hitboxYDimension")]
    public float HitboxYDimension { get; set; }
    [JsonPropertyName("hitboxZDimension")]
    public float HitboxZDimension { get; set; }
    [JsonPropertyName("ghostMode")]
    public bool GhostMode { get; set; }
    [JsonPropertyName("godMode")]
    public bool GodMode { get; set; }
    [JsonPropertyName("headshotsAreFatal")]
    public bool HeadshotsAreFatal { get; set; }
    [JsonPropertyName("criticalStateChance")]
    public float CriticalStateChance { get; set; }
    [JsonPropertyName("itemId")]
    public required string ItemId { get; set; }
}