using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class CustomToolbar : Panel
    {
        // Events for toolbar actions
        public event EventHandler? NewClicked;
        public event EventHandler? OpenClicked;
        public event EventHandler? SaveClicked;
        public event EventHandler? BoldClicked;
        public event EventHandler? ItalicClicked;
        public event EventHandler? UnderlineClicked;
        public event EventHandler? FontClicked;

        public CustomToolbar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Height = 56;
            this.Dock = DockStyle.Top;
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.Padding = new Padding(16, 10, 16, 10);
            this.BorderStyle = BorderStyle.None;

            // Add a bottom border
            var bottomBorder = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            this.Controls.Add(bottomBorder);

            // Container panel for layout
            var container = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            this.Controls.Add(container);

            // File operations group
            var fileGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0)
            };
            
            var newButton = CreateToolbarButton("New Document", "📝");
            newButton.Width = 40;
            newButton.Click += (s, e) => NewClicked?.Invoke(this, EventArgs.Empty);
            
            var openButton = CreateToolbarButton("Open File", "📂");
            openButton.Width = 40;
            openButton.Click += (s, e) => OpenClicked?.Invoke(this, EventArgs.Empty);
            
            var saveButton = CreateToolbarButton("Save", "💾");
            saveButton.Width = 40;
            saveButton.Click += (s, e) => SaveClicked?.Invoke(this, EventArgs.Empty);
            
            fileGroup.Controls.Add(newButton);
            fileGroup.Controls.Add(openButton);
            fileGroup.Controls.Add(saveButton);
            
            // Divider
            var divider = new Panel
            {
                Width = 1,
                Height = 36,
                BackColor = Color.FromArgb(230, 230, 230),
                Margin = new Padding(16, 0, 16, 0)
            };
            
            // Format operations group
            var formatGroup = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0)
            };
            
            var boldButton = CreateToolbarButton("Bold", "B");
            boldButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            boldButton.Click += (s, e) => BoldClicked?.Invoke(this, EventArgs.Empty);
            
            var italicButton = CreateToolbarButton("Italic", "I");
            italicButton.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
            italicButton.Click += (s, e) => ItalicClicked?.Invoke(this, EventArgs.Empty);
            
            var underlineButton = CreateToolbarButton("Underline", "U");
            underlineButton.Font = new Font("Segoe UI", 11F, FontStyle.Underline);
            underlineButton.Click += (s, e) => UnderlineClicked?.Invoke(this, EventArgs.Empty);
            
            var fontButton = CreateToolbarButton("Font", "Aa");
            fontButton.Width = 40;
            fontButton.Click += (s, e) => FontClicked?.Invoke(this, EventArgs.Empty);
            
            formatGroup.Controls.Add(boldButton);
            formatGroup.Controls.Add(italicButton);
            formatGroup.Controls.Add(underlineButton);
            formatGroup.Controls.Add(fontButton);
            
            container.Controls.Add(fileGroup);
            container.Controls.Add(divider);
            container.Controls.Add(formatGroup);
        }
        
        private Button CreateToolbarButton(string tooltip, string text)
        {
            var button = new Button
            {
                Text = text,
                Width = 36,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(60, 60, 70),
                Margin = new Padding(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = true
            };
            
            button.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);
            
            // Add tooltip
            var toolTip = new ToolTip();
            toolTip.SetToolTip(button, tooltip);
            
            return button;
        }
    }
} 