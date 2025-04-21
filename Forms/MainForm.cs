using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public partial class MainForm : MaterialForm
    {
        private readonly ChapterManager? chapterManager;
        private readonly UIManager? uiManager;
        private readonly FileManager? fileManager;
        private readonly DialogManager? dialogManager;
        private readonly MainFormEventHandler? eventHandler;

        public MainForm()
        {
            try
            {
                // Basic form setup
                InitializeComponent();
                this.Text = "Lanry Editor";
                this.MinimumSize = new Size(800, 600);
                this.Size = new Size(1280, 800);
                this.StartPosition = FormStartPosition.CenterScreen;

                // Initialize managers
                chapterManager = new ChapterManager();
                uiManager = new UIManager(this, chapterManager);

                // Create UI components
                uiManager.CreateUIComponents();

                // Show login dialog
                using (var loginForm = new LoginForm())
                {
                    loginForm.ShowDialog();
                    if (loginForm.DialogResult == DialogResult.OK && !loginForm.SkippedLogin)
                    {
                        // Ensure we update UI only after successful login
                        if (LoginForm.IsLoggedIn && !string.IsNullOrEmpty(LoginForm.CurrentUsername))
                        {
                            uiManager.UpdateProfileUI();
                        }
                    }
                }

                // Initialize remaining managers with the created UI components
                fileManager = new FileManager(uiManager.EditorPanel!);
                dialogManager = new DialogManager(chapterManager, uiManager.EditorPanel!, uiManager.NavigationPanel!);
                eventHandler = new MainFormEventHandler(
                    this,
                    uiManager.EditorPanel!,
                    uiManager.NavigationPanel!,
                    uiManager.Toolbar!,
                    chapterManager,
                    fileManager,
                    dialogManager,
                    uiManager
                );

                // Wire up events
                eventHandler.WireUpEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Initialization Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
} 