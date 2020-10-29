﻿using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Notepad
{
    internal partial class Blank : Form
    {
        private bool _isSaved;
        private string _bufferText = "";
        private string _pagePath = "";
        private string _pageName = "";
        private string _pageFormat = ".txt";
        private readonly int _pageNumber;
        private readonly Menu _menu;
        private readonly string _numberOfCharactersLable;
        private readonly string _formatLable;
        
        internal SearchBox SearchBox { get; set; }

        internal string PageName => _pageName;

        internal Blank(Menu menu)
        {
            InitializeComponent();

            _menu = menu;
            MdiParent = menu;
            _pageNumber = menu.GetNumberOfMdiChildren() - menu.NumberOfSearhBoxInstance;

            _pageName = "Сторінка " + _pageNumber;
            Text = _pageName;
            _isSaved = false;
            
            richTextBox.Modified = false;

            _numberOfCharactersLable = "Кількість символів: ";
            _formatLable = "Формат: ";

            amountToolStripStatusLabel.Text = _numberOfCharactersLable + 
                                              richTextBox.Text.Length.ToString();
            formatToolStripStatusLabel.Text = _formatLable + _pageFormat;
        }

        internal void Cut()
        {
            if (!string.IsNullOrEmpty(richTextBox.SelectedText))
            {
                _bufferText = richTextBox.SelectedText;
                richTextBox.SelectedText = "";
            }
        }

        internal void Copy()
        {
            if (!string.IsNullOrEmpty(richTextBox.SelectedText))
            {
                _bufferText = richTextBox.SelectedText;
            }
        }

        internal void Paste()
        {
            richTextBox.SelectedText = _bufferText;
        }

        internal void SelectAll()
        {
            richTextBox.SelectAll();
        }

        internal void Delete()
        {
            richTextBox.SelectedText = "";
            _bufferText = "";
        }

        internal void Undo()
        {
            richTextBox.Undo();
        }

        internal void Redo()
        {
            richTextBox.Redo();
        }

        internal void Open(string pagePath)
        {
            _isSaved = true;
            _pagePath = pagePath;
            _pageFormat = Path.GetExtension(pagePath);
            formatToolStripStatusLabel.Text = _formatLable + _pageFormat;
            _pageName = _pagePath;
            Text = _pageName;
            ReadFromFileToRichTextBox();
            Show();
        }

        private void WriteToRTF()
        {
            using (StreamWriter writer = new StreamWriter(_pagePath, false))
            {
                writer.WriteLine(richTextBox.Rtf);
            }
            richTextBox.Modified = false;
        }
        
        private void WriteToTXT()
        {
            using (StreamWriter writer = new StreamWriter(_pagePath, false))
            {
                writer.WriteLine(richTextBox.Text);
            }
            richTextBox.Modified = false;
        }

        private void WriteToFileFromRichTextBox()
        {
            if (Path.GetExtension(_pagePath) == ".rtf")
            {
                WriteToRTF();
            }

            if (Path.GetExtension(_pagePath) == ".txt")
            {
                WriteToTXT();
            }
        }

        private void ReadFromRTF()
        {
            richTextBox.Rtf = File.ReadAllText(_pagePath);
            richTextBox.Modified = false;
            UnmarkPage();
        }

        private void ReadFromTXT()
        {
            using (StreamReader reader = new StreamReader(_pagePath))
            {
                richTextBox.Text = reader.ReadToEnd();
            }
        }

        private void ReadFromFileToRichTextBox()
        {
            if (Path.GetExtension(_pagePath) == ".rtf")
            {
                ReadFromRTF();
            }

            if (Path.GetExtension(_pagePath) == ".txt")
            {
                ReadFromTXT();
            }
        }

        internal void Save()
        {
            if (!_isSaved)
            {
                WriteToFileFromRichTextBox();
                UnmarkPage();
            }
        }

        internal void SaveAs()
        {
            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _isSaved = true;
                _pagePath = saveFileDialog.FileName;
                _pageName = _pagePath;
                Text = _pageName;
                WriteToFileFromRichTextBox();
            }
        }

        internal void ChangeFont()
        {
            if (string.IsNullOrEmpty(richTextBox.SelectedText))
            {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox.Font = fontDialog.Font;
                }
            }
            else
            {
                fontDialog.Font = richTextBox.SelectionFont;
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox.SelectionFont = fontDialog.Font;
                }
            }
        }

        internal void ChangeColor()
        {
            if (string.IsNullOrEmpty(richTextBox.SelectedText))
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox.ForeColor = colorDialog.Color;
                }
            }
            else
            {
                colorDialog.Color = richTextBox.SelectionColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox.SelectionColor = colorDialog.Color;
                }
            }
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void MarkPage()
        {
            Text = _pageName.Insert(0, "*");
            _isSaved = false;
        }

        private void UnmarkPage()
        {
            if (Text[0] == '*')
            {
                Text = Text.Remove(0, 1);
            }

            _isSaved = true;
        }

        internal void RichTextBox_ModifiedChanged(object sender, EventArgs e)
        {
            if (richTextBox.Modified == false)
            {
                return;
            }

            MarkPage();
        }

        private void Blank_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isSaved)
            {
                DialogResult result = MessageBox.Show(
                    "Зберегти внесені зміни до файлу?", 
                    "Notepad", 
                    MessageBoxButtons.YesNoCancel, 
                    MessageBoxIcon.Exclamation);

                if (result == DialogResult.Yes)
                {
                    if (String.IsNullOrEmpty(_pagePath))
                        SaveAs();
                    else
                        Save();
                }

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            if (_menu.GetNumberOfMdiChildren() == 1)
            {
                _menu.DisableAllItemsRelatedToBlank();
            }
        }

        private void Blank_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (SearchBox != null)
            {
                SearchBox.Close();
            }
        }

        private void Blank_Activated(object sender, EventArgs e)
        {
            if (_isSaved)
                _menu.EnableItemsAfterSaveBlank();
            else
                _menu.DisableItemsBeforeSaveBlank();
        }

        private void FontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeFont();
        }

        private void FontColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeColor();
        }

        private void RichTextBox_TextChanged(object sender, EventArgs e)
        {
            amountToolStripStatusLabel.Text = _numberOfCharactersLable +
                                              " " + 
                                              richTextBox.Text.Length.ToString();
        }

        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SearchBox(_menu, this);
        }

        internal Match SearchInRichTextBox2(Regex regex, int initialIndex)
        {
            return regex.Match(richTextBox.Text, initialIndex);
        }

        internal void ClearHighlight()
        {
            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = Color.White;
            richTextBox.DeselectAll();
        }

        internal void HighlightSearchString(int initialIndex, int lenght)
        {
            richTextBox.Select(initialIndex, lenght);
            richTextBox.SelectionBackColor = Color.Yellow;
        }
    }
}
