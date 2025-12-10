using PathOfTerraria.Content.Items.Currency;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;

namespace PathOfTerraria.Content.Achievements;

public class MirrorMirrorAchievement : ModAchievement
{
	public CustomFlagCondition FindEchoingShardCondition { get; private set; }

	public override void SetStaticDefaults()
	{
		Achievement.SetCategory(AchievementCategory.Collector);
		
		FindEchoingShardCondition = AddCondition("FindEchoingShard");
	}
}