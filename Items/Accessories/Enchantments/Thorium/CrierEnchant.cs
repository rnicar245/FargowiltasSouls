using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using Terraria.Localization;
using ThoriumMod.Items.BardItems;

namespace FargowiltasSouls.Items.Accessories.Enchantments.Thorium
{
    public class CrierEnchant : ModItem
    {
        private readonly Mod thorium = ModLoader.GetMod("ThoriumMod");

        public override bool Autoload(ref string name)
        {
            return ModLoader.GetMod("ThoriumMod") != null;
        }
        
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crier Enchantment");
            Tooltip.SetDefault(
@"'Nothing to cry about'
Your symphonic empowerments will last an additional 3 seconds");
            DisplayName.AddTranslation(GameCulture.Chinese, "传迅员魔石");
            Tooltip.AddTranslation(GameCulture.Chinese, 
@"'没什么可说的'
增加10%灵感回复
拥有音符的效果");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.accessory = true;
            ItemID.Sets.ItemNoGravity[item.type] = true;
            item.rare = 1;
            item.value = 40000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (!Fargowiltas.Instance.ThoriumLoaded) return;

            ThoriumPlayer thoriumPlayer = player.GetModPlayer<ThoriumPlayer>();
            thoriumPlayer.bardBuffDuration += 180;
        }

        public override void AddRecipes()
        {
            if (!Fargowiltas.Instance.ThoriumLoaded) return;
            
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ModContent.ItemType<BardCap>());
            recipe.AddIngredient(ModContent.ItemType<BardChest>());
            recipe.AddIngredient(ModContent.ItemType<BardLeggings>());
            recipe.AddRecipeGroup("FargowiltasSouls:AnyBugleHorn");
            recipe.AddIngredient(ModContent.ItemType<WoodenWhistle>());
            recipe.AddIngredient(ModContent.ItemType<Ukulele>());
            recipe.AddIngredient(ModContent.ItemType<DrumMallet>());
            recipe.AddIngredient(ModContent.ItemType<Harmonica>());
            recipe.AddIngredient(ModContent.ItemType<DynastyGuzheng>());
            recipe.AddIngredient(ModContent.ItemType<Piano>());

            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
