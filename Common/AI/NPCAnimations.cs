using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Core.Time;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace PathOfTerraria.Common.AI;

internal record struct SpriteAnimation()
{
	public required string Id;
	public required int[] Frames;
	public float Speed = 1f;
	public bool Loop = true;

	public readonly bool Is(in SpriteAnimation other)
	{
		return Id == other.Id;
	}
}

/// <summary> Implements advanced animation playback. Make sure to call <see cref="Advance"/>. </summary>
internal sealed class NPCAnimations : NPCComponent
{
	private SpriteAnimation animation;
	private int animationFrame;
	private int? animationFramePrev;
	private bool animationOver;
	private float frameCounter;

	public SpriteFrame BaseFrame { get; set; } = new(1, 1);
	public ref readonly SpriteAnimation Current => ref animation;

	public void Advance()
	{
		frameCounter += animation.Speed * TimeSystem.LogicDeltaTime;
		animationFramePrev = animationFrame;

		while (frameCounter >= 1f)
		{
			(int numFrames, int nextFrame) = (animation.Frames.Length, animationFrame + 1);
			animationFrame = animation.Loop ? (nextFrame % numFrames) : Math.Min(nextFrame, numFrames);
			animationOver |= !animation.Loop && nextFrame >= numFrames;
			frameCounter -= 1f;
		}
	}

	public void Set(in SpriteAnimation animation)
	{
		if (animation.Is(this.animation))
		{
			this.animation.Speed = animation.Speed;
			return;
		}

		this.animation = animation;
		animationFrame = 0;
		animationFramePrev = null;
		frameCounter = 0;
		animationOver = false;
	}

	public override void FindFrame(NPC npc, int frameHeight)
	{
		if (!Enabled || Main.dedServ || animation.Frames == null || TextureAssets.Npc[npc.type] is not { IsLoaded: true, Value: { } texture })
		{
			return;
		}

		int frameIndex = animation.Frames[animationFrame % animation.Frames.Length];
		SpriteFrame frameRect = BaseFrame.With((byte)(frameIndex % BaseFrame.ColumnCount), (byte)(frameIndex / BaseFrame.RowCount));
		npc.frame = frameRect.GetSourceRectangle(texture);
	}
}
