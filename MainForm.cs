using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public partial class MainForm : MaterialForm
    {
        private EditorPanel? editorPanel;
        private NavigationPanel? navigationPanel;
        private CustomToolbar? toolbar;
        private MaterialCard? profilePanel;
        private MaterialLabel? usernameLabel;
        private string? currentFilePath;
        private SplitContainer? splitContainer;
        private bool isInitializing = true;
        private readonly ChapterManager chapterManager;
        private ChapterInfo? currentChapter;
        private readonly MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;

        public MainForm()
        {
            chapterManager = new ChapterManager();
            try
            {
                // Initialize MaterialSkinManager
                materialSkinManager.AddFormToManage(this);
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                materialSkinManager.ColorScheme = new ColorScheme(
                    Primary.Blue400,
                    Primary.Blue500,
                    Primary.Blue500,
                    Accent.LightBlue200,
                    TextShade.WHITE
                );

                // Basic form setup
                InitializeComponent();
                this.Text = "Lanry Editor";
                this.MinimumSize = new Size(800, 600);
                this.Size = new Size(1280, 800);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Show login dialog
                using (var loginForm = new LoginForm())
                {
                    loginForm.ShowDialog();
                    if (loginForm.DialogResult == DialogResult.OK && !loginForm.SkippedLogin)
                    {
                        // Ensure we update UI only after successful login
                        if (LoginForm.IsLoggedIn && !string.IsNullOrEmpty(LoginForm.CurrentUsername))
                        {
                            UpdateProfileUI();
                        }
                    }
                }

                // Create UI components
                CreateUIComponents();

                // Wire up events
                this.Shown += MainForm_Shown;
                this.Resize += MainForm_Resize;
                WireUpEvents();
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
                            Console.WriteLine($"Error setting desired width: {ex.Message}");
                        }
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
            if (!isInitializing)
            {
                if (splitContainer != null)
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
        }

        private void CreateUIComponents()
        {
            // Create the toolbar
            toolbar = new CustomToolbar();
            toolbar.Dock = DockStyle.Top;
            this.Controls.Add(toolbar);

            // Create profile panel inline with toolbar
            CreateProfilePanel();

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
                Orientation = Orientation.Vertical,
                SplitterWidth = 1,
                BackColor = MaterialSkinManager.Instance.ColorScheme.PrimaryColor.Lighten(0.9f)
            };

            // Create and add the navigation panel
            navigationPanel = new NavigationPanel(chapterManager);
            splitContainer.Panel1.Controls.Add(navigationPanel);
            splitContainer.Panel1.Padding = new Padding(0);

            // Create and add the editor panel
            editorPanel = new EditorPanel(chapterManager);
            splitContainer.Panel2.Controls.Add(editorPanel);
            splitContainer.Panel2.Padding = new Padding(0);

            // Add the split container to its container panel
            containerPanel.Controls.Add(splitContainer);

            // Now that the splitter has a real width, set up its constraints and initial distance
            splitContainer.Panel1MinSize = 200;
            splitContainer.Panel2MinSize = 400;

            // Clamp 280 into the [Panel1MinSize, Width-Panel2MinSize-SplitterWidth] range
            int desired = 280;
            int maxPossible = splitContainer.Width - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;
            int safeDistance = Math.Min(Math.Max(desired, splitContainer.Panel1MinSize), maxPossible);
            splitContainer.SplitterDistance = safeDistance;

            // Set proper Z-order
            containerPanel.BringToFront();
            toolbar.BringToFront();
        }

        private void CreateProfilePanel()
        {
            // Create profile panel
            profilePanel = new MaterialCard
            {
                Size = new Size(200, 40),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            // Add username label with MaterialSkin styling
            usernameLabel = new MaterialLabel
            {
                AutoSize = false,
                Size = new Size(160, 40),
                Location = new Point(40, 0),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                Text = LoginForm.IsLoggedIn ? LoginForm.CurrentUsername ?? "Guest" : "Guest",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular)
            };

            // Add user icon
            var userIcon = new MaterialLabel
            {
                AutoSize = false,
                Size = new Size(40, 40),
                Location = new Point(0, 0),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                Text = "👤",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F)
            };

            profilePanel.Controls.Add(userIcon);
            profilePanel.Controls.Add(usernameLabel);
            
            // Add the profile panel to the toolbar
            if (toolbar != null)
            {
                toolbar.AddProfilePanel(profilePanel);
            }
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

            using (var form = new MaterialForm())
            {
                // Apply MaterialSkin to the dialog
                MaterialSkinManager.Instance.AddFormToManage(form);
                
                form.Text = "Add New Chapter";
                form.Size = new Size(400, 250);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var chapterNumberLabel = new MaterialLabel
                {
                    Text = "Chapter Number:",
                    Location = new Point(20, 70),
                    AutoSize = true,
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(chapterNumberLabel);

                var chapterNumberInput = new MaterialTextBox
                {
                    Location = new Point(20, 100),
                    Width = 340,
                    Text = "1",
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(chapterNumberInput);

                var titleLabel = new MaterialLabel
                {
                    Text = "Chapter Title:",
                    Location = new Point(20, 130),
                    AutoSize = true,
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(titleLabel);

                var titleInput = new MaterialTextBox
                {
                    Location = new Point(20, 160),
                    Width = 340,
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(titleInput);

                var addButton = new MaterialButton
                {
                    Text = "Add",
                    Type = MaterialButton.MaterialButtonType.Contained,
                    Location = new Point(180, 200),
                    Width = 80,
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(addButton);

                var cancelButton = new MaterialButton
                {
                    Text = "Cancel",
                    Type = MaterialButton.MaterialButtonType.Outlined,
                    Location = new Point(280, 200),
                    Width = 80,
                    Depth = 0,
                    MouseState = MaterialSkin.MouseState.HOVER
                };
                form.Controls.Add(cancelButton);

                addButton.Click += (s, e) => form.DialogResult = DialogResult.OK;
                cancelButton.Click += (s, e) => form.DialogResult = DialogResult.Cancel;

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

                    if (int.TryParse(chapterNumberInput.Text, out int chapterNumber))
                    {
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
            this.Text = "Lanry Editor";
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
                        this.Text = $"Lanry Editor - {Path.GetFileName(currentFilePath)}";
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
                        this.Text = $"Lanry Editor - {Path.GetFileName(currentFilePath)}";
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
                if (!LoginForm.IsLoggedIn)
                {
                    using (var loginForm = new LoginForm())
                    {
                        if (loginForm.ShowDialog() != DialogResult.OK || loginForm.SkippedLogin)
                        {
                            return;
                        }
                    }
                }

                // Get novel ID
                using (var form = new MaterialForm())
                {
                    MaterialSkinManager.Instance.AddFormToManage(form);
                    
                    form.Text = "Upload Chapter";
                    form.Size = new Size(400, 400);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var novelIdLabel = new MaterialLabel
                    {
                        Text = "Novel ID:",
                        Location = new Point(20, 70),
                        AutoSize = true,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(novelIdLabel);

                    var novelIdInput = new MaterialTextBox
                    {
                        Location = new Point(20, 100),
                        Width = 340,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(novelIdInput);

                    var chapterNumberLabel = new MaterialLabel
                    {
                        Text = "Chapter Number:",
                        Location = new Point(20, 140),
                        AutoSize = true,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(chapterNumberLabel);

                    var chapterNumberInput = new MaterialTextBox
                    {
                        Location = new Point(20, 170),
                        Width = 340,
                        Text = "1",
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(chapterNumberInput);

                    var titleLabel = new MaterialLabel
                    {
                        Text = "Chapter Title:",
                        Location = new Point(20, 210),
                        AutoSize = true,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(titleLabel);

                    var titleInput = new MaterialTextBox
                    {
                        Location = new Point(20, 240),
                        Width = 340,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(titleInput);

                    var ageRatingLabel = new MaterialLabel
                    {
                        Text = "Age Rating:",
                        Location = new Point(20, 280),
                        AutoSize = true,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(ageRatingLabel);

                    var ageRatingCombo = new MaterialComboBox
                    {
                        Location = new Point(20, 310),
                        Width = 340,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    ageRatingCombo.Items.AddRange(new[] { "EVERYONE", "TEEN", "MATURE", "ADULT" });
                    ageRatingCombo.SelectedIndex = 0;
                    form.Controls.Add(ageRatingCombo);

                    var uploadButton = new MaterialButton
                    {
                        Text = "Upload",
                        Type = MaterialButton.MaterialButtonType.Contained,
                        Location = new Point(180, 350),
                        Width = 80,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(uploadButton);

                    var cancelButton = new MaterialButton
                    {
                        Text = "Cancel",
                        Type = MaterialButton.MaterialButtonType.Outlined,
                        Location = new Point(280, 350),
                        Width = 80,
                        Depth = 0,
                        MouseState = MaterialSkin.MouseState.HOVER
                    };
                    form.Controls.Add(cancelButton);

                    uploadButton.Click += (s, e) => form.DialogResult = DialogResult.OK;
                    cancelButton.Click += (s, e) => form.DialogResult = DialogResult.Cancel;

                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    if (int.TryParse(chapterNumberInput.Text, out int chapterNumber))
                    {
                        var uploader = new ChapterUploader();
                        var request = new ChapterUploader.ChapterUploadRequest
                        {
                            ChapterNumber = chapterNumber,
                            Title = titleInput.Text.Trim(),
                            Content = editorPanel.Text.Trim(),
                            AgeRating = ageRatingCombo.SelectedItem?.ToString() ?? "EVERYONE"
                        };

                        var success = await uploader.UploadChapterAsync(novelIdInput.Text.Trim(), request);
                        if (success)
                        {
                            MaterialMessageBox.Show("Chapter uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Error during upload: {ex.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateProfileUI()
        {
            if (usernameLabel != null)
            {
                var newText = LoginForm.IsLoggedIn ? LoginForm.CurrentUsername ?? "Guest" : "Guest";
                usernameLabel.Text = newText;
            }
        }
    }
} 