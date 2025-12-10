using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;

namespace PathOfTerraria.Content.Achievements;


public class PeakAchievement : ModAchievement
{
	
	public CustomFlagCondition Level100Condition { get; private set; }

	public override void SetStaticDefaults()
	{
		Achievement.SetCategory(AchievementCategory.Challenger);
		
		Level100Condition = AddCondition("ReachLevel100");
	}

	
	public override void OnCompleted(Achievement achievement)
	{
		Main.NewText("Congratulations! You've reached the peak of power!", new Color(255, 215, 0)); 
	}
}

