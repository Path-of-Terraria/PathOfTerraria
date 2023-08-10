using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace FunnyExperience.Content.Passives
{
	internal class LifePassive : Passive
	{
		public LifePassive() : base()
		{
			Name = "Empowered Flesh";
			Tooltip = "Increases your maximum life by 20 per level";
			MaxLevel = 5;
			TreePos = new Vector2(400, 400);
		}

		public override void BuffPlayer(Player player)
		{
			player.statLifeMax2 += 20 * Level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<MeleePassive>(all, player);
			Connect<RangedPassive>(all, player);
			Connect<MagicPassive>(all, player);
			Connect<SummonPassive>(all, player);
		}
	}
}
