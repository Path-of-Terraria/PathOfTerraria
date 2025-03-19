using Terraria.Localization;
using Terraria.ModLoader.IO;
using PathOfTerraria.Common.NPCs.QuestMarkers;
using Terraria.UI.Chat;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class QuestStep
{
	public enum StepCompletion
	{
		Locked,
		Current,
		Completed
	}

	public static Color DefaultTextColor = new(43, 28, 17);

	public virtual int LineCount => 1;
	public virtual bool NoUI => false;

	public bool IsDone { get; internal set; }

	/// <summary>
	/// This means the current step will make the marker show as completed on the <see cref="IQuestMarkerNPC"/>.<br/>
	/// Set this on every step that is either completed automatically or requires only talking to the NPC.
	/// </summary>
	public bool CountsAsCompletedOnMarker { get; init; }

	/// <summary>
	/// Called every frame on the player. This should be used to complete steps, check conditions, so on and so on.
	/// </summary>
	/// <param name="player">The player that is using the quest.</param>
	/// <returns>Whether the step should complete or not.</returns>
	public abstract bool Track(Player player);

	/// <summary>
	/// Used to display in the Quest book. Should use a backing <see cref="LocalizedText"/> for proper localization.
	/// </summary>
	/// <returns>The display string.</returns>
	public virtual string DisplayString() { return ""; }

	/// <summary>
	/// Used to display in the Quest book. Make sure to use <see cref="LocalizedText"/>s and not hardcoded strings.<br/>
	/// Additionally, the <see cref="StepColor(StepCompletion)"/> - provides a default color based on the current <paramref name="currentStep"/> -<br/>
	/// <see cref="DrawString(string, Vector2, Color, StepCompletion)"/> - draws a string with default parameters for ease of use -<br/>and <see cref="DefaultTextColor"/> 
	/// may be used to more easily draw the step.
	/// </summary>
	public abstract void DrawQuestStep(Vector2 topLeft, out int uiHeight, StepCompletion currentStep);

	/// <summary>
	/// Draws a string with default Quest Book parameters - namely, faded if completed, 0.7f scale, no transparency with the ItemStack font and Main.spriteBatch.
	/// </summary>
	/// <param name="text">Text to display.</param>
	/// <param name="position">Position to draw at.</param>
	/// <param name="color">Color to draw with. Will be overriden if <paramref name="currentStep"/> is <see cref="StepCompletion.Completed"/>.</param>
	/// <param name="currentStep">Color of the current step. Used to override <paramref name="color"/> if the step is <see cref="StepCompletion.Completed"/>.</param>
	protected static void DrawString(string text, Vector2 position, Color color, StepCompletion currentStep)
	{
		ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;

		if (currentStep == StepCompletion.Locked)
		{
			color = DefaultTextColor * 0.25f;
		}

		ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, text, position, color, Color.Transparent, 0f, Vector2.Zero, new(0.7f), -1, 2);
	}

	/// <summary>
	/// Gets the default color of the given step.<br/>
	/// <c><see cref="StepCompletion.Locked"/></c> => <c>DefaultTextColor * 0.25f</c>,<br/>
	/// <c><see cref="StepCompletion.Current"/></c> => <c>DefaultTextColor</c>,<br/>
	/// <c><see cref="StepCompletion.Completed"/></c> => <c>Color.Green</c><br/>
	/// </summary>
	/// <param name="step"></param>
	/// <returns></returns>
	protected static Color StepColor(StepCompletion step)
	{
		return step switch
		{
			StepCompletion.Locked => DefaultTextColor * 0.25f,
			StepCompletion.Current => DefaultTextColor,
			_ => Color.Green,
		};
	}

	public virtual void Save(TagCompound tag) { }
	public virtual void Load(TagCompound tag) { }
	public virtual void OnKillNPC(Player player, NPC target, NPC.HitInfo hitInfo, int damageDone) { }

	public virtual void OnComplete()
	{
		IsDone = true;
	}

	public override string ToString()
	{
		return DisplayString();
	}
}
