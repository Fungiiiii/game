using System.Collections.Generic;
using Fungiiiii.Inventory;
using Fungiiiii.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Fungiiiii.Tests.EditMode
{
    public sealed class ItemIconTests
    {
        private readonly List<Object> created = new List<Object>();
        private Image image;
        private ItemIcon icon;

        [SetUp]
        public void SetUp()
        {
            var iconObject = new GameObject("Item Icon", typeof(RectTransform), typeof(Image));
            created.Add(iconObject);
            image = iconObject.GetComponent<Image>();
            icon = iconObject.AddComponent<ItemIcon>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created)
            {
                Object.DestroyImmediate(asset);
            }

            created.Clear();
        }

        [Test]
        public void TheDroppedIconMaterial_CutsOutTheTransparentCorners()
        {
            // An asset, so that the build keeps the cut-out variant: a material made by code alone lost it, and the corners showed black.
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/DroppedIcon.mat");

            Assert.That(material, Is.Not.Null);
            Assert.That(material.IsKeywordEnabled("_ALPHATEST_ON"), Is.True);
            Assert.That(material.GetFloat("_AlphaClip"), Is.EqualTo(1f));
            Assert.That(material.renderQueue, Is.EqualTo((int)UnityEngine.Rendering.RenderQueue.AlphaTest));
        }

        [Test]
        public void AnItemWithoutIcon_IsTheSquareOfItsColour()
        {
            var wood = Item("wood", new Color(0.54f, 0.37f, 0.26f));

            icon.Show(wood);

            Assert.That(image.enabled, Is.True);
            Assert.That(image.sprite, Is.Null);
            Assert.That(image.color, Is.EqualTo(wood.Color));
            Assert.That(icon.Item, Is.SameAs(wood));
        }

        [Test]
        public void AnItemWithAnIcon_ShowsIt_Untinted()
        {
            var net = Item("net", Color.yellow);
            var sprite = Sprite();
            SetIcon(net, sprite);

            icon.Show(net);

            Assert.That(image.sprite, Is.SameAs(sprite));
            Assert.That(image.color, Is.EqualTo(Color.white));
            Assert.That(image.preserveAspect, Is.True);
        }

        [Test]
        public void NoItem_ShowsNothing()
        {
            icon.Show(Item("wood", Color.red));

            icon.Show(null);

            Assert.That(image.enabled, Is.False);
            Assert.That(icon.Item, Is.Null);
        }

        private ItemDefinition Item(string id, Color color)
        {
            var item = ItemDefinition.Create(id, color, 1);
            created.Add(item);
            return item;
        }

        private Sprite Sprite()
        {
            var texture = new Texture2D(4, 4);
            created.Add(texture);
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), Vector2.one * 0.5f);
            created.Add(sprite);
            return sprite;
        }

        private static void SetIcon(ItemDefinition item, Sprite sprite)
        {
            var serialized = new SerializedObject(item);
            serialized.FindProperty("icon").objectReferenceValue = sprite;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
