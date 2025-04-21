using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public class CustomToolbar : MaterialCard
    {
        // Events for toolbar actions
        public event EventHandler? NewClicked;
        public event EventHandler? OpenClicked;
        public event EventHandler? SaveClicked;
        public event EventHandler? BoldClicked;
        public event EventHandler? ItalicClicked;
        public event EventHandler? UnderlineClicked;
        public event EventHandler? FontClicked;
        public event EventHandler? UploadClicked;
        
        private FlowLayoutPanel container = new FlowLayoutPanel();

        public CustomToolbar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Height = 56;
            this.Dock = DockStyle.Top;
            this.Depth = 0;
            this.MouseState = MaterialSkin.MouseState.HOVER;
            this.Padding = new Padding(16, 8, 16, 8); // Normal padding, we'll position profile panel differently

            // Add bottom border
            this.Paint += (s, e) =>
            {
                // Draw a subtle border at the bottom
                using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
                {
                    e.Graphics.DrawLine(pen, 0, this.Height - 1, this.Width, this.Height - 1);
                }
            };

            // Container panel for layout
            container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0)
            };
            this.Controls.Add(container);

            // File operations group
            var fileGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            
            var newButton = CreateToolbarButton("New Document (Ctrl+N)", "📄", MaterialButton.MaterialButtonType.Outlined);
            var openButton = CreateToolbarButton("Open File (Ctrl+O)", "📂", MaterialButton.MaterialButtonType.Outlined);
            var saveButton = CreateToolbarButton("Save (Ctrl+S)", "💾", MaterialButton.MaterialButtonType.Outlined);

            newButton.Click += (s, e) => NewClicked?.Invoke(this, EventArgs.Empty);
            openButton.Click += (s, e) => OpenClicked?.Invoke(this, EventArgs.Empty);
            saveButton.Click += (s, e) => SaveClicked?.Invoke(this, EventArgs.Empty);
            
            fileGroup.Controls.Add(newButton);
            fileGroup.Controls.Add(openButton);
            fileGroup.Controls.Add(saveButton);
            
            // Divider
            var divider1 = new MaterialDivider
            {
                Height = 36,
                Margin = new Padding(8, 0, 8, 0)
            };
            
            // Format operations group
            var formatGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            
            var boldButton = CreateToolbarButton("Bold (Ctrl+B)", "B", MaterialButton.MaterialButtonType.Outlined);
            boldButton.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            
            var italicButton = CreateToolbarButton("Italic (Ctrl+I)", "I", MaterialButton.MaterialButtonType.Outlined);
            italicButton.Font = new Font("Segoe UI", 16F, FontStyle.Italic);
            
            var underlineButton = CreateToolbarButton("Underline (Ctrl+U)", "U", MaterialButton.MaterialButtonType.Outlined);
            underlineButton.Font = new Font("Segoe UI", 16F, FontStyle.Underline);
            
            var fontButton = CreateToolbarButton("Font Settings", "Aa", MaterialButton.MaterialButtonType.Outlined);
            fontButton.Font = new Font("Segoe UI", 16F);
            
            boldButton.Click += (s, e) => BoldClicked?.Invoke(this, EventArgs.Empty);
            italicButton.Click += (s, e) => ItalicClicked?.Invoke(this, EventArgs.Empty);
            underlineButton.Click += (s, e) => UnderlineClicked?.Invoke(this, EventArgs.Empty);
            fontButton.Click += (s, e) => FontClicked?.Invoke(this, EventArgs.Empty);
            
            formatGroup.Controls.Add(boldButton);
            formatGroup.Controls.Add(italicButton);
            formatGroup.Controls.Add(underlineButton);
            formatGroup.Controls.Add(fontButton);

            // Divider
            var divider2 = new MaterialDivider
            {
                Height = 36,
                Margin = new Padding(8, 0, 8, 0)
            };

            // Upload group
            var uploadGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            var uploadButton = CreateToolbarButton("Upload Chapter to Website", "⬆️", MaterialButton.MaterialButtonType.Contained);
            uploadButton.Click += (s, e) => UploadClicked?.Invoke(this, EventArgs.Empty);
            uploadGroup.Controls.Add(uploadButton);
            
            container.Controls.Add(fileGroup);
            container.Controls.Add(divider1);
            container.Controls.Add(formatGroup);
            container.Controls.Add(divider2);
            container.Controls.Add(uploadGroup);
        }
        
        private MaterialButton CreateToolbarButton(string tooltip, string text, MaterialButton.MaterialButtonType buttonType)
        {
            var button = new MaterialButton
            {
                Text = text,
                Type = buttonType,
                Size = new Size(40, 40),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                UseAccentColor = false,
                AutoSize = false,
                Margin = new Padding(2),
                HighEmphasis = buttonType == MaterialButton.MaterialButtonType.Contained,
                Font = new Font("Segoe UI", 16F, FontStyle.Regular)
            };
            
            var toolTip = new ToolTip
            {
                InitialDelay = 200,
                ShowAlways = true
            };
            toolTip.SetToolTip(button, tooltip);
            
            return button;
        }
        
        // Method to add profile panel to the toolbar
        public void AddProfilePanel(Control profilePanel)
        {
            // Create a panel to push contents to the right
            var spacer = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Right,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            
            // Create a panel for the profile on the right
            var profileContainer = new Panel
            {
                Dock = DockStyle.Right,
                Size = new Size(profilePanel.Width, this.Height),
                Padding = new Padding(0, 8, 16, 8),
                BackColor = Color.Transparent
            };
            
            // Add the profile panel to its container
            profilePanel.Location = new Point(0, 0);
            profileContainer.Controls.Add(profilePanel);
            
            // Add the profile container to the toolbar
            this.Controls.Add(profileContainer);
        }
    }
} 