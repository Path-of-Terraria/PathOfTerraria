using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class LunarShard : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
	}

	public override void AddRecipes()
	{
		// Placeholder recipe to assuage obtainment issues
		// Should be removed in the future 
		CreateRecipe()
			.AddIngredient(ItemID.Star, 5)
			.AddCondition(new Condition(this.GetLocalization("InQuest"), () => Common.Systems.Questing.Quest.GetLocalPlayerInstance<EoCQuest>().Active))
			.Register();
	}
}
