using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Content.Items.BossDomain;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using SubworldLibrary;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

[AutoloadBanner]
internal class NostalgicShard : ModNPC
{
	private Player Target => Main.player[NPC.target];

	private ref float Timer => ref NPC.ai[0];
	private ref float State => ref NPC.ai[1];

	private bool Init
	{
		get => NPC.ai[2] == 1;
		set => NPC.ai[2] = value ? 1 : 0;
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(42);
		NPC.damage = 80;
		NPC.aiStyle = -1;
		NPC.defense = 80;
		NPC.lifeMax = 800;
		NPC.noGravity = true;
		NPC.knockBackResist = 0f;
		NPC.noTileCollide = true;
		NPC.dontTakeDamage = true;
		NPC.npcSlots = 0;
		NPC.Opacity = 0.2f;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.PinkFairy, 4));
			c.AddDust(new(DustID.PinkFairy, 16, NPCHitEffects.OnDeath));

			for (int i = 0; i < 3; ++i)
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			}
		});
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return target.GetModPlayer<CrystalVisors.VisorPlayer>().Active;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheHallow");
	}

	public override void OnSpawn(IEntitySource source)
	{
		if (source is EntitySource_SpawnNPC && SubworldSystem.Current is not EmpressDomain)
		{
			NPC.position.Y -= 240;
			NPC.netUpdate = true;
		}
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddCommon<PrismShardItem>();
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		return NPC.downedFishron && spawnInfo.SpawnTileType is TileID.HallowedGrass or TileID.HallowedIce or TileID.HallowHardenedSand or TileID.HallowSandstone or TileID.GolfGrassHallowed 
			or TileID.Pearlsand ? 0.3f : 0;
	}

	public override void AI()
	{
		if (!Init)
		{
			Init = true;

			NPC.target = -1;
		}

		if (NPC.target == -1)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(NPC.Center) < 300 * 300 && CanBeSeen(player))
				{
					NPC.target = player.whoAmI;
					NPC.dontTakeDamage = false;
					NPC.velocity.Y = -8;
					NPC.netUpdate = true;

					State = 1;
					Timer = 0;

					for (int i = 0; i < 12; ++i)
					{
						Dust.NewDust(NPC.BottomLeft - new Vector2(0, 4), NPC.width, 4, DustID.PinkFairy, 0, 6);
					}

					break;
				}
			}
		}

		if (!Main.dedServ)
		{
			NPC.ShowNameOnHover = CanBeSeen(Main.LocalPlayer);
		}

		if (State == 0)
		{
			NPC.velocity.X *= 0.95f;
			NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, MathF.Cos(Timer * 0.03f) * 0.5f, 0.2f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.018f);
		}
		else if (State == 1)
		{
			NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1, 0.05f);
			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
			NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 14, 0.024f);

			if (Target.dead || !Target.active)
			{
				State = 0;
				NPC.target = -1;
			}
		}

		Timer++;
	}

	public static bool CanBeSeen(Player player)
	{
		return player.GetModPlayer<CrystalVisors.VisorPlayer>().Active || SubworldSystem.Current is EmpressDomain;
	}

	public override void FindFrame(int frameHeight)
	{
		if (NPC.IsABestiaryIconDummy)
		{
			NPC.Opacity = 1f;
		}
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return (NPC.color == Color.Transparent ? Color.White : NPC.color) * NPC.Opacity;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		return CanBeSeen(Main.LocalPlayer);
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(NPC.dontTakeDamage);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		NPC.dontTakeDamage = reader.ReadBoolean();
	}
}
