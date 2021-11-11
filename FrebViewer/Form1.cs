using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace FrebViewer
{
    public class Form1 : Form
    {
        private IContainer components = null;

        private FolderBrowserDialog folderBrowserDialog1;

        private TextBox textBox2;

        private Button button4;

        private SplitContainer splitContainer1;

        private DataGridView dataGridView1;

        private WebBrowser webBrowser1;

        private Button button6;

        private Label label1;

        private TextBox textBox1;

        private Button button1;

        private Button button2;

        public Form1()
        {
            this.InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void LoadSelectedFolder()
        {
            if (this.textBox1.Text.Trim() == "")
            {
                MessageBox.Show("Please select the correct folder.");
                return;
            }
            string verb;
            verb = "";
            int timeTaken;
            timeTaken = 0;
            this.dataGridView1.Visible = true;
            this.webBrowser1.Visible = true;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
            this.dataGridView1.Columns.Clear();
            this.dataGridView1.Columns.Add("filename", "FileName");
            this.dataGridView1.Columns.Add("url", "url");
            this.dataGridView1.Columns.Add("verb", "verb");
            this.dataGridView1.Columns.Add("appPoolID", "AppPoolName");
            this.dataGridView1.Columns.Add("statusCode", "StatusCode");
            this.dataGridView1.Columns.Add("timeTaken", "TimeTaken");
            try
            {
                FileInfo[] files;
                files = new DirectoryInfo(this.textBox1.Text).GetFiles("fr*.xml", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    MessageBox.Show("There are no FREB trace files in the selected folder. Please select the correct folder.");
                    return;
                }
                FileInfo[] array;
                array = files;
                foreach (FileInfo fileInfo in array)
                {
                    this.GetDetailsFromFREBFile(fileInfo.FullName, out var url, out verb, out var appPool, out var statusCode, out timeTaken);
                    this.dataGridView1.Rows.Add(fileInfo.FullName, url, verb, appPool, statusCode, timeTaken);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something bad happened. Please report it to rakkim@microsoft.com. Message : " + ex.Message + " Stack : " + ex.StackTrace);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.button4.Text == "search text")
            {
                foreach (DataGridViewRow item in (IEnumerable)this.dataGridView1.Rows)
                {
                    bool visible;
                    visible = false;
                    for (int i = 0; i < item.Cells.Count; i++)
                    {
                        if (item.Cells[i].Value != null && item.Cells[i].Value.ToString().ToLower().Contains(this.textBox2.Text.ToLower()))
                        {
                            visible = true;
                        }
                    }
                    item.Visible = visible;
                }
                this.button4.Text = "load all";
                return;
            }
            foreach (DataGridViewRow item2 in (IEnumerable)this.dataGridView1.Rows)
            {
                item2.Visible = true;
            }
            this.button4.Text = "search text";
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Result result;
            result = this.FindSlowEvent(this.dataGridView1.CurrentRow.Cells[0].Value.ToString());
            MessageBox.Show("Maximum delay of " + result.delay + "ms between '" + result.pEventName + "' and '" + result.eventName + "'");
        }

        private Result FindSlowEvent(string fileName)
        {
            XmlDocument xmlDocument;
            xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            XmlNode xmlNode;
            xmlNode = xmlDocument.SelectSingleNode("failedRequest");
            long num;
            num = 0L;
            long num2;
            num2 = 0L;
            long num3;
            num3 = 0L;
            XmlNode xmlNode2;
            xmlNode2 = null;
            XmlNode xmlNode3;
            xmlNode3 = null;
            XmlNode xmlNode4;
            xmlNode4 = null;
            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                DateTime dateTime;
                dateTime = DateTime.Parse(childNode["System"]["TimeCreated"].GetAttribute("SystemTime"));
                if (num != 0)
                {
                    num2 = dateTime.Ticks - num;
                    if (num3 < num2)
                    {
                        num3 = num2;
                        xmlNode2 = childNode;
                        xmlNode3 = xmlNode4;
                    }
                }
                xmlNode4 = childNode;
                num = dateTime.Ticks;
            }
            string innerText;
            innerText = xmlNode2["RenderingInfo"].ChildNodes[0].InnerText;
            Result result;
            result = new Result();
            result.delay = num3 / 10000;
            result.eventName = innerText;
            result.pEventName = xmlNode3["RenderingInfo"].ChildNodes[0].InnerText;
            return result;
        }

        private void GetDetailsFromFREBFile(string p, out string url, out string verb, out string appPool, out int statusCode, out int timeTaken)
        {
            url = (appPool = (verb = ""));
            statusCode = (timeTaken = 0);
            try
            {
                XmlDocument xmlDocument;
                xmlDocument = new XmlDocument();
                xmlDocument.Load(p);
                XmlNode xmlNode;
                xmlNode = xmlDocument.SelectSingleNode("failedRequest");
                url = xmlNode.Attributes["url"].Value;
                verb = xmlNode.Attributes["verb"].Value;
                appPool = xmlNode.Attributes["appPoolId"].Value;
                timeTaken = int.Parse(xmlNode.Attributes["timeTaken"].Value);
                statusCode = int.Parse(xmlNode.Attributes["statusCode"].Value.Split('.')[0]);
            }
            catch (Exception ex)
            {
                try
                {
                    if (ex.GetType().ToString() == "System.Xml.XmlException")
                    {
                        string text;
                        text = " ";
                        string text2;
                        text2 = " ";
                        string text3;
                        text3 = " ";
                        TextReader textReader;
                        textReader = new StreamReader(p);
                        while (!text.Contains("failedRequest url"))
                        {
                            text = textReader.ReadLine();
                            if (text == null || text.Contains("xmlns:freb="))
                            {
                                break;
                            }
                        }
                        text = text.Substring(20).Replace('"', ' ');
                        while (!appPool.Contains("appPoolId="))
                        {
                            appPool = textReader.ReadLine();
                            if (appPool.Contains("xmlns:freb="))
                            {
                                break;
                            }
                        }
                        appPool = appPool.Substring(27).Replace('"', ' ').Trim();
                        while (!verb.Contains("verb="))
                        {
                            verb = textReader.ReadLine();
                            if (verb.Contains("xmlns:freb="))
                            {
                                break;
                            }
                        }
                        verb = verb.Substring(20).Replace('"', ' ').Trim();
                        while (!text2.Contains("statusCode="))
                        {
                            text2 = textReader.ReadLine();
                            if (text2.Contains("xmlns:freb="))
                            {
                                break;
                            }
                        }
                        text2 = text2.Substring(27).Replace('"', ' ');
                        while (!text3.Contains("timeTaken="))
                        {
                            text3 = textReader.ReadLine();
                            if (text3.Contains("xmlns:freb="))
                            {
                                break;
                            }
                        }
                        text3 = text3.Substring(25).Replace('"', ' ');
                        url = text.Trim();
                        statusCode = int.Parse(text2.Trim());
                        timeTaken = int.Parse(text3.Trim());
                    }
                    else
                    {
                        MessageBox.Show("Something bad happened. Please report it to rakkim@microsoft.com. Message : " + ex.Message + " Stack : " + ex.StackTrace);
                    }
                }
                catch (Exception ex2)
                {
                    MessageBox.Show("Something bad happened. Please report it to rakkim@microsoft.com. Message : " + ex2.Message + " Stack : " + ex2.StackTrace);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.splitContainer1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.splitContainer1.Width = base.Width - 18;
            this.splitContainer1.Height = base.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void splitContainer1_Resize(object sender, EventArgs e)
        {
            this.splitContainer1.Width = base.Width - 18;
            this.splitContainer1.Height = base.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 10;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            this.splitContainer1.Width = base.Width - 18;
            this.splitContainer1.Height = base.Height - 70;
            this.dataGridView1.Width = this.splitContainer1.Width - 10;
            this.dataGridView1.Height = this.splitContainer1.Panel1.Height;
            this.webBrowser1.Height = this.splitContainer1.Panel2.Height - 20;
            this.webBrowser1.Width = this.splitContainer1.Panel2.Width - 5;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!this.dataGridView1.CurrentRow.IsNewRow)
            {
                this.dataGridView1.CurrentRow.Selected = true;
                this.webBrowser1.Navigate(((DataGridView)sender).CurrentRow.Cells[0].Value.ToString());
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridView1.CurrentRow.Selected = true;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.dataGridView1.CurrentRow.Selected = true;
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            int num;
            num = this.dataGridView1.CurrentRow.Index;
            if (num >= this.dataGridView1.Rows.Count - 1)
            {
                num--;
            }
            this.dataGridView1.Rows[num].Selected = true;
            this.webBrowser1.Navigate(((DataGridView)sender).Rows[num].Cells[1].Value.ToString());
        }

        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Name == "StatusCode" || e.Column.Name == "TimeTaken")
            {
                if (int.Parse(e.CellValue1.ToString()) < int.Parse(e.CellValue2.ToString()))
                {
                    e.SortResult = 1;
                }
                else if (int.Parse(e.CellValue1.ToString()) > int.Parse(e.CellValue2.ToString()))
                {
                    e.SortResult = -1;
                }
                else
                {
                    e.SortResult = 0;
                }
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.ShowDialog();
            this.textBox1.Text = this.folderBrowserDialog1.SelectedPath;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.LoadSelectedFolder();
            this.splitContainer1.Visible = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button6 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            base.SuspendLayout();
            this.textBox2.Location = new System.Drawing.Point(407, 14);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(124, 23);
            this.textBox2.TabIndex = 8;
            this.button4.Location = new System.Drawing.Point(537, 10);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(80, 28);
            this.button4.TabIndex = 9;
            this.button4.Text = "search text";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(button4_Click);
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(642, 374);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.Visible = false;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(5, 3);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(358, 181);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.Visible = false;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellClick);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellContentClick);
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(dataGridView1_ColumnHeaderMouseClick);
            this.dataGridView1.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(dataGridView1_SortCompare);
            this.dataGridView1.KeyUp += new System.Windows.Forms.KeyEventHandler(dataGridView1_KeyUp);
            this.splitContainer1.Location = new System.Drawing.Point(7, 44);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(1042, 731);
            this.splitContainer1.SplitterDistance = 139;
            this.splitContainer1.TabIndex = 6;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer1_SplitterMoved);
            this.splitContainer1.Resize += new System.EventHandler(splitContainer1_Resize);
            this.button6.Location = new System.Drawing.Point(620, 10);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(122, 28);
            this.button6.TabIndex = 11;
            this.button6.Text = "Show the Slowness";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(button6_Click);
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Select the folder";
            this.textBox1.Location = new System.Drawing.Point(107, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(204, 23);
            this.textBox1.TabIndex = 14;
            this.button1.Location = new System.Drawing.Point(317, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 28);
            this.button1.TabIndex = 15;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(button1_Click);
            this.button2.Location = new System.Drawing.Point(347, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(54, 28);
            this.button2.TabIndex = 16;
            this.button2.Text = "load";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(button2_Click_1);
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            base.ClientSize = new System.Drawing.Size(1356, 772);
            base.Controls.Add(this.button2);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.textBox1);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.button6);
            base.Controls.Add(this.button4);
            base.Controls.Add(this.textBox2);
            base.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            base.Name = "Form1";
            this.Text = "FrebViewer - navigate your FREB traces easily";
            base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            base.Load += new System.EventHandler(Form1_Load);
            base.Resize += new System.EventHandler(Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
            this.splitContainer1.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}
