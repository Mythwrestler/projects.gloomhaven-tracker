using System;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;

namespace GloomhavenTracker.Service.Models.Content;

public enum CONTENT_TYPE
{
    game,
    scenario,
    monster,
    character,
    objective,
    attackModifier,
    item,
    perk
}

public interface ContentItem
{
    public Guid Id { get; }
    public string ContentCode { get; }
    public string Name { get; }
    public string Description { get; }
    public string GameContentCode { get; }
    public ContentSummary Summary { get; }
}

[Serializable]
public struct ContentSummary
{
    public ContentSummary(string ContentCode, string Name, string Description, string Game, int? SortOrder = null)
    {
        this.ContentCode = ContentCode;
        this.Name = Name;
        this.Description = Description;
        this.Game = Game;
        this.SortOrder = SortOrder;
    }
    [JsonPropertyName("contentCode")]
    public string ContentCode { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("description")]
    public string Description { get; }

    [JsonPropertyName("sortOrder")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SortOrder { get; }

    [JsonPropertyName("game")]
    public string Game { get; }
}


public static class GameUtils
{
    public static string GameTypeString(GAME_TYPE? game)
    {
        switch (game)
        {
            case GAME_TYPE.jawsOfTheLion:
                return "jawsOfTheLion";
            case GAME_TYPE.original:
                return "original";
            default:
                return "";
        }
    }

    public static GAME_TYPE GameType(string gameString)
    {
        switch (gameString.ToUpper())
        {
            case "JAWSOFTHELION":
                return GAME_TYPE.jawsOfTheLion;
            case "ORIGINAL":
                return GAME_TYPE.original;
            default:
                throw new InvalidCastException("invalid game code string");
        }
    }

    public static string ContentTypeString(CONTENT_TYPE? type)
    {
        switch (type)
        {
            case CONTENT_TYPE.game:
                return "game";
            case CONTENT_TYPE.monster:
                return "monster";
            case CONTENT_TYPE.objective:
                return "objective";
            case CONTENT_TYPE.character:
                return "character";
            case CONTENT_TYPE.scenario:
                return "scenario";
            case CONTENT_TYPE.attackModifier:
                return "attackModifier";
            case CONTENT_TYPE.item:
                return "item";
            case CONTENT_TYPE.perk:
                return "perk";
            default:
                return "";
        }
    }

    public static int GetPlayerLevel(Character character, int experience)
    {
        return character.BaseStats.Levels
            .Where(lvl => lvl.Experience <= experience)
            .OrderByDescending(lvl => lvl.Experience)
            .FirstOrDefault().Level;
    }

    public static int GetPlayerBaseHealth(Character character, int experience)
    {
        var level = GetPlayerLevel(character, experience);
        return character.BaseStats.Health
            .Where(hl => hl.Level <= level)
            .OrderByDescending(hl => hl.Level)
            .FirstOrDefault().Health;
    }

    public static int ResolveModifierValue(string expression, int attackValue)
    {
        return ResolveExpression(expression, 0, 0, attackValue);
    }

    public static int ResolveStatExpression(string expression, int characterCount = 0, int scenarioLevel = 0)
    {
        return ResolveExpression(expression, characterCount, scenarioLevel, 0);
    }

    public static int ResolveExpression(string expression, int characterCount = 0, int scenarioLevel = 0, int attackValue = 0)
    {
        var expressionThings = expression.Replace("C", characterCount.ToString());
        expressionThings = expressionThings.Replace("L", scenarioLevel.ToString());
        expressionThings = expressionThings.Replace("A", attackValue.ToString());
        return Convert.ToInt16(new DataTable().Compute(expressionThings, null));
    }

}