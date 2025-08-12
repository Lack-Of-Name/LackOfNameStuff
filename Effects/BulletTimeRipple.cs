using Microsoft.Xna.Framework;
using MyMod.Items.Accessories;

namespace MyMod.Effects
{
    // Visual ripple effect class
    public class BulletTimeRipple
    {
        public Vector2 Position;
        public int MaxLifetime;
        public int CurrentLifetime;
        public bool IsActivation;
        public float MaxRadius;

        public BulletTimeRipple(Vector2 position, int lifetime, bool isActivation)
        {
            Position = position;
            MaxLifetime = lifetime;
            CurrentLifetime = lifetime;
            IsActivation = isActivation;
            MaxRadius = ChronosWatch.RippleMaxRadius;
        }

        public void Update()
        {
            CurrentLifetime--;
        }

        public bool IsExpired()
        {
            return CurrentLifetime <= 0;
        }

        public float GetProgress()
        {
            return 1f - (float)CurrentLifetime / MaxLifetime;
        }

        public float GetRadius()
        {
            return MaxRadius * GetProgress();
        }

        public float GetOpacity()
        {
            float progress = GetProgress();
            // Fade in quickly, fade out slowly
            if (progress < 0.3f)
                return progress / 0.3f;
            else
                return 1f - ((progress - 0.3f) / 0.7f);
        }
    }
}