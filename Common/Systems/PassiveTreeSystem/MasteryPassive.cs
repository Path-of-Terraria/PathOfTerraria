using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Systems.PassiveTreeSystem;



public abstract class MasteryPassive : Passive
{
	public List<Passive> ChoicePassives { get; private set; } = new List<Passive>();

	
	//LoadPassives
	public override void OnLoad()
	{
		ChoicePassives = new List<Passive>();
	}



	public override void BuffPlayer(Player player)
	{

	}

	public void AllocateChoice(int choiceIndex, Player player)
	{
		if (choiceIndex < 0 || choiceIndex >= ChoicePassives.Count)
			return;

		// Deallocate all choices first
		foreach (var passive in ChoicePassives)
			passive.Level = 0;

		// Allocate chosen passive
		ChoicePassives[choiceIndex].Level = 1;

	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		base.Draw(spriteBatch, center); // Draw the big mastery node

	}
}