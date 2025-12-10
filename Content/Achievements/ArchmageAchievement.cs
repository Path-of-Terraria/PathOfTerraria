using Terraria.Achievements;
using Terraria.GameContent.Achievements;

namespace PathOfTerraria.Content.Achievements;

public class ArchmageAchievement : ModAchievement
{
	public CustomFlagCondition ManaCondition { get; private set; }

	public override void SetStaticDefaults()
	{
		Achievement.SetCategory(AchievementCategory.Challenger);
		
		ManaCondition = AddCondition("Reach1000Mana");
	}
}