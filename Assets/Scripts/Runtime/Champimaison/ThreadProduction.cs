using System;
using Fungiiiii.Inventory;

namespace Fungiiiii.Champimaison
{
    /// <summary>
    /// The mycelium thread an awake Champimaison produces: one every few seconds, kept
    /// until collected, up to a stock. A full stock waits: nothing is produced until
    /// something is collected.
    /// </summary>
    public sealed class ThreadProduction
    {
        private readonly float secondsPerThread;
        private readonly int maxStored;
        private float elapsed;

        public ThreadProduction(float secondsPerThread, int maxStored)
        {
            if (secondsPerThread <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(secondsPerThread), "A thread takes some time to produce.");
            }

            if (maxStored <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStored), "The stock holds at least one thread.");
            }

            this.secondsPerThread = secondsPerThread;
            this.maxStored = maxStored;
        }

        /// <summary>Raised when the stock changes.</summary>
        public event Action Changed;

        public bool IsRunning { get; private set; }

        public int Stored { get; private set; }

        public int MaxStored => maxStored;

        public void Start()
        {
            IsRunning = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || Stored >= maxStored)
            {
                return;
            }

            elapsed += deltaTime;
            var produced = 0;
            while (elapsed >= secondsPerThread && Stored < maxStored)
            {
                elapsed -= secondsPerThread;
                Stored++;
                produced++;
            }

            if (Stored >= maxStored)
            {
                elapsed = 0f;
            }

            if (produced > 0)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>Gives the stock to <paramref name="inventory"/>; what does not fit stays.</summary>
        /// <returns>How many threads were collected.</returns>
        public int Collect(PlayerInventory inventory, ItemDefinition threadItem)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (threadItem == null)
            {
                throw new ArgumentNullException(nameof(threadItem));
            }

            if (Stored == 0)
            {
                return 0;
            }

            var collected = inventory.Collect(threadItem, Stored);
            if (collected > 0)
            {
                Stored -= collected;
                Changed?.Invoke();
            }

            return collected;
        }
    }
}
