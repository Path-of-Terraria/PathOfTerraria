using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Focuses;

internal class SporeLoci : Focus<IncreasedMagicDamageAffix>
{
	protected override float Strength => 6f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 4, false));
		ItemID.Sets.AnimatesAsSoul[Type] = true;
	}
}
