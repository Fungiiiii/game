using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Scene-owned entry point so the dedicated prototype can be opened and played directly.
    /// </summary>
    internal sealed class VitalsHudPrototypeSceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            var model = new PlayerVitals(100f, 100f);
            var hud = gameObject.AddComponent<VitalsHud>();
            var controls = gameObject.AddComponent<VitalsHudPrototypeControls>();

            hud.Initialize(model);
            controls.Initialize(model);
        }
    }
}
