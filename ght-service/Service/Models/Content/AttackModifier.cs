using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GloomhavenTracker.Service.Models.Content;

[Serializable]
public struct AttackModifier : ContentItem
{
    public AttackModifier(Guid id, string contentCode, string name, string description, bool isCurse, bool isBlessing, bool triggerShuffle, string value, List<Effect> effects, string gameContentCode)
    {
        Id = id;
        ContentCode = contentCode;
        Name = name;
        Description = description;
        IsCurse = isCurse;
        IsBlessing = isBlessing;
        TriggerShuffle = triggerShuffle;
        Value = value;
        Effects = effects;
        GameContentCode = gameContentCode;
    }

    [JsonIgnore]
    public Guid Id { get; }

    [JsonPropertyName("contentCode")]
    public string ContentCode { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("description")]
    public string Description { get; }

    [JsonPropertyName("isCurse")]
    public bool IsCurse { get; }

    [JsonPropertyName("isBlessing")]
    public bool IsBlessing { get; }

    [JsonPropertyName("triggerShuffle")]
    public bool TriggerShuffle { get; }

    [JsonPropertyName("value")]
    public string Value { get; }

    [JsonPropertyName("effects")]
    public List<Effect> Effects { get; }

    [JsonPropertyName("game")]
    public string GameContentCode { get; }

    [JsonIgnore]
    public ContentSummary Summary
    {
        get
        {
            return new ContentSummary(
                ContentCode,
                Name,
                Description,
                Game: GameContentCode
            );
        }
    }
}