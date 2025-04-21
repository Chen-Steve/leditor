using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class NavigationPanel : Panel
    {
        private readonly TreeView navigationTree;
        private readonly Button? addChapterButton;
        
        public event EventHandler<NavigationItemSelectedEventArgs>? ItemSelected;
        public event EventHandler? AddChapterRequested;

        public NavigationPanel()
        {
            try
            {
                this.BackColor = Color.FromArgb(250, 250, 250);
                this.Dock = DockStyle.Fill;
                
                // Add container panel with padding
                var contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5, 5, 5, 5)
                };
                this.Controls.Add(contentPanel);
                
                // Create tree view with custom styling
                navigationTree = new TreeView
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    Indent = 24,
                    ShowLines = false,
                    ShowPlusMinus = true,
                    ItemHeight = 30,
                    Font = new Font("Segoe UI", 10.5F),
                    BackColor = Color.FromArgb(250, 250, 250),
                    ForeColor = Color.FromArgb(60, 60, 70),
                    FullRowSelect = true,
                    HideSelection = false
                };
                
                navigationTree.AfterSelect += (s, e) => 
                {
                    if (e.Node != null)
                    {
                        ItemSelected?.Invoke(this, new NavigationItemSelectedEventArgs(e.Node.Text, e.Node.Tag));
                    }
                };
                
                contentPanel.Controls.Add(navigationTree);
                
                // Create bottom toolbar
                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(245, 245, 245),
                    Padding = new Padding(10, 8, 10, 8)
                };
                
                // Add a top border to button panel
                var topBorder = new Panel
                {
                    Height = 1,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(230, 230, 230)
                };
                buttonPanel.Controls.Add(topBorder);
                
                addChapterButton = new Button
                {
                    Text = "Add Chapter",
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 70),
                    Cursor = Cursors.Hand
                };
                addChapterButton.FlatAppearance.BorderSize = 1;
                addChapterButton.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                addChapterButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
                addChapterButton.Click += (s, e) => AddChapterRequested?.Invoke(this, EventArgs.Empty);
                
                buttonPanel.Controls.Add(addChapterButton);
                this.Controls.Add(buttonPanel);
                
                // Initialize with empty chapters list
                this.HandleCreated += (s, e) => InitializeTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing navigation panel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Create a minimal TreeView to prevent null reference exceptions
                navigationTree = new TreeView
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None
                };
                this.Controls.Add(navigationTree);
            }
        }
        
        private void InitializeTreeView()
        {
            try
            {
                // Clear existing nodes first
                navigationTree.Nodes.Clear();
                
                // Add chapters root node
                TreeNode chaptersNode = navigationTree.Nodes.Add("Chapters");
                chaptersNode.Tag = "chapters_root";
                chaptersNode.Expand();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing tree view: {ex.Message}");
            }
        }

        public void AddChapter(int chapterNumber, string title)
        {
            try
            {
                if (navigationTree.Nodes.Count == 0)
                {
                    InitializeTreeView();
                }

                var chaptersNode = navigationTree.Nodes[0];

                // Check for existing chapter with the same number
                foreach (TreeNode node in chaptersNode.Nodes)
                {
                    if (node.Tag is ChapterInfo existingChapter && existingChapter.Id == chapterNumber)
                    {
                        MessageBox.Show($"Chapter {chapterNumber} already exists.", "Duplicate Chapter", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var newChapterInfo = new ChapterInfo { Id = chapterNumber, Title = title };
                var newNodeText = $"Chapter {chapterNumber}: {title}";

                // Find the correct position to insert the new chapter
                int insertIndex = 0;
                while (insertIndex < chaptersNode.Nodes.Count)
                {
                    if (chaptersNode.Nodes[insertIndex].Tag is ChapterInfo existingChapter 
                        && existingChapter.Id > chapterNumber)
                    {
                        break;
                    }
                    insertIndex++;
                }

                // Insert the new node at the correct position
                var chapterNode = chaptersNode.Nodes.Insert(insertIndex, newNodeText);
                chapterNode.Tag = newChapterInfo;
                chaptersNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding chapter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    
    public class NavigationItemSelectedEventArgs : EventArgs
    {
        public string Title { get; }
        public object? Tag { get; }
        
        public NavigationItemSelectedEventArgs(string title, object? tag)
        {
            Title = title;
            Tag = tag;
        }
    }
    
    public class ChapterInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
} 