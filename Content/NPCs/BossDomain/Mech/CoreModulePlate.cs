using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

internal class CoreModulePlate : ModNPC
{
	private NPC Parent => Main.npc[ParentWhoAmI];

	private int ParentWhoAmI => (int)NPC.ai[0];
	private ref float PlateNumber => ref NPC.ai[1];

	public override bool IsLoadingEnabled(Mod mod)
	{
		return false;
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
		{
			Hide = true
		};
		NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(86);
		NPC.aiStyle = -1;
		NPC.lifeMax = 8000;
		NPC.defense = 50;
		NPC.damage = 20;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;
		NPC.value = Item.buyPrice(0, 0, 5);
		NPC.hide = true;

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.MinecartSpark, 6, null, static x => x.scale = 10));
			c.AddDust(new(DustID.MinecartSpark, 20, NPCHitEffects.OnDeath, static x => x.scale = 10));

			//c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke1, 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke2, 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke3, 1, NPCHitEffects.OnDeath));
		});
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsOverPlayers.Add(index);
	}

	public override bool? CanFallThroughPlatforms()
	{
		return true;
	}

	public override void AI()
	{
		NPC.Center = Parent.Center + GetOffsetFromParent();
	}

	private Vector2 GetOffsetFromParent()
	{
		const int HorizontalSpacing = 43;
		const int VerticalSpacing = 43;

		return PlateNumber switch
		{
			0 => new Vector2(-HorizontalSpacing, -VerticalSpacing),
			1 => new Vector2(HorizontalSpacing + 2, -VerticalSpacing),
			2 => new Vector2(-HorizontalSpacing, VerticalSpacing + 2),
			_ => new Vector2(HorizontalSpacing + 2, VerticalSpacing + 2)
		};
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Rectangle frame = PlateNumber switch
		{
			0 => new Rectangle(0, 0, 86, 86),
			1 => new Rectangle(88, 0, 86, 86),
			2 => new Rectangle(0, 88, 86, 86),
			_ => new Rectangle(88, 88, 86, 86)
		};

		spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, frame, drawColor, 0f, frame.Size() / 2f, 1f, SpriteEffects.None, 0);

		return false;
	}

	//public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	//{
	//	Vector2 position = NPC.Center - screenPos + new Vector2(0, 6);
	//	spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	//}
}
