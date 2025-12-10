using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace PathOfTerraria.Content.Achievements;

public class NewBeginningsAchievement : ModAchievement
{
	public CustomFlagCondition FirstVisitCondition { get; private set; }

	public override void SetStaticDefaults()
	{
		Achievement.SetCategory(AchievementCategory.Explorer);
		
		FirstVisitCondition = AddCondition("FirstRavencrestVisit");
	}

	public override void OnCompleted(Achievement achievement)
	{
		Main.NewText("Welcome to Ravencrest! Your journey begins here.", new Color(255, 215, 0));
	}
}