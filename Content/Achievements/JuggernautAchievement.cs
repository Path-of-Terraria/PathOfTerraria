using Terraria.Achievements;
using Terraria.GameContent.Achievements;

namespace PathOfTerraria.Content.Achievements;


public class JuggernautAchievement : ModAchievement
{
	public CustomFlagCondition LifeCondition { get; private set; }

	public override void SetStaticDefaults()
	{
		Achievement.SetCategory(AchievementCategory.Challenger);
		
		LifeCondition = AddCondition("Reach1000Life");
	}
}
