namespace FunnyExperience.Core.Systems.TreeSystem.Passives
{
	internal class MeleePassive : Passive
	{
		public MeleePassive() : base()
		{
			maxLevel = 5;

			treePos = new Vector2(200, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Melee) += 0.2f;
		}
	}

	internal class RangedPassive : Passive
	{
		public RangedPassive() : base()
		{
			maxLevel = 5;

			treePos = new Vector2(300, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Ranged) += 0.2f;
		}
	}

	internal class MagicPassive : Passive
	{
		public MagicPassive() : base()
		{
			maxLevel = 5;

			treePos = new Vector2(500, 250);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Magic) += 0.2f;
		}
	}

	internal class SummonPassive : Passive
	{
		public SummonPassive() : base()
		{
			maxLevel = 5;

			treePos = new Vector2(600, 300);
		}

		public override void BuffPlayer(Player player)
		{
			player.GetDamage(DamageClass.Summon) += 0.2f;
		}
	}
}
