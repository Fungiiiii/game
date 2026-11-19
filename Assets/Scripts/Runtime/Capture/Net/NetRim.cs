using System.Collections.Generic;
using UnityEngine;

namespace Fungiiiii.Capture
{
    /// <summary>
    /// The points of a net, as Transforms: one centre anchor and a ring of rim anchors,
    /// laid out by <see cref="NetRimLayout"/>, flat in the air or draped on the ground.
    ///
    /// The lines drawn between them are a placeholder. A net model replaces them by
    /// binding to <see cref="Centre"/> and <see cref="RimAnchors"/>, for instance one
    /// bone per anchor, without touching how the net moves.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetRim : MonoBehaviour
    {
        private Transform centre;
        private Transform[] rimAnchors;
        private Vector3[] rimPoints;
        private LineRenderer rimLine;
        private LineRenderer spokeLine;

        public Transform Centre => centre;

        public IReadOnlyList<Transform> RimAnchors => rimAnchors;

        /// <summary>
        /// Creates the anchors and the placeholder lines. Called once, right after
        /// AddComponent. Spokes link the centre to each rim point, like the cords of a net.
        /// </summary>
        public void Build(int rimPointCount, Material lineMaterial, float lineWidth, bool withSpokes)
        {
            rimPoints = new Vector3[rimPointCount];
            rimAnchors = new Transform[rimPointCount];

            centre = new GameObject("Centre").transform;
            centre.SetParent(transform, false);

            for (var index = 0; index < rimPointCount; index++)
            {
                rimAnchors[index] = new GameObject($"Rim {index:00}").transform;
                rimAnchors[index].SetParent(transform, false);
            }

            rimLine = CreateLine("Rim Line", lineMaterial, lineWidth, rimPointCount);
            rimLine.loop = true;

            if (withSpokes)
            {
                spokeLine = CreateLine("Spoke Lines", lineMaterial, lineWidth, rimPointCount * 2);
            }
        }

        /// <summary>Shows the net flat and level, as in the air.</summary>
        public void ShowFlat(Vector3 centrePosition, float radius)
        {
            NetRimLayout.Flat(centrePosition, radius, rimPoints);
            Apply(centrePosition);
        }

        /// <summary>Drapes the net on whatever lies below each of its points.</summary>
        public void ShowDraped(Vector3 centrePosition, float radius, NetGroundProbe probe, float probeHeight, float lift)
        {
            var drapedCentre = NetRimLayout.Drape(centrePosition, radius, rimPoints, probe, probeHeight, lift);
            Apply(drapedCentre);
        }

        private void Apply(Vector3 centrePosition)
        {
            centre.position = centrePosition;

            for (var index = 0; index < rimPoints.Length; index++)
            {
                rimAnchors[index].position = rimPoints[index];
                rimLine.SetPosition(index, rimPoints[index]);

                if (spokeLine != null)
                {
                    spokeLine.SetPosition(index * 2, centrePosition);
                    spokeLine.SetPosition(index * 2 + 1, rimPoints[index]);
                }
            }
        }

        private LineRenderer CreateLine(string objectName, Material material, float width, int positionCount)
        {
            var lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(transform, false);

            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = positionCount;
            line.startWidth = width;
            line.endWidth = width;
            line.sharedMaterial = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return line;
        }
    }
}
