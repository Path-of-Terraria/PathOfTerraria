namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class MinionPassive : Passive
	{
		public MinionPassive() : base()
		{
			name = "Empowered Horde";
			tooltip = "Increases your minions' damage by 10% per level";
			maxLevel = 3;
			treePos = new Vector2(640, 300);
		}
	}

	internal class SentryPassive : Passive
	{
		public SentryPassive() : base()
		{
			name = "Steadfast Sentries";
			tooltip = "Increases your sentries' damage by 10% per level";
			maxLevel = 3;
			treePos = new Vector2(620, 230);
		}
	}
}
