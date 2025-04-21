using System;
using System.Drawing;
using System.Windows.Forms;

namespace LightNovelEditor
{
    public class CustomMenuBar : MenuStrip
    {
        // File menu events
        public event EventHandler? NewClicked;
        public event EventHandler? OpenClicked;
        public event EventHandler? SaveClicked;
        public event EventHandler? SaveAsClicked;
        public event EventHandler? ExitClicked;
        
        // Edit menu events
        public event EventHandler? UndoClicked;
        public event EventHandler? RedoClicked;
        public event EventHandler? CutClicked;
        public event EventHandler? CopyClicked;
        public event EventHandler? PasteClicked;
        public event EventHandler? SelectAllClicked;
        
        // Format menu events
        public event EventHandler? FontClicked;
        public event EventHandler? BoldClicked;
        public event EventHandler? ItalicClicked;
        public event EventHandler? UnderlineClicked;
        
        public CustomMenuBar()
        {
            this.Renderer = new CustomMenuRenderer();
            this.BackColor = Color.FromArgb(48, 48, 56);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Padding = new Padding(8, 3, 0, 3);
            
            // File Menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.ForeColor = Color.White;
            this.Items.Add(fileMenu);
            
            var newMenuItem = new ToolStripMenuItem("New Document", null, (s, e) => NewClicked?.Invoke(this, EventArgs.Empty));
            newMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            
            var openMenuItem = new ToolStripMenuItem("Open...", null, (s, e) => OpenClicked?.Invoke(this, EventArgs.Empty));
            openMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            
            var saveMenuItem = new ToolStripMenuItem("Save", null, (s, e) => SaveClicked?.Invoke(this, EventArgs.Empty));
            saveMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            
            var saveAsMenuItem = new ToolStripMenuItem("Save As...", null, (s, e) => SaveAsClicked?.Invoke(this, EventArgs.Empty));
            saveAsMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            
            var exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitClicked?.Invoke(this, EventArgs.Empty));
            exitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                newMenuItem, openMenuItem, saveMenuItem, saveAsMenuItem,
                new ToolStripSeparator(), exitMenuItem
            });
            
            // Edit Menu
            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.ForeColor = Color.White;
            this.Items.Add(editMenu);
            
            var undoMenuItem = new ToolStripMenuItem("Undo", null, (s, e) => UndoClicked?.Invoke(this, EventArgs.Empty));
            undoMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            
            var redoMenuItem = new ToolStripMenuItem("Redo", null, (s, e) => RedoClicked?.Invoke(this, EventArgs.Empty));
            redoMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            
            var cutMenuItem = new ToolStripMenuItem("Cut", null, (s, e) => CutClicked?.Invoke(this, EventArgs.Empty));
            cutMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            
            var copyMenuItem = new ToolStripMenuItem("Copy", null, (s, e) => CopyClicked?.Invoke(this, EventArgs.Empty));
            copyMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            
            var pasteMenuItem = new ToolStripMenuItem("Paste", null, (s, e) => PasteClicked?.Invoke(this, EventArgs.Empty));
            pasteMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            
            var selectAllMenuItem = new ToolStripMenuItem("Select All", null, (s, e) => SelectAllClicked?.Invoke(this, EventArgs.Empty));
            selectAllMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            
            editMenu.DropDownItems.AddRange(new ToolStripItem[] {
                undoMenuItem, redoMenuItem, new ToolStripSeparator(),
                cutMenuItem, copyMenuItem, pasteMenuItem, new ToolStripSeparator(),
                selectAllMenuItem
            });
            
            // Format Menu
            var formatMenu = new ToolStripMenuItem("Format");
            formatMenu.ForeColor = Color.White;
            this.Items.Add(formatMenu);
            
            var fontMenuItem = new ToolStripMenuItem("Font...", null, (s, e) => FontClicked?.Invoke(this, EventArgs.Empty));
            
            var boldMenuItem = new ToolStripMenuItem("Bold", null, (s, e) => BoldClicked?.Invoke(this, EventArgs.Empty));
            boldMenuItem.ShortcutKeys = Keys.Control | Keys.B;
            
            var italicMenuItem = new ToolStripMenuItem("Italic", null, (s, e) => ItalicClicked?.Invoke(this, EventArgs.Empty));
            italicMenuItem.ShortcutKeys = Keys.Control | Keys.I;
            
            var underlineMenuItem = new ToolStripMenuItem("Underline", null, (s, e) => UnderlineClicked?.Invoke(this, EventArgs.Empty));
            underlineMenuItem.ShortcutKeys = Keys.Control | Keys.U;
            
            formatMenu.DropDownItems.AddRange(new ToolStripItem[] {
                fontMenuItem, new ToolStripSeparator(),
                boldMenuItem, italicMenuItem, underlineMenuItem
            });
        }
        
        private class CustomMenuRenderer : ToolStripProfessionalRenderer
        {
            public CustomMenuRenderer() : base(new CustomColorTable())
            {
                this.RoundedEdges = false;
            }
            
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (!e.Item.Selected)
                {
                    base.OnRenderMenuItemBackground(e);
                    return;
                }
                
                var rect = new Rectangle(0, 0, e.Item.Width, e.Item.Height);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(65, 65, 75)), rect);
            }
        }
        
        private class CustomColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(65, 65, 75);
            public override Color MenuItemBorder => Color.FromArgb(48, 48, 56);
            public override Color MenuBorder => Color.FromArgb(48, 48, 56);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(65, 65, 75);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(65, 65, 75);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(65, 65, 75);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(65, 65, 75);
            public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 52);
            public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 52);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 52);
            public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 52);
        }
    }
} 