using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostChanger
{
  

    public partial class Form1 : Form
    {
        int TotalCheckBoxes = 0;
        int TotalCheckedCheckBoxes = 0;
        CheckBox HeaderCheckBox = null;
        bool IsHeaderCheckBoxClicked = false;

        string buttonFlag = null;

        private static string HOSTS_FILE_PATH = @"C:\Windows\System32\drivers\etc\hosts";

        public Form1()
        {
            InitializeComponent();            
        }

        private void dgvSelectAll_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex == 0)
                ResetHeaderCheckBoxLocation(e.ColumnIndex, e.RowIndex);
        }

        private void ResetHeaderCheckBoxLocation(int ColumnIndex, int RowIndex)
        {
            //Get the column header cell bounds
            Rectangle oRectangle = this.dataGridView1.GetCellDisplayRectangle(ColumnIndex, RowIndex, true);

            Point oPoint = new Point();

            oPoint.X = oRectangle.Location.X + (oRectangle.Width - HeaderCheckBox.Width) / 2 + 1;
            oPoint.Y = oRectangle.Location.Y + (oRectangle.Height - HeaderCheckBox.Height) / 2 + 1;

            //Change the location of the CheckBox to make it stay on the header
            HeaderCheckBox.Location = oPoint;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddHeaderCheckBox();

            HeaderCheckBox.KeyUp += new KeyEventHandler(HeaderCheckBox_KeyUp);
            HeaderCheckBox.MouseClick += new MouseEventHandler(HeaderCheckBox_MouseClick);
            dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(dgvSelectAll_CellValueChanged);
            dataGridView1.CurrentCellDirtyStateChanged += new EventHandler(dgvSelectAll_CurrentCellDirtyStateChanged);
            dataGridView1.CellPainting += new DataGridViewCellPaintingEventHandler(dgvSelectAll_CellPainting);

            comboBox1.Items.Add("전체");
            comboBox1.SelectedIndex = 0;

            BindGridView();
            ReadHostsFIle();            
        }

        private string[] GetPreStrArray(string preStr)
        {
            string[] preStrArray = new string[2];
            string[] regArray = Regex.Split(preStr, @"(\s|\t)+");

            int pos = 0;
            for (int i = 0; i < regArray.Length; i++)
            {
                if (regArray[i].Trim(new char[] { '\t', ' ' }).Length != 0)
                {
                    preStrArray[pos] = regArray[i];
                    pos++;
                }
            }

            return preStrArray;
        }

        private void DelteBlankLine() {
            string[] text = File.ReadAllLines(@HOSTS_FILE_PATH).Where(s => s.Trim() != string.Empty).ToArray();
            File.Delete(@HOSTS_FILE_PATH);
            File.WriteAllLines(@HOSTS_FILE_PATH, text);

        }

        private void ReadHostsFIle()
        {

            DelteBlankLine();

            string[] lines = System.IO.File.ReadAllLines(@HOSTS_FILE_PATH);
            Regex regex = new Regex("^.*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\\s|\t)+\\S+(((\\s|\t)+#\\S*){1,2}|)$");

            int fileIndex = 0;

            foreach (string line in lines)
            {
                Match m = regex.Match(line);                
                if (m.Success && line.Trim().Length > 0)
                {
                    object[] row = new object[8];                    

                    if (line.StartsWith("#")) // 비활성화
                    {
                        row[0] = false;

                        int index = line.IndexOf("#", 1);

                        if (index != -1)
                        {
                            string preStr = line.Substring(1, index - 1);
                            string postStr = line.Substring(index + 1, (line.Length - preStr.Length) - 2);
                            string[] preStrArray = GetPreStrArray(preStr);
                            string[] postStrArray = postStr.Split('#');

                            row[1] = preStrArray[0];
                            row[2] = preStrArray[1];
                            row[3] = postStrArray[0].TrimEnd();
                            row[4] = postStrArray[1].TrimEnd();
                        }
                        else {
                            string preStr = line.Substring(1, line.Length - 1);
                            string[] preStrArray = GetPreStrArray(preStr);
                            row[1] = preStrArray[0];
                            row[2] = preStrArray[1];
                            row[3] = "";
                            row[4] = "";
                        }                        
                    }
                    else // 활성화
                    {
                        row[0] = true;

                        int index = line.IndexOf("#");

                        if (index != -1)
                        {
                            string preStr = line.Substring(0, index);
                            string postStr = line.Substring(index + 1, (line.Length - preStr.Length) - 1);
                            string[] preStrArray = GetPreStrArray(preStr);
                            string[] postStrArray = postStr.Split('#');

                            row[1] = preStrArray[0];
                            row[2] = preStrArray[1];
                            row[3] = postStrArray[0].TrimEnd();
                            row[4] = postStrArray[1].TrimEnd();
                        }
                        else
                        {
                            string preStr = line.Substring(0, line.Length - 1);
                            string[] preStrArray = GetPreStrArray(preStr);
                            row[1] = preStrArray[0];
                            row[2] = preStrArray[1];
                            row[3] = "";
                            row[4] = "";
                        }
                    }


                    if (!comboBox1.Items.Contains(row[3]) && ((string)row[3]).Trim().Length > 0)
                    {
                        comboBox1.Items.Add(row[3]);
                    }

                    row[5] = fileIndex;
                    row[6] = "";
                    row[7] = Properties.Resources.check; 
                    

                    dataGridView1.Rows.Add(row);
                }

                fileIndex++;
            }
        }

        private void BindGridView()
        {            
            TotalCheckBoxes = dataGridView1.RowCount;
            TotalCheckedCheckBoxes = 0;
        }

        private void dgvSelectAll_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                if (!IsHeaderCheckBoxClicked)
                    RowCheckBoxClick((DataGridViewCheckBoxCell)dataGridView1[e.ColumnIndex, e.RowIndex]);
            }

            
            if (e.ColumnIndex != 0 && e.ColumnIndex != 7 && e.ColumnIndex != 6 && e.ColumnIndex != 5)
            {
                if (!"A".Equals(dataGridView1.Rows[e.RowIndex].Cells["Status"].Value) &&
                    !"D".Equals(dataGridView1.Rows[e.RowIndex].Cells["Status"].Value) &&
                    !"ACTIVE".Equals(dataGridView1.Rows[e.RowIndex].Cells["Status"].Value))
                {
                    dataGridView1.Rows[e.RowIndex].Cells["Status"].Value = "U";
                    ((DataGridViewImageCell)dataGridView1.Rows[e.RowIndex].Cells["Image"]).Value = Properties.Resources.update;
                }               

            }

        }

        private void dgvSelectAll_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell is DataGridViewCheckBoxCell)
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void HeaderCheckBox_MouseClick(object sender, MouseEventArgs e)
        {
            HeaderCheckBoxClick((CheckBox)sender);
        }

        private void HeaderCheckBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                HeaderCheckBoxClick((CheckBox)sender);
        }

        private void AddHeaderCheckBox()
        {
            HeaderCheckBox = new CheckBox();

            HeaderCheckBox.Size = new Size(15, 15);

            //Add the CheckBox into the DataGridView
            this.dataGridView1.Controls.Add(HeaderCheckBox);
        }
     
        private void HeaderCheckBoxClick(CheckBox HCheckBox)
        {
            IsHeaderCheckBoxClicked = true;

            foreach (DataGridViewRow Row in dataGridView1.Rows)
                ((DataGridViewCheckBoxCell)Row.Cells["CheckBox"]).Value = HCheckBox.Checked;

            dataGridView1.RefreshEdit();

            TotalCheckedCheckBoxes = HCheckBox.Checked ? TotalCheckBoxes : 0;

            IsHeaderCheckBoxClicked = false;
        }

        private void RowCheckBoxClick(DataGridViewCheckBoxCell RCheckBox)
        {
            if (RCheckBox != null)
            {
                //Modifiy Counter;            
                if ((bool)RCheckBox.Value && TotalCheckedCheckBoxes < TotalCheckBoxes)
                    TotalCheckedCheckBoxes++;
                else if (TotalCheckedCheckBoxes > 0)
                    TotalCheckedCheckBoxes--;

                //Change state of the header CheckBox.
                if (TotalCheckedCheckBoxes < TotalCheckBoxes)
                    HeaderCheckBox.Checked = false;
                else if (TotalCheckedCheckBoxes == TotalCheckBoxes)
                    HeaderCheckBox.Checked = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();

            ReadHostsFIle();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            ComboBox comboBox = (ComboBox)sender;
            string itemNm = (string)comboBox1.SelectedItem;

            if ("전체".Equals(itemNm))
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {                   
                     row.Visible = true;
                }
            }
            else
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.Cells["Column4"].Value.Equals(itemNm))
                    {
                        row.Visible = false;
                    }
                    else
                    {
                        row.Visible = true;
                    }
                }
            }
        }

        public static List<int> AddFile(List<string> data)
        {
            List<int> indexs = new List<int>();

            string[] arrLine = File.ReadAllLines(HOSTS_FILE_PATH);
            string[] addArrLine = new string[arrLine.Length + data.Count];
            Array.Copy(arrLine, 0, addArrLine, 0, arrLine.Length);


            for (int i = 0; i < data.Count; i++)
            {
                addArrLine[arrLine.Length + i] = data[i];
                indexs.Add(arrLine.Length + i);
            }

            File.WriteAllLines(HOSTS_FILE_PATH, addArrLine);
            return indexs;
        }

        public static void UpdateFile(List<int> list, List<string> data)
        {
            string[] arrLine = File.ReadAllLines(HOSTS_FILE_PATH);

            for( int i = 0; i < list.Count; i++)
            {
                arrLine[list[i]] = data[i];
            }

            File.WriteAllLines(HOSTS_FILE_PATH, arrLine);
        }

        public static void deleteFile(List<int> list)
        {
            string[] arrLine = File.ReadAllLines(HOSTS_FILE_PATH);

            for (int i = 0; i < list.Count; i++)
            {
                arrLine[list[i]] = " ";
            }

            File.WriteAllLines(HOSTS_FILE_PATH, arrLine);
        }

        public static void EnAbleFileLine(List<int> list)
        {
            string[] arrLine = File.ReadAllLines(HOSTS_FILE_PATH);

            foreach( int index in list)
            {
                arrLine[index] = arrLine[index].Substring(1, arrLine[index].Length - 1);
            }

            File.WriteAllLines(HOSTS_FILE_PATH, arrLine);          
        }

        public static void DisAbleFileLine(List<int> list)
        {
            string[] arrLine = File.ReadAllLines(HOSTS_FILE_PATH);
            foreach (int index in list)
            {
                arrLine[index] = "#" + arrLine[index];
            }
            File.WriteAllLines(HOSTS_FILE_PATH, arrLine);
        }


        // 전체활성화
        private void button4_Click(object sender, EventArgs e)
        {            
            List<int> list = new List<int>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Visible == true && 
                    !bool.Parse(row.Cells["CheckBox"].Value.ToString()) &&
                     !"A".Equals(row.Cells["Status"].Value))
                {                    
                    row.Cells["Status"].Value = "";
                    row.Cells["CheckBox"].Value = true;
                    row.Cells["Image"].Value = Properties.Resources.check;

                    list.Add(int.Parse(row.Cells["Line"].Value.ToString()));
                }
            }

            EnAbleFileLine(list);
        }

        // 전체 비활성화
        private void button5_Click(object sender, EventArgs e)
        {
            
            List<int> list = new List<int>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Visible == true &&
                    bool.Parse(row.Cells["CheckBox"].Value.ToString()) &&
                    !"A".Equals( row.Cells["Status"].Value))
                {
                    row.Cells["Status"].Value = "";
                    row.Cells["CheckBox"].Value = false;
                    row.Cells["Image"].Value = Properties.Resources.check; 
                        
                    list.Add(int.Parse(row.Cells["Line"].Value.ToString()));
                    
                }
            }

            DisAbleFileLine(list);
        }

        // 삭제
        private void button3_Click(object sender, EventArgs e)
        {         
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Selected == true && row.Visible == true)
                {
                    row.Cells["Status"].Value = "D";
                    row.Cells["Image"].Value = Properties.Resources.delete;
                }
            }
        }

        //저장
        private void button2_Click(object sender, EventArgs e)
        {

            List<int> updateIndex = new List<int>();
            List<string> updateData = new List<string>();

            List<string> addData = new List<string>();
            List<DataGridViewRow> addRows = new List<DataGridViewRow>();

            List<int> deleteIndex = new List<int>();
            List<DataGridViewRow> deleteRows = new List<DataGridViewRow>();





            for ( int i = 0; i < dataGridView1.Rows.Count; i++)            
            {
                DataGridViewRow row =  dataGridView1.Rows[i];

                if (row.Visible == true)
                {
                    if ("U".Equals(row.Cells["Status"].Value))
                    {
                        StringBuilder sb = new StringBuilder();

                        if (!bool.Parse(row.Cells["CheckBox"].Value.ToString()))
                        {
                            sb.Append("#");
                        }


                        sb.Append(row.Cells[1].Value);
                        sb.Append("    ");
                        sb.Append(row.Cells[2].Value);

                        if (row.Cells[3].Value.ToString().Trim().Length > 0)
                        {
                            sb.Append("    ");
                            sb.Append("#");
                            sb.Append(row.Cells[3].Value);
                        }

                        if (row.Cells[4].Value.ToString().Trim().Length > 0)
                        {
                            sb.Append("    ");
                            sb.Append("#");
                            sb.Append(row.Cells[4].Value);
                        }

                        updateIndex.Add(int.Parse(row.Cells["Line"].Value.ToString()));
                        updateData.Add(sb.ToString());

                        row.Cells["Status"].Value = "";
                        row.Cells["Image"].Value = Properties.Resources.check;
                    }

                    if ("A".Equals(row.Cells["Status"].Value) &&
                        row.Cells["Column2"].Value.ToString().Trim().Length > 0 &&
                        row.Cells["Column3"].Value.ToString().Trim().Length > 0)
                    {

                        StringBuilder sb = new StringBuilder();

                        if (!bool.Parse(row.Cells["CheckBox"].Value.ToString()))
                        {
                            sb.Append("#");
                        }


                        sb.Append(row.Cells[1].Value);
                        sb.Append("    ");
                        sb.Append(row.Cells[2].Value);

                        if (row.Cells[3].Value.ToString().Trim().Length > 0)
                        {
                            sb.Append("    ");
                            sb.Append("#");
                            sb.Append(row.Cells[3].Value);
                        }

                        if (row.Cells[4].Value.ToString().Trim().Length > 0)
                        {
                            sb.Append("    ");
                            sb.Append("#");
                            sb.Append(row.Cells[4].Value);
                        }

                        addData.Add(sb.ToString());
                        addRows.Add(row);
                    }

                    if ("D".Equals(row.Cells["Status"].Value))
                    {
                        deleteIndex.Add(int.Parse(row.Cells["Line"].Value.ToString()));
                        deleteRows.Add(row);
                    }
                }

            }

            if (updateData.Count > 0)
            {
                UpdateFile(updateIndex, updateData);
            }

            if (addData.Count > 0)
            {
              List<int> indexs = AddFile(addData);

              for( int i = 0; i < addRows.Count; i++) {
                DataGridViewRow row = addRows[i];
                row.Cells["Status"].Value = "";
                row.Cells["Image"].Value = Properties.Resources.check;
                row.Cells["Line"].Value = indexs[i];
              }
            }

            if (deleteIndex.Count > 0)
            {
                deleteFile(deleteIndex);

                for (int i = 0; i < deleteRows.Count; i++)
                {
                    DataGridViewRow row = deleteRows[i];
                    dataGridView1.Rows.RemoveAt(row.Index);
                }
            }

            //ReadHostsFIle();
        }

        // 추가
        private void button6_Click(object sender, EventArgs e)
        {
            object[] row = new object[]{
                true,
                "",
                "",
                comboBox1.SelectedItem.Equals("전체") ? "" : comboBox1.SelectedItem,
                "",
                "",
                "A",
                Properties.Resources.add
            };
            
            
            dataGridView1.Rows.Add(row);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {

        }

        // 삭제
        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                dataGridView1.Rows[e.RowIndex].Cells["Status"].Value = "U";
                ((DataGridViewImageCell)dataGridView1.Rows[e.RowIndex].Cells["Image"]).Value = Properties.Resources.update;
            }
        }
    }
}
