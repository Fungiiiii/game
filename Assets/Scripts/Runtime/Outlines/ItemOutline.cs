using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fungiiiii.Outlines
{
    /// <summary>
    /// The outline of one thing in the world: thin while it is merely in view, stronger
    /// while it is aimed at within reach; or hidden until it is shown, for the mushroom
    /// the net is aimed at.
    /// </summary>
    internal sealed class ItemOutline
    {
        private readonly List<MeshRenderer> renderers;
        private readonly Material idle;
        private readonly Material targeted;

        public ItemOutline(List<MeshRenderer> outlineRenderers, Material idleMaterial, Material targetedMaterial)
        {
            renderers = outlineRenderers;
            idle = idleMaterial;
            targeted = targetedMaterial;
        }

        public void SetVisible(bool visible)
        {
            foreach (var outlineRenderer in renderers)
            {
                if (outlineRenderer != null)
                {
                    outlineRenderer.enabled = visible;
                }
            }
        }

        public void SetTargeted(bool isTargeted)
        {
            var material = isTargeted ? targeted : idle;
            foreach (var outlineRenderer in renderers)
            {
                // Destroyed with its model when the scene unloads, possibly before its owner lets go.
                if (outlineRenderer != null)
                {
                    outlineRenderer.sharedMaterial = material;
                }
            }
        }
    }

    /// <summary>
    /// Builds the outlines of the gatherable models. An outline is a copy of the model's
    /// mesh carrying smoothed normals, drawn by the ItemOutline shader on a child of each
    /// of the model's renderers: it follows the model, and is hidden with it.
    /// </summary>
    internal sealed class ItemOutlineFactory : IDisposable
    {
        /// <summary>The shader reads the smoothed normals from TEXCOORD3.</summary>
        private const int SmoothNormalChannel = 3;

        /// <summary>Vertices closer than this are one corner of the model.</summary>
        private const float WeldDistance = 0.0001f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int WidthId = Shader.PropertyToID("_Width");
        private static readonly int FadeStartId = Shader.PropertyToID("_FadeStart");
        private static readonly int FadeEndId = Shader.PropertyToID("_FadeEnd");

        private readonly Dictionary<Mesh, Mesh> outlineMeshes = new Dictionary<Mesh, Mesh>();
        private readonly Material idle;
        private readonly Material targeted;
        private readonly Material chance;

        public ItemOutlineFactory(Shader shader, Settings settings)
        {
            if (shader == null)
            {
                throw new ArgumentNullException(nameof(shader));
            }

            idle = CreateMaterial(shader, "Item Outline", settings.Color, settings.Width, settings);
            targeted = CreateMaterial(shader, "Item Outline (targeted)", settings.TargetedColor, settings.TargetedWidth, settings);
            chance = CreateMaterial(shader, "Item Outline (capture chance)", settings.TargetedColor, settings.TargetedWidth, settings);
        }

        /// <summary>Outlines every mesh of a model. Meshes that cannot be read are skipped, with an error.</summary>
        public ItemOutline Create(GameObject model)
        {
            return new ItemOutline(BuildRenderers(model, idle), idle, targeted);
        }

        /// <summary>
        /// Outlines a mushroom to catch, hidden until shown, in the colour of the capture
        /// chance: one material for all of them, since the net aims at one at a time.
        /// </summary>
        public ItemOutline CreateChanceOutline(GameObject model)
        {
            var outline = new ItemOutline(BuildRenderers(model, chance), chance, chance);
            outline.SetVisible(false);
            return outline;
        }

        public void SetChanceColor(Color color)
        {
            chance.SetColor(ColorId, color);
        }

        public Color ChanceColor => chance.GetColor(ColorId);

        private List<MeshRenderer> BuildRenderers(GameObject model, Material material)
        {
            var renderers = new List<MeshRenderer>();

            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                var outlineMesh = OutlineMeshOf(filter.sharedMesh);
                if (outlineMesh == null)
                {
                    continue;
                }

                var outlineObject = new GameObject("Outline");
                outlineObject.transform.SetParent(filter.transform, false);
                outlineObject.AddComponent<MeshFilter>().sharedMesh = outlineMesh;

                var outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
                outlineRenderer.sharedMaterial = material;
                outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
                outlineRenderer.receiveShadows = false;
                outlineRenderer.lightProbeUsage = LightProbeUsage.Off;
                outlineRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderers.Add(outlineRenderer);
            }

            return renderers;
        }

        public void Dispose()
        {
            foreach (var mesh in outlineMeshes.Values)
            {
                if (mesh != null)
                {
                    UnityEngine.Object.Destroy(mesh);
                }
            }

            outlineMeshes.Clear();
            UnityEngine.Object.Destroy(idle);
            UnityEngine.Object.Destroy(targeted);
            UnityEngine.Object.Destroy(chance);
        }

        private Mesh OutlineMeshOf(Mesh source)
        {
            if (source == null)
            {
                return null;
            }

            if (outlineMeshes.TryGetValue(source, out var existing))
            {
                return existing;
            }

            Mesh outlineMesh = null;
            if (source.isReadable)
            {
                outlineMesh = BuildOutlineMesh(source);
            }
            else
            {
                Debug.LogError(
                    $"Mesh '{source.name}' is not readable: it gets no outline. Enable Read/Write in its model import settings.");
            }

            // Cached even when null, so a mesh that cannot be read is reported once.
            outlineMeshes.Add(source, outlineMesh);
            return outlineMesh;
        }

        /// <summary>
        /// The same geometry, with the normals of every corner averaged: the faces of a low
        /// poly model do not share normals, and a rim pushed along them would open at each edge.
        /// </summary>
        internal static Mesh BuildOutlineMesh(Mesh source)
        {
            var vertices = source.vertices;
            var normals = source.normals;
            var smoothed = new List<Vector3>(vertices.Length);
            var corners = new Dictionary<Vector3Int, Vector3>();

            for (var index = 0; index < vertices.Length; index++)
            {
                var corner = CornerOf(vertices[index]);
                corners.TryGetValue(corner, out var sum);
                corners[corner] = sum + normals[index];
            }

            for (var index = 0; index < vertices.Length; index++)
            {
                var sum = corners[CornerOf(vertices[index])];
                smoothed.Add(sum.sqrMagnitude > 0f ? sum.normalized : normals[index]);
            }

            var outlineMesh = new Mesh
            {
                name = source.name + " (outline)",
                indexFormat = source.indexFormat,
                vertices = vertices,
                triangles = source.triangles,
                bounds = source.bounds,
            };
            outlineMesh.SetUVs(SmoothNormalChannel, smoothed);
            outlineMesh.UploadMeshData(true);
            return outlineMesh;
        }

        private static Vector3Int CornerOf(Vector3 vertex)
        {
            return new Vector3Int(
                Mathf.RoundToInt(vertex.x / WeldDistance),
                Mathf.RoundToInt(vertex.y / WeldDistance),
                Mathf.RoundToInt(vertex.z / WeldDistance));
        }

        private static Material CreateMaterial(Shader shader, string materialName, Color color, float width, Settings settings)
        {
            var material = new Material(shader) { name = materialName };
            material.SetColor(ColorId, color);
            material.SetFloat(WidthId, width);
            material.SetFloat(FadeStartId, settings.FadeStart);
            material.SetFloat(FadeEndId, settings.FadeEnd);
            return material;
        }

        /// <summary>How the outlines look.</summary>
        public readonly struct Settings
        {
            public Settings(Color color, float width, Color targetedColor, float targetedWidth, float fadeStart, float fadeEnd)
            {
                Color = color;
                Width = width;
                TargetedColor = targetedColor;
                TargetedWidth = targetedWidth;
                FadeStart = fadeStart;
                FadeEnd = fadeEnd;
            }

            public Color Color { get; }

            /// <summary>In pixels on a 1080p screen.</summary>
            public float Width { get; }

            public Color TargetedColor { get; }

            public float TargetedWidth { get; }

            /// <summary>Distance up to which the outline is at full strength, in metres.</summary>
            public float FadeStart { get; }

            /// <summary>Distance from which the outline is invisible, in metres.</summary>
            public float FadeEnd { get; }
        }
    }
}
