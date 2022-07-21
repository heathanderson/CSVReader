using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CSVReader
{
    public partial class Form1 : Form
    {
        private string[] alphabet = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private char splitChar = '^';
        private bool fileLoaded = false;
        private FileInfo fi;
        private DataTable dt1 = null;
        private DataTable dt2 = null;

        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string filename)
        {
            InitializeComponent();
            txtFile.Text = filename;
            loadFile();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadFile();
        }

        private bool loadFile()
        {
            int numColumns = 0;
            int rowCount = 0;

            dataGridView1.Columns.Clear();

            try
            {
                using (TextReader reader = new StreamReader(txtFile.Text))
                {
                    fi = new FileInfo(txtFile.Text);

                    string line;
                    if (dt1 != null)
                        dt2 = dt1.Copy();

                    dt1 = new DataTable();

                    while ((line = reader.ReadLine()) != null)
                    {
                        rowCount++;

                        string[] lineArray = split(line);

                        for (int i = numColumns; i < lineArray.Length; i++)
                        {
                            numColumns++;
                            string colName = getColumnName(numColumns);
                            dt1.Columns.Add(colName);
                        }

                        dt1.Rows.Add(lineArray);
                    }
                }
                LoadTable();
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        private string getColumnName(int i)
        {
            string name = (i + 1) + ":";

            int factor = i / alphabet.Length - 1;

            if (factor >= 0)
            {
                name += alphabet[factor];
            }
            name += alphabet[i % alphabet.Length];

            return name;
        }

        private string[] split(string line)
        {
            string[] lineArray;
            string newLine = "";
            bool inString = false;

            if (line == null)
                return new string[0];

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inString = !inString;
                    if (i < line.Length - 1 && line[i + 1] == '"')
                    {
                        newLine += line[i];
                    }
                }
                else if (!inString && line[i] == ',')
                {
                    newLine += splitChar;

                }
                else
                {
                    newLine += line[i];
                }
            }
            lineArray = newLine.Split(splitChar);
            return lineArray;
        }

        private void LoadTable()
        {
            for (int i = 0; i < dt1.Columns.Count; i++)
            {
                string colName = getColumnName(i);
                dataGridView1.Columns.Add(colName, colName);
            }

            for (int rowNum = 0; rowNum < dt1.Rows.Count; rowNum++)
            {
                DataRow dr1;
                DataRow dr2;

                dataGridView1.Rows.Add();

                dr1 = dt1.Rows[rowNum];

                if (dt2 != null && rowNum <= dt2.Rows.Count)
                    dr2 = dt2.Rows[rowNum];
                else
                    dr2 = null;

                if (rowNum % 2 == 0)
                    dataGridView1.Rows[rowNum].DefaultCellStyle.BackColor = Color.LightBlue;
                else
                    dataGridView1.Rows[rowNum].DefaultCellStyle.BackColor = Color.LightCyan;

                for (int colNum = 0; colNum < dt1.Columns.Count; colNum++)
                {
                    try
                    {
                        dataGridView1.Rows[rowNum].Cells[colNum].Value = dr1[colNum].ToString();

                        if (
                            dr2 != null
                            && colNum <= dt2.Columns.Count
                            && !dr2[colNum].Equals(dr1[colNum])
                            )
                        {
                            dataGridView1.Rows[rowNum].Cells[colNum].ToolTipText = dr2[colNum].ToString();
                            dataGridView1.Rows[rowNum].Cells[colNum].Style.BackColor = Color.Red;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }


        #region Events (Button Clicks etc...)
        private void txtFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                loadFile();
        }

        private void btnFile1_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = openFileDialog.FileName;
            }
        }
        #endregion

        #region Drag and Drop stuff
        private void txtFile_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                txtFile.Text = e.Data.GetData(DataFormats.Text).ToString();
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                txtFile.Text = s[0];
                txtFile.Focus();
                this.Activate();
                loadFile();
            }
        }

        private void txtFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        #endregion

        #region Timer
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (ChkAutoRefresh.Checked)
            {
                try
                {
                    FileInfo f = new FileInfo(txtFile.Text);
                    if (f.LastWriteTimeUtc != fi.LastWriteTimeUtc)
                        loadFile();
                }
                catch (Exception ex) { }
            }
        }
        #endregion

    }
}
