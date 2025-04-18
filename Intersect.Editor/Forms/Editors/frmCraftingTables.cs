using DarkUI.Forms;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Crafting;
using Intersect.GameObjects;
using Intersect.Models;
using System.Diagnostics;

namespace Intersect.Editor.Forms.Editors;


public partial class FrmCraftingTables : EditorForm
{

    private List<CraftingTableDescriptor> mChanged = new List<CraftingTableDescriptor>();

    private string mCopiedItem;

    private CraftingTableDescriptor mEditorItem;

    private List<string> mKnownFolders = new List<string>();

    public FrmCraftingTables()
    {
        ApplyHooks();
        InitializeComponent();
        Icon = Program.Icon;
        _btnSave = btnSave;
        _btnCancel = btnCancel;

        lstGameObjects.Init(UpdateToolStripItems, AssignEditorItem, toolStripItemNew_Click, toolStripItemCopy_Click, toolStripItemUndo_Click, toolStripItemPaste_Click, toolStripItemDelete_Click);
    }
    private void AssignEditorItem(Guid id)
    {
        mEditorItem = CraftingTableDescriptor.Get(id);
        UpdateEditor();
    }

    protected override void GameObjectUpdatedDelegate(GameObjectType type)
    {
        if (type == GameObjectType.CraftTables)
        {
            InitEditor();
            if (mEditorItem != null && !DatabaseObject<CraftingTableDescriptor>.Lookup.Values.Contains(mEditorItem))
            {
                mEditorItem = null;
                UpdateEditor();
            }
        }
    }

    private void UpdateEditor()
    {
        if (mEditorItem != null)
        {
            pnlContainer.Show();

            txtName.Text = mEditorItem.Name;

            cmbFolder.Text = mEditorItem.Folder;

            UpdateList();

            if (mChanged.IndexOf(mEditorItem) == -1)
            {
                mChanged.Add(mEditorItem);
                mEditorItem.MakeBackup();
            }
        }
        else
        {
            pnlContainer.Hide();
        }

        var hasItem = mEditorItem != null;
        UpdateEditorButtons(hasItem);
        UpdateToolStripItems();
    }

    private void txtName_TextChanged(object sender, EventArgs e)
    {
        mEditorItem.Name = txtName.Text;
        lstGameObjects.UpdateText(txtName.Text);
    }


    private void FrmCraftingTables_FormClosed(object sender, FormClosedEventArgs e)
    {
        btnCancel_Click(null, null);
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        foreach (var item in mChanged)
        {
            item.RestoreBackup();
            item.DeleteBackup();
        }

        Hide();
        Globals.CurrentEditor = -1;
        Dispose();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        //Send Changed items
        foreach (var item in mChanged)
        {
            PacketSender.SendSaveObject(item);
            item.DeleteBackup();
        }

        Hide();
        Globals.CurrentEditor = -1;
        Dispose();
    }

    private void toolStripItemNew_Click(object sender, EventArgs e)
    {
        PacketSender.SendCreateObject(GameObjectType.CraftTables);
    }

