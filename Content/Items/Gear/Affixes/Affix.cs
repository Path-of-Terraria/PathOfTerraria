namespace FunnyExperience.Content.Items.Gear.Affixes
{
	internal class Affix
	{
		public float minValue;
		public float maxValue;
		public float value;

		public string tooltip;

		public GearInfluence requiredInfluence;

		public GearType possibleTypes;

		public virtual void BuffPassive(Player player) { }
	}
}
