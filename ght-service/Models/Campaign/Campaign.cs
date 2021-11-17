using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GloomhavenTracker.Service.Models.Campaign;

public class Campaign
{
    public Guid Id {get;}
    public string Description {get;}
    public string Game {get;}
    public List<string> CompletedScenarios {get;}
    public List<string> AvailableScenarios {get;}
    public Party Party {get;}



    public CampaignSummary Summary => new CampaignSummary()
    {
        Id = this.Id.ToString(),
        Game = this.Game.ToString(),
        Description = this.Description
    };

    public Campaign(CampaignDO campaign)
    {
        this.Id = new Guid(campaign.Id);
        this.Description = campaign.Description;
        this.Game = campaign.Game;
        this.CompletedScenarios = campaign.CompletedScenarios;
        this.AvailableScenarios = campaign.AvailableScenarios;
        this.Party = new Party(campaign.Party);
    }

    public CampaignDO ToDO()
    {
        return new CampaignDO()
        {
            Id = this.Id.ToString(),
            Description = this.Description,
            Game = this.Game,
            CompletedScenarios = this.CompletedScenarios,
            AvailableScenarios = this.AvailableScenarios,
            Party = this.Party.ToDO()
        };
    }

}

[Serializable]
public struct CampaignDO
{
    [JsonPropertyName("id")]
    public string Id {get; set;}

    [JsonPropertyName("description")]
    public string Description {get; set;}
    
    [JsonPropertyName("game")]
    public string Game {get; set;}

    [JsonPropertyName("completedScenarios")]
    public List<string> CompletedScenarios {get; set;}

    [JsonPropertyName("availableScenarios")]
    public List<string> AvailableScenarios {get; set;}

    [JsonPropertyName("party")]
    public PartyDO Party {get; set;}
}


[Serializable]
public struct CampaignSummary
{
    [JsonPropertyName("id")]
    public string Id {get; set;}

    [JsonPropertyName("description")]
    public string Description {get; set;}

    [JsonPropertyName("game")]
    public string Game {get; set;}
}