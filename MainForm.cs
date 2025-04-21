using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public partial class MainForm : Form
    {
        private EditorPanel? editorPanel;
        private NavigationPanel? navigationPanel;
        private CustomMenuBar? menuBar;
        private CustomToolbar? toolbar;
        private string? currentFilePath;
        private SplitContainer? splitContainer;
        private bool isInitializing = true;

        public MainForm()
        {
            try
            {
                // Basic form setup
                InitializeComponent();
                this.Text = "Light Novel Editor";
                this.MinimumSize = new Size(800, 600);
                this.Size = new Size(1280, 800);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Create UI components
                CreateUIComponents();

                // Wire up events
                this.Shown += MainForm_Shown;
                this.Resize += MainForm_Resize;
                WireUpEvents();

                try
                {
                    this.Icon = new Icon(SystemIcons.Application, 40, 40);
                }
                catch
                {
                    // Ignore icon error
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Initialization Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            try
            {
                if (splitContainer != null)
                {
                    // Force layout to ensure all controls are properly sized
                    this.PerformLayout();
                    Application.DoEvents();

                    // Get all the relevant measurements
                    int containerWidth = splitContainer.Width;
                    int splitterWidth = splitContainer.SplitterWidth;
                    int panel1Min = splitContainer.Panel1MinSize;
                    int panel2Min = splitContainer.Panel2MinSize;
                    int maxPossibleDistance = containerWidth - panel2Min - splitterWidth;

                    // Show the current values
                    string message = $"Container Width: {containerWidth}\n" +
                                   $"Splitter Width: {splitterWidth}\n" +
                                   $"Panel1 MinSize: {panel1Min}\n" +
                                   $"Panel2 MinSize: {panel2Min}\n" +
                                   $"Max Possible Distance: {maxPossibleDistance}\n" +
                                   $"Current SplitterDistance: {splitContainer.SplitterDistance}";
                    
                    MessageBox.Show(message, "Debug Info");

                    // Only proceed if we have valid dimensions
                    if (containerWidth > 0 && maxPossibleDistance > panel1Min)
                    {
                        // Start with panel1 at its minimum size
                        splitContainer.SplitterDistance = panel1Min;
                        
                        // Then try to adjust to desired width
                        try
                        {
                            int desiredWidth = 280;
                            if (desiredWidth >= panel1Min && desiredWidth <= maxPossibleDistance)
                            {
                                splitContainer.SplitterDistance = desiredWidth;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error setting desired width: {ex.Message}\n\n" +
                                          $"Attempted to set SplitterDistance to: {280}\n" +
                                          $"Valid range is: {panel1Min} to {maxPossibleDistance}",
                                          "Splitter Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Invalid dimensions for splitter:\n{message}", "Layout Error");
                    }
                }

                // Focus the editor
                editorPanel?.Focus();
                
                isInitializing = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during form shown: {ex.Message}", "Layout Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (!isInitializing && splitContainer != null)
            {
                try
                {
                    // Get the current available width
                    int availableWidth = splitContainer.Width;
                    
                    // Only adjust if we have enough width for both panels
                    if (availableWidth > (splitContainer.Panel1MinSize + splitContainer.Panel2MinSize + splitContainer.SplitterWidth))
                    {
                        // Try to maintain current Panel1 width if possible
                        int currentPanel1Width = splitContainer.SplitterDistance;
                        int maxPanel1Width = availableWidth - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
                        
                        // Ensure we're within bounds
                        int safeDistance = Math.Min(currentPanel1Width, maxPanel1Width);
                        safeDistance = Math.Max(safeDistance, splitContainer.Panel1MinSize);
                        
                        if (splitContainer.SplitterDistance != safeDistance)
                        {
                            splitContainer.SplitterDistance = safeDistance;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't show message box to avoid annoying the user
                    Console.WriteLine($"Error adjusting splitter: {ex.Message}");
                }
            }
        }

        private void CreateUIComponents()
        {
            // Create the menu bar
            menuBar = new CustomMenuBar();
            menuBar.Dock = DockStyle.Top;
            this.Controls.Add(menuBar);
            
            // Create the toolbar
            toolbar = new CustomToolbar();
            toolbar.Dock = DockStyle.Top;
            this.Controls.Add(toolbar);

            // Create container panel for the split container
            var containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            this.Controls.Add(containerPanel);
            
            // Create the split container with safe initial values
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                FixedPanel = FixedPanel.Panel1,
                IsSplitterFixed = false,
                Orientation = Orientation.Vertical
            };

            // Create and add the navigation panel
            navigationPanel = new NavigationPanel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(200, 0)
            };
            splitContainer.Panel1.Controls.Add(navigationPanel);

            // Create and add the editor panel
            editorPanel = new EditorPanel
            {
                Dock = DockStyle.Fill,
                MinimumSize = new Size(400, 0)
            };
            splitContainer.Panel2.Controls.Add(editorPanel);

            // Add the split container to its container panel
            containerPanel.Controls.Add(splitContainer);

            // Now that the splitter has a real width, set up its constraints and initial distance
            splitContainer.Panel1MinSize = 200;
            splitContainer.Panel2MinSize = 400;
            splitContainer.SplitterWidth = 1;

            // Clamp 280 into the [Panel1MinSize, Width-Panel2MinSize-SplitterWidth] range
            int desired = 280;
            int maxPossible = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
            int safeDistance = Math.Min(Math.Max(desired, splitContainer.Panel1MinSize), maxPossible);
            splitContainer.SplitterDistance = safeDistance;

            // Set proper Z-order
            containerPanel.BringToFront();
            toolbar.BringToFront();
            menuBar.BringToFront();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1280, 800);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
        
        private void WireUpEvents()
        {
            if (menuBar == null || editorPanel == null || navigationPanel == null || toolbar == null)
            {
                MessageBox.Show("Error: UI components not initialized properly.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                // Menu events
                menuBar.NewClicked += (s, e) => NewFile();
                menuBar.OpenClicked += (s, e) => OpenFile();
                menuBar.SaveClicked += (s, e) => SaveFile(false);
                menuBar.SaveAsClicked += (s, e) => SaveFile(true);
                menuBar.ExitClicked += (s, e) => Application.Exit();
                
                menuBar.UndoClicked += (s, e) => editorPanel.Undo();
                menuBar.RedoClicked += (s, e) => editorPanel.Redo();
                menuBar.CutClicked += (s, e) => editorPanel.Cut();
                menuBar.CopyClicked += (s, e) => editorPanel.Copy();
                menuBar.PasteClicked += (s, e) => editorPanel.Paste();
                menuBar.SelectAllClicked += (s, e) => editorPanel.SelectAll();
                
                menuBar.FontClicked += (s, e) => FormatFont();
                menuBar.BoldClicked += (s, e) => editorPanel.ToggleBold();
                menuBar.ItalicClicked += (s, e) => editorPanel.ToggleItalic();
                menuBar.UnderlineClicked += (s, e) => editorPanel.ToggleUnderline();
                
                // Toolbar events
                toolbar.NewClicked += (s, e) => NewFile();
                toolbar.OpenClicked += (s, e) => OpenFile();
                toolbar.SaveClicked += (s, e) => SaveFile(false);
                toolbar.BoldClicked += (s, e) => editorPanel.ToggleBold();
                toolbar.ItalicClicked += (s, e) => editorPanel.ToggleItalic();
                toolbar.UnderlineClicked += (s, e) => editorPanel.ToggleUnderline();
                toolbar.FontClicked += (s, e) => FormatFont();
                
                // Navigation panel events
                navigationPanel.ItemSelected += (s, e) => 
                {
                    if (e.Tag is ChapterInfo chapterInfo)
                    {
                        editorPanel.SetDocumentTitle($"Chapter {chapterInfo.Id}: {chapterInfo.Title}");
                        // In a real app, we would load the chapter content here
                    }
                    else if (e.Tag is CharacterInfo characterInfo)
                    {
                        editorPanel.SetDocumentTitle($"Character: {characterInfo.Name}");
                        // In a real app, we would load the character information here
                    }
                    else if (e.Tag is NoteInfo noteInfo)
                    {
                        editorPanel.SetDocumentTitle($"Note: {noteInfo.Title}");
                        // In a real app, we would load the note content here
                    }
                };
                
                // Form closing event
                this.FormClosing += (s, e) =>
                {
                    if (editorPanel.Modified)
                    {
                        var result = MessageBox.Show("Do you want to save changes before closing?", 
                            "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            SaveFile(false);
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            e.Cancel = true;
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Event wiring error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewFile()
        {
            if (editorPanel == null) return;
            
            if (editorPanel.Modified)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Confirmation",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile(false);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            editorPanel.Clear();
            editorPanel.SetDocumentTitle("Untitled Document");
            currentFilePath = null;
            this.Text = "Light Novel Editor";
        }

        private void OpenFile()
        {
            if (editorPanel == null) return;
            
            if (editorPanel.Modified)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Confirmation",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile(false);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Open Document";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        editorPanel.LoadFromFile(openFileDialog.FileName);
                        currentFilePath = openFileDialog.FileName;
                        editorPanel.SetDocumentTitle(Path.GetFileName(currentFilePath));
                        this.Text = $"Light Novel Editor - {Path.GetFileName(currentFilePath)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveFile(bool saveAs)
        {
            if (editorPanel == null) return;
            
            if (currentFilePath == null || saveAs)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Text Files (*.txt)|*.txt";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.DefaultExt = "rtf";
                    saveFileDialog.Title = "Save Document";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentFilePath = saveFileDialog.FileName;
                        editorPanel.SetDocumentTitle(Path.GetFileName(currentFilePath));
                        this.Text = $"Light Novel Editor - {Path.GetFileName(currentFilePath)}";
                    }
                    else
                    {
                        return;
                    }
                }
            }

            editorPanel.SaveToFile(currentFilePath!);
        }

        private void FormatFont()
        {
            if (editorPanel == null) return;
            
            using (FontDialog fontDialog = new FontDialog())
            {
                fontDialog.Font = editorPanel.SelectionFont;
                fontDialog.ShowColor = true;
                fontDialog.Color = editorPanel.SelectionColor;

                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    editorPanel.SelectionFont = fontDialog.Font;
                    editorPanel.SelectionColor = fontDialog.Color;
                }
            }
        }
    }
} 