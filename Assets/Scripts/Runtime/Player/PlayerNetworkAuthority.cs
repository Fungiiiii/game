using Unity.Netcode;
using UnityEngine;

namespace Fungiiiii.Runtime.Player
{
    /// <summary>
    /// Hands each networked player to the client that owns it.
    ///
    /// Movement is owner-authoritative: only the owner reads input and runs the motor,
    /// and the NetworkTransform components replicate the result. Every other copy of
    /// the player is driven by replication alone, so its input reader and motor are
    /// switched off; left on, they would read this machine's keyboard and fight the
    /// replicated position.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerInputReader))]
    [DisallowMultipleComponent]
    public sealed class PlayerNetworkAuthority : NetworkBehaviour
    {
        [SerializeField]
        private PlayerMotor motor;

        [SerializeField]
        private PlayerInputReader inputReader;

        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private Renderer visualRenderer;

        [Tooltip("Tint of the player controlled from this machine, to tell it apart while testing.")]
        [SerializeField]
        private Color localPlayerColour = new(0.95f, 0.72f, 0.12f);

        [SerializeField]
        private Color remotePlayerColour = new(0.25f, 0.55f, 0.95f);

        [SerializeField]
        private Vector3 firstSpawnPosition = new(0f, 1.2f, 0f);

        [Tooltip("Metres between the spawn points of successive clients, so players do not overlap.")]
        [SerializeField, Min(0f)]
        private float spawnSpacing = 2f;

        private Material tintedMaterial;

        public override void OnNetworkSpawn()
        {
            var isLocalPlayer = IsOwner;

            motor.enabled = isLocalPlayer;
            inputReader.enabled = isLocalPlayer;
            Tint(isLocalPlayer ? localPlayerColour : remotePlayerColour);

            if (isLocalPlayer)
            {
                PlaceAtSpawnPoint();
            }
        }

        public override void OnDestroy()
        {
            if (tintedMaterial != null)
            {
                Destroy(tintedMaterial);
            }

            base.OnDestroy();
        }

        private void PlaceAtSpawnPoint()
        {
            // A CharacterController keeps its own cached position and would snap back to
            // it on the next Move, so it has to be off while the transform is teleported.
            characterController.enabled = false;
            transform.position = firstSpawnPosition + Vector3.right * (spawnSpacing * OwnerClientId);
            characterController.enabled = true;
        }

        private void Tint(Color colour)
        {
            if (visualRenderer == null)
            {
                return;
            }

            // Renderer.material instantiates a copy per player. Done once on spawn, and the
            // copy is destroyed with the player.
            tintedMaterial = visualRenderer.material;
            tintedMaterial.color = colour;
        }
    }
}
