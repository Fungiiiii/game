using UnityEngine;

namespace Fungiiiii.Runtime.World
{
    /// <summary>
    /// Shapes and materials made at runtime from Unity's primitives: the net, the state
    /// discs under the wild mushrooms, the picture of a dropped item. Simple on purpose
    /// (Charte IA: no art made for the game here).
    /// </summary>
    internal static class Primitives
    {
        public static GameObject Create(
            PrimitiveType type,
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool keepCollider)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = objectName;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;

            if (!keepCollider)
            {
                Object.Destroy(primitive.GetComponent<Collider>());
            }

            return primitive;
        }

        public static Material CreateMaterial(string materialName, Color color)
        {
            // Null coalescing is not usable here: Unity overloads ==, but not ??, so a
            // destroyed object would slip through instead of falling back (UNT0007).
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader)
            {
                name = materialName,
                color = color
            };
        }

        /// <summary>Unlit-looking colour for markers that must read the same in shadow and in light.</summary>
        public static Material CreateUnlitMaterial(string materialName, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                return CreateMaterial(materialName, color);
            }

            var material = new Material(shader) { name = materialName };
            material.SetColor("_BaseColor", color);
            return material;
        }
    }
}
