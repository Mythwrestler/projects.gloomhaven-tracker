using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GloomhavenTracker.Database.Models;
using GloomhavenTracker.Database.Models.Combat;
using GloomhavenTracker.Service.Models.Combat.Combatant;
using GloomhavenTracker.Service.Models.Combat.Hub;

namespace GloomhavenTracker.Service.Models.Combat;

public class CombatMapperProfile : Profile
{
    public CombatMapperProfile()
    {
        #region Combat Users

        CreateMap<User, CombatUser>().ConvertUsing((src, dst, ctx) =>
        {
            return new CombatUser()
            {
                UserId = src.UserId,
                Username = src.UserName
            };
        });

        #endregion


        #region Attack Modifier Deck
        CreateMap<AttackModifierDeckDAO, AttackModifierDeck>().ConvertUsing((src, dst, ctx) =>
        {
            Dictionary<int, Content.AttackModifier> deck;

            if (src.Cards is null)
                deck = new Dictionary<int, Content.AttackModifier>();
            else
                deck = src.Cards.ToDictionary(
                    card => card.position,
                    card => ctx.Mapper.Map<Content.AttackModifier>(card.AttackModifier)
                );

            return new AttackModifierDeck(
              src.Id,
              deck,
              src.Positions ?? new List<int>()
            );
        });

        CreateMap<AttackModifierDeck, AttackModifierDeckDAO>().ConvertUsing((src, dst, ctx) =>
        {

            List<AttackModifierDeckCardDAO> cards = src.Deck.Select(kvp => new AttackModifierDeckCardDAO()
            {
                DeckId = src.Id,
                AttackModifierId = kvp.Value.Id,
                position = kvp.Key
            }).ToList();

            return new AttackModifierDeckDAO()
            {
                Id = src.Id,
                Cards = cards,
                Positions = src.Positions
            };
        });

        CreateMap<AttackModifierDeck, AttackModifierDeckDTO>().ConvertUsing((src, dst, ctx) =>
        {
            return new AttackModifierDeckDTO()
            {
                DiscardPileCount = src.DiscardPile.Count(),
                DrawPileCount = src.DrawPile.Count(),
                ShownCards = src.ShownCards
            };
        });

        #endregion

        #region Combatants

        CreateMap<CharacterDAO, Character>().ConvertUsing((src, dst, ctx) =>
        {

            return new Character(
              src.Id,
              src.Level,
              src.Health,
              0,
              ctx.Mapper.Map<Campaign.Character>(src.CampaignCharacter)
            );
        });

        CreateMap<Character, CharacterDAO>().ConvertUsing((src, dst, ctx) =>
        {
            return new CharacterDAO()
            {
                Id = src.Id,
                ActiveEffects = new List<CharacterActiveEffectDAO>(),
                CampaignCharacterId = src.CampaignCharacter.Id,
                Health = src.Health,
                Level = src.Level,
            };
        });

        CreateMap<Character, CharacterDTO>().ConvertUsing((src, dst, ctx) =>
        {
            return new CharacterDTO()
            {
                Id = src.Id,
                Level = src.Level,
                Health = src.Health,
                CharacterContentCode = src.CampaignCharacter.CharacterContent.ContentCode,
                Initiative = src.Initiative
            };
        });

        #endregion


        #region Hub



        CreateMap<CombatHubClientDAO, HubClient>().ConvertUsing((src, dst, ctx) =>
        {
            List<Character> Characters = ctx.Mapper.Map<List<Character>>(
              src.Characters
              .Select(chrHub => chrHub.Character)
              .Select(character => character as CharacterDAO)
              .ToList()
            );

            return new HubClient(
              id: src.Id,
              clientId: src.ClientId,
              groupId: src.CombatId.ToString(),
              user: ctx.Mapper.Map<User>(src.User),
              lastSeen: src.LastSeen,
              Characters,
              src.IsObserver
            );
        });

        CreateMap<HubClient, CombatHubClientDAO>().ConvertUsing((src, dst, ctx) =>
        {
            List<CharacterDAO> characters = ctx.Mapper.Map<List<CharacterDAO>>(src.Characters);

            CombatHubClientDAO client = new CombatHubClientDAO()
            {
                Id = src.Id,
                UserId = src.User.UserId,
                CombatId = Guid.Parse(src.GroupId),
                ClientId = src.ClientId,
                LastSeen = src.LastSeen,
                Characters = new List<CharacterCombatHubClientDAO>()
            };

            List<CharacterCombatHubClientDAO> hubCharacters = src.Characters.Select(chr =>
            {
                CharacterDAO characterDAO = ctx.Mapper.Map<CharacterDAO>(chr);
                return new CharacterCombatHubClientDAO()
                {
                    CharacterId = chr.Id,
                    Character = characterDAO,
                    CombatHubClientId = src.Id,
                    CombatHubClient = client
                };
            }).ToList();

            client.Characters = hubCharacters;

            return client;
        });

        #endregion

        #region Combat




        CreateMap<CombatDAO, Combat>().ConvertUsing((src, dst, ctx) =>
        {
            return new Combat(
              src.Id,
              ctx.Mapper.Map<Campaign.Campaign>(src.Campaign),
              ctx.Mapper.Map<Content.Scenario>(src.Scenario),
              src.ScenarioLevel,
              ctx.Mapper.Map<AttackModifierDeck>(src.MonsterModifierDeck),
              ctx.Mapper.Map<List<HubClient>>(src.HubClients.ToList()),
              ctx.Mapper.Map<List<Character>>(src.Characters.ToList())
            );
        });

        CreateMap<Combat, CombatDAO>().ConvertUsing((src, dst, ctx) =>
        {
            var hubClients = ctx.Mapper.Map<List<CombatHubClientDAO>>(src.RegisteredClients);
            hubClients.ForEach(client => client.CombatId = src.Id);

            return new CombatDAO()
            {
                Id = src.Id,
                CampaignId = src.Campaign.Id,
                ScenarioId = src.Scenario.Id,
                ScenarioLevel = src.ScenarioLevel,
                MonsterModifierDeck = ctx.Mapper.Map<AttackModifierDeckDAO>(src.MonsterModifierDeck),
                HubClients = hubClients
            };
        });

        CreateMap<Combat, CombatDTO>().ConvertUsing((src, dst, ctx) =>
        {
            return new CombatDTO()
            {
                CampaignId = src.Campaign.Id,
                Description = src.Description,
                ScenarioContentCode = src.Scenario.ContentCode,
                Id = src.Id,
                ScenarioLevel = src.ScenarioLevel,
                MonsterModifierDeck = ctx.Mapper.Map<AttackModifierDeckDTO>(src.MonsterModifierDeck),
                Characters = ctx.Mapper.Map<List<CharacterDTO>>(src.Characters)
            };
        });

        #endregion
    }
}