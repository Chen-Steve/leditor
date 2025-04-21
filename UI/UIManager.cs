using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public class UIManager
    {
        private readonly MainForm mainForm;
        private readonly ChapterManager chapterManager;
        private readonly MaterialSkinManager materialSkinManager;

        public CustomToolbar? Toolbar { get; private set; }
        public EditorPanel? EditorPanel { get; private set; }
        public NavigationPanel? NavigationPanel { get; private set; }
        public MaterialCard? ProfilePanel { get; private set; }
        public MaterialLabel? UsernameLabel { get; private set; }
        public SplitContainer? SplitContainer { get; private set; }

        public UIManager(MainForm form, ChapterManager chapterManager)
        {
            this.mainForm = form;
            this.chapterManager = chapterManager;
            this.materialSkinManager = MaterialSkinManager.Instance;
            
            InitializeMaterialSkin();
        }

        private void InitializeMaterialSkin()
        {
            materialSkinManager.AddFormToManage(mainForm);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue400,
                Primary.Blue500,
                Primary.Blue500,
                Accent.LightBlue200,
                TextShade.WHITE
            );
        }

        public void CreateUIComponents()
        {
            // Create the toolbar
            Toolbar = new CustomToolbar();
            Toolbar.Dock = DockStyle.Top;
            mainForm.Controls.Add(Toolbar);

            // Create profile panel inline with toolbar
            CreateProfilePanel();

            // Create container panel for the split container
            var containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 56, 0, 0)
            };
            mainForm.Controls.Add(containerPanel);
            
            // Create the split container with safe initial values
            SplitContainer = new SplitContainer
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
            NavigationPanel = new NavigationPanel(chapterManager);
            SplitContainer.Panel1.Controls.Add(NavigationPanel);
            SplitContainer.Panel1.Padding = new Padding(0);

            // Create and add the editor panel
            EditorPanel = new EditorPanel(chapterManager);
            SplitContainer.Panel2.Controls.Add(EditorPanel);
            SplitContainer.Panel2.Padding = new Padding(0);

            // Add the split container to its container panel
            containerPanel.Controls.Add(SplitContainer);

            // Now that the splitter has a real width, set up its constraints and initial distance
            SplitContainer.Panel1MinSize = 200;
            SplitContainer.Panel2MinSize = 400;

            // Clamp 280 into the [Panel1MinSize, Width-Panel2MinSize-SplitterWidth] range
            int desired = 280;
            int maxPossible = SplitContainer.Width - SplitContainer.Panel2MinSize - SplitContainer.SplitterWidth;
            int safeDistance = Math.Min(Math.Max(desired, SplitContainer.Panel1MinSize), maxPossible);
            SplitContainer.SplitterDistance = safeDistance;

            // Set proper Z-order
            containerPanel.BringToFront();
            Toolbar.BringToFront();
        }

        private void CreateProfilePanel()
        {
            // Create profile panel
            ProfilePanel = new MaterialCard
            {
                Size = new Size(200, 40),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            // Add username label with MaterialSkin styling
            UsernameLabel = new MaterialLabel
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

            ProfilePanel.Controls.Add(userIcon);
            ProfilePanel.Controls.Add(UsernameLabel);
            
            // Add the profile panel to the toolbar
            if (Toolbar != null)
            {
                Toolbar.AddProfilePanel(ProfilePanel);
            }
        }

        public void UpdateProfileUI()
        {
            if (UsernameLabel != null)
            {
                var newText = LoginForm.IsLoggedIn ? LoginForm.CurrentUsername ?? "Guest" : "Guest";
                UsernameLabel.Text = newText;
            }
        }

        public void HandleSplitterLayout()
        {
            if (SplitContainer != null)
            {
                try
                {
                    // Force layout to ensure all controls are properly sized
                    mainForm.PerformLayout();
                    Application.DoEvents();

                    // Get all the relevant measurements
                    int containerWidth = SplitContainer.Width;
                    int splitterWidth = SplitContainer.SplitterWidth;
                    int panel1Min = SplitContainer.Panel1MinSize;
                    int panel2Min = SplitContainer.Panel2MinSize;
                    int maxPossibleDistance = containerWidth - panel2Min - splitterWidth;

                    // Only proceed if we have valid dimensions
                    if (containerWidth > 0 && maxPossibleDistance > panel1Min)
                    {
                        // Start with panel1 at its minimum size
                        SplitContainer.SplitterDistance = panel1Min;
                        
                        // Then try to adjust to desired width
                        try
                        {
                            int desiredWidth = 280;
                            if (desiredWidth >= panel1Min && desiredWidth <= maxPossibleDistance)
                            {
                                SplitContainer.SplitterDistance = desiredWidth;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting desired width: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during layout: {ex.Message}", "Layout Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void HandleResize()
        {
            if (SplitContainer != null)
            {
                try
                {
                    // Get the current available width
                    int availableWidth = SplitContainer.Width;
                    
                    // Only adjust if we have enough width for both panels
                    if (availableWidth > (SplitContainer.Panel1MinSize + SplitContainer.Panel2MinSize + SplitContainer.SplitterWidth))
                    {
                        // Try to maintain current Panel1 width if possible
                        int currentPanel1Width = SplitContainer.SplitterDistance;
                        int maxPanel1Width = availableWidth - SplitContainer.Panel2MinSize - SplitContainer.SplitterWidth;
                        
                        // Ensure we're within bounds
                        int safeDistance = Math.Min(currentPanel1Width, maxPanel1Width);
                        safeDistance = Math.Max(safeDistance, SplitContainer.Panel1MinSize);
                        
                        if (SplitContainer.SplitterDistance != safeDistance)
                        {
                            SplitContainer.SplitterDistance = safeDistance;
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
} 