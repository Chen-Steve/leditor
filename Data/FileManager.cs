using System;
using System.IO;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class FileManager
    {
        private readonly EditorPanel editorPanel;
        private string? currentFilePath;

        public string? CurrentFilePath => currentFilePath;

        public FileManager(EditorPanel editorPanel)
        {
            this.editorPanel = editorPanel ?? throw new ArgumentNullException(nameof(editorPanel));
        }

        public void NewFile(Form parentForm)
        {
            if (editorPanel.Modified)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Confirmation",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile(false, parentForm);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            editorPanel.Clear();
            editorPanel.SetDocumentTitle("Untitled Document");
            currentFilePath = null;
            parentForm.Text = "Lanry Editor";
        }

        public void OpenFile(Form parentForm)
        {
            if (editorPanel.Modified)
            {
                var result = MessageBox.Show("Do you want to save changes?", "Confirmation",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveFile(false, parentForm);
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
                        parentForm.Text = $"Lanry Editor - {Path.GetFileName(currentFilePath)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void SaveFile(bool saveAs, Form parentForm)
        {
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
                        parentForm.Text = $"Lanry Editor - {Path.GetFileName(currentFilePath)}";
                    }
                    else
                    {
                        return;
                    }
                }
            }

            editorPanel.SaveToFile(currentFilePath!);
        }

        public void FormatFont()
        {
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