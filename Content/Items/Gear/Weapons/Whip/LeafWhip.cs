namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class LeafWhip : Whip
{
	public override WhipDrawData DrawData => new(new(14, 26), new(0, 26, 14, 16), new(0, 42, 14, 16), new(0, 58, 14, 16), new(0, 74, 14, 18), false);

	public override WhipSettings WhipSettings => new()
	{
		Segments = 24,
		RangeMultiplier = 1,
	};
}