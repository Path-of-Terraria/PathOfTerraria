using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace FunnyExperience.Content.Passives
{
	internal class StartPassive : Passive
	{
		public StartPassive() : base()
		{
			name = "Anchor";
			tooltip = "Your journey starts here";
			level = 1;
			maxLevel = 1;

			width = 58;
			height = 58;
			treePos = new Vector2(400, 500);
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<LifePassive>(all, player);
		}

		public override bool CanDeallocate(Player player)
		{
			return false;
		}
	}
}
