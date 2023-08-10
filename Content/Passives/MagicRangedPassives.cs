using FunnyExperience.Core.Systems.TreeSystem;

namespace FunnyExperience.Content.Passives
{
	internal class LongRangePassive : Passive
	{
		public LongRangePassive() : base()
		{
			Name = "Sniper";
			Tooltip = "Increases your damage against distant enemies by 10% per level";
			MaxLevel = 3;
			TreePos = new Vector2(400, 180);
		}
	}
}
