using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives.Summon.Masteries;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class WhipReachPlayer : ModPlayer
{
	public sealed class WhipReachProjectile : GlobalProjectile
	{
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bool sendRange = ProjectileID.Sets.IsAWhip[projectile.type];
			bitWriter.WriteBit(sendRange);

			if (sendRange)
			{
				binaryWriter.Write(projectile.WhipSettings.RangeMultiplier);
			}
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			bool receiveRange = bitReader.ReadBit();

			if (receiveRange)
			{
				projectile.WhipSettings.RangeMultiplier = binaryReader.ReadSingle();
			}
		}
	}

	public StatModifier WhipReach = StatModifier.Default;

	public override void ResetEffects()
	{
		WhipReach = StatModifier.Default;
	}

	public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (ProjectileID.Sets.IsAWhip[type] && Player.GetModPlayer<PassiveTreePlayer>().GetCumulativeLevel(nameof(WhipReachMastery)) != 0)
		{
			Projectile.NewProjectileDirect(null, position, velocity, type, damage, knockback, Player.whoAmI).WhipSettings.RangeMultiplier *= WhipReach.ApplyTo(1);
			return false;
		}

		return true;
	}
}