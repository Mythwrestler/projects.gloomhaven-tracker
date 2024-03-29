
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GloomhavenTracker.Database;
using GloomhavenTracker.Database.Models;
using GloomhavenTracker.Database.Models.Combat;
using GloomhavenTracker.Service.Models.Combat;
using GloomhavenTracker.Service.Models.Combat.Hub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GloomhavenTracker.Service.Repos;

public partial interface CombatRepo
{
    public bool CombatExists(Guid combatId);
    public void CreateCombat(Combat combatToSave);
    public Combat GetCombatById(Guid combatId);
    public List<Combat> GetCombatListing();
    public Combat UpdateCombat(Combat combat);
    public Combat UpdateCombat(Combat combat, List<HubClient> clients);
}

public partial class CombatRepoImplementation : CombatRepo
{
    private readonly IMapper mapper;
    private readonly GloomhavenContext context;
    private readonly ILogger<CampaignRepoImplementation> logger;

    public CombatRepoImplementation(GloomhavenContext context, IMapper mapper, ILogger<CampaignRepoImplementation> logger)
    {
        this.context = context;
        this.mapper = mapper;
        this.logger = logger;
    }

    public Combat GetCombatById(Guid combatId)
    {
        return mapper.Map<Combat>(GetCombatDAOById(combatId));
    }

    public List<Combat> GetCombatListing()
    {
        List<CombatDAO> listing = context.CombatCombat
        .Include(combat => combat.Campaign).ThenInclude(campaign => campaign.Game)
        .Include(combat => combat.Campaign).ThenInclude(campaign => campaign.Managers)
        .Include(combat => combat.Scenario)
        .ToList();
        return mapper.Map<List<Combat>>(listing);
    }

    public bool CombatExists(Guid combatId)
    {
        var combat = context.CombatCombat.Find(combatId);
        return (combat != null);
    }

    private CombatDAO GetCombatDAOById(Guid combatId)
    {
        CombatDAO combat = this.context.CombatCombat.Where(combat => combat.Id == combatId)
            // Campaign
            .Include(combat => combat.Campaign).ThenInclude(campaign => campaign.Party).ThenInclude(character => character.CharacterContent)
            .Include(combat => combat.Campaign).ThenInclude(campaign => campaign.Game)
            .Include(combat => combat.Campaign).ThenInclude(campaign => campaign.Managers)

            // Monster Mod Deck
            .Include(combat => combat.MonsterModifierDeck).ThenInclude(modDeck => modDeck.Cards).ThenInclude(card => card.AttackModifier)

            // Scenario
            .Include(combat => combat.Scenario).ThenInclude(scenario => scenario.Monsters).ThenInclude(ms => ms.Monster)
            .Include(combat => combat.Scenario).ThenInclude(scenario => scenario.Objectives).ThenInclude(os => os.Objective)

            // Hubs
            .Include(combat => combat.HubClients).ThenInclude(hubClient => hubClient.User)

            // Characters
            .Include(combat => combat.Characters).ThenInclude(cmbChr => cmbChr.CampaignCharacter).ThenInclude(cmpChr => cmpChr.CharacterContent)
            .Include(combat => combat.Characters).ThenInclude(cmbChr => cmbChr.CombatHubClient)

            .First();
        return combat;
    }

    public void CreateCombat(Combat combatToSave)
    {
        CombatDAO combatDAO = mapper.Map<CombatDAO>(combatToSave);
        context.CombatCombat.Add(combatDAO);
        if (combatDAO.MonsterModifierDeck != null)
            CreateModDeck(combatDAO.MonsterModifierDeck);

        context.SaveChanges();
    }

    public Combat UpdateCombat(Combat combat)
    {
        CombatDAO combatSaving = mapper.Map<CombatDAO>(combat);

        return mapper.Map<Combat>(UpdateCombat(combatSaving));
    }


    public Combat UpdateCombat(Combat combat, List<HubClient> clients)
    {
        CombatDAO combatSaving = mapper.Map<CombatDAO>(combat);

        List<CombatHubClientDAO> clientDAOs = mapper.Map<List<CombatHubClientDAO>>(clients);

        clientDAOs.Where(client => client.CombatId == combatSaving.Id)
            .ToList().ForEach(client => client.Characters.ToList().ForEach(character =>
            {
                var combatCharacter = combatSaving.Characters.FirstOrDefault(chr => chr.Id == character.CharacterId);
                if (combatCharacter is not null) combatCharacter.CombatHubClient = character;
            }));
        return mapper.Map<Combat>(UpdateCombat(combatSaving));
    }

    
    private CombatDAO UpdateCombat(CombatDAO combatSaving)
    {

        CombatDAO combatToUpdate = GetCombatDAOById(combatSaving.Id);

        combatToUpdate.HubClients = combatSaving.HubClients;

        combatToUpdate.Characters.ToList().ForEach(upd => {
            var updateCharacterHub = combatSaving.Characters.FirstOrDefault(chr => chr.Id == upd.Id);
            if (updateCharacterHub is not null) updateCharacterHub.CombatHubClient = upd.CombatHubClient;
        });

        context.SaveChanges();

        return combatToUpdate;
    }
}