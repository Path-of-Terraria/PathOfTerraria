using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Common.Items;

/// <summary>
/// Allows an item to be "temporary", which makes it despawn under a condition. Items not marked as temporary are unaffected.
/// </summary>
internal interface ITemporaryItem
{
	internal class TemporaryGlobalItem : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public bool IsTemporary = false;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.ModItem is ITemporaryItem;
		}

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			CheckDespawn(item);
		}

		public override void UpdateInventory(Item item, Player player)
		{
			CheckDespawn(item);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			CheckDespawn(item);
		}

		private static void CheckDespawn(Item item)
		{
			if (item.GetGlobalItem<TemporaryGlobalItem>().IsTemporary && item.ModItem is ITemporaryItem temp && temp.DespawnCondition())
			{
				item.TurnToAir();
				item.active = false; // 1.4.5: Use WorldItem properly
			}
		}

		public override Color? GetAlpha(Item item, Color lightColor)
		{
			return IsTemporary ? Color.Lerp(lightColor, new Color(0.5f, 0.7f, 1f), MathUtils.Sin01(Main.GameUpdateCount * 0.08f)) * 0.5f : null;
		}
	}

	public bool DespawnCondition();
}