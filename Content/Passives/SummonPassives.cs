using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class MinionPassive : Passive
	{
		public MinionPassive() : base()
		{
			Name = "Empowered Horde";
			Tooltip = "Increases your minions' damage by 10% per level";
			MaxLevel = 3;
			TreePos = new Vector2(640, 300);
		}
	}

	internal class SentryPassive : Passive
	{
		public SentryPassive() : base()
		{
			Name = "Steadfast Sentries";
			Tooltip = "Increases your sentries' damage by 10% per level";
			MaxLevel = 3;
			TreePos = new Vector2(620, 230);
		}
	}
}
