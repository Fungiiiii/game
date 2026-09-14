using NUnit.Framework;
using UnityEditor;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>
    /// Guards the two editor settings every serialization rule in this project
    /// depends on. In Force Binary or Mixed, scenes and prefabs are opaque blobs:
    /// no diff, no merge, no review. Hidden .meta files break asset GUIDs, which
    /// are contracts. Both are set once at project creation and are easy to flip
    /// by accident afterwards - see docs/unity-init.md.
    /// </summary>
    public sealed class ProjectSerializationSettingsTests
    {
        [Test]
        public void AssetSerializationMode_IsForceText()
        {
            Assert.That(
                EditorSettings.serializationMode,
                Is.EqualTo(SerializationMode.ForceText),
                "Asset Serialization must stay Force Text: Unity YAML has to be diffable and mergeable.");
        }

        [Test]
        public void VersionControlMode_IsVisibleMetaFiles()
        {
            Assert.That(
                VersionControlSettings.mode,
                Is.EqualTo("Visible Meta Files"),
                ".meta files must be visible and committed: asset GUIDs are contracts.");
        }
    }
}
