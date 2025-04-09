using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Core.Graphics.Camera;
using PathOfTerraria.Core.Graphics.Camera.Modifiers;
using PathOfTerraria.Core.Graphics.Zoom;
using PathOfTerraria.Core.Graphics.Zoom.Modifiers;
using PathOfTerraria.Core.Physics.Verlet;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed partial class SunDevourerNPC : ModNPC
{
	/// <summary>
	///		Gets the <see cref="Player"/> instance that the NPC is targeting. Shorthand for <c>Main.player[NPC.target]</c>.
	/// </summary>
	public Player Player => Main.player[NPC.target];

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		NPCID.Sets.TrailingMode[Type] = 3;
		NPCID.Sets.TrailCacheLength[Type] = 10;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		NPC.noTileCollide = true;
		NPC.lavaImmune = true;
		NPC.noGravity = true;
		NPC.boss = true;
		
		NPC.width = 20;
		NPC.height = 20;

		NPC.lifeMax = 10000;
		NPC.defense = 20;

		NPC.aiStyle = -1;

		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}

	public override void OnSpawn(IEntitySource source)
	{
		base.OnSpawn(source);
		
		Mode = Main.dayTime ? MODE_DAY_TIME : MODE_NIGHT_TIME;

		if (Mode == MODE_NIGHT_TIME)
		{
			return;
		}
		
		Main.Moondialing();
	}

	public override void OnKill()
	{
		base.OnKill();

		if (Mode == MODE_NIGHT_TIME)
		{
			return;
		}
		
		Main.Sundialing();
	}
	
	public override void SendExtraAI(BinaryWriter writer)
	{
		base.SendExtraAI(writer);
		
		writer.Write(Previous);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		base.ReceiveExtraAI(reader);

		Previous = reader.ReadSingle();
	}

	public override void AI()
	{
		base.AI();

		NPC.TargetClosest();

		switch (State)
		{
			
		}
	}

	public override void BossLoot(ref string name, ref int potionType)
	{
		base.BossLoot(ref name, ref potionType);

		potionType = ItemID.GreaterHealingPotion;
	}
	
	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
	{
		scale = 1.5f;
		
		return null;
	}

	private VerletChain chain;

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (chain == null)
		{
			chain = VerletChainBuilder.CreatePinnedRope(NPC.Center, 10, 32f, 0.9f);
		}
		else
		{
			// TODO: Just for testing purposes. Eventually make this follow the NPC's tail position.
			chain.Points[0].Position = Main.MouseWorld;
			
			chain.Update();
			chain.Render(new SunDevourerVerletRenderer());
		}
		
		DrawNPC(in screenPos, in drawColor);
		
		return false;
	}

	private void ApplyFocus(int duration)
	{
		ZoomSystem.AddModifier(new FocusZoomModifier($"{PoTMod.ModName}:{nameof(SunDevourerNPC)}_Zoom", duration));
		
		Main.instance.CameraModifiers.Add(new FocusCameraModifier($"{PoTMod.ModName}:{nameof(SunDevourerNPC)}_Camera", duration, () => NPC.Center + NPC.Size / 2f));
	}
	
	private void DrawNPC(in Vector2 screenPosition, in Color drawColor)
	{
		var texture = TextureAssets.Npc[Type].Value;

		var position = NPC.Center - screenPosition + new Vector2(0f, NPC.gfxOffY + DrawOffsetY);

		var origin = NPC.frame.Size() / 2f;

		var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		
		Main.EntitySpriteDraw(texture, position, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects);
	}
}