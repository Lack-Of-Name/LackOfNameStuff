using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using LackOfNameStuff.Common;
using LackOfNameStuff.Items.Tools;
using LackOfNameStuff.Systems;

namespace LackOfNameStuff.Players
{
    public class WulfrumVeinminerPlayer : ModPlayer
    {
        private delegate void KillTileGetItemDropsDelegate(int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops);

    private const int MaxTilesPerActivation = 180;
    private const int OutOfChargesNotifyCooldown = 120;
    private const int VeinmineTileInterval = 2;

        private static readonly Point[] NeighborOffsets =
        {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1),
            new Point(1, 1),
            new Point(1, -1),
            new Point(-1, 1),
            new Point(-1, -1)
        };

    private bool _veinMining;
    private bool _veinmineKeyHeld;
    private bool _veinmineKeyJustPressed;
    private bool _suppressActivation;
    private int _outOfChargesTimer;
    private int _veinmineTileTimer;
    private readonly Queue<Point> _pendingVeinmineTiles = new();
    private IEntitySource _pendingDropSource;

    private static readonly KillTileGetItemDropsDelegate KillTileGetItemDrops = InitializeKillTileGetItemDrops();

        private static KillTileGetItemDropsDelegate InitializeKillTileGetItemDrops()
        {
            MethodInfo method = typeof(WorldGen).GetMethod("KillTile_GetItemDrops", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                return null;
            }

            try
            {
                return (KillTileGetItemDropsDelegate)Delegate.CreateDelegate(typeof(KillTileGetItemDropsDelegate), method);
            }
            catch
            {
                return null;
            }
        }

        public override void ResetEffects()
        {
            _veinmineKeyHeld = false;
            _veinmineKeyJustPressed = false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            ModKeybind keybind = LackOfNameKeybinds.Veinmine;
            if (keybind == null)
            {
                return;
            }

            bool hasBinding = KeybindHelper.HasBinding(keybind);
            if (!hasBinding)
            {
                return;
            }

            _veinmineKeyHeld = keybind.Current;
            _veinmineKeyJustPressed = keybind.JustPressed;
        }

        public override void PostUpdate()
        {
            if (_outOfChargesTimer > 0)
            {
                _outOfChargesTimer--;
            }

            if (_pendingVeinmineTiles.Count > 0)
            {
                if (_veinmineTileTimer > 0)
                {
                    _veinmineTileTimer--;
                }
                else
                {
                    ProcessNextVeinmineTile();
                    _veinmineTileTimer = VeinmineTileInterval;
                }
            }
            else if (_pendingDropSource != null)
            {
                _pendingDropSource = null;
            }
        }

        public void HandleOreTileMined(int i, int j, int tileType)
        {
            if (_suppressActivation || Player == null || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (!IsOre(tileType))
            {
                return;
            }

            ModKeybind keybind = LackOfNameKeybinds.Veinmine;
            if (keybind == null)
            {
                return;
            }

            if (!KeybindHelper.HasBinding(keybind))
            {
                KeybindHelper.EnsureBound(keybind, Player);
                return;
            }

            bool keyActive = _veinmineKeyHeld || _veinmineKeyJustPressed || keybind.Current || keybind.JustPressed;
            if (!keyActive)
            {
                return;
            }

            if (Player.HeldItem == null || Player.HeldItem.IsAir || Player.HeldItem.pick <= 0)
            {
                return;
            }

            if (!HasAdjacentOre(i, j, tileType))
            {
                return;
            }

            if (!TryConsumeCharge())
            {
                TryNotifyOutOfCharges();
                return;
            }

            _veinMining = true;
            int minedTiles = PerformVeinmine(i, j, tileType);

            if (minedTiles > 0)
            {
                SoundEngine.PlaySound(SoundID.Item23 with { Volume = 0.6f, Pitch = -0.25f }, Player.Center);
            }
            else
            {
                _veinMining = false;
            }
        }

        private bool TryConsumeCharge()
        {
            for (int index = 0; index < Player.inventory.Length; index++)
            {
                Item item = Player.inventory[index];
                if (item == null || item.IsAir || item.type != ModContent.ItemType<WulfrumVeinminer>())
                {
                    continue;
                }

                if (item.ModItem is not WulfrumVeinminer veinminer || !veinminer.HasCharges)
                {
                    continue;
                }

                if (veinminer.TryConsumeCharge(Player))
                {
                    return true;
                }
            }

            return false;
        }

        private void TryNotifyOutOfCharges()
        {
            if (_outOfChargesTimer > 0)
            {
                return;
            }

            _outOfChargesTimer = OutOfChargesNotifyCooldown;
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("You are out of Wulfrum Veinminer charges.", Color.OrangeRed);
            }
        }

