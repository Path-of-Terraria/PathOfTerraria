using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Skills.Ranged.RainOfArrowsVFX;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Utilities;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Swamp;

internal class LogwoodCrossbow : Gear
{
	internal class LogwoodProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private readonly int[] _hitNpcs = [-1, -1, -1, -1, -1];

		private bool _logWood = false;
		private int _hits = 0;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.arrow;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo { Item: Item item })
			{
				_logWood = true;
			}
		}

		public override bool? CanHitNPC(Projectile projectile, NPC target)
		{
			for (int i = 0; i < _hitNpcs.Length; ++i)
			{
				if (_hitNpcs[i] == target.whoAmI)
				{
					return false;
				}
			}

			return null;
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.HasBuff<PoisonedDebuff>() && _hits < 5)
			{
				_hitNpcs[_hits] = target.whoAmI;
				_hits++;
				
				if (projectile.penetrate >= 0)
				{
					projectile.penetrate++;
				}

				PriorityQueue<int, float> options = new();

				foreach (NPC nearby in Main.ActiveNPCs)
				{
					bool skip = false;

					for (int i = 0; i < _hitNpcs.Length; ++i)
					{
						if (_hitNpcs[i] == nearby.whoAmI)
						{
							skip = true;
							break;
						}
					}

					if (skip)
					{
						continue;
					}

					if (nearby.CanBeChasedBy() && nearby.DistanceSQ(projectile.Center) < 800 * 800 && Collision.CanHit(projectile, nearby))
					{
						options.Enqueue(nearby.whoAmI, Main.rand.NextFloat());
					}
				}

				if (options.Count > 0)
				{
					NPC npc = Main.npc[options.Dequeue()];
					float len = projectile.velocity.Length();
					projectile.velocity = projectile.DirectionTo(npc.Center) * len;
				}
			}

			if (_logWood)
			{
				PoisonedDebuff.Apply(target, 4 * 60, Main.player[projectile.owner]);
			}
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if (_logWood && !NaturesBarrageBatching.Batching)
			{
				NaturesBarrageBatching.AddCache(NaturesBarrageBatching.ArrowRainShaderType.Barrage, projectile.whoAmI);
				return false;
			}

			return true;
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bitWriter.WriteBit(_logWood);
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			_logWood = bitReader.ReadBit();
		}
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.CloneDefaults(ItemID.WoodenBow);
		Item.width = 42;
		Item.height = 22;
		Item.useTime = 22;
		Item.useAnimation = 22;
		Item.shootSpeed = 12;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.channel = false;
		Item.damage = 185;
		Item.crit = 6;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Bow;
	}

	public override bool? UseItem(Player player)
	{

		return null;
	}
}
