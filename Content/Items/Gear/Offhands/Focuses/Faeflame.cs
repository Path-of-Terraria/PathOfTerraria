using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Focuses;

internal class Faeflame : Focus<IncreasedMagicDamageAffix>
{
	protected override float Strength => 3f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 4, false));
		ItemID.Sets.AnimatesAsSoul[Type] = true;
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		gravity = 0f;

		Item.velocity *= 0.97f;
	}
}
