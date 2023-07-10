namespace FunnyExperience.Core.Systems.TreeSystem.Passives
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
