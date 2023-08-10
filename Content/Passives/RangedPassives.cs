using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class AmmoPassive : Passive
	{
		public AmmoPassive() : base()
		{
			Name = "Secret Compartment";
			Tooltip = "5% chance to not consume ammo per level";
			MaxLevel = 3;
			TreePos = new Vector2(300, 180);
		}
	}
}
