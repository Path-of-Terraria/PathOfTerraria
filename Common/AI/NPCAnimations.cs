using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Core.Time;
using Terraria.DataStructures;
using Terraria.GameContent;

#nullable enable

namespace PathOfTerraria.Common.AI;

internal record struct SpriteAnimation()
{
	public required string Id;
	public required int[] Frames;
	public float Speed = 1f;
	public bool Loop = true;
	public bool UpdateDirection = true;

	public readonly bool Is(in SpriteAnimation other)
	{
		return Id == other.Id;
	}
}

/// <summary> Implements advanced animation playback. Make sure to call <see cref="Advance"/>. </summary>
internal sealed class NPCAnimations : NPCComponent
{
	private SpriteAnimation animation;
	private float frameCounter;

	public bool ManualInvoke { get; set; }
	public bool IgnoreAlpha { get; set; }
	public Vector2 SpriteOffset { get; set; }
	public SpriteFrame BaseFrame { get; set; } = new SpriteFrame(1, 1) with { PaddingX = 0, PaddingY = 0 };
	public bool Completed { get; private set; }
	public int CurrentFrame { get; private set; }
	public int? PreviousFrame { get; private set; }
	public ref readonly SpriteAnimation Current => ref animation;

	public override void PostAI(NPC npc)
	{
		if (Current.UpdateDirection)
		{
			npc.spriteDirection = npc.direction;
		}
	}

	/// <summary> Should be called in FindFrame(). </summary>
	public void Advance()
	{
		frameCounter += animation.Speed * TimeSystem.LogicDeltaTime;
		PreviousFrame = CurrentFrame;

		while (frameCounter >= 1f)
		{
			(int numFrames, int nextFrame) = (animation.Frames.Length, CurrentFrame + 1);
			CurrentFrame = animation.Loop ? (nextFrame % numFrames) : Math.Min(nextFrame, numFrames - 1);
			Completed |= !animation.Loop && nextFrame >= numFrames;
			frameCounter -= 1f;
		}
	}

	public void Set(SpriteAnimation? animation)
	{
		if (animation.HasValue) { Set(animation.Value); }
	}
	public void Set(SpriteAnimation animation)
	{
		if (animation.Is(this.animation))
		{
			this.animation.Speed = animation.Speed;
			return;
		}

		this.animation = animation;
		CurrentFrame = 0;
		PreviousFrame = null;
		frameCounter = 0;
		Completed = false;
	}

	public override void SetDefaults(NPC npc)
	{
		// Recalculate the default frame to avoid showing a full-width source-rect.
		if (!Main.dedServ && Enabled && BaseFrame.RowCount > 0 && TextureAssets.Npc[npc.type] is { IsLoaded: true, Value: { } texture })
		{
			FindFrame(npc, texture.Height / BaseFrame.RowCount);
		}
	}

	public override void FindFrame(NPC npc, int frameHeight)
	{
		if (!Enabled || Main.dedServ || TextureAssets.Npc[npc.type] is not { IsLoaded: true, Value: { } texture })
		{
			return;
		}

		int frameIndex = animation.Frames != null ? animation.Frames[CurrentFrame % animation.Frames.Length] : 0;
		byte x = (byte)(frameIndex % BaseFrame.ColumnCount);
		byte y = (byte)(frameIndex / BaseFrame.ColumnCount);
		SpriteFrame frameRect = BaseFrame.With(columnToUse: x, rowToUse: y);
		npc.frame = frameRect.GetSourceRectangle(texture);
	}

	public override bool PreDraw(NPC npc, SpriteBatch sb, Vector2 screenPos, Color drawColor)
	{
		if (!Enabled || ManualInvoke) { return true; }

		if (npc.IsABestiaryIconDummy)
		{
			FindFrame(npc, 0);
		}

		Render(TextureAssets.Npc[npc.type].Value, npc, screenPos, drawColor);

		return false;
	}

	// Renders the NPC using the proper source rectangle.
	public void Render(Texture2D texture, NPC npc, Vector2 screenPos, Color drawColor, Vector2? origin = null)
	{
		origin ??= (npc.frame.Size() * 0.5f) - (SpriteOffset * new Vector2(npc.spriteDirection, 1f));
		
		Vector2 position = npc.Center + new Vector2(0f, npc.gfxOffY) - screenPos;
		Color alphaColor = Color.White.MultiplyRGBA(new Color(Vector4.One * ((byte.MaxValue - npc.alpha) / (float)byte.MaxValue)));
		if (IgnoreAlpha) { alphaColor = Color.White; }
		Main.EntitySpriteDraw(texture, position, npc.frame, drawColor.MultiplyRGBA(alphaColor), npc.rotation, origin.Value, npc.scale, npc.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);

		string glowmaskPath = $"{npc.ModNPC?.Texture}_Glowmask";
		if (ModContent.HasAsset(glowmaskPath) && ModContent.Request<Texture2D>(glowmaskPath) is { IsLoaded: true, Value: { } glowmask })
		{
			Main.EntitySpriteDraw(glowmask, position, npc.frame, alphaColor, npc.rotation, npc.frame.Size() * 0.5f, npc.scale, npc.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);
		}
	}
}
