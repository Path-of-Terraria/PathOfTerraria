namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class CloseRangePassive : Passive
	{
		public CloseRangePassive() : base()
		{
			name = "Close Combatant";
			tooltip = "Increases your damage against nearby enemies by 10% per level";
			maxLevel = 3;
			treePos = new Vector2(160, 300);
		}
	}

	internal class BleedPassive : Passive
	{
		public BleedPassive() : base()
		{
			name = "Crimson Dance";
			tooltip = "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
			maxLevel = 3;
			treePos = new Vector2(180, 230);
		}
	}
}
