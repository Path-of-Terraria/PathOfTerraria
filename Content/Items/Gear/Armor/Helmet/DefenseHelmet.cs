using PathOfTerraria.Common.Systems.ItemStats;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

internal abstract class DefenseHelmet : Helmet, IDefenseRangeItem
{
	protected abstract int MinimumDropItemLevel { get; }
	protected abstract int MaximumDropItemLevel { get; }
	protected abstract int MinimumDefense { get; }
	protected abstract int MaximumDefense { get; }

	public (int Minimum, int Maximum) DefenseRange => (MinimumDefense, MaximumDefense);

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Helmet/{GetType().Name}";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		this.GetStaticData().SetDropItemLevelRange(MinimumDropItemLevel, MaximumDropItemLevel);
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
