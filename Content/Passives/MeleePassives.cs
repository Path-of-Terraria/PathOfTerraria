using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class CloseRangePassive : Passive
	{
		public CloseRangePassive() : base()
		{
			Name = "Close Combatant";
			Tooltip = "Increases your damage against nearby enemies by 10% per level";
			MaxLevel = 3;
			TreePos = new Vector2(160, 300);
		}
	}

	internal class BleedPassive : Passive
	{
		public BleedPassive() : base()
		{
			Name = "Crimson Dance";
			Tooltip = "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
			MaxLevel = 3;
			TreePos = new Vector2(180, 230);
		}
	}
}
