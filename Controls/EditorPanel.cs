using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using MaterialSkin;
using MaterialSkin.Controls;

namespace LightNovelEditor
{
    public class EditorPanel : MaterialCard
    {
        private readonly RichTextBox richTextBox;
        private readonly MaterialLabel headerLabel;
        private readonly MaterialLabel wordCountLabel;
        private readonly ChapterManager chapterManager;
        
        public event EventHandler? ContentChanged;
        
        public EditorPanel(ChapterManager chapterManager)
        {
            this.chapterManager = chapterManager;
            this.Dock = DockStyle.Fill;
            this.Depth = 0;
            this.MouseState = MaterialSkin.MouseState.HOVER;
            this.Padding = new Padding(20);
            
            // Create a container panel
            var containerPanel = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Depth = 1,
                MouseState = MaterialSkin.MouseState.HOVER,
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
                ForeColor = Color.FromArgb(33, 33, 33),
                Padding = new Padding(10)
            };
            
            richTextBox.TextChanged += (s, e) => {
                ContentChanged?.Invoke(this, EventArgs.Empty);
                UpdateWordCount();
            };
            
            richTextBox.KeyDown += (s, e) => {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.B:
                            e.SuppressKeyPress = true;
                            ToggleBold();
                            break;
                        case Keys.I:
                            e.SuppressKeyPress = true;
                            ToggleItalic();
                            break;
                        case Keys.U:
                            e.SuppressKeyPress = true;
                            ToggleUnderline();
                            break;
                    }
                }
            };
            
            // Add a header with the document name
            var headerPanel = new MaterialCard
            {
                Dock = DockStyle.Top,
                Height = 50,
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            
            headerLabel = new MaterialLabel
            {
                Text = "Untitled Document",
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 0),
                Size = new Size(400, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            
            // Add word count label
            wordCountLabel = new MaterialLabel
            {
                Text = "Words: 0",
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right,
                Padding = new Padding(0, 0, 20, 0),
                AutoSize = false,
                Size = new Size(150, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            
            headerPanel.Controls.Add(wordCountLabel);
            headerPanel.Controls.Add(headerLabel);

            // Add padding panel first (at the bottom of the stack)
            var paddingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15)
            };
            paddingPanel.Controls.Add(richTextBox);
            containerPanel.Controls.Add(paddingPanel);

            // Then add header and divider (they will stack on top)
            containerPanel.Controls.Add(headerPanel);
            
            // Add a divider
            var headerDivider = new MaterialDivider
            {
                Dock = DockStyle.Top
            };
            containerPanel.Controls.Add(headerDivider);
            
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
                MaterialMessageBox.Show($"Error saving file: {ex.Message}", "Error", 
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
                MaterialMessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        public void SetDocumentTitle(string title)
        {
            headerLabel.Text = title;
        }
        
        private void UpdateWordCount()
        {
            var text = richTextBox.Text.Trim();
            int wordCount = text.Length == 0 ? 0 : text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            wordCountLabel.Text = $"Words: {wordCount:N0}";
        }

        public void LoadChapter(ChapterInfo chapterInfo)
        {
            Text = chapterManager.LoadChapterContent(chapterInfo.Id, chapterInfo.Title);
            SetDocumentTitle($"Chapter {chapterInfo.Id}: {chapterInfo.Title}");
        }
    }
} 