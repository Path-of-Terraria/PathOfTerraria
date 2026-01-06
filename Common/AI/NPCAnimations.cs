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
	private float frameCounter;

	public bool Completed { get; private set; }
	public int CurrentFrame { get; private set; }
	public int? PreviousFrame { get; private set; }
	public SpriteFrame BaseFrame { get; set; } = new(1, 1);
	public ref readonly SpriteAnimation Current => ref animation;

	public void Advance()
	{
		frameCounter += animation.Speed * TimeSystem.LogicDeltaTime;
		PreviousFrame = CurrentFrame;

		while (frameCounter >= 1f)
		{
			(int numFrames, int nextFrame) = (animation.Frames.Length, CurrentFrame + 1);
			CurrentFrame = animation.Loop ? (nextFrame % numFrames) : Math.Min(nextFrame, numFrames);
			Completed |= !animation.Loop && nextFrame >= numFrames;
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
		CurrentFrame = 0;
		PreviousFrame = null;
		frameCounter = 0;
		Completed = false;
	}

	public override void FindFrame(NPC npc, int frameHeight)
	{
		if (!Enabled || Main.dedServ || animation.Frames == null || TextureAssets.Npc[npc.type] is not { IsLoaded: true, Value: { } texture })
		{
			return;
		}

		int frameIndex = animation.Frames[CurrentFrame % animation.Frames.Length];
		byte x = (byte)(frameIndex % BaseFrame.ColumnCount);
		byte y = (byte)(frameIndex / BaseFrame.ColumnCount);
		SpriteFrame frameRect = BaseFrame.With(columnToUse: x, rowToUse: y);
		npc.frame = frameRect.GetSourceRectangle(texture);
	}
}
