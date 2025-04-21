using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class EditorPanel : Panel
    {
        private readonly RichTextBox richTextBox;
        private readonly Label headerLabel;
        
        public event EventHandler? ContentChanged;
        
        public EditorPanel()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);
            
            // Create a container panel with shadow effect
            var containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            
            // Create the rich text editor
            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12F),
                AcceptsTab = true,
                HideSelection = false,
                BackColor = Color.White,
                Padding = new Padding(10),
                ForeColor = Color.FromArgb(60, 60, 60)
            };
            
            richTextBox.TextChanged += (s, e) => ContentChanged?.Invoke(this, EventArgs.Empty);
            
            // Add a header with the document name
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None
            };
            
            headerLabel = new Label
            {
                Text = "Untitled Document",
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 0),
                Size = new Size(400, 50)
            };
            
            headerPanel.Controls.Add(headerLabel);
            containerPanel.Controls.Add(headerPanel);
            
            // Add a 1px border at the bottom of the header
            var headerDivider = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            containerPanel.Controls.Add(headerDivider);
            
            // Add padding panel
            var paddingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.White
            };
            paddingPanel.Controls.Add(richTextBox);
            containerPanel.Controls.Add(paddingPanel);
            
            this.Controls.Add(containerPanel);
        }
        
        public bool Modified => richTextBox.Modified;
        
        public Font SelectionFont
        {
            get => richTextBox.SelectionFont ?? richTextBox.Font;
            set => richTextBox.SelectionFont = value;
        }
        
        public Color SelectionColor
        {
            get => richTextBox.SelectionColor;
            set => richTextBox.SelectionColor = value;
        }
        
        public new string Text
        {
            get => richTextBox.Text;
            set => richTextBox.Text = value;
        }
        
        public void Clear()
        {
            richTextBox.Clear();
        }
        
        public void Undo()
        {
            if (richTextBox.CanUndo)
                richTextBox.Undo();
        }
        
        public void Redo()
        {
            if (richTextBox.CanRedo)
                richTextBox.Redo();
        }
        
        public void Cut()
        {
            richTextBox.Cut();
        }
        
        public void Copy()
        {
            richTextBox.Copy();
        }
        
        public void Paste()
        {
            richTextBox.Paste();
        }
        
        public void SelectAll()
        {
            richTextBox.SelectAll();
        }
        
        public void ToggleBold()
        {
            Font currentFont = SelectionFont;
            if (currentFont != null)
            {
                FontStyle newStyle = currentFont.Style ^ FontStyle.Bold;
                SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            }
        }
        
        public void ToggleItalic()
        {
            Font currentFont = SelectionFont;
            if (currentFont != null)
            {
                FontStyle newStyle = currentFont.Style ^ FontStyle.Italic;
                SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            }
        }
        
        public void ToggleUnderline()
        {
            Font currentFont = SelectionFont;
            if (currentFont != null)
            {
                FontStyle newStyle = currentFont.Style ^ FontStyle.Underline;
                SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            }
        }
        
        public void SaveToFile(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".rtf")
                {
                    richTextBox.SaveFile(filePath, RichTextBoxStreamType.RichText);
                }
                else
                {
                    File.WriteAllText(filePath, richTextBox.Text);
                }
                richTextBox.Modified = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void LoadFromFile(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".rtf")
                {
                    richTextBox.LoadFile(filePath, RichTextBoxStreamType.RichText);
                }
                else
                {
                    richTextBox.Text = File.ReadAllText(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void SetDocumentTitle(string title)
        {
            headerLabel.Text = title;
        }
    }
} 