using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.Time;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

/// <summary> The player applies reduced flames resistance to nearby enemies. </summary>
internal class NearbyEnemiesScorchedAffix : ItemAffix
{
	internal sealed class NearbyEnemiesScorchedPlayer : ModPlayer
	{
		public bool Active;

		public override void ResetEffects()
		{
			Active = false;
		}

		public override void PostUpdateBuffs()
		{
			const double timeInSeconds = 1.0;

			if (!Active)
			{
				return;
			}

			Vector2 playerCenter = Player.Center;
			Rectangle playerRect = Player.getRect();
			int debuffType = ModContent.BuffType<ScorchingFlamesDebuff>();

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.friendly || npc.dontTakeDamage) { continue; }
				if (npc.buffImmune[debuffType]) { continue; }
				if (!Player.CanNPCBeHitByPlayerOrPlayerProjectile(npc)) { continue; }

				Vector2 npcPoint = npc.getRect().ClosestPointInRect(playerCenter);
				Vector2 playerPoint = playerRect.ClosestPointInRect(npc.Center);
				if (!npcPoint.WithinRange(playerPoint, PoTMod.NearbyDistance)) { continue; }

				npc.AddBuff(debuffType, (int)(timeInSeconds * TimeSystem.LogicFramerate), quiet: true);
			}
		}
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<NearbyEnemiesScorchedPlayer>().Active = true;
	}
}
