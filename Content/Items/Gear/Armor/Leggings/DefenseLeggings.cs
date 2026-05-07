using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

internal abstract class DefenseLeggings : Leggings
{
	protected abstract int MinimumDropItemLevel { get; }
	protected abstract int MaximumDropItemLevel { get; }
	protected abstract int MinimumDefense { get; }
	protected abstract int MaximumDefense { get; }

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Legs/{GetType().Name}";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		this.GetStaticData().SetDropItemLevelRange(MinimumDropItemLevel, MaximumDropItemLevel);
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public override void PostRoll()
	{
		Item.defense = CalculateDefense();
	}

	private int CalculateDefense()
	{
		int itemLevel = GetItemLevel.Invoke(Item);
		float progress = MathHelper.Clamp((itemLevel - MinimumDropItemLevel) / (float)Math.Max(1, MaximumDropItemLevel - MinimumDropItemLevel), 0f, 1f);
		return (int)MathF.Round(MathHelper.Lerp(MinimumDefense, MaximumDefense, progress));
	}
}
