using System;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class MainFormEventHandler
    {
        private readonly MainForm mainForm;
        private readonly EditorPanel editorPanel;
        private readonly NavigationPanel navigationPanel;
        private readonly CustomToolbar toolbar;
        private readonly ChapterManager chapterManager;
        private readonly FileManager fileManager;
        private readonly DialogManager dialogManager;
        private readonly UIManager uiManager;
        private ChapterInfo? currentChapter;

        public MainFormEventHandler(
            MainForm mainForm,
            EditorPanel editorPanel,
            NavigationPanel navigationPanel,
            CustomToolbar toolbar,
            ChapterManager chapterManager,
            FileManager fileManager,
            DialogManager dialogManager,
            UIManager uiManager)
        {
            this.mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            this.editorPanel = editorPanel ?? throw new ArgumentNullException(nameof(editorPanel));
            this.navigationPanel = navigationPanel ?? throw new ArgumentNullException(nameof(navigationPanel));
            this.toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
            this.chapterManager = chapterManager ?? throw new ArgumentNullException(nameof(chapterManager));
            this.fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            this.dialogManager = dialogManager ?? throw new ArgumentNullException(nameof(dialogManager));
            this.uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
        }

        public void WireUpEvents()
        {
            try
            {
                // Toolbar events
                toolbar.NewClicked += (s, e) => fileManager.NewFile(mainForm);
                toolbar.OpenClicked += (s, e) => fileManager.OpenFile(mainForm);
                toolbar.SaveClicked += (s, e) => fileManager.SaveFile(false, mainForm);
                toolbar.BoldClicked += (s, e) => editorPanel.ToggleBold();
                toolbar.ItalicClicked += (s, e) => editorPanel.ToggleItalic();
                toolbar.UnderlineClicked += (s, e) => editorPanel.ToggleUnderline();
                toolbar.FontClicked += (s, e) => fileManager.FormatFont();
                toolbar.UploadClicked += async (s, e) => await dialogManager.ShowUploadChapterDialog();
                
                // Navigation panel events
                navigationPanel.ItemSelected += (s, e) => 
                {
                    if (e.Node?.Tag is ChapterInfo chapterInfo)
                    {
                        // Save current chapter content if we have one
                        if (currentChapter != null)
                        {
                            chapterManager.SaveChapterContent(
                                currentChapter.Id,
                                currentChapter.Title,
                                editorPanel.Text
                            );
                        }

                        // Load the selected chapter
                        currentChapter = chapterInfo;
                        editorPanel.SetDocumentTitle($"Chapter {chapterInfo.Id}: {chapterInfo.Title}");
                        editorPanel.Text = chapterManager.LoadChapterContent(chapterInfo.Id, chapterInfo.Title);
                    }
                };

                // Editor content changed event
                editorPanel.ContentChanged += (s, e) =>
                {
                    if (currentChapter != null)
                    {
                        chapterManager.SaveChapterContent(
                            currentChapter.Id,
                            currentChapter.Title,
                            editorPanel.Text
                        );
                    }
                };

                navigationPanel.AddChapterRequested += (s, e) => dialogManager.ShowAddChapterDialog(ref currentChapter);
                
                // Form closing event - no need to prompt for saves anymore as content is auto-saved
                mainForm.FormClosing += (s, e) =>
                {
                    if (currentChapter != null)
                    {
                        chapterManager.SaveChapterContent(
                            currentChapter.Id,
                            currentChapter.Title,
                            editorPanel.Text
                        );
                    }
                };

                // Form layout events
                mainForm.Shown += (s, e) => OnMainFormShown();
                mainForm.Resize += (s, e) => OnMainFormResize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Event wiring error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMainFormShown()
        {
            uiManager.HandleSplitterLayout();
            editorPanel.Focus();
        }

        private void OnMainFormResize()
        {
            uiManager.HandleResize();
        }
    }
} 