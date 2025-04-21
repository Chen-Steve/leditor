using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public class DialogManager
    {
        private readonly ChapterManager chapterManager;
        private readonly EditorPanel editorPanel;
        private readonly NavigationPanel navigationPanel;

        public DialogManager(ChapterManager chapterManager, EditorPanel editorPanel, NavigationPanel navigationPanel)
        {
            this.chapterManager = chapterManager ?? throw new ArgumentNullException(nameof(chapterManager));
            this.editorPanel = editorPanel ?? throw new ArgumentNullException(nameof(editorPanel));
            this.navigationPanel = navigationPanel ?? throw new ArgumentNullException(nameof(navigationPanel));
        }

        public void ShowAddChapterDialog(ref ChapterInfo? currentChapter)
        {
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

        public async Task<bool> ShowUploadChapterDialog()
        {
            try
            {
                // Show login dialog if not authenticated
                if (!LoginForm.IsLoggedIn)
                {
                    using (var loginForm = new LoginForm())
                    {
                        if (loginForm.ShowDialog() != DialogResult.OK || loginForm.SkippedLogin)
                        {
                            return false;
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
                        return false;
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
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Error during upload: {ex.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 