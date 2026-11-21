using Fungiiiii.Survival;
using UnityEngine;

namespace Fungiiiii.UI
{
    /// <summary>
    /// Entry point of HUDScene, the prototype of the vital gauges: their vitals, driven
    /// from the keyboard, so the scene can be opened and played directly.
    /// </summary>
    [RequireComponent(typeof(VitalGaugesPrototypeControls))]
    internal sealed class VitalGaugesPrototypeBootstrap : MonoBehaviour
    {
        [SerializeField]
        private VitalGauges gauges;

        private void Awake()
        {
            var model = new PlayerVitals(100f, 100f);
            gauges.Initialize(model);
            GetComponent<VitalGaugesPrototypeControls>().Initialize(model);
        }
    }
}
