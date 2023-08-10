using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class ManaPassive : Passive
	{
		public ManaPassive() : base()
		{
			Name = "Open Mind";
			Tooltip = "Increases your maximum mana by 20 per level";
			MaxLevel = 3;
			TreePos = new Vector2(500, 180);
		}
	}
}
