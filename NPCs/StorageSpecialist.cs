using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LackOfNameStuff.NPCs
{
    public class StorageSpecialist : ModNPC
    {
        private const string MagicStorageModName = "MagicStorage";
        private const string ShopName = "MagicStorageShop";

        private static readonly Dictionary<int, UnlockStage> ItemUnlockStages = new();
        private static readonly Dictionary<int, int> ItemCustomPrices = new();

        // Individual price overrides for each Magic Storage item sold by this NPC.
        // Edit the Item.buyPrice values here to rebalance the shop.
        private static readonly Dictionary<string, int> PriceOverrides = new()
        {
            ["StorageComponent"] = Item.buyPrice(gold: 10),
            ["StorageConnector"] = Item.buyPrice(gold: 10),
            ["StorageUnitTiny"] = Item.buyPrice(gold: 8),

            ["StorageHeart"] = Item.buyPrice(gold: 25),
            ["StorageUnit"] = Item.buyPrice(gold: 20),
            ["StorageAccess"] = Item.buyPrice(gold: 2, silver: 50),
            ["StorageDeactivator"] = Item.buyPrice(gold: 2),

            ["StorageUnitDemonite"] = Item.buyPrice(gold: 45),
            ["StorageUnitCrimtane"] = Item.buyPrice(gold: 45),
            ["UpgradeDemonite"] = Item.buyPrice(gold: 20, silver: 50),
            ["UpgradeCrimtane"] = Item.buyPrice(gold: 20, silver: 50),

            ["CraftingAccess"] = Item.buyPrice(gold: 15),
            ["RemoteAccess"] = Item.buyPrice(gold: 15),
            ["PortableAccessPreHM"] = Item.buyPrice(gold: 7, silver: 50),
            ["PortableCraftingAccessPreHM"] = Item.buyPrice(gold: 7, silver: 50),
            ["Locator"] = Item.buyPrice(gold: 12),
            ["LocatorDisk"] = Item.buyPrice(gold: 12, silver: 50),

            ["StorageUnitHellstone"] = Item.buyPrice(gold: 65),
            ["UpgradeHellstone"] = Item.buyPrice(gold: 20),
            ["PortableAccessHM"] = Item.buyPrice(gold: 20),
            ["PortableCraftingAccessHM"] = Item.buyPrice(gold: 20),
            ["ShadowDiamond"] = Item.buyPrice(gold: 15),

            ["StorageUnitHallowed"] = Item.buyPrice(gold: 85),
            ["UpgradeHallowed"] = Item.buyPrice(gold: 20),
            ["EnvironmentAccess"] = Item.buyPrice(gold: 15, silver: 50),

            ["StorageUnitBlueChlorophyte"] = Item.buyPrice(gold: 115),
            ["UpgradeBlueChlorophyte"] = Item.buyPrice(gold: 20),

            ["StorageUnitTerra"] = Item.buyPrice(gold: 155),
            ["UpgradeTerra"] = Item.buyPrice(gold: 20),

            ["StorageUnitLuminite"] = Item.buyPrice(gold: 135),
            ["UpgradeLuminite"] = Item.buyPrice(gold: 20),
            ["PortableAccess"] = Item.buyPrice(gold: 25),
            ["PortableCraftingAccess"] = Item.buyPrice(gold: 25),
            ["CreativeStorageUnit"] = Item.buyPrice(gold: 80),
            ["RadiantJewel"] = Item.buyPrice(gold: 85),
            ["RadiantJewelBag"] = Item.buyPrice(gold: 95)
        };

        // Each entry represents a Magic Storage item and the boss stage required before it appears in the shop.
        private static readonly (string ItemName, UnlockStage Stage)[] ShopCatalog =
        {
            ("StorageComponent", UnlockStage.None),
            ("StorageConnector", UnlockStage.None),
            ("StorageUnitTiny", UnlockStage.None),
            ("StorageHeart", UnlockStage.EyeOfCthulhu),
            ("StorageUnit", UnlockStage.EyeOfCthulhu),
            ("StorageAccess", UnlockStage.EyeOfCthulhu),
            ("StorageDeactivator", UnlockStage.EyeOfCthulhu),

            ("StorageUnitDemonite", UnlockStage.EvilBoss),
            ("StorageUnitCrimtane", UnlockStage.EvilBoss),
            ("UpgradeDemonite", UnlockStage.EvilBoss),
            ("UpgradeCrimtane", UnlockStage.EvilBoss),

            ("CraftingAccess", UnlockStage.Skeletron),
            ("RemoteAccess", UnlockStage.Skeletron),
            ("PortableAccessPreHM", UnlockStage.Skeletron),
            ("PortableCraftingAccessPreHM", UnlockStage.Skeletron),
            ("Locator", UnlockStage.Skeletron),
            ("LocatorDisk", UnlockStage.Skeletron),

            ("StorageUnitHellstone", UnlockStage.WallOfFlesh),
            ("UpgradeHellstone", UnlockStage.WallOfFlesh),
            ("PortableAccessHM", UnlockStage.WallOfFlesh),
            ("PortableCraftingAccessHM", UnlockStage.WallOfFlesh),
            ("ShadowDiamond", UnlockStage.WallOfFlesh),

            ("StorageUnitHallowed", UnlockStage.MechBoss),
            ("UpgradeHallowed", UnlockStage.MechBoss),
            ("EnvironmentAccess", UnlockStage.MechBoss),

            ("StorageUnitBlueChlorophyte", UnlockStage.Plantera),
            ("UpgradeBlueChlorophyte", UnlockStage.Plantera),

            ("StorageUnitTerra", UnlockStage.Golem),
            ("UpgradeTerra", UnlockStage.Golem),

            ("StorageUnitLuminite", UnlockStage.MoonLord),
            ("UpgradeLuminite", UnlockStage.MoonLord),
            ("PortableAccess", UnlockStage.MoonLord),
            ("PortableCraftingAccess", UnlockStage.MoonLord),
            ("CreativeStorageUnit", UnlockStage.MoonLord),
            ("RadiantJewel", UnlockStage.MoonLord),
            ("RadiantJewelBag", UnlockStage.MoonLord)
        };

    public override string Texture => "LackOfNameStuff/NPCs/StorageSpecialist";

    public override string HeadTexture => "LackOfNameStuff/NPCs/StorageSpecialist_Head";

        private static bool MagicStorageAvailable => ModLoader.HasMod(MagicStorageModName);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Merchant];

            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.Merchant];
            NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.Merchant];
            NPCID.Sets.DangerDetectRange[Type] = NPCID.Sets.DangerDetectRange[NPCID.Merchant];
            NPCID.Sets.AttackType[Type] = NPCID.Sets.AttackType[NPCID.Merchant];
            NPCID.Sets.AttackTime[Type] = NPCID.Sets.AttackTime[NPCID.Merchant];
            NPCID.Sets.AttackAverageChance[Type] = NPCID.Sets.AttackAverageChance[NPCID.Merchant];
            NPCID.Sets.HatOffsetY[Type] = NPCID.Sets.HatOffsetY[NPCID.Merchant];
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.damage = 10;
            NPC.defense = 18;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Merchant;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return MagicStorageAvailable && NPC.downedBoss1;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string>
            {
                Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Names.0"),
                Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Names.1"),
                Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Names.2")
            };
        }

        public override string GetChat()
        {
            List<string> lines = new()
            {
                Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Chat.Standard1"),
                Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Chat.Standard2")
            };

            if (IsStageUnlocked(UnlockStage.MechBoss))
            {
                lines.Add(Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Chat.PostMech"));
            }

            if (IsStageUnlocked(UnlockStage.MoonLord))
            {
                lines.Add(Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Chat.PostMoonLord"));
            }

            if (lines.Count == 0)
            {
                return base.GetChat();
            }

            return lines[Main.rand.Next(lines.Count)];
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = ShopName;
            }
        }

        public override void AddShops()
        {
            if (!MagicStorageAvailable)
            {
                return;
            }

            ItemUnlockStages.Clear();
            ItemCustomPrices.Clear();

            NPCShop shop = new(Type, ShopName);

            foreach ((string itemName, UnlockStage stage) in ShopCatalog)
            {
                if (!TryGetMagicStorageItemType(itemName, out int itemType))
                {
                    continue;
                }

                int price = ResolvePrice(itemName, itemType);

                ItemUnlockStages[itemType] = stage;
                ItemCustomPrices[itemType] = price;
                shop.Add(itemType);
            }

            if (shop.Entries.Count > 0)
            {
                shop.Register();
            }
        }

        public override void ModifyActiveShop(string shopName, Item[] items)
        {
            if (shopName != ShopName || !MagicStorageAvailable)
            {
                return;
            }

            for (int i = items.Length - 1; i >= 0; i--)
            {
                ref Item item = ref items[i];
                if (item.type <= ItemID.None)
                {
                    continue;
                }

                if (!ItemUnlockStages.TryGetValue(item.type, out UnlockStage stage) || IsStageUnlocked(stage))
                {
                    if (ItemCustomPrices.TryGetValue(item.type, out int customPrice))
                    {
                        item.shopCustomPrice = customPrice;
                    }

                    continue;
                }

                item.TurnToAir();
            }
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 3f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 25;
            randExtraCooldown = 10;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ProjectileID.RubyBolt;
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            gravityCorrection = 0f;
            randomOffset = 1f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.Add(new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.LackOfNameStuff.NPCs.StorageSpecialist.Bestiary")));
        }

        private static bool TryGetMagicStorageItemType(string internalName, out int itemType)
        {
            itemType = 0;

            if (!MagicStorageAvailable)
            {
                return false;
            }

            if (!ModContent.TryFind(MagicStorageModName, internalName, out ModItem modItem))
            {
                return false;
            }

            itemType = modItem.Type;
            return true;
        }

        private static bool IsStageUnlocked(UnlockStage stage)
        {
            return stage switch
            {
                UnlockStage.None => true,
                UnlockStage.EyeOfCthulhu => NPC.downedBoss1,
                UnlockStage.EvilBoss => NPC.downedBoss2,
                UnlockStage.Skeletron => NPC.downedBoss3,
                UnlockStage.WallOfFlesh => Main.hardMode,
                UnlockStage.MechBoss => NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3,
                UnlockStage.Plantera => NPC.downedPlantBoss,
                UnlockStage.Golem => NPC.downedGolemBoss,
                UnlockStage.MoonLord => NPC.downedMoonlord,
                _ => true
            };
        }

        private static int ResolvePrice(string itemName, int itemType)
        {
            if (PriceOverrides.TryGetValue(itemName, out int configuredPrice))
            {
                return configuredPrice;
            }

            // Fall back to the item's default value if no override is specified.
            return ContentSamples.ItemsByType[itemType].value;
        }

        private enum UnlockStage
        {
            None,
            EyeOfCthulhu,
            EvilBoss,
            Skeletron,
            WallOfFlesh,
            MechBoss,
            Plantera,
            Golem,
            MoonLord
        }
    }
}
