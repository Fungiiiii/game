using System;

namespace Fungiiiii.Gathering
{
    /// <summary>
    /// Something that fills up by chance, like a bush that grows berries: while it is
    /// empty, every interval it has a chance to fill. Once taken, it is empty again and
    /// starts drawing anew. The water fern's drops work this way.
    /// </summary>
    public sealed class ChanceRefill
    {
        private readonly float intervalSeconds;
        private readonly float chance;
        private readonly Random random;
        private float untilDraw;

        /// <param name="intervalSeconds">Time between two draws while empty.</param>
        /// <param name="chance">Chance to fill at each draw, from 0 to 1.</param>
        public ChanceRefill(float intervalSeconds, float chance, Random random)
        {
            if (float.IsNaN(intervalSeconds) || intervalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
            }

            if (float.IsNaN(chance) || chance < 0f || chance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(chance));
            }

            this.intervalSeconds = intervalSeconds;
            this.chance = chance;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            untilDraw = intervalSeconds;
        }

        public bool IsFull { get; private set; }

        /// <summary>Counts time down while empty. Returns true on the tick it fills.</summary>
        public bool Tick(float deltaTime)
        {
            if (IsFull || deltaTime <= 0f)
            {
                return false;
            }

            untilDraw -= deltaTime;
            while (untilDraw <= 0f)
            {
                untilDraw += intervalSeconds;
                if (random.NextDouble() < chance)
                {
                    IsFull = true;
                    untilDraw = intervalSeconds;
                    return true;
                }
            }

            return false;
        }

        /// <summary>Takes what it holds. Refused while empty.</summary>
        public bool TryTake()
        {
            if (!IsFull)
            {
                return false;
            }

            IsFull = false;
            untilDraw = intervalSeconds;
            return true;
        }

        /// <summary>Starts over, full with the chance <paramref name="fullChance"/>, for the start of a game.</summary>
        public void Restart(float fullChance)
        {
            if (float.IsNaN(fullChance) || fullChance < 0f || fullChance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(fullChance));
            }

            IsFull = random.NextDouble() < fullChance;
            untilDraw = intervalSeconds;
        }
    }
}
