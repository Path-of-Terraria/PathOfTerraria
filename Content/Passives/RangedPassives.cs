using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class AmmoPassive : Passive
	{
		public AmmoPassive() : base()
		{
			name = "Secret Compartment";
			tooltip = "5% chance to not consume ammo per level";
			maxLevel = 3;
			treePos = new Vector2(300, 180);
		}
	}
}
