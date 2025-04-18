﻿using Intersect.Editor.Localization;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Events.Commands;
using Intersect.GameObjects;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands;


public partial class EventCommandStartQuest : UserControl
{

    private readonly FrmEvent mEventEditor;

    private EventPage mCurrentPage;

    private StartQuestCommand mMyCommand;

    public EventCommandStartQuest(StartQuestCommand refCommand, EventPage page, FrmEvent editor)
    {
        InitializeComponent();
        mMyCommand = refCommand;
        mCurrentPage = page;
        mEventEditor = editor;
        InitLocalization();
        cmbQuests.Items.Clear();
        cmbQuests.Items.AddRange(QuestDescriptor.Names);
        cmbQuests.SelectedIndex = QuestDescriptor.ListIndex(refCommand.QuestId);
        chkShowOfferWindow.Checked = refCommand.Offer;
    }

    private void InitLocalization()
    {
        grpStartQuest.Text = Strings.EventStartQuest.title;
        lblQuest.Text = Strings.EventStartQuest.label;
        chkShowOfferWindow.Text = Strings.EventStartQuest.showwindow;
        btnSave.Text = Strings.EventStartQuest.okay;
        btnCancel.Text = Strings.EventStartQuest.cancel;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        mMyCommand.QuestId = QuestDescriptor.IdFromList(cmbQuests.SelectedIndex);
        mMyCommand.Offer = chkShowOfferWindow.Checked;
        mEventEditor.FinishCommandEdit();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        mEventEditor.CancelCommandEdit();
    }

}
