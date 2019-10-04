// Decompiled with JetBrains decompiler
// Type: Sitecore.Data.Items.Item
// Assembly: Sitecore.Kernel, Version=11.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F8DE3552-BE39-41F4-8D7E-04A0C08DC796
// Assembly location: C:\inetpub\wwwroot\daltile.sc\bin\Sitecore.Kernel.dll

using Sitecore.Caching;
using Sitecore.Caching.FastItemKeys;
using Sitecore.Caching.Generics;
using Sitecore.Collections;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Data.Archiving;
using Sitecore.Data.Fields;
using Sitecore.Data.LanguageFallback;
using Sitecore.Data.Locking;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Install;
using Sitecore.Links;
using Sitecore.Reflection;
using Sitecore.Security.AccessControl;
using Sitecore.SecurityModel;
using Sitecore.Sites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Sitecore.Data.Items
{
    /// <summary>Represents an item.</summary>
    /// <remarks>
    /// <para>
    /// The item is language-specific and represents a single version.
    /// </para>
    /// <para>
    /// The Item class is a top-level class that provide access to many operations
    ///     within the system.
    /// </para>
    /// <para>
    /// An item is usually obtained directly from a database.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para class="sourcecode">
    /// Item item = Sitecore.Context.Database["/sitecore/content/Home"];
    /// </para>
    /// </example>
    [DebuggerDisplay("{Name,nq} ({Language.ToString(),nq}#{Version.ToString(),nq}@{Database.Name,nq}), id: {ID.ToString(),nq}", Name = "{Name}")]
    [DebuggerTypeProxy(typeof(Item.ItemDebuggerTypeProxy))]
    public class Item : BaseItem, ISecurable
    {
        private static readonly Lazy<ICache<ItemDatabaseSpecificKey>> ItemCloningRelations = new Lazy<ICache<ItemDatabaseSpecificKey>>((Func<ICache<ItemDatabaseSpecificKey>>)(() => CacheManager.GetNamedInstance<ItemDatabaseSpecificKey>(nameof(ItemCloningRelations), Settings.Caching.DefaultDataCacheSize, true)));
        /// <summary>The _item id.</summary>
        private readonly ID _itemID;
        /// <summary>The _lock.</summary>
        private object _lock;
        /// <summary>The _appearance.</summary>
        private ItemAppearance _appearance;
        /// <summary>The _axes.</summary>
        private ItemAxes _axes;
        /// <summary>The _changes.</summary>
        private ItemChanges _changes;
        /// <summary>The _database.</summary>
        [TypeUtil.IgnoreSize]
        private Database _database;
        /// <summary>The _editing.</summary>
        private ItemEditing _editing;
        /// <summary>The _fields.</summary>
        private FieldCollection _fields;
        /// <summary>The _help.</summary>
        private ItemHelp _help;
        /// <summary>The _inner data.</summary>
        private ItemData _innerData;
        /// <summary>The _key.</summary>
        private string _key;
        /// <summary>The _links.</summary>
        private ItemLinks _links;
        /// <summary>The _locking.</summary>
        private ItemLocking _locking;
        /// <summary>The _paths.</summary>
        private ItemPath _paths;
        /// <summary>The _publishing.</summary>
        private ItemPublishing _publishing;
        private Language _originalLanguage;
        /// <summary>The _runtime settings.</summary>
        private ItemRuntimeSettings _runtimeSettings;
        /// <summary>The _security.</summary>
        private ItemSecurity _security;
        /// <summary>The _source.</summary>
        private Reference<ItemUri> _source;
        /// <summary>The _state.</summary>
        private ItemState _state;
        /// <summary>The _statistics.</summary>
        private ItemStatistics _statistics;
        /// <summary>The _unique id.</summary>
        private string _uniqueId;
        /// <summary>The _versions.</summary>
        private ItemVersions _versions;
        /// <summary>The _visualization.</summary>
        private ItemVisualization _visualization;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Data.Items.Item" /> class.
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <param name="data">The data.</param>
        /// <param name="database">The database.</param>
        public Item(ID itemID, ItemData data, Database database)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)itemID, nameof(itemID));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)data, nameof(data));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)database, nameof(database));
            this._itemID = itemID;
            this._innerData = data;
            this._database = database;
        }

        /// <summary>Provides access to the Access framework.</summary>
        /// <value>The access.</value>
        /// <remarks>
        /// The Access framework determine user rights from security and workflow
        /// settings.
        /// </remarks>
        public virtual ItemAccess Access
        {
            get
            {
                return new ItemAccess(this);
            }
        }

        /// <summary>Provides access to appearance information.</summary>
        /// <value>The appearance.</value>
        /// <remarks>Appearance information includes CSS style and an icon.</remarks>
        public virtual ItemAppearance Appearance
        {
            get
            {
                return this._appearance ?? (this._appearance = new ItemAppearance(this));
            }
        }

        /// <summary>
        /// Provides access to functions that retrieve other item relative to this
        /// item.
        /// </summary>
        /// <value>The axes.</value>
        public virtual ItemAxes Axes
        {
            get
            {
                return this._axes ?? (this._axes = new ItemAxes(this));
            }
        }

        /// <summary>Gets the branch from which the item was created.</summary>
        /// <value>The branch.</value>
        public virtual BranchItem Branch
        {
            get
            {
                if (!this.BranchId.IsNull)
                    return (BranchItem)this.Database.GetItem(this.BranchId);
                return (BranchItem)null;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Sitecore.Data.ID">id</see> of the branch that was used to create this item.
        /// </summary>
        /// <value>The branch ID.</value>
        public virtual ID BranchId
        {
            get
            {
                return (ID)this.GetProperty("branchid") ?? this._innerData.Definition.BranchId;
            }
            set
            {
                Sitecore.Diagnostics.Assert.ArgumentNotNull((object)value, nameof(value));
                this.Editing.AssertEditing();
                this.SetProperty("branchid", value, this._innerData.Definition.BranchId);
            }
        }

        /// <summary>
        /// Gets a list of branches that are allowed to be created under this item.
        /// </summary>
        /// <value>The branches.</value>
        public virtual BranchItem[] Branches
        {
            get
            {
                ArrayList arrayList = new ArrayList();
                string str1 = this[FieldIDs.Branches];
                char[] chArray = new char[1] { '|' };
                foreach (string str2 in str1.Split(chArray))
                {
                    if (!string.IsNullOrEmpty(str2))
                    {
                        BranchItem branch = this.Database.Branches[ID.Parse(str2)];
                        if (branch != null)
                            arrayList.Add((object)branch);
                    }
                }
                return (BranchItem[])arrayList.ToArray(typeof(BranchItem));
            }
        }

        /// <summary>Gets a list of child items.</summary>
        /// <value>The children.</value>
        public virtual ChildList Children
        {
            get
            {
                return new ChildList(this);
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Sitecore.Data.ID">id</see> of the branch that was used to create this item.
        /// </summary>
        /// <value>The branch ID.</value>
        public virtual DateTime Created
        {
            get
            {
                return this._innerData.Definition.Created;
            }
        }

        /// <summary>Gets the database.</summary>
        /// <value>The database.</value>
        /// <remarks>
        /// 	<para>Gets the database that item resides in.</para>
        /// 	<para>The Database class is a top-level class that provides access many other
        /// functions within the system.</para>
        /// </remarks>
        public Database Database
        {
            get
            {
                return this._database;
            }
        }

        /// <summary>Gets the language-specific display name.</summary>
        /// <value>The name of the display.</value>
        public virtual string DisplayName
        {
            get
            {
                string str = this[FieldIDs.DisplayName];
                if (str.Length > 0)
                    return str;
                return this.Name;
            }
        }

        /// <summary>Provides access to the publishing framework.</summary>
        /// <value>The editing.</value>
        public virtual ItemEditing Editing
        {
            get
            {
                if (this._editing == null)
                    this._editing = new ItemEditing(this);
                return this._editing;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Sitecore.Data.Items.Item" /> is empty (i.e. has no versions in any language).
        /// </summary>
        /// <value><c>true</c> if empty; otherwise, <c>false</c>.</value>
        public virtual bool Empty
        {
            get
            {
                return this.Database.DataManager.DataSource.GetVersionUris(this.ID).Count == 0;
            }
        }

        /// <summary>Gets the list of fields.</summary>
        /// <value>The fields.</value>
        /// <remarks>
        /// 	<para>The list only contains the fields that have content.</para>
        /// 	<para>To get all fields defined by the template, use the ReadAll method on the
        /// returned list.</para>
        /// </remarks>
        public override FieldCollection Fields
        {
            get
            {
                return this._fields ?? (this._fields = new FieldCollection(this));
            }
        }

        /// <summary>
        /// Gets a value that indicates if the item has child items.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// 	<para>This is an efficient method for determine if the item has child items.</para>
        /// 	<para>Do not use Children.Count as this retrieves all child items.</para>
        /// </remarks>
        public virtual bool HasChildren
        {
            get
            {
                return ItemManager.HasChildren(this);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has clones.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has clones; otherwise, <c>false</c>.
        /// </value>
        public virtual bool HasClones
        {
            get
            {
                return this.GetClones().Any<Item>();
            }
        }

        /// <summary>Provides access to help information.</summary>
        /// <value>The help.</value>
        /// <remarks>
        /// Help information include Short Description (<c>ToolTip</c>) Long Help (<c>Description</c>) and
        /// Help link.
        /// </remarks>
        public virtual ItemHelp Help
        {
            get
            {
                return this._help ?? (this._help = new ItemHelp(this));
            }
        }

        /// <summary>Gets the ID of the item.</summary>
        /// <value>The ID.</value>
        public ID ID
        {
            get
            {
                return this._itemID;
            }
        }

        /// <summary>Gets the inner data.</summary>
        /// <value>The inner data.</value>
        public ItemData InnerData
        {
            get
            {
                return this._innerData;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is clone.
        /// </summary>
        /// <value><c>true</c> if this instance is clone; otherwise, <c>false</c>.</value>
        public virtual bool IsClone
        {
            get
            {
                return this.SourceUri != (ItemUri)null;
            }
        }

        /// <summary>
        /// Gets a value that indicates if the item is a fallback item.
        /// </summary>
        /// <value>
        /// <c>true</c> if this item is a fallback item; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsFallback
        {
            get
            {
                return this._originalLanguage != (Language)null;
            }
        }

        /// <summary>Gets the key of item.</summary>
        /// <value>The key.</value>
        /// <remarks>The key is always %Name% in lowercase.</remarks>
        public string Key
        {
            get
            {
                return this._key ?? (this._key = this.Name.ToLowerInvariant());
            }
        }

        /// <summary>Get the language of the item.</summary>
        /// <value>The language.</value>
        public virtual Language Language
        {
            get
            {
                return this._innerData.Language;
            }
        }

        /// <summary>
        /// Gets a list of languages that the item has content in.
        /// </summary>
        /// <value>The languages.</value>
        public virtual Language[] Languages
        {
            get
            {
                return ItemManager.GetContentLanguages(this).ToArray();
            }
        }

        /// <summary>Provides access to the Links framework.</summary>
        /// <value>The links.</value>
        /// <remarks>
        /// The Links framework contains information about which items links to this item and
        /// which items are references from this item.
        /// </remarks>
        public virtual ItemLinks Links
        {
            get
            {
                return this._links ?? (this._links = new ItemLinks(this));
            }
        }

        /// <summary>Provides to the Locking framework.</summary>
        /// <value>The locking.</value>
        /// <remarks>
        /// The Locking framework ensures, if enabled, that only one user can edit an item at
        /// a time.
        /// </remarks>
        public virtual ItemLocking Locking
        {
            get
            {
                return this._locking ?? (this._locking = new ItemLocking(this));
            }
        }

        /// <summary>Gets the master from which the item was created.</summary>
        /// <value>The master.</value>
        [Obsolete("Deprecated - Use BranchItem instead.")]
        public BranchItem Master
        {
            get
            {
                return this.Branch;
            }
        }

        /// <summary>
        /// Gets a value that indicates if the item has been modified.
        /// </summary>
        /// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
        public virtual bool Modified
        {
            get
            {
                if (this.RuntimeSettings.SaveAll || this.RuntimeSettings.ForceModified)
                    return true;
                ItemChanges changes = this._changes;
                if (changes != null)
                    return !changes.IsEmpty;
                return false;
            }
        }

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        /// <remarks>
        /// 	<para>The name is a short textual description of the item, like a filename in
        /// Windows.</para>
        /// 	<para>It can not be language versioned.</para>
        /// </remarks>
        public virtual string Name
        {
            get
            {
                return (string)this.GetProperty("name") ?? this._innerData.Definition.Name;
            }
            set
            {
                Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(value, nameof(value));
                this.Editing.AssertEditing();
                if (!string.Equals(this.Name, value, StringComparison.InvariantCulture))
                    ItemUtil.AssertItemName(this.Parent, value);
                this.SetProperty("name", value, this._innerData.Definition.Name);
                this._key = (string)null;
            }
        }

        /// <summary>Gets the originator id.</summary>
        /// <value>The originator id.</value>
        public virtual ID OriginatorId
        {
            get
            {
                string id = this[FieldIDs.Originator];
                if (!ID.IsID(id))
                    return ID.Null;
                return ID.Parse(id);
            }
        }

        /// <summary>Gets or sets the original language.</summary>
        /// <value>The original language.</value>
        public virtual Language OriginalLanguage
        {
            get
            {
                Language originalLanguage = this._originalLanguage;
                if ((object)originalLanguage != null)
                    return originalLanguage;
                return this.Language;
            }
            set
            {
                this._originalLanguage = value;
            }
        }

        /// <summary>Gets the parent item.</summary>
        /// <value>The parent.</value>
        /// <remarks>For the root element, null is returned.</remarks>
        /// <example>
        /// Get the parent of the Home item:
        /// <code lang="CS">
        /// Item home = Sitecore.Context.Database["/sitecore/content/Home"];
        /// Item parent = home.Parent;
        /// MainUtil.Out("Parent is: " + parent.Parent.Path);
        /// // Parent is: /sitecore/content
        /// </code>
        /// 	<code lang="CS">
        /// 	</code>
        /// </example>
        public virtual Item Parent
        {
            get
            {
                return ItemManager.GetParent(this);
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Sitecore.Data.ID">ID</see> of the parent item.
        /// </summary>
        /// <value>The parent ID.</value>
        public virtual ID ParentID
        {
            get
            {
                ID id = ItemManager.GetParent(this, SecurityCheck.Disable)?.ID;
                if ((object)id != null)
                    return id;
                return ID.Null;
            }
        }

        /// <summary>Provides access to Path information.</summary>
        /// <value>The paths.</value>
        /// <remarks>Path information describes the items position in relation to other items.</remarks>
        public virtual ItemPath Paths
        {
            get
            {
                return this._paths ?? (this._paths = new ItemPath(this));
            }
        }

        /// <summary>Provides access to the publishing framework.</summary>
        /// <value>The publishing.</value>
        public virtual ItemPublishing Publishing
        {
            get
            {
                return this._publishing ?? (this._publishing = new ItemPublishing(this));
            }
        }

        /// <summary>Provides access to miscellaneous runtime information.</summary>
        /// <value>The runtime settings.</value>
        public virtual ItemRuntimeSettings RuntimeSettings
        {
            get
            {
                return this._runtimeSettings ?? (this._runtimeSettings = new ItemRuntimeSettings(this));
            }
        }

        /// <summary>Gets the access rules defined for the item.</summary>
        /// <value>The access rules.</value>
        public virtual ItemSecurity Security
        {
            get
            {
                ItemSecurity security = this._security;
                if (security != null)
                    return security;
                ItemSecurity itemSecurity = ItemSecurity.FromItem(this);
                this._security = itemSecurity;
                return itemSecurity;
            }
            set
            {
                Sitecore.Diagnostics.Assert.ArgumentNotNull((object)value, nameof(value));
                this._security = value;
            }
        }

        /// <summary>Gets the source.</summary>
        /// <value>The source.</value>
        public virtual Item Source
        {
            get
            {
                ItemUri sourceUri = this.SourceUri;
                if (sourceUri == (ItemUri)null)
                    return (Item)null;
                Database database = Factory.GetDatabase(sourceUri.DatabaseName, false);
                if (database == null)
                    return (Item)null;
                ItemData itemData = database.DataManager.DataSource.GetItemData(sourceUri.ItemID, sourceUri.Language, sourceUri.Version);
                if (itemData != null)
                    return new Item(sourceUri.ItemID, itemData, database);
                return (Item)null;
            }
        }

        /// <summary>Gets the source URI.</summary>
        /// <value>The source URI.</value>
        public virtual ItemUri SourceUri
        {
            get
            {
                if (!Settings.ItemCloning.Enabled)
                    return (ItemUri)null;
                if (this._source == null)
                {
                    ItemUri uriFromSourceItem = this.GetItemUriFromSourceItem();
                    this._source = uriFromSourceItem == (ItemUri)null || uriFromSourceItem.ItemID == ID.Null ? new Reference<ItemUri>((ItemUri)null) : new Reference<ItemUri>(uriFromSourceItem);
                }
                return this._source.Value;
            }
        }

        /// <summary>Gets the state.</summary>
        /// <value>The state.</value>
        public virtual ItemState State
        {
            get
            {
                return this._state ?? (this._state = new ItemState(this));
            }
        }

        /// <summary>Provides access to statistical information.</summary>
        /// <value>The statistics.</value>
        /// <remarks>
        /// Statistical information is creation time, author, last updated time and
        /// more.
        /// </remarks>
        public virtual ItemStatistics Statistics
        {
            get
            {
                return this._statistics ?? (this._statistics = new ItemStatistics(this));
            }
        }

        /// <summary>Gets the sync root.</summary>
        /// <value>The sync root.</value>
        public virtual object SyncRoot
        {
            get
            {
                if (this._lock == null)
                    Interlocked.CompareExchange(ref this._lock, new object(), (object)null);
                return this._lock;
            }
        }

        /// <summary>Gets the template of the item.</summary>
        /// <value>The template.</value>
        public virtual TemplateItem Template
        {
            get
            {
                TemplateItem template = this.RuntimeSettings.TemplateDatabase.Templates[this.TemplateID, this.Language];
                if (template == null && !PackageInstallationContext.IsActive)
                    Sitecore.Diagnostics.Log.SingleError(string.Format("Data template '{0}' not found for item '{1}' in '{2}' database", (object)this.TemplateID, (object)this.Paths.FullPath, (object)this.Database), (object)this);
                return template;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Sitecore.Data.ID">ID</see> of the template.
        /// </summary>
        /// <value>The template ID.</value>
        public virtual ID TemplateID
        {
            get
            {
                return (ID)this.GetProperty("templateid") ?? this._innerData.Definition.TemplateID;
            }
            set
            {
                Sitecore.Diagnostics.Assert.ArgumentNotNull((object)value, nameof(value));
                this.Editing.AssertEditing();
                this.SetProperty("templateid", value, this._innerData.Definition.TemplateID);
                this._fields = (FieldCollection)null;
            }
        }

        /// <summary>check is current item is language</summary>
        /// <value>The IsLanguageItem</value>
        /// <remarks></remarks>
        internal bool IsLanguageItem
        {
            get
            {
                return this.TemplateID == TemplateIDs.Language;
            }
        }

        /// <summary>Gets the name of the template.</summary>
        /// <value>The name of the template.</value>
        public virtual string TemplateName
        {
            get
            {
                Sitecore.Data.Templates.Template template = TemplateManager.GetTemplate(this);
                if (template != null)
                    return template.Name;
                return string.Empty;
            }
        }

        /// <summary>Gets an URI for the item.</summary>
        /// <value>The URI.</value>
        public virtual ItemUri Uri
        {
            get
            {
                return new ItemUri(this.ID, this.Language, this.Version, this.Database);
            }
        }

        /// <summary>Gets the version represented by this item.</summary>
        /// <value>The version.</value>
        public virtual Sitecore.Data.Version Version
        {
            get
            {
                return this._innerData.Version;
            }
        }

        /// <summary>Provides access to different versions of the item.</summary>
        /// <value>The versions.</value>
        public virtual ItemVersions Versions
        {
            get
            {
                return this._versions ?? (this._versions = new ItemVersions(this));
            }
        }

        /// <summary>
        /// Provides access to the visible representations of the item.
        /// </summary>
        /// <value>The visualization.</value>
        /// <remarks>Visible representation is usually the layout of the item.</remarks>
        public virtual ItemVisualization Visualization
        {
            get
            {
                if (this._visualization == null)
                    this._visualization = new ItemVisualization(this);
                return this._visualization;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any version of this item is a clone.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this entire item is a clone; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsItemClone
        {
            get
            {
                return this.SharedFieldsSource != null;
            }
        }

        /// <summary>Gets or sets the shared fields source.</summary>
        /// <value>The shared fields source.</value>
        public virtual Item SharedFieldsSource
        {
            get
            {
                if (!Settings.ItemCloning.Enabled)
                    return (Item)null;
                ItemUri itemUri;
                if (this.TryGetCacheValue(this, out itemUri))
                {
                    if (itemUri == (ItemUri)null)
                        return (Item)null;
                    return Database.GetItem(itemUri);
                }
                Item source = this.Source;
                if (source != null)
                {
                    this.AddToItemCloningRelationsCache(this, source.Uri);
                    return source;
                }
                foreach (Language language in this.Languages)
                {
                    if (!(language == this.Language))
                    {
                        Item obj;
                        using (new CacheWriteDisabler())
                            obj = ItemManager.GetItem(this.ID, language, Sitecore.Data.Version.Latest, this.Database, SecurityCheck.Disable);
                        if (obj != null && obj.Versions.Count != 0)
                        {
                            source = obj.Source;
                            break;
                        }
                    }
                }
                this.AddToItemCloningRelationsCache(this, source == null ? (ItemUri)null : source.Uri);
                return source;
            }
        }

        /// <summary>Creates an item under this item from a master.</summary>
        /// <param name="name">The name.</param>
        /// <param name="branch">The master.</param>
        /// <returns>The add.</returns>
        public virtual Item Add(string name, BranchItem branch)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)branch, nameof(branch));
            return this.Add(name, new Sitecore.Data.BranchId(branch.ID));
        }

        /// <summary>Creates an item under this item from a master ID.</summary>
        /// <param name="name">The name.</param>
        /// <param name="branchId">The master ID.</param>
        /// <returns>The add.</returns>
        public virtual Item Add(string name, Sitecore.Data.BranchId branchId)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            ItemUtil.AssertItemName(this, name);
            return ItemManager.AddFromTemplate(name, branchId.ID, this);
        }

        /// <summary>Creates an item under this item from a template.</summary>
        /// <param name="name">The name.</param>
        /// <param name="template">The template.</param>
        /// <returns>The add.</returns>
        public virtual Item Add(string name, TemplateItem template)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)template, nameof(template));
            return this.Add(name, new Sitecore.Data.TemplateID(template.ID));
        }

        /// <summary>Creates an item under this item from a template.</summary>
        /// <param name="name">The name.</param>
        /// <param name="template">The template.</param>
        /// <param name="newItemId">The new item identifier.</param>
        /// <returns>Added item.</returns>
        public virtual Item Add(string name, TemplateItem template, ID newItemId)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)template, nameof(template));
            return this.Add(name, new Sitecore.Data.TemplateID(template.ID), newItemId);
        }

        /// <summary>Creates an item under this item from a template ID.</summary>
        /// <param name="name">The name.</param>
        /// <param name="templateID">The template ID.</param>
        /// <returns>The add.</returns>
        public virtual Item Add(string name, Sitecore.Data.TemplateID templateID)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty((ID)templateID, nameof(templateID));
            ItemUtil.AssertItemName(this, name);
            return this.Add(name, templateID, ID.NewID);
        }

        /// <summary>
        /// Creates an item with specified ID under this item from a template ID.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="templateID">The template ID.</param>
        /// <param name="newItemID">The new item identifier.</param>
        /// <returns>Added item.</returns>
        public virtual Item Add(string name, Sitecore.Data.TemplateID templateID, ID newItemID)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Error.AssertID((ID)templateID, nameof(templateID), false);
            ItemUtil.AssertItemName(this, name);
            return ItemManager.AddFromTemplate(name, (ID)templateID, this, newItemID);
        }

        /// <summary>Changes the template of the item.</summary>
        /// <param name="template">The template.</param>
        /// <remarks>Field values are transfered by name.</remarks>
        public virtual void ChangeTemplate(TemplateItem template)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)template, nameof(template));
            Sitecore.Data.Templates.Template target = (Sitecore.Data.Templates.Template)null;
            lock (this.SyncRoot)
            {
                Sitecore.Data.Templates.Template template1 = TemplateManager.GetTemplate(this);
                target = TemplateManager.GetTemplate(template.ID, this.Database);
                Sitecore.Diagnostics.Error.AssertNotNull((object)target, "Could not get target in ChangeTemplate");
                TemplateManager.ChangeTemplate(this, template1.GetTemplateChangeList(target));
            }
            if (!Settings.ItemCloning.ChangeTemplateForceUpdate)
                return;
            foreach (Item clone in this.GetClones(false))
            {
                lock (clone.SyncRoot)
                {
                    TemplateChangeList templateChangeList = TemplateManager.GetTemplate(clone).GetTemplateChangeList(target);
                    TemplateManager.ChangeTemplate(clone, templateChangeList);
                }
            }
        }

        /// <summary>Clones the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// </returns>
        public virtual Item Clone(Item item)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            Item obj = this.Clone(item.ID, item.Database);
            obj._uniqueId = item._uniqueId;
            return obj;
        }

        /// <summary>Clones the specified item.</summary>
        /// <param name="cloneID">The clone ID.</param>
        /// <param name="ownerDatabase">
        /// The database that is to own the clone.
        /// </param>
        /// <returns>
        /// </returns>
        public virtual Item Clone(ID cloneID, Database ownerDatabase)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)cloneID, nameof(cloneID));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)ownerDatabase, nameof(ownerDatabase));
            Item owner = new Item(cloneID, this.InnerData, ownerDatabase);
            ItemEditing editing = this._editing;
            if (editing != null)
                owner._editing = editing.Clone(owner);
            ItemChanges changes = this._changes;
            if (changes != null)
                owner._changes = changes.Clone(owner);
            ItemRuntimeSettings runtimeSettings = this._runtimeSettings;
            if (runtimeSettings != null)
                owner._runtimeSettings = runtimeSettings.Clone(owner);
            return owner;
        }

        /// <summary>Clones the item to the given destination.</summary>
        /// <param name="destination">The destination.</param>
        /// <returns>The clone.</returns>
        public virtual Item CloneTo(Item destination)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            return this.CloneTo(destination, true);
        }

        /// <summary>Clones the item to the given destination.</summary>
        /// <param name="destination">The destination.</param>
        /// <param name="deep">if set to <c>true</c> clone descendants too.</param>
        /// <returns>The clone.</returns>
        public virtual Item CloneTo(Item destination, bool deep)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            return this.CloneTo(destination, this.Name, deep);
        }

        /// <summary>Clones the item to the given destination.</summary>
        /// <param name="destination">The destination.</param>
        /// <param name="name">The clone name.</param>
        /// <param name="deep">
        /// if set to <c>true</c> clone descendants too.
        /// </param>
        /// <returns>The created clone.</returns>
        public virtual Item CloneTo(Item destination, string name, bool deep)
        {
            if (!Settings.ItemCloning.Enabled)
                return (Item)null;
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)name, nameof(name));
            Sitecore.Diagnostics.Assert.IsNotNull((object)this.Template, "Cannot clone an item without template");
            Sitecore.Diagnostics.Assert.IsTrue(destination.Access.CanCreate(), "Cannot create new items at destination");
            Item destination1 = destination.Add(name, this.Template);
            Sitecore.Diagnostics.Assert.IsNotNull((object)destination1, "Failed to create clone item");
            TemplateItem template = this.Template;
            List<ID> idList = new List<ID>((IEnumerable<ID>)CloneItem.GetNonInheritedFieldIDs());
            bool flag1 = false;
            string[] nonInheritedKeys = this.GetNonInheritedFieldKeys();
            Func<TemplateFieldItem, bool> func = (Func<TemplateFieldItem, bool>)(tFieldItem =>
            {
                foreach (string str in nonInheritedKeys)
                {
                    ID result;
                    if (ID.TryParse(str, out result))
                    {
                        if (result == tFieldItem.ID)
                            return true;
                    }
                    else if (tFieldItem.Key == str)
                        return true;
                }
                return false;
            });
            using (new SecurityDisabler())
            {
                foreach (Language language in this.Languages)
                {
                    Item obj1 = destination1.Database.GetItem(destination1.ID, language, Sitecore.Data.Version.Latest);
                    if (obj1 != null)
                    {
                        Item obj2 = this.Language == language ? this : this.Database.GetItem(this.ID, language, Sitecore.Data.Version.Latest);
                        if (obj2 == null || obj2.Versions.GetVersionNumbers().Length == 0)
                        {
                            obj1.Versions.RemoveVersion();
                        }
                        else
                        {
                            obj1.Editing.BeginEdit();
                            foreach (TemplateFieldItem field in template.Fields)
                            {
                                bool flag2 = func(field);
                                if (!idList.Contains(field.ID) && !flag2)
                                    obj1.Fields[field.ID].Reset();
                                if (flag2)
                                    obj1.Fields[field.ID].SetValue(obj2[field.ID], true);
                            }
                            obj1.Fields[FieldIDs.Lock].SetValue(string.Empty, true);
                            if (!Settings.ItemCloning.InheritWorkflowData)
                            {
                                obj1.Fields[FieldIDs.Workflow].SetValue(obj2[FieldIDs.Workflow], true);
                                obj1.Fields[FieldIDs.WorkflowState].SetValue(obj2[FieldIDs.WorkflowState], true);
                            }
                            new VersionLinkField(obj1.Fields[FieldIDs.Source]).Uri = obj2.Uri;
                            if (!flag1)
                            {
                                obj1.Fields[FieldIDs.SourceItem].SetValue(this.Uri.ToString(false), true);
                                flag1 = true;
                            }
                            obj1.Editing.EndEdit();
                        }
                    }
                }
                destination1._source = (Reference<ItemUri>)null;
                if (deep)
                {
                    foreach (Item child in this.GetChildren(ChildListOptions.SkipSorting))
                    {
                        if (!destination1.Paths.LongID.StartsWith(child.Paths.LongID, StringComparison.InvariantCulture))
                            child.CloneTo(destination1, child.Name, true);
                    }
                }
            }
            destination1.Reload();
            Item.RemoveItemFromCloningCache(destination1);
            Event.RaiseEvent("item:cloneAdded", (object)destination1);
            return destination1;
        }

        /// <summary>Gets the non inherited field keys.</summary>
        /// <returns>The list of filed keys that should not be inherited when cloning the item.</returns>
        protected virtual string[] GetNonInheritedFieldKeys()
        {
            return CloneItem.GetNonInheritedFieldKeys();
        }

        /// <summary>Copies an item to another location.</summary>
        /// <param name="destination">
        /// The destination item (i.e. the item that will serve as the parent of the
        /// copy).
        /// </param>
        /// <param name="copyName">Name of the copy.</param>
        /// <returns>
        /// The copied item (i.e. the root of the copied subtree).
        /// </returns>
        /// <remarks>
        /// <para>
        /// Copies an item and all of its descendants to another location in the content
        /// tree.
        /// </para>
        /// <para>
        /// All item IDs are changed before the copy is merged into the content
        /// tree.
        /// </para>
        /// </remarks>
        public virtual Item CopyTo(Item destination, string copyName)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(copyName, nameof(copyName));
            ItemUtil.AssertItemName(destination, copyName);
            return this.CopyTo(destination, copyName, ID.NewID, true);
        }

        /// <summary>Copies an item to another location.</summary>
        /// <param name="destination">
        /// The destination item (i.e. the item that will serve as the parent of the
        /// copy).
        /// </param>
        /// <param name="copyName">Name of the copy.</param>
        /// <param name="copyID">The copy ID.</param>
        /// <param name="deep">Indicates if child items are copied.</param>
        /// <returns>
        /// The copied item (i.e. the root of the copied subtree).
        /// </returns>
        /// <remarks>
        /// <para>
        /// Copies an item to another location in the content tree.
        /// </para>
        /// <para>
        /// All item IDs are changed before the copy is merged into the content
        /// tree.
        /// </para>
        /// </remarks>
        public virtual Item CopyTo(Item destination, string copyName, ID copyID, bool deep)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(copyName, nameof(copyName));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)copyID, nameof(copyID));
            Sitecore.Diagnostics.Error.AssertID(copyID, nameof(copyID), false);
            Sitecore.Diagnostics.Error.AssertAccess((this.Access.CanCopyTo(destination) ? 1 : 0) != 0, "Copy item (" + (object)this.ID + " -> " + (object)destination.ID + ")");
            ItemUtil.AssertItemName(destination, copyName);
            return ItemManager.CopyItem(this, destination, deep, copyName, copyID);
        }

        /// <summary>Deletes the item.</summary>
        public virtual void Delete()
        {
            this.Delete(true);
        }

        /// <summary>
        /// Deletes all child items to which the current user has delete access.
        /// </summary>
        /// <returns>The number of child items deleted.</returns>
        public virtual int DeleteChildren()
        {
            ChildList children = this.GetChildren(ChildListOptions.IgnoreSecurity | ChildListOptions.SkipSorting);
            int num = 0;
            for (int index = 0; index < children.Count; ++index)
            {
                Item obj = children[index];
                if (obj.Access.CanDelete())
                {
                    obj.Delete();
                    ++num;
                }
            }
            return num;
        }

        /// <summary>Duplicates the item.</summary>
        /// <returns>The new item.</returns>
        /// <remarks>
        /// Creates a copy of the item and all of its child items under the same parent
        /// item. All new items will have new IDs assigned.
        /// </remarks>
        public virtual Item Duplicate()
        {
            return this.Duplicate(ItemUtil.GetCopyOfName(this.Parent, this.Name));
        }

        /// <summary>Duplicates the item.</summary>
        /// <param name="copyName">Name of the copy.</param>
        /// <returns>The new item.</returns>
        /// <remarks>
        /// Creates a copy of the item and all of its subitems under the same parent
        /// item. All new items will have new IDs assigned
        /// </remarks>
        public virtual Item Duplicate(string copyName)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(copyName, nameof(copyName));
            Sitecore.Diagnostics.Error.AssertString(copyName, nameof(copyName), false);
            Sitecore.Diagnostics.Error.AssertAccess(this.Access.CanDuplicate(), "Duplicate item (" + (object)this.ID + ")");
            Item parent = this.Parent;
            if (parent != null)
                return this.CopyTo(parent, copyName);
            return (Item)null;
        }

        /// <summary>Gets the children.</summary>
        /// <returns>
        /// </returns>
        public virtual ChildList GetChildren()
        {
            return this.GetChildren(ChildListOptions.None);
        }

        /// <summary>Gets the children.</summary>
        /// <param name="options">The options.</param>
        /// <returns>
        /// </returns>
        public virtual ChildList GetChildren(ChildListOptions options)
        {
            return Sitecore.Diagnostics.Assert.ResultNotNull<ChildList>(ItemManager.GetChildren(this, (options & ChildListOptions.IgnoreSecurity) != ChildListOptions.None ? SecurityCheck.Disable : SecurityCheck.Enable, options));
        }

        /// <summary>Gets the clones of this item.</summary>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetClones()
        {
            return this.GetClones(false);
        }

        /// <summary>Gets the clones of this item.</summary>
        /// <param name="processChildren">if set to <c>true</c> then child items will be processed and will be included to result.</param>
        /// <returns></returns>
        public virtual IEnumerable<Item> GetClones(bool processChildren)
        {
            if (!Settings.ItemCloning.Enabled)
                return (IEnumerable<Item>)Array.Empty<Item>();
            return this.GetClonesInternal(this, processChildren);
        }

        /// <summary>Gets the outer XML of the item.</summary>
        /// <param name="includeSubitems">
        /// if set to <c>true</c> the XML includes subitems.
        /// </param>
        /// <returns>The get outer xml.</returns>
        public virtual string GetOuterXml(bool includeSubitems)
        {
            ItemSerializerOptions defaultOptions = ItemSerializerOptions.GetDefaultOptions();
            defaultOptions.ProcessChildren = includeSubitems;
            return this.GetOuterXml(defaultOptions);
        }

        /// <summary>Gets the outer XML.</summary>
        /// <param name="options">The options.</param>
        /// <returns>Returns outer xml of item.</returns>
        public virtual string GetOuterXml(ItemSerializerOptions options)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)options, nameof(options));
            return Sitecore.Diagnostics.Assert.ResultNotNull<string>(ItemSerializer.GetItemXml(this, options));
        }

        /// <summary>
        /// Gets a value that indicates if language fallback functionality is enabled for the current item.
        /// Use <see cref="P:Sitecore.Data.Items.Item.IsFallback" /> to determine whether item is a fallback item.
        /// </summary>
        /// <seealso cref="P:Sitecore.Data.Items.Item.IsFallback" />
        /// <value>
        /// <c>true</c> if language fallback functionality is enabled for the current item; otherwise, <c>false</c>.
        /// </value>
        public virtual bool LanguageFallbackEnabled
        {
            get
            {
                bool? currentValue = Switcher<bool?, LanguageFallbackItemSwitcher>.CurrentValue;
                bool? nullable1 = currentValue;
                bool flag1 = false;
                if ((nullable1.GetValueOrDefault() == flag1 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
                    return false;
                bool? nullable2 = currentValue;
                bool flag2 = true;
                SiteContext site;
                if ((nullable2.GetValueOrDefault() == flag2 ? (!nullable2.HasValue ? 1 : 0) : 1) != 0 && ((site = Context.Site) == null || !site.SiteInfo.EnableItemLanguageFallback) || StandardValuesManager.IsStandardValuesHolder(this) && this.Fields[FieldIDs.EnableItemFallback].GetValue(false) != "1")
                    return false;
                using (new LanguageFallbackItemSwitcher(new bool?(false)))
                {
                    if (this.Fields[FieldIDs.EnableItemFallback].GetValue(true, true, false) != "1")
                        return false;
                }
                return true;
            }
        }

        /// <summary>Gets the fallback item.</summary>
        /// <returns></returns>
        public virtual Item GetFallbackItem()
        {
            if (this.LanguageFallbackEnabled)
            {
                Language fallbackLanguage = LanguageFallbackManager.GetFallbackLanguage(this.Language, this.Database, this.ID);
                if (fallbackLanguage != (Language)null && !string.IsNullOrEmpty(fallbackLanguage.Name) && !fallbackLanguage.Equals((object)this.Language))
                    return this.Database.GetItem(this.ID, fallbackLanguage, Sitecore.Data.Version.Latest);
            }
            return (Item)null;
        }

        /// <summary>Moves the item to another location.</summary>
        /// <param name="destination">
        /// Destination item (i.e. the item that will serve as the new parent of the
        /// item).
        /// </param>
        /// <remarks>
        /// Moves an item and all of its descendants to another
        /// location in the content tree.
        /// </remarks>
        public virtual void MoveTo(Item destination)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)destination, nameof(destination));
            Sitecore.Diagnostics.Error.AssertAccess((this.Access.CanMoveTo(destination) ? 1 : 0) != 0, "Move item (" + (object)this.ID + " --> " + (object)destination.ID + ")");
            if (!destination.ID.Equals(this.ParentID))
                ItemUtil.AssertDuplicateItemName(destination, this.Name);
            ItemManager.MoveItem(this, destination);
        }

        /// <summary>Inserts new items from an XML string.</summary>
        /// <param name="xml">The XML.</param>
        /// <param name="changeIDs">
        /// if set to <c>true</c> ids should be changed.
        /// </param>
        /// <param name="mode">The mode.</param>
        public virtual void Paste(string xml, bool changeIDs, PasteMode mode)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(xml, nameof(xml));
            this.PasteItem(xml, changeIDs, mode);
        }

        /// <summary>Inserts new items from an XML string.</summary>
        /// <param name="xml">The XML.</param>
        /// <param name="changeIDs">
        /// if set to <c>true</c> ids should be changed.
        /// </param>
        /// <param name="mode">The mode.</param>
        /// <returns>The item.</returns>
        public virtual Item PasteItem(string xml, bool changeIDs, PasteMode mode)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(xml, nameof(xml));
            return ItemSerializer.PasteItemXml(xml, this, changeIDs, mode);
        }

        /// <summary>Puts the item in the Recycle Bin database.</summary>
        /// <returns>
        /// The id of archive entry. If an archive entry hasn't been created, the return value will be <c>Guid.Empty</c>.
        /// </returns>
        public virtual Guid Recycle()
        {
            Sitecore.Diagnostics.Assert.HasAccess((this.Access.CanDelete() ? 1 : 0) != 0, "User does not have the necessary rights to recycle the item ({0})", (object)this.ID);
            if (Settings.RecycleBinActive)
            {
                if (this.RuntimeSettings.IsVirtual)
                    return Guid.Empty;
                Archive archive = ArchiveManager.GetArchive("recyclebin", this.Database);
                if (archive != null)
                    return archive.ArchiveItem(this);
            }
            this.Delete();
            return Guid.Empty;
        }

        /// <summary>
        /// Deletes all child items to which the current user has delete access.
        /// </summary>
        /// <returns>The number of child items deleted.</returns>
        public virtual int RecycleChildren()
        {
            ChildList children = ItemManager.GetChildren(this, SecurityCheck.Disable, ChildListOptions.SkipSorting);
            int num = 0;
            for (int index = 0; index < children.Count; ++index)
            {
                Item obj = children[index];
                if (obj.Access.CanDelete())
                {
                    obj.Recycle();
                    ++num;
                }
            }
            return num;
        }

        /// <summary>Puts the item version in the Recycle Bin database.</summary>
        /// <returns>
        /// The id of archive entry. If an archive entry hasn't been created, the return value will be <c>Guid.Empty</c>.
        /// </returns>
        public virtual Guid RecycleVersion()
        {
            Sitecore.Diagnostics.Assert.HasAccess((this.Access.CanRemoveVersion() ? 1 : 0) != 0, "User does not have the necessary rights to recycle the version ({0})", (object)this.ID);
            if (Settings.RecycleBinActive)
            {
                if (this.RuntimeSettings.IsVirtual)
                    return Guid.Empty;
                Archive archive = ArchiveManager.GetArchive("recyclebin", this.Database);
                if (archive != null)
                    return archive.ArchiveVersion(this);
            }
            this.Versions.RemoveVersion();
            return Guid.Empty;
        }

        /// <summary>Reloads the item from storage.</summary>
        public virtual void Reload()
        {
            this.Publishing.ClearPublishingCache();
            Item obj = ItemManager.GetItem(this.ID, this.Language, this.Version, this.Database, SecurityCheck.Disable);
            if (obj == null)
                return;
            this.SetInnerData(obj);
            this.Fields.Reset();
            Item.RemoveItemFromCloningCache(this);
        }

        /// <summary>
        /// Gets a value that indicates if the item is empty (i.e. has no versions).
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <param name="database">The database.</param>
        /// <returns>
        /// <c>true</c> if the specified item ID is empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmpty(ID itemID, Database database)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)itemID, nameof(itemID));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)database, nameof(database));
            Item obj = ItemManager.GetItem(itemID, Language.Current, Sitecore.Data.Version.Latest, database);
            if (obj == null)
                return true;
            foreach (Language language in (Collection<Language>)LanguageManager.GetLanguages(database))
            {
                VersionCollection versions = ItemManager.GetVersions(obj, language);
                if (versions != null && versions.Count > 0)
                    return false;
            }
            return true;
        }

        /// <summary>Gets a unique id.</summary>
        /// <remarks>
        /// <para>
        /// The id must be unique enough to be used in a global cache.
        /// </para>
        /// <para>
        /// For example, an <see cref="T:Sitecore.Data.Items.Item" /> might implement this as <c>Database.Name + ID</c>.
        /// </para>
        /// <para>
        /// The resulting key used by the <see cref="T:Sitecore.Security.AccessControl.AuthorizationManager" /> might therefore be <c>typeof(Item).FullName + divider + Database.Name + ID</c>.
        /// </para>
        /// </remarks>
        /// <returns>The get unique id.</returns>
        public virtual string GetUniqueId()
        {
            if (this._uniqueId == null)
                this._uniqueId = this.Database.Name + (object)this.ID + this.Language.Name + (object)this.Version.Number;
            return this._uniqueId;
        }

        /// <summary>Adds to item cloning relations cache.</summary>
        /// <param name="item">The item.</param>
        /// <param name="uri">The URI to add to cache.</param>
        protected virtual void AddToItemCloningRelationsCache(Item item, ItemUri uri)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            Item.ItemCloningRelations.Value.Add(new ItemDatabaseSpecificKey(item), uri == (ItemUri)null ? (object)string.Empty : (object)uri.ToString());
        }

        /// <summary>Gets the clones (internal method).</summary>
        /// <param name="item">The item.</param>
        /// <param name="processChildren">if set to <c>true</c> then child items will be processed and will be included to result.</param>
        /// <returns></returns>
        protected virtual IEnumerable<Item> GetClonesInternal(
          Item item,
          bool processChildren)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            List<Item> clones = new List<Item>();
            using (new SecurityDisabler())
            {
                ItemLink[] itemLinkArray = Globals.LinkDatabase.GetReferrers(item);
                for (int index = 0; index < itemLinkArray.Length; ++index)
                {
                    ItemLink itemLink = itemLinkArray[index];
                    if (!(itemLink.SourceFieldID != FieldIDs.Source) || !(itemLink.SourceFieldID != FieldIDs.SourceItem))
                    {
                        Item clone = itemLink.GetSourceItem();
                        if (clone != null && clones.FirstOrDefault<Item>((Func<Item, bool>)(c => c.Uri == clone.Uri)) == null)
                        {
                            clones.Add(clone);
                            yield return clone;
                        }
                    }
                }
                itemLinkArray = (ItemLink[])null;
            }
            if (processChildren)
            {
                foreach (Item child in item.Children)
                {
                    foreach (Item obj in this.GetClonesInternal(child, true))
                        yield return obj;
                }
            }
        }

        /// <summary>Gets the property.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The property.</returns>
        private object GetProperty(string name)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            return this.GetChanges(false)?.GetPropertyValue(name);
        }

        /// <summary>Sets the property.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="originalValue">The original value.</param>
        private void SetProperty(string name, string value, string originalValue)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(value, nameof(value));
            this.GetChanges(true)?.SetPropertyValue(name, value, originalValue);
        }

        /// <summary>Sets the property.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="originalValue">The original value.</param>
        private void SetProperty(string name, ID value, ID originalValue)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)value, nameof(value));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)originalValue, nameof(originalValue));
            this.GetChanges(true)?.SetPropertyValue(name, value, originalValue);
        }

        /// <summary>
        /// Tries the get value from item cloning relations cache.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemUri">The item URI.</param>
        /// <returns></returns>
        protected virtual bool TryGetCacheValue(Item item, out ItemUri itemUri)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            itemUri = (ItemUri)null;
            string itemUri1 = Item.ItemCloningRelations.Value.GetValue(new ItemDatabaseSpecificKey(item)) as string;
            if (itemUri1 == null)
                return false;
            if (!string.IsNullOrEmpty(itemUri1))
                itemUri = ItemUri.Parse(itemUri1);
            return true;
        }

        /// <summary>
        /// Gets the Item URI from __Source item and __Source fields
        /// </summary>
        /// <returns>ItemUri retreived from __Source item and __Source fields</returns>
        private ItemUri GetItemUriFromSourceItem()
        {
            ItemUri itemUri1 = (ItemUri)null;
            string itemUri2 = this.Fields[FieldIDs.Source].GetValue(false, false);
            string itemUri3 = this.Fields[FieldIDs.SourceItem].GetValue(false, false);
            if (!string.IsNullOrEmpty(itemUri2))
                itemUri1 = ItemUri.Parse(itemUri2);
            if (string.IsNullOrEmpty(itemUri3))
                return (ItemUri)null;
            Sitecore.Data.Version version = itemUri1 == (ItemUri)null ? this.Version : itemUri1.Version;
            return new ItemUri(new ItemUri(itemUri3).ToString(false), this.Language, version, this.Database);
        }

        /// <summary>Gets the changes.</summary>
        /// <param name="force">
        /// if set to <c>true</c> this instance is force.
        /// </param>
        /// <returns>The changes.</returns>
        internal virtual ItemChanges GetChanges(bool force)
        {
            ItemChanges changes = this._changes;
            if (changes == null & force)
            {
                lock (this.SyncRoot)
                {
                    if (this._changes == null)
                        this._changes = new ItemChanges(this);
                    changes = this._changes;
                }
            }
            return changes;
        }

        /// <summary>Gets the full changes.</summary>
        /// <returns>The full changes.</returns>
        internal ItemChanges GetFullChanges()
        {
            ItemChanges changes = this.GetChanges(true);
            Sitecore.Diagnostics.Assert.IsNotNull((object)changes, "changes");
            FieldCollection fields = this.Fields;
            fields.ReadAll();
            foreach (ID fieldId in this.InnerData.Fields.GetFieldIDs())
            {
                Field field1 = fields[fieldId];
                string field2 = field1.GetValue(false, false);
                if (field2 != null && field2.Length == 0 && (this.RuntimeSettings.SaveAll && this.InnerData.Fields[field1.ID] != null) && field1.IsBlobField)
                    field2 = this.InnerData.Fields[fieldId];
                if (changes.FieldChanges[field1.ID] == null)
                    changes.SetFieldValue(field1, field2);
            }
            return Sitecore.Diagnostics.Assert.ResultNotNull<ItemChanges>(changes);
        }

        /// <summary>Deletes the item.</summary>
        /// <param name="removeBlobs">if set to <c>true</c> then removes BLOBs if delete was successful.</param>
        internal void Delete(bool removeBlobs)
        {
            Database database = this.Database;
            Sitecore.Data.Templates.Template template = TemplateManager.GetTemplate(this);
            List<Guid> guidList = new List<Guid>();
            bool flag1 = removeBlobs && template != null;
            if (flag1)
                guidList = ((IEnumerable<TemplateField>)template.GetFields(true)).Where<TemplateField>((Func<TemplateField, bool>)(f => f.IsBlob)).Select<TemplateField, ID>((Func<TemplateField, ID>)(f => f.ID)).Select<ID, Guid>((Func<ID, Guid>)(id => MainUtil.GetGuid((object)this[id]))).Where<Guid>((Func<Guid, bool>)(guid => guid != Guid.Empty)).ToList<Guid>();
            bool flag2 = ItemManager.DeleteItem(this);
            Item.RemoveItemFromCloningCache(this);
            if (!(flag1 & flag2))
                return;
            foreach (Guid blobId in guidList)
                ItemManager.RemoveBlobStream(blobId, database);
        }

        /// <summary>Rejects the changes.</summary>
        internal void RejectChanges()
        {
            this._changes = (ItemChanges)null;
        }

        /// <summary>Removes the item from cloning cache.</summary>
        /// <param name="item">The item.</param>
        internal static void RemoveItemFromCloningCache(Item item)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            Item.ItemCloningRelations.Value.Remove(new ItemDatabaseSpecificKey(item));
        }

        /// <summary>Sets the changes.</summary>
        /// <param name="changes">The changes.</param>
        internal void SetChanges(ItemChanges changes)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)changes, nameof(changes));
            this._changes = changes;
        }

        /// <summary>Sets the database.</summary>
        /// <param name="database">The database.</param>
        internal void SetDatabase(Database database)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)database, nameof(database));
            this._database = database;
        }

        /// <summary>Sets the inner data.</summary>
        /// <param name="item">The item.</param>
        internal void SetInnerData(Item item)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            this._innerData = item.InnerData;
            this._changes = (ItemChanges)null;
        }

        /// <summary>ItemDebuggerTypeProxy class</summary>
        public class ItemDebuggerTypeProxy
        {
            /// <summary>The _item.</summary>
            private readonly Item _item;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Sitecore.Data.Items.Item.ItemDebuggerTypeProxy" /> class.
            /// </summary>
            /// <param name="item">The item.</param>
            public ItemDebuggerTypeProxy(Item item)
            {
                Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
                this._item = item;
            }

            /// <summary>Gets the database name.</summary>
            /// <value>The database.</value>
            public string Database
            {
                get
                {
                    return this._item.Database.Name;
                }
            }

            /// <summary>Gets the item id.</summary>
            /// <value>The id.</value>
            public string Id
            {
                get
                {
                    return this._item.ID.ToString();
                }
            }

            /// <summary>Gets the item language.</summary>
            /// <value>The language.</value>
            public string Language
            {
                get
                {
                    return this._item.Language.ToString();
                }
            }

            /// <summary>Gets the item name.</summary>
            /// <value>The name.</value>
            public string Name
            {
                get
                {
                    return this._item.Name;
                }
            }

            /// <summary>Gets the item version.</summary>
            /// <value>The version.</value>
            public string Version
            {
                get
                {
                    return this._item.Version.ToString();
                }
            }
        }
    }
}
