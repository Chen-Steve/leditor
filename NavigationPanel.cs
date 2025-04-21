using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class NavigationPanel : Panel
    {
        private readonly TreeView navigationTree;
        
        public event EventHandler<NavigationItemSelectedEventArgs>? ItemSelected;

        public NavigationPanel()
        {
            try
            {
                this.BackColor = Color.FromArgb(250, 250, 250);
                this.Dock = DockStyle.Fill;
                
                // Create header
                var titlePanel = new Panel
                {
                    Height = 56,
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(48, 48, 56)
                };
                
                var titleLabel = new Label
                {
                    Text = "PROJECT EXPLORER",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 11F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                
                titlePanel.Controls.Add(titleLabel);
                this.Controls.Add(titlePanel);

                // Add container panel with padding
                var contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5, 10, 5, 5)
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
                
                var addButton = new Button
                {
                    Text = "+",
                    Size = new Size(34, 34),
                    Location = new Point(12, 8),
                    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 70),
                    Cursor = Cursors.Hand
                };
                addButton.FlatAppearance.BorderSize = 1;
                addButton.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                addButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
                
                var removeButton = new Button
                {
                    Text = "-",
                    Size = new Size(34, 34),
                    Location = new Point(54, 8),
                    Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(60, 60, 70),
                    Cursor = Cursors.Hand
                };
                removeButton.FlatAppearance.BorderSize = 1;
                removeButton.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
                removeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
                
                // Add tooltips
                var addTooltip = new ToolTip();
                addTooltip.SetToolTip(addButton, "Add Item");
                
                var removeTooltip = new ToolTip();
                removeTooltip.SetToolTip(removeButton, "Remove Item");
                
                buttonPanel.Controls.Add(addButton);
                buttonPanel.Controls.Add(removeButton);
                this.Controls.Add(buttonPanel);
                
                // Initialize with demo content after control is fully created
                this.HandleCreated += (s, e) => PopulateTreeView();
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
        
        private void PopulateTreeView()
        {
            try
            {
                // Clear existing nodes first
                navigationTree.Nodes.Clear();
                
                // Add project root node
                TreeNode projectNode = navigationTree.Nodes.Add("My Novel");
                projectNode.ImageIndex = 0;
                
                // Add chapters section
                TreeNode chaptersNode = projectNode.Nodes.Add("Chapters");
                chaptersNode.Tag = "chapter_section";
                
                // Add sample chapters
                var chapter1 = chaptersNode.Nodes.Add("Chapter 1: The Beginning");
                chapter1.Tag = new ChapterInfo { Id = 1, Title = "The Beginning" };
                
                var chapter2 = chaptersNode.Nodes.Add("Chapter 2: The Journey");
                chapter2.Tag = new ChapterInfo { Id = 2, Title = "The Journey" };
                
                var chapter3 = chaptersNode.Nodes.Add("Chapter 3: The Revelation");
                chapter3.Tag = new ChapterInfo { Id = 3, Title = "The Revelation" };
                
                // Add characters section
                TreeNode charactersNode = projectNode.Nodes.Add("Characters");
                charactersNode.Tag = "character_section";
                
                var mainChar = charactersNode.Nodes.Add("Protagonist");
                mainChar.Tag = new CharacterInfo { Id = 1, Name = "Protagonist", Type = "Main" };
                
                var supportChar = charactersNode.Nodes.Add("Supporting Character");
                supportChar.Tag = new CharacterInfo { Id = 2, Name = "Supporting Character", Type = "Supporting" };
                
                var antagonistChar = charactersNode.Nodes.Add("Antagonist");
                antagonistChar.Tag = new CharacterInfo { Id = 3, Name = "Antagonist", Type = "Villain" };
                
                // Add notes section
                TreeNode notesNode = projectNode.Nodes.Add("Notes");
                notesNode.Tag = "notes_section";
                
                var worldBuilding = notesNode.Nodes.Add("World Building");
                worldBuilding.Tag = new NoteInfo { Id = 1, Title = "World Building" };
                
                var plotIdeas = notesNode.Nodes.Add("Plot Ideas");
                plotIdeas.Tag = new NoteInfo { Id = 2, Title = "Plot Ideas" };
                
                // Expand all nodes for initial view
                projectNode.Expand();
                chaptersNode.Expand();
                charactersNode.Expand();
                notesNode.Expand();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating tree view: {ex.Message}");
                // Continue without crashing
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
    
    public class CharacterInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
    
    public class NoteInfo
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
} 