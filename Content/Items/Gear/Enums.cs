namespace PathOfTerraria.Content.Items.Gear
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

	public enum GearRarity : int
	{
		Normal,
		Magic,
		Rare,
		Unique
	}

	public enum GearInfluence : int
	{
		None,
		Solar,
		Lunar
	}
}
