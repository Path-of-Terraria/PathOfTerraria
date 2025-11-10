using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class FishingCatchBuffMastery : Passive
{
	internal class FishingCatchBuffPlayer : ModPlayer
	{
		public override void Load()
		{
			IL_Projectile.AI_061_FishingBobber_GiveItemToPlayer += AddOnCatchHook;
		}

		private void AddOnCatchHook(ILContext il)
		{
			ILCursor c = new(il);

			if (!c.TryGotoNext(x => x.MatchNewobj<Item>()))
			{
				return;
			}

			int localIndex = -1;

			if (!c.TryGotoNext(x => x.MatchStloc(out localIndex)))
			{
				return;
			}

			if (localIndex == -1)
			{
				return;
			}

			if (!c.TryGotoNext(x => x.MatchCall(typeof(ItemLoader).GetMethod(nameof(ItemLoader.CaughtFishStack)))))
			{
				return;
			}

			c.Emit(OpCodes.Ldloc_S, (byte)localIndex);
			c.Emit(OpCodes.Ldarg_S, (byte)1);
			c.EmitDelegate(PostCatchFish);
		}

		private static void PostCatchFish(Item item, Player player)
		{
			if (item.rare == ItemRarityID.Gray)
			{
				return;
			}

			if (player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<FishingCatchBuffMastery>() > 0)
			{
				FishingBuff.Apply(player);
			}
		}
	}
}
