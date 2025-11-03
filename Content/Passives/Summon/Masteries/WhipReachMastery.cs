using Humanizer;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class WhipReachMastery : Passive
{
	public sealed class WhipReachMasteryPlayer : ModPlayer
	{
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (ProjectileID.Sets.IsAWhip[type] && Player.GetModPlayer<PassiveTreePlayer>().HasNode<WhipReachMastery>())
			{
				Projectile.NewProjectileDirect(null, position, velocity, type, damage, knockback, Player.whoAmI).WhipSettings.RangeMultiplier *= (1 + RangeIncrease);
				return false;
			}

			return true;
		}
	}

	/// <summary> Used solely to sync changes to <see cref="Projectile.WhipSettings"/> in <see cref="WhipReachMasteryPlayer"/>. </summary>
	public sealed class WhipReachMasteryProjectile : GlobalProjectile
	{
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bool usingMastery = projectile.TryGetOwner(out Player owner) && owner.GetModPlayer<PassiveTreePlayer>().HasNode<WhipReachMastery>();
			bitWriter.WriteBit(usingMastery);

			if (usingMastery)
			{
				binaryWriter.Write(projectile.WhipSettings.RangeMultiplier);
			}
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			bool usingMastery = bitReader.ReadBit();

			if (usingMastery)
			{
				projectile.WhipSettings.RangeMultiplier = binaryReader.ReadSingle();
			}
		}
	}

	public const float RangeIncrease = 0.5f;
	public const float UseSpeedDecrease = 0.25f;

	public override string DisplayTooltip 
		=> Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(RangeIncrease), MathUtils.Percent(UseSpeedDecrease));

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) -= UseSpeedDecrease;
	}
}