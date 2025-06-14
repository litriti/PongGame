using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.U2D.Animation
{
    internal interface INameHash
    {
        string name { get; set; }
        int hash { get; }
    }

    [Serializable]
    [MovedFrom("UnityEngine.Experimental.U2D.Animation")]
    internal class SpriteCategoryEntry : INameHash, ISpriteLibraryLabel
    {
        [SerializeField]
        string m_Name;
        [SerializeField]
        [HideInInspector]
        int m_Hash;
        [SerializeField]
        Sprite m_Sprite;

        public string name
        {
            get => m_Name;
            set
            {
                m_Name = value;
                m_Hash = SpriteLibraryUtility.GetStringHash(m_Name);
            }
        }

        public int hash => m_Hash;

        public Sprite sprite
        {
            get => m_Sprite;
            set => m_Sprite = value;
        }

        public void UpdateHash()
        {
            m_Hash = SpriteLibraryUtility.GetStringHash(m_Name);
        }
    }

    [Serializable]
    [MovedFrom("UnityEngine.Experimental.U2D.Animation")]
    internal class SpriteLibCategory : INameHash, ISpriteLibraryCategory
    {
        [SerializeField]
        string m_Name;
        [SerializeField]
        int m_Hash;
        [SerializeField]
        List<SpriteCategoryEntry> m_CategoryList;

        public string name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                m_Hash = SpriteLibraryUtility.GetStringHash(m_Name);
            }
        }

        public int hash => m_Hash;

        public List<SpriteCategoryEntry> categoryList
        {
            get => m_CategoryList;
            set => m_CategoryList = value;
        }

        public IEnumerable<ISpriteLibraryLabel> labels => m_CategoryList;

        public void UpdateHash()
        {
            m_Hash = SpriteLibraryUtility.GetStringHash(m_Name);
            foreach (SpriteCategoryEntry s in m_CategoryList)
                s.UpdateHash();
        }

        internal void ValidateLabels(bool log = true)
        {
            SpriteLibraryAsset.RenameDuplicate(m_CategoryList,
                (originalName, newName)
                    =>
                {
                    if (log)
                        Debug.LogWarning(string.Format("Label {0} renamed to {1} due to hash clash", originalName, newName));
                });
        }
    }

    /// <summary>
    /// A custom Asset that stores Sprites grouping.
    /// </summary>
    /// <remarks>
    /// Sprites are grouped under a given category as categories. Each category and label needs to have
    /// a name specified so that it can be queried.
    /// </remarks>
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.animation@latest/index.html?subfolder=/manual/AssetUpgrader.html%23upgrading-sprite-libraries")]
    [MovedFrom("UnityEngine.Experimental.U2D.Animation")]
    [Icon(IconUtility.IconPath + "Animation.SpriteLibrary.png")]
    public class SpriteLibraryAsset : ScriptableObject
    {
        [SerializeField]
        List<SpriteLibCategory> m_Labels = new List<SpriteLibCategory>();
        [SerializeField]
        long m_ModificationHash;
        [SerializeField]
        int m_Version;

        internal static SpriteLibraryAsset CreateAsset(List<SpriteLibCategory> categories, string assetName, long modificationHash)
        {
            SpriteLibraryAsset asset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
            asset.m_Labels = categories;
            asset.ValidateCategories();
            asset.name = assetName;
            asset.UpdateHashes();
            asset.m_ModificationHash = modificationHash;
            asset.version = 1;
            return asset;
        }

        internal List<SpriteLibCategory> categories
        {
            get => m_Labels;
            set
            {
                m_Labels = value;
                ValidateCategories();
            }
        }

        /// <summary>
        /// Hash to quickly check if the library has any changes made to it.
        /// </summary>
        internal long modificationHash
        {
            get => m_ModificationHash;
            set => m_ModificationHash = value;
        }

        /// <summary>
        /// File version number.
        /// </summary>
        internal int version
        {
            set => m_Version = value;
        }

        void OnEnable()
        {
            if (m_Version < 1)
                UpdateToVersionOne();
        }

        void UpdateToVersionOne()
        {
            UpdateHashes();
            m_Version = 1;
        }

        internal Sprite GetSprite(int categoryHash, int labelHash)
        {
            SpriteLibCategory category = m_Labels.FirstOrDefault(x => x.hash == categoryHash);
            if (category != null)
            {
                SpriteCategoryEntry spriteLabel = category.categoryList.FirstOrDefault(x => x.hash == labelHash);
                if (spriteLabel != null)
                {
                    return spriteLabel.sprite;
                }
            }

            return null;
        }

        internal Sprite GetSprite(int categoryHash, int labelHash, out bool validEntry)
        {
            SpriteLibCategory category = null;
            for (int i = 0; i < m_Labels.Count; ++i)
            {
                if (m_Labels[i].hash == categoryHash)
                {
                    category = m_Labels[i];
                    break;
                }
            }

            if (category != null)
            {
                SpriteCategoryEntry spritelabel = null;
                for (int i = 0; i < category.categoryList.Count; ++i)
                {
                    if (category.categoryList[i].hash == labelHash)
                    {
                        spritelabel = category.categoryList[i];
                        break;
                    }
                }

                if (spritelabel != null)
                {
                    validEntry = true;
                    return spritelabel.sprite;
                }
            }

            validEntry = false;
            return null;
        }

        /// <summary>
        /// Returns the Sprite registered in the Asset given the Category and Label value.
        /// </summary>
        /// <param name="category">Category string value.</param>
        /// <param name="label">Label string value.</param>
        /// <returns>An instance of a Sprite, or null if there is no such Category, or Label.</returns>
        public Sprite GetSprite(string category, string label)
        {
            int categoryHash = SpriteLibraryUtility.GetStringHash(category);
            int labelHash = SpriteLibraryUtility.GetStringHash(label);
            return GetSprite(categoryHash, labelHash);
        }

        /// <summary>
        /// Return all the Category names of the Sprite Library Asset that is associated.
        /// </summary>
        /// <returns>A Enumerable string value representing the name.</returns>
        public IEnumerable<string> GetCategoryNames()
        {
            return m_Labels.Select(x => x.name);
        }

        /// <summary>
        /// (Obsolete) Returns the labels' name for the given name.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <returns>A Enumerable string representing labels' name.</returns>
        [Obsolete("GetCategorylabelNames has been deprecated. Please use GetCategoryLabelNames (UnityUpgradable) -> GetCategoryLabelNames(*)")]
        public IEnumerable<string> GetCategorylabelNames(string category)
        {
            return GetCategoryLabelNames(category);
        }

        /// <summary>
        /// Returns the labels' name for the given name.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <returns>A Enumerable string representing labels' name.</returns>
        public IEnumerable<string> GetCategoryLabelNames(string category)
        {
            SpriteLibCategory label = m_Labels.FirstOrDefault(x => x.name == category);
            return label == null ? new string[0] : label.categoryList.Select(x => x.name);
        }

        /// <summary>
        /// Add or replace and existing Sprite into the given Category and Label.
        /// </summary>
        /// <param name="sprite">Sprite to add.</param>
        /// <param name="category">Category to add the Sprite to.</param>
        /// <param name="label">Label of the Category to add the Sprite to. If this parameter is null or an empty string, it will attempt to add a empty category.</param>
        public void AddCategoryLabel(Sprite sprite, string category, string label)
        {
            category = category.Trim();
            label = label?.Trim();
            if (string.IsNullOrEmpty(category))
                throw new ArgumentException("Cannot add empty or null Category string");

            int catHash = SpriteLibraryUtility.GetStringHash(category);
            SpriteCategoryEntry categorylabel = null;
            SpriteLibCategory libCategory = null;
            libCategory = m_Labels.FirstOrDefault(x => x.hash == catHash);

            if (libCategory != null)
            {
                if (string.IsNullOrEmpty(label))
                    throw new ArgumentException("Cannot add empty or null Label string");
                Assert.AreEqual(libCategory.name, category, "Category string  hash clashes with another existing Category. Please use another string");

                int labelHash = SpriteLibraryUtility.GetStringHash(label);
                categorylabel = libCategory.categoryList.FirstOrDefault(y => y.hash == labelHash);
                if (categorylabel != null)
                {
                    Assert.AreEqual(categorylabel.name, label, "Label string hash clashes with another existing label. Please use another string");
                    categorylabel.sprite = sprite;
                }
                else
                {
                    categorylabel = new SpriteCategoryEntry()
                    {
                        name = label,
                        sprite = sprite
                    };
                    libCategory.categoryList.Add(categorylabel);
                }
            }
            else
            {
                SpriteLibCategory slc = new SpriteLibCategory()
                {
                    categoryList = new List<SpriteCategoryEntry>(),
                    name = category
                };
                if (!string.IsNullOrEmpty(label))
                {
                    slc.categoryList.Add(new SpriteCategoryEntry()
                    {
                        name = label,
                        sprite = sprite
                    });
                }

                m_Labels.Add(slc);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Remove a Label from a given Category.
        /// </summary>
        /// <param name="category">Category to remove from.</param>
        /// <param name="label">Label to remove.</param>
        /// <param name="deleteCategory">Indicate to remove the Category if it is empty.</param>
        public void RemoveCategoryLabel(string category, string label, bool deleteCategory)
        {
            int catHash = SpriteLibraryUtility.GetStringHash(category);
            SpriteLibCategory libCategory = null;
            libCategory = m_Labels.FirstOrDefault(x => x.hash == catHash);

            if (libCategory != null)
            {
                int labelHash = SpriteLibraryUtility.GetStringHash(label);
                libCategory.categoryList.RemoveAll(x => x.hash == labelHash);
                if (deleteCategory && libCategory.categoryList.Count == 0)
                    m_Labels.RemoveAll(x => x.hash == libCategory.hash);

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        internal void UpdateHashes()
        {
            foreach (SpriteLibCategory e in m_Labels)
                e.UpdateHash();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        internal void ValidateCategories(bool log = true)
        {
            RenameDuplicate(m_Labels, (originalName, newName)
                =>
            {
                if (log)
                    Debug.LogWarning($"Category {originalName} renamed to {newName} due to hash clash");
            });
            for (int i = 0; i < m_Labels.Count; ++i)
            {
                // Verify categories have no hash clash
                SpriteLibCategory category = m_Labels[i];

                // Verify labels have no clash
                category.ValidateLabels(log);
            }
        }

        internal static void RenameDuplicate(IEnumerable<INameHash> nameHashList, Action<string, string> onRename)
        {
            const int k_IncrementMax = 1000;
            for (int i = 0; i < nameHashList.Count(); ++i)
            {
                // Verify categories have no hash clash
                INameHash category = nameHashList.ElementAt(i);
                IEnumerable<INameHash> categoriesClash = nameHashList.Where(x => (x.hash == category.hash || x.name == category.name) && x != category);
                int increment = 0;
                for (int j = 0; j < categoriesClash.Count(); ++j)
                {
                    INameHash categoryClash = categoriesClash.ElementAt(j);

                    while (increment < k_IncrementMax)
                    {
                        string name = categoryClash.name;
                        name = $"{name}_{increment}";
                        int nameHash = SpriteLibraryUtility.GetStringHash(name);
                        INameHash exist = nameHashList.FirstOrDefault(x => (x.hash == nameHash || x.name == name) && x != categoryClash);
                        if (exist == null)
                        {
                            onRename(categoryClash.name, name);
                            categoryClash.name = name;
                            break;
                        }

                        ++increment;
                    }
                }
            }
        }
    }
}
