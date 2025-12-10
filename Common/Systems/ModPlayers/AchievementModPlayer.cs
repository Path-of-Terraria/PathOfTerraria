using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Content.Achievements;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class AchievementModPlayer : ModPlayer
{
	private bool _hasVisitedRavencrest = false;

	public override void PostUpdate()
	{
		if (Main.myPlayer != Player.whoAmI || Main.dedServ)
		{
			return;
		}

		CheckStatBasedAchievements();
		CheckLocationBasedAchievements();

	}

	private void CheckStatBasedAchievements()
	{
		// Check Juggernaut achievement
		var juggernautAchievement = ModContent.GetInstance<JuggernautAchievement>();
		if (Player.statLife >= 1000 && !juggernautAchievement.LifeCondition.IsCompleted)
		{
			juggernautAchievement.LifeCondition.Complete();
		}

		// Check Archmage achievement 
		var archmageAchievement = ModContent.GetInstance<ArchmageAchievement>();
		if (Player.statMana >= 1000 && !archmageAchievement.ManaCondition.IsCompleted)
		{
			archmageAchievement.ManaCondition.Complete();
		}
	}
	
	private void CheckLocationBasedAchievements()
	{
		// Check New Beginnings achievement - First visit to Ravencrest
		if (!_hasVisitedRavencrest && SubworldSystem.Current is RavencrestSubworld)
		{
			var newBeginningsAchievement = ModContent.GetInstance<NewBeginningsAchievement>();
			if (!newBeginningsAchievement.FirstVisitCondition.IsCompleted)
			{
				newBeginningsAchievement.FirstVisitCondition.Complete();
				_hasVisitedRavencrest = true;
			}
		}
	}

}
