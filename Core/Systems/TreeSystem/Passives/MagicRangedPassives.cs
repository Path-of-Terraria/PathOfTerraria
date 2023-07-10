namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class LongRangePassive : Passive
	{
		public LongRangePassive() : base()
		{
			name = "Sniper";
			tooltip = "Increases your damage against distant enemies by 10% per level";
			maxLevel = 3;
			treePos = new Vector2(400, 180);
		}
	}
}
