using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace LightNovelEditor
{
    public partial class MainForm : Form
    {
        private EditorPanel? editorPanel;
        private NavigationPanel? navigationPanel;
        private CustomToolbar? toolbar;
        private string? currentFilePath;
        private SplitContainer? splitContainer;
        private bool isInitializing = true;
        private readonly ChapterManager chapterManager;
        private ChapterInfo? currentChapter;

        public MainForm()
        {
            chapterManager = new ChapterManager();
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
            // Create the toolbar
            toolbar = new CustomToolbar();
            toolbar.Dock = DockStyle.Top;
            this.Controls.Add(toolbar);

            // Create container panel for the split container
            var containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 56, 0, 0)
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
            if (editorPanel == null || navigationPanel == null || toolbar == null)
            {
                MessageBox.Show("Error: UI components not initialized properly.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            try
            {
                // Toolbar events
                toolbar.NewClicked += (s, e) => NewFile();
                toolbar.OpenClicked += (s, e) => OpenFile();
                toolbar.SaveClicked += (s, e) => SaveFile(false);
                toolbar.BoldClicked += (s, e) => editorPanel.ToggleBold();
                toolbar.ItalicClicked += (s, e) => editorPanel.ToggleItalic();
                toolbar.UnderlineClicked += (s, e) => editorPanel.ToggleUnderline();
                toolbar.FontClicked += (s, e) => FormatFont();
                toolbar.UploadClicked += (s, e) => UploadChapter();
                
                // Navigation panel events
                navigationPanel.ItemSelected += (s, e) => 
                {
                    if (e.Tag is ChapterInfo chapterInfo)
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

                navigationPanel.AddChapterRequested += (s, e) => ShowAddChapterDialog();
                
                // Form closing event - no need to prompt for saves anymore as content is auto-saved
                this.FormClosing += (s, e) =>
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Event wiring error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAddChapterDialog()
        {
            if (navigationPanel == null || editorPanel == null) return;

            using (var form = new Form())
            {
                form.Text = "Add New Chapter";
                form.Size = new Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var chapterNumberLabel = new Label
                {
                    Text = "Chapter Number:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                form.Controls.Add(chapterNumberLabel);

                var chapterNumberInput = new NumericUpDown
                {
                    Location = new Point(20, 40),
                    Width = 340,
                    Minimum = 1,
                    Maximum = 9999
                };
                form.Controls.Add(chapterNumberInput);

                var titleLabel = new Label
                {
                    Text = "Chapter Title:",
                    Location = new Point(20, 70),
                    AutoSize = true
                };
                form.Controls.Add(titleLabel);

                var titleInput = new TextBox
                {
                    Location = new Point(20, 90),
                    Width = 340
                };
                form.Controls.Add(titleInput);

                var addButton = new Button
                {
                    Text = "Add",
                    DialogResult = DialogResult.OK,
                    Location = new Point(180, 130),
                    Width = 80
                };
                form.Controls.Add(addButton);

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(280, 130),
                    Width = 80
                };
                form.Controls.Add(cancelButton);

                form.AcceptButton = addButton;
                form.CancelButton = cancelButton;

                if (form.ShowDialog() == DialogResult.OK)
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

                    var chapterNumber = (int)chapterNumberInput.Value;
                    var title = titleInput.Text.Trim();
                    
                    // Create the new chapter
                    navigationPanel.AddChapter(chapterNumber, title);
                    currentChapter = new ChapterInfo { Id = chapterNumber, Title = title };
                    editorPanel.Clear();
                    editorPanel.Text = "";
                    editorPanel.SetDocumentTitle($"Chapter {chapterNumber}: {title}");
                }
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

        private async void UploadChapter()
        {
            if (editorPanel == null) return;

            try
            {
                // Show login dialog if not authenticated
                if (string.IsNullOrEmpty(SupabaseConfig.AccessToken))
                {
                    using (var loginForm = new LoginForm())
                    {
                        if (loginForm.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }
                }

                // Get novel ID
                using (var form = new Form())
                {
                    form.Text = "Upload Chapter";
                    form.Size = new Size(400, 300);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var novelIdLabel = new Label
                    {
                        Text = "Novel ID:",
                        Location = new Point(20, 20),
                        AutoSize = true
                    };
                    form.Controls.Add(novelIdLabel);

                    var novelIdInput = new TextBox
                    {
                        Location = new Point(20, 40),
                        Width = 340
                    };
                    form.Controls.Add(novelIdInput);

                    var chapterNumberLabel = new Label
                    {
                        Text = "Chapter Number:",
                        Location = new Point(20, 70),
                        AutoSize = true
                    };
                    form.Controls.Add(chapterNumberLabel);

                    var chapterNumberInput = new NumericUpDown
                    {
                        Location = new Point(20, 90),
                        Width = 340,
                        Minimum = 1,
                        Maximum = 9999
                    };
                    form.Controls.Add(chapterNumberInput);

                    var titleLabel = new Label
                    {
                        Text = "Chapter Title:",
                        Location = new Point(20, 120),
                        AutoSize = true
                    };
                    form.Controls.Add(titleLabel);

                    var titleInput = new TextBox
                    {
                        Location = new Point(20, 140),
                        Width = 340
                    };
                    form.Controls.Add(titleInput);

                    var ageRatingLabel = new Label
                    {
                        Text = "Age Rating:",
                        Location = new Point(20, 170),
                        AutoSize = true
                    };
                    form.Controls.Add(ageRatingLabel);

                    var ageRatingCombo = new ComboBox
                    {
                        Location = new Point(20, 190),
                        Width = 340,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    ageRatingCombo.Items.AddRange(new[] { "EVERYONE", "TEEN", "MATURE", "ADULT" });
                    ageRatingCombo.SelectedIndex = 0;
                    form.Controls.Add(ageRatingCombo);

                    var uploadButton = new Button
                    {
                        Text = "Upload",
                        DialogResult = DialogResult.OK,
                        Location = new Point(180, 220),
                        Width = 80
                    };
                    form.Controls.Add(uploadButton);

                    var cancelButton = new Button
                    {
                        Text = "Cancel",
                        DialogResult = DialogResult.Cancel,
                        Location = new Point(280, 220),
                        Width = 80
                    };
                    form.Controls.Add(cancelButton);

                    form.AcceptButton = uploadButton;
                    form.CancelButton = cancelButton;

                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    var uploader = new ChapterUploader();
                    var request = new ChapterUploader.ChapterUploadRequest
                    {
                        ChapterNumber = (int)chapterNumberInput.Value,
                        Title = titleInput.Text.Trim(),
                        Content = editorPanel.Text.Trim(),
                        AgeRating = ageRatingCombo.SelectedItem?.ToString() ?? "EVERYONE"
                    };

                    var success = await uploader.UploadChapterAsync(novelIdInput.Text.Trim(), request);
                    if (success)
                    {
                        MessageBox.Show("Chapter uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during upload: {ex.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 