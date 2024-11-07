using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

/// <summary>
/// Defines a real time generation step.<br/>
/// If this returns true, the step did something and should reset the associated <see cref="RealtimeGenerationAction"/>'s placement timer.
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
/// <returns></returns>
public delegate bool RealtimeAction(int x, int y);

public readonly record struct RealtimeStep(RealtimeAction Action, Point16 Position);

internal class RealtimeGenerationAction(IEnumerable<RealtimeStep> actions, float placeSpeedInSeconds)
{
	public bool Done => Actions.Count == 0;

	public readonly Queue<RealtimeStep> Actions = new(actions);
	public readonly float PlaceSpeedInSeconds = placeSpeedInSeconds;

	private float _timer = 0;

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
