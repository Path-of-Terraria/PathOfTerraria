namespace FunnyExperience.Content.Items.Gear
{
	[Flags]
	public enum GearType : int
	{
		Sword,
		Spear,
		Bow,
		Gun,
		Staff,
		Tome,
		Helmet,
		Chestplate,
		Leggings,
		Ring,
		Charm
	}

	public enum GearRarity
	{
		Normal,
		Magic,
		Rare,
		Unique
	}

	public enum GearInfluence
	{
		None,
		Solar,
		Lunar
	}
}
