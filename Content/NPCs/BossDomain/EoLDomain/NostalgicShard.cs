using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

//[AutoloadBanner]
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
		NPC.defense = 50;
		NPC.lifeMax = 1500;
		NPC.noGravity = true;
		NPC.knockBackResist = 0f;
		NPC.noTileCollide = true;
		NPC.dontTakeDamage = true;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.BubbleBurst_White, 4));
			c.AddDust(new(DustID.BubbleBurst_White, 16, NPCHitEffects.OnDeath));

			//for (int i = 0; i < 3; ++i)
			//{
			//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
			//}
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
				if (player.DistanceSQ(NPC.Center) < 700 * 700 && player.GetModPlayer<CrystalVisors.VisorPlayer>().Active)
				{
					NPC.target = player.whoAmI;
					NPC.dontTakeDamage = false;
					NPC.velocity.Y = -6;

					State = 1;
					Timer = 0;
					break;
				}
			}
		}

		if (!Main.dedServ)
		{
			NPC.ShowNameOnHover = Main.LocalPlayer.GetModPlayer<CrystalVisors.VisorPlayer>().Active;
		}

		if (State == 0)
		{
			NPC.velocity.X *= 0.95f;
			NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, MathF.Sin(Timer * 0.03f) * 0.5f, 0.2f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.018f);
		}
		else if (State == 1)
		{
			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
			NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 14, 0.018f);

			if (Target.dead || !Target.active)
			{
				State = 0;
			}
		}

		Timer++;
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter++;
	}

	public override Color? GetAlpha(Color drawColor)
	{
		return NPC.color == Color.Transparent ? Color.White : NPC.color;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		return Main.LocalPlayer.GetModPlayer<CrystalVisors.VisorPlayer>().Active;
	}
}
