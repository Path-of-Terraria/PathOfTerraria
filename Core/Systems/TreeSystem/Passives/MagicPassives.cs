namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class ManaPassive : Passive
	{
		public ManaPassive() : base()
		{
			name = "Open Mind";
			tooltip = "Increases your maximum mana by 20 per level";
			maxLevel = 3;
			treePos = new Vector2(500, 180);
		}
	}
}
