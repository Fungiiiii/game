using System.IO;
using System.Linq;
using Fungiiiii.Inventory;
using NUnit.Framework;
using UnityEditor;

namespace Fungiiiii.Tests.EditMode
{
    /// <summary>Since the playtest of 22/11, every item says what it is for, in the UI string table.</summary>
    public sealed class ItemTextsTests
    {
        private const string SharedTablePath = "Assets/Localization/Tables/UI Shared Data.asset";

        [Test]
        public void EveryItem_HasADescription_InTheTable_AndItsSourceTooWhenItHasOne()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ItemCatalog>("Assets/Data/ItemCatalog.asset");
            var table = File.ReadAllLines(SharedTablePath).Select(line => line.Trim()).ToList();
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Items, Is.Not.Empty);

            foreach (var item in catalog.Items)
            {
                Assert.That(item.DescriptionKey, Is.Not.Empty, $"{item.name} has no description.");
                Assert.That(table, Does.Contain($"m_Key: {item.DescriptionKey}"), $"{item.DescriptionKey} is not in the table.");
                if (!string.IsNullOrEmpty(item.SourceKey))
                {
                    Assert.That(table, Does.Contain($"m_Key: {item.SourceKey}"), $"{item.SourceKey} is not in the table.");
                }
            }
        }
    }
}