        private int PerformVeinmine(int originX, int originY, int oreType)
        {
            Queue<Point> frontier = new Queue<Point>();
            HashSet<Point> visited = new HashSet<Point>();
            List<Point> collected = new List<Point>();

            Point origin = new Point(originX, originY);
            frontier.Enqueue(origin);
            visited.Add(origin);

            while (frontier.Count > 0 && collected.Count < MaxTilesPerActivation)
            {
                Point current = frontier.Dequeue();

                foreach (Point offset in NeighborOffsets)
                {
                    Point candidate = new Point(current.X + offset.X, current.Y + offset.Y);
                    if (!visited.Add(candidate))
                    {
                        continue;
                    }

                    if (!WorldGen.InWorld(candidate.X, candidate.Y, 1))
                    {
                        continue;
                    }

                    Tile tile = Framing.GetTileSafely(candidate.X, candidate.Y);
                    if (!tile.HasTile || tile.TileType != oreType)
                    {
                        continue;
                    }

                    collected.Add(candidate);
                    frontier.Enqueue(candidate);

                    if (collected.Count >= MaxTilesPerActivation)
                    {
                        break;
                    }
                }
            }

            foreach (Point tilePos in collected)
            {
                _pendingVeinmineTiles.Enqueue(tilePos);
            }

            if (collected.Count > 0)
            {
                _pendingDropSource ??= Player.GetSource_Misc("WulfrumVeinminer");
                _veinmineTileTimer = 0;
            }

            return collected.Count;
        }

        private static List<Item> CollectDrops(int i, int j, Tile tile)
        {
            List<Item> drops = new List<Item>();

            ModTile modTile = TileLoader.GetTile(tile.TileType);
            if (modTile != null)
            {
                IEnumerable<Item> modDrops = modTile.GetItemDrops(i, j);
                if (modDrops != null)
                {
                    foreach (Item drop in modDrops)
                    {
                        if (drop != null && !drop.IsAir && drop.stack > 0)
                        {
                            drops.Add(drop.Clone());
                        }
                    }
                }

                return drops;
            }

            if (KillTileGetItemDrops != null)
            {
                KillTileGetItemDrops(i, j, tile, out int dropItem, out int dropStack, out int secondaryItem, out int secondaryStack, false);
                AppendDrop(drops, dropItem, dropStack);
                AppendDrop(drops, secondaryItem, secondaryStack);
            }
            else
            {
                int fallbackType = TileLoader.GetItemDropFromTypeAndStyle(tile.TileType);
                AppendDrop(drops, fallbackType, 1);
            }

            return drops;
        }

        private static void AppendDrop(List<Item> drops, int itemType, int stack)
        {
            if (itemType > ItemID.None && stack > 0)
            {
                Item item = new Item();
                item.SetDefaults(itemType);
                item.stack = stack;
                drops.Add(item);
            }
        }

        private void GiveDropsToPlayer(List<Item> drops, IEntitySource source)
        {
            foreach (Item drop in drops)
            {
                Player.QuickSpawnItem(source, drop.type, drop.stack);
            }
        }

        private void ProcessNextVeinmineTile()
        {
            if (_pendingVeinmineTiles.Count == 0)
            {
                FinalizeVeinmineIfComplete();
                return;
            }

            Point tilePos = _pendingVeinmineTiles.Dequeue();

            if (!WorldGen.InWorld(tilePos.X, tilePos.Y, 1))
            {
                FinalizeVeinmineIfComplete();
                return;
            }

            Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
            if (!tile.HasTile || !IsOre(tile.TileType))
            {
                FinalizeVeinmineIfComplete();
                return;
            }

            List<Item> drops = CollectDrops(tilePos.X, tilePos.Y, tile);
            _suppressActivation = true;
            try
            {
                WorldGen.KillTile(tilePos.X, tilePos.Y, false, false, true);
            }
            finally
            {
                _suppressActivation = false;
            }

            if (Framing.GetTileSafely(tilePos.X, tilePos.Y).HasTile)
            {
                FinalizeVeinmineIfComplete();
                return;
            }

            WorldGen.SquareTileFrame(tilePos.X, tilePos.Y, true);

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, tilePos.X, tilePos.Y, 1);
            }

            IEntitySource source = _pendingDropSource ?? Player.GetSource_Misc("WulfrumVeinminer");
            GiveDropsToPlayer(drops, source);
            SpawnVeinmineDust(tilePos);

            if (_pendingVeinmineTiles.Count == 0)
            {
                _pendingDropSource = null;
                _veinMining = false;
            }
        }

        private void FinalizeVeinmineIfComplete()
        {
            if (_pendingVeinmineTiles.Count == 0)
            {
                _pendingDropSource = null;
                _veinMining = false;
            }
        }

        private static void SpawnVeinmineDust(Point tilePos)
        {
            Vector2 worldPosition = new Vector2(tilePos.X * 16, tilePos.Y * 16);
            for (int i = 0; i < 6; i++)
            {
                int dustIndex = Dust.NewDust(worldPosition, 16, 16, DustID.Electric, 0f, 0f, 150, default, Main.rand.NextFloat(0.8f, 1.2f));
                Dust dust = Main.dust[dustIndex];
                dust.velocity *= 0.5f;
                dust.noGravity = true;
            }
        }

        private static bool IsOre(int tileType)
        {
            if (tileType < 0)
            {
                return false;
            }

            if (tileType < TileID.Sets.Ore.Length && TileID.Sets.Ore[tileType])
            {
                return true;
            }

            return tileType < Main.tileOreFinderPriority.Length && Main.tileOreFinderPriority[tileType] > 0;
        }

        private static bool HasAdjacentOre(int originX, int originY, int oreType)
        {
            foreach (Point offset in NeighborOffsets)
            {
                int checkX = originX + offset.X;
                int checkY = originY + offset.Y;

                if (!WorldGen.InWorld(checkX, checkY, 1))
                {
                    continue;
                }

                Tile tile = Framing.GetTileSafely(checkX, checkY);
                if (tile.HasTile && tile.TileType == oreType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
