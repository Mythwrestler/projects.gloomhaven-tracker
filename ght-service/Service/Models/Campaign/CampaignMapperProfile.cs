using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GloomhavenTracker.Database.Models;
using GloomhavenTracker.Database.Models.Campaign;
using GloomhavenTracker.Service.Models;
using GloomhavenTracker.Service.Models.Campaign;
using GloomhavenTracker.Service.Models.Content;
using CampaignCharacter = GloomhavenTracker.Service.Models.Campaign.Character;
using CampaignScenario = GloomhavenTracker.Service.Models.Campaign.Scenario;
using ContentCharacter = GloomhavenTracker.Service.Models.Content.Character;
using ContentScenario = GloomhavenTracker.Service.Models.Content.Scenario;

namespace GloomhavenTracker.Service.Models.Campaign;

public class CampaignMapperProfile : Profile
{
    public CampaignMapperProfile()
    {
        CreateMap<CampaignScenario, ScenarioDAO>().ConvertUsing((src, dst, ctx) => new ScenarioDAO()
        {
            Id = src.Id,
            ScenarioContentId = src.ContentScenario.Id,
            IsClosed = src.IsClosed,
            IsCompleted = src.IsCompleted
        });

        CreateMap<ScenarioDAO, CampaignScenario>().ConvertUsing((src, dst, ctx) => new CampaignScenario(
            id: src.Id,
            contentScenario: ctx.Mapper.Map<ContentScenario>(src.ScenarioContent),
            isClosed: src.IsClosed,
            isCompleted: src.IsCompleted
        ));

        CreateMap<CampaignScenario, ScenarioDTO>().ConvertUsing((src, dst, ctx) => new ScenarioDTO(
            scenarioContentCode: src.ContentScenario.ContentCode,
            scenarioNumber: src.ContentScenario.ScenarioNumber,
            src.IsClosed,
            src.IsCompleted
        ));

        CreateMap<CampaignCharacter, CharacterDAO>().ConvertUsing((src, dst, ctx) => new CharacterDAO()
        {
            Id = src.Id,
            Name = src.Name,
            Experience = src.Experience,
            Gold = src.Gold,
            PerkPoints = src.PerkPoints,
            CharacterContentId = src.CharacterContent.Id
        });

        CreateMap<CharacterDAO, CampaignCharacter>().ConvertUsing((src, dst, ctx) => new CampaignCharacter(
            id: src.Id,
            name: src.Name,
            characterContent: ctx.Mapper.Map<ContentCharacter>(src.CharacterContent),
            experience: src.Experience,
            gold: src.Gold,
            perkPoints: src.PerkPoints
        ));

        CreateMap<CampaignCharacter, CharacterSummary>().ConvertUsing((src, dst, ctx) => new CharacterSummary(
            name: src.Name,
            characterContentCode: src.CharacterContent.ContentCode
        ));

        CreateMap<CampaignCharacter, CharacterDTO>().ConvertUsing((src, dst, ctx) => new CharacterDTO(
            name: src.Name,
            characterContentCode: src.CharacterContent.ContentCode,
            experience: src.Experience,
            gold: src.Gold,
            items: new List<string>(),
            perkPoints: src.PerkPoints
        ));

        CreateMap<UserCampaignDAO, User>().ConvertUsing((src, dst, ctx) =>
        {
            return new User()
            {
                UserId = src.UserId,
                UserName = src.User?.UserName ?? "",
                FirstName = src.User?.FirstName ?? "",
                LastName = src.User?.FirstName ?? "",
                Email = src.User?.Email ?? ""
            };
        });

        CreateMap<Campaign, CampaignDAO>()
            .ConvertUsing((src, dst, ctx) =>
            {
                List<ScenarioDAO> campaignScenarios = src.Scenarios.Select(kvp => kvp.Value)
                    .Select(scn => new ScenarioDAO()
                    {
                        Id = scn.Id,
                        ScenarioContentId = scn.ContentScenario.Id,
                        CampaignId = src.Id,
                        IsClosed = scn.IsClosed,
                        IsCompleted = scn.IsClosed
                    }).ToList();

                List<CharacterDAO> campaignParty = src.Party.Select(kvp => kvp.Value)
                    .Select(chr => new CharacterDAO()
                    {
                        Id = chr.Id,
                        Name = chr.Name,
                        CharacterContentId = chr.CharacterContent.Id,
                        CampaignId = src.Id,
                        Experience = chr.Experience,
                        Gold = chr.Gold,
                        PerkPoints = chr.PerkPoints
                    }).ToList();


                List<UserCampaignDAO> managers = src.Managers.Values
                    .Select(user => new UserCampaignDAO()
                    {
                        UserId = user.UserId,
                        CampaignId = src.Id,
                        IsOwner = src.Owner.UserId == user.UserId
                    }).ToList();

                CampaignDAO campaign = new CampaignDAO()
                {
                    Id = src.Id,
                    Name = src.Name,
                    Description = src.Description,
                    Party = campaignParty,
                    Scenarios = campaignScenarios,
                    Items = new List<CampaignItemDAO>(),
                    GameId = src.Game.Id,
                    Managers = managers
                };

                return campaign;
            });


        CreateMap<CampaignDAO, Campaign>().ConvertUsing((src, dst, ctx) =>
        {

            Dictionary<string, CampaignScenario> scenarios = src.Scenarios.ToDictionary(
                scn => scn.ScenarioContent?.ContentCode ?? "",
                scn => ctx.Mapper.Map<CampaignScenario>(scn)
            );

            Dictionary<string, CampaignCharacter> party = src.Party.ToDictionary(
                chr => chr.CharacterContent?.ContentCode ?? "",
                chr => ctx.Mapper.Map<CampaignCharacter>(chr)
            );

            Dictionary<Guid, User> managers = src.Managers.ToDictionary(
                mgr => mgr.UserId,
                mgr => ctx.Mapper.Map<User>(mgr)
            );

            return new Campaign(
                id: src.Id,
                name: src.Name,
                description: src.Description,
                game: ctx.Mapper.Map<Game>(src.Game),
                scenarios: scenarios,
                party: party,
                owner: ctx.Mapper.Map<User>(src.Managers.First(m => m.IsOwner)),
                managers: managers
            );
        });

        CreateMap<Campaign, CampaignSummary>().ConvertUsing((src, dst, ctx) => new CampaignSummary(
            id: src.Id,
            name: src.Name,
            description: src.Description,
            gameContentCode: src.Game.ContentCode
        ));

        CreateMap<Campaign, CampaignDTO>().ConvertUsing((src, dst, ctx) => new CampaignDTO(
            id: src.Id,
            name: src.Name,
            description: src.Description,
            gameContentCode: src.Game.ContentCode,
            scenarios: ctx.Mapper.Map<List<ScenarioDTO>>(src.Scenarios.Select(kvp => kvp.Value)),
            party: ctx.Mapper.Map<List<CharacterSummary>>(src.Party.Select(kvp => kvp.Value))
        ));

    }
}