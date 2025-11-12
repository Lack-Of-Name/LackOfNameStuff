using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace LackOfNameStuff.Players
{
    public class DefiledMirrorPlayer : ModPlayer
    {
        private Vector2 _lastDeathPosition;
        private bool _hasValidDeathPosition;

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            _lastDeathPosition = Player.Center;
            _hasValidDeathPosition = true;
        }

        public void InvalidateDeathPosition()
        {
            _hasValidDeathPosition = false;
        }

        public bool TryGetLastDeathPosition(out Vector2 position)
        {
            if (_hasValidDeathPosition)
            {
                position = _lastDeathPosition;
                return true;
            }

            position = default;
            return false;
        }

        public override void OnEnterWorld()
        {
            _hasValidDeathPosition = false;
        }
    }
}
