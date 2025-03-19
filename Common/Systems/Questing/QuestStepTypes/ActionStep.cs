
namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Allows you to create a behaviour mid-quest. For example, spawning an item or teleporting the player.<br/>
/// This step does not show on the UI.
/// </summary>
/// <param name="action">What this step does.</param>
internal class ActionStep(Func<Player, QuestStep, bool> action) : QuestStep
{
	private readonly Func<Player, QuestStep, bool> Action = action;

	public override bool NoUI => true;

	public override void DrawQuestStep(Vector2 topLeft, out int uiHeight, StepCompletion currentStep)
	{
		uiHeight = 0; // Draw nothing and don't add a space
	}

	public override bool Track(Player player)
	{
		return Action(player, this); 
	}
}
