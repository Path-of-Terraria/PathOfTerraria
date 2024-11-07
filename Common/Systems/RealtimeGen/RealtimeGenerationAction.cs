using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

/// <summary>
/// Defines a real time generation step.<br/>
/// If this returns true, the step did something and should reset the associated <see cref="RealtimeGenerationAction"/>'s placement timer.<br/>
/// Basically, returning false with this will instantly move to the next queued step.
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <returns></returns>
public delegate bool RealtimeAction(int x, int y);

/// <summary>
/// Contains an instance of a cached realtime step. 
/// This will run at <see cref="Position"/> in the associated <see cref="RealtimeGenerationAction"/>.
/// </summary>
/// <param name="Action">Action to take once this is queued.</param>
/// <param name="Position">The position to run the action on.</param>
public readonly record struct RealtimeStep(RealtimeAction Action, Point16 Position);

/// <summary>
/// Defines a "step" of realtime generation.<br/>
/// This runs <paramref name="actions"/> in order, running an action every <paramref name="placeSpeedInSeconds"/> seconds.<br/>
/// If <see cref="Done"/>, this action has finished running and will be removed.<br/><br/>
/// You may want to use <see cref="PriorityQueue{TElement, TPriority}"/>, <see cref="List{T}.Sort(Comparison{T})"/>,
/// and similar classes and methods to order the steps.
/// </summary>
/// <param name="actions"></param>
/// <param name="placeSpeedInSeconds"></param>
internal class RealtimeGenerationAction(IEnumerable<RealtimeStep> actions, float placeSpeedInSeconds)
{
	/// <summary>
	/// Whether the current action is done and will be removed.
	/// </summary>
	public bool Done => Actions.Count == 0;

	public readonly Queue<RealtimeStep> Actions = new(actions);
	public readonly float PlaceSpeedInSeconds = placeSpeedInSeconds;

	private float _timer = 0;

	/// <summary>
	/// Updates the action, dequeues actions in order and finishes if nothing is left in the queue.
	/// </summary>
	public void Update()
	{
		_timer++;

		while (!Done && _timer > PlaceSpeedInSeconds * 60)
		{
			RealtimeStep action = Actions.Dequeue();

			while (!action.Action.Invoke(action.Position.X, action.Position.Y))
			{
				if (Done)
				{
					break;
				}

				action = Actions.Dequeue();
			}

			_timer -= PlaceSpeedInSeconds * 60;
		}
	}
}
