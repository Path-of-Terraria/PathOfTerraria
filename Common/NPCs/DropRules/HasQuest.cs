using PathOfTerraria.Common.Systems.Questing;
using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Common.NPCs.DropRules;

internal class HasQuest(string questKey) : IItemDropRuleCondition
{
	private readonly string _questKey = questKey;

	public bool CanDrop(DropAttemptInfo info)
	{
		return Quest.PlayerHasQuest(info.player.whoAmI, _questKey);
	}

	public bool CanShowItemDropInUI()
	{
		return false;
	}

	public string GetConditionDescription()
	{
		return "";
	}
}