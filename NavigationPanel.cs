using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace LightNovelEditor
{
    public class NavigationPanel : Panel
    {
        private readonly TreeView treeView;
        private readonly ChapterManager chapterManager;

        public event EventHandler<ChapterSelectedEventArgs>? ChapterSelected;
        public event EventHandler<TreeViewEventArgs>? ItemSelected;
        public event EventHandler? AddChapterRequested;

        public NavigationPanel(ChapterManager chapterManager)
        {
            this.chapterManager = chapterManager;
            Dock = DockStyle.Left;
            Width = 250;
            BackColor = Color.White;

            treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                ShowLines = true,
                HideSelection = false
            };

            treeView.AfterSelect += TreeView_AfterSelect;
            Controls.Add(treeView);

            // Add a button for adding new chapters
            var addButton = new Button
            {
                Text = "Add Chapter",
                Dock = DockStyle.Bottom,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(60, 60, 60),
                Font = new Font("Segoe UI", 9F)
            };
            addButton.Click += (s, e) => AddChapterRequested?.Invoke(this, EventArgs.Empty);
            Controls.Add(addButton);

            // Load existing chapters
            LoadExistingChapters();
        }

        private void LoadExistingChapters()
        {
            var existingChapters = chapterManager.LoadExistingChapters().ToList();
            foreach (var chapter in existingChapters)
            {
                var node = new TreeNode($"Chapter {chapter.Id}: {chapter.Title}")
                {
                    Tag = chapter
                };
                treeView.Nodes.Add(node);
            }
        }

        public void AddChapter(int chapterNumber, string title)
        {
            var chapterInfo = new ChapterInfo { Id = chapterNumber, Title = title };
            var node = new TreeNode($"Chapter {chapterNumber}: {title}")
            {
                Tag = chapterInfo
            };
            treeView.Nodes.Add(node);

            // Select the newly added node
            treeView.SelectedNode = node;
        }

        private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is ChapterInfo chapterInfo)
            {
                ChapterSelected?.Invoke(this, new ChapterSelectedEventArgs(chapterInfo));
                ItemSelected?.Invoke(this, e);
            }
        }
    }

    public class ChapterSelectedEventArgs : EventArgs
    {
        public ChapterInfo ChapterInfo { get; }

        public ChapterSelectedEventArgs(ChapterInfo chapterInfo)
        {
            ChapterInfo = chapterInfo;
        }
    }

    public class ChapterInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
    }
} 