    private void toolStripItemDelete_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && lstGameObjects.Focused)
        {
            if (DarkMessageBox.ShowWarning(
                    Strings.CraftingTableEditor.deleteprompt, Strings.CraftingTableEditor.delete,
                    DarkDialogButton.YesNo, Icon
                ) ==
                DialogResult.Yes)
            {
                PacketSender.SendDeleteObject(mEditorItem);
            }
        }
    }

    private void toolStripItemCopy_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && lstGameObjects.Focused)
        {
            mCopiedItem = mEditorItem.JsonData;
            toolStripItemPaste.Enabled = true;
        }
    }

    private void toolStripItemPaste_Click(object sender, EventArgs e)
    {
        if (mEditorItem != null && mCopiedItem != null && lstGameObjects.Focused)
        {
            mEditorItem.Load(mCopiedItem, true);
            UpdateEditor();
        }
    }

    private void toolStripItemUndo_Click(object sender, EventArgs e)
    {
        if (mChanged.Contains(mEditorItem) && mEditorItem != null)
        {
            if (DarkMessageBox.ShowWarning(
                    Strings.CraftingTableEditor.undoprompt, Strings.CraftingTableEditor.undotitle,
                    DarkDialogButton.YesNo, Icon
                ) ==
                DialogResult.Yes)
            {
                mEditorItem.RestoreBackup();
                UpdateEditor();
            }
        }
    }

    private void UpdateToolStripItems()
    {
        toolStripItemCopy.Enabled = mEditorItem != null && lstGameObjects.Focused;
        toolStripItemPaste.Enabled = mEditorItem != null && mCopiedItem != null && lstGameObjects.Focused;
        toolStripItemDelete.Enabled = mEditorItem != null && lstGameObjects.Focused;
        toolStripItemUndo.Enabled = mEditorItem != null && lstGameObjects.Focused;
    }

    private void lstGameObjects_FocusChanged(object sender, EventArgs e)
    {
        UpdateToolStripItems();
    }

    private void form_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            if (e.KeyCode == Keys.N)
            {
                toolStripItemNew_Click(null, null);
            }
        }
    }

    private void frmCrafting_Load(object sender, EventArgs e)
    {
        UpdateEditor();
        cmbCrafts.Items.Clear();
        cmbCrafts.Items.AddRange(CraftingRecipeDescriptor.Names);

        InitLocalization();
    }

    private void InitLocalization()
    {
        Text = Strings.CraftingTableEditor.title;
        toolStripItemNew.Text = Strings.CraftingTableEditor.New;
        toolStripItemDelete.Text = Strings.CraftingTableEditor.delete;
        toolStripItemCopy.Text = Strings.CraftingTableEditor.copy;
        toolStripItemPaste.Text = Strings.CraftingTableEditor.paste;
        toolStripItemUndo.Text = Strings.CraftingTableEditor.undo;

        grpTables.Text = Strings.CraftingTableEditor.tables;
        grpCrafts.Text = Strings.CraftingTableEditor.crafts;

        lblAddCraftedItem.Text = Strings.CraftingTableEditor.addcraftlabel;
        btnAddCraftedItem.Text = Strings.CraftingTableEditor.add;
        btnRemoveCraftedItem.Text = Strings.CraftingTableEditor.remove;

        grpGeneral.Text = Strings.CraftingTableEditor.general;
        lblName.Text = Strings.CraftingTableEditor.name;

        //Searching/Sorting
        btnAlphabetical.ToolTipText = Strings.CraftingTableEditor.sortalphabetically;
        txtSearch.Text = Strings.CraftingTableEditor.searchplaceholder;
        lblFolder.Text = Strings.CraftingTableEditor.folderlabel;

        btnSave.Text = Strings.CraftingTableEditor.save;
        btnCancel.Text = Strings.CraftingTableEditor.cancel;
    }

    public void UpdateList()
    {
        lstCrafts.Items.Clear();
        foreach (var id in mEditorItem.Crafts)
        {
            lstCrafts.Items.Add(CraftingRecipeDescriptor.GetName(id));
        }
    }

    private void btnAddCraftedItem_Click(object sender, EventArgs e)
    {
        var id = CraftingRecipeDescriptor.IdFromList(cmbCrafts.SelectedIndex);
        var craft = CraftingRecipeDescriptor.Get(id);
        if (craft != null && !mEditorItem.Crafts.Contains(id))
        {
            mEditorItem.Crafts.Add(id);
            UpdateList();
        }
    }

    private void btnRemoveCraftedItem_Click(object sender, EventArgs e)
    {
        if (lstCrafts.SelectedIndex > -1)
        {
            mEditorItem.Crafts.RemoveAt(lstCrafts.SelectedIndex);
            UpdateList();
        }
    }

    private void btnCraftUp_Click(object sender, EventArgs e)
    {
        if (lstCrafts.SelectedIndex > 0 && lstCrafts.Items.Count > 1)
        {
            var index = lstCrafts.SelectedIndex;
            var swapWith = mEditorItem.Crafts[index - 1];
            mEditorItem.Crafts[index - 1] = mEditorItem.Crafts[index];
            mEditorItem.Crafts[index] = swapWith;
            UpdateList();
            lstCrafts.SelectedIndex = index - 1;
        }
    }

    private void btnCraftDown_Click(object sender, EventArgs e)
    {
        if (lstCrafts.SelectedIndex > -1 && lstCrafts.SelectedIndex + 1 != lstCrafts.Items.Count)
        {
            var index = lstCrafts.SelectedIndex;
            var swapWith = mEditorItem.Crafts[index + 1];
            mEditorItem.Crafts[index + 1] = mEditorItem.Crafts[index];
            mEditorItem.Crafts[index] = swapWith;
            UpdateList();
            lstCrafts.SelectedIndex = index + 1;
        }
    }

    #region "Item List - Folders, Searching, Sorting, Etc"

    public void InitEditor()
    {
        //Collect folders
        var mFolders = new List<string>();
        foreach (var itm in CraftingTableDescriptor.Lookup)
        {
            if (!string.IsNullOrEmpty(((CraftingTableDescriptor) itm.Value).Folder) &&
                !mFolders.Contains(((CraftingTableDescriptor) itm.Value).Folder))
            {
                mFolders.Add(((CraftingTableDescriptor) itm.Value).Folder);
                if (!mKnownFolders.Contains(((CraftingTableDescriptor) itm.Value).Folder))
                {
                    mKnownFolders.Add(((CraftingTableDescriptor) itm.Value).Folder);
                }
            }
        }

        mFolders.Sort();
        mKnownFolders.Sort();
        cmbFolder.Items.Clear();
        cmbFolder.Items.Add("");
        cmbFolder.Items.AddRange(mKnownFolders.ToArray());

        var items = CraftingTableDescriptor.Lookup.OrderBy(p => p.Value?.Name).Select(pair => new KeyValuePair<Guid, KeyValuePair<string, string>>(pair.Key,
            new KeyValuePair<string, string>(((CraftingTableDescriptor)pair.Value)?.Name ?? Models.DatabaseObject<CraftingTableDescriptor>.Deleted, ((CraftingTableDescriptor)pair.Value)?.Folder ?? ""))).ToArray();
        lstGameObjects.Repopulate(items, mFolders, btnAlphabetical.Checked, CustomSearch(), txtSearch.Text);
    }

    private void btnAddFolder_Click(object sender, EventArgs e)
    {
        var folderName = string.Empty;
        var result = DarkInputBox.ShowInformation(
            Strings.CraftingTableEditor.folderprompt, Strings.CraftingTableEditor.foldertitle, ref folderName,
            DarkDialogButton.OkCancel
        );

        if (result == DialogResult.OK && !string.IsNullOrEmpty(folderName))
        {
            if (!cmbFolder.Items.Contains(folderName))
            {
                mEditorItem.Folder = folderName;
                lstGameObjects.ExpandFolder(folderName);
                InitEditor();
                cmbFolder.Text = folderName;
            }
        }
    }

    private void cmbFolder_SelectedIndexChanged(object sender, EventArgs e)
    {
        mEditorItem.Folder = cmbFolder.Text;
        InitEditor();
    }

    private void btnAlphabetical_Click(object sender, EventArgs e)
    {
        btnAlphabetical.Checked = !btnAlphabetical.Checked;
        InitEditor();
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        InitEditor();
    }

    private void txtSearch_Leave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            txtSearch.Text = Strings.CraftingTableEditor.searchplaceholder;
        }
    }

    private void txtSearch_Enter(object sender, EventArgs e)
    {
        txtSearch.SelectAll();
        txtSearch.Focus();
    }

    private void btnClearSearch_Click(object sender, EventArgs e)
    {
        txtSearch.Text = Strings.CraftingTableEditor.searchplaceholder;
    }

    private bool CustomSearch()
    {
        return !string.IsNullOrWhiteSpace(txtSearch.Text) &&
               txtSearch.Text != Strings.CraftingTableEditor.searchplaceholder;
    }

    private void txtSearch_Click(object sender, EventArgs e)
    {
        if (txtSearch.Text == Strings.CraftingTableEditor.searchplaceholder)
        {
            txtSearch.SelectAll();
        }
    }

    #endregion
}
