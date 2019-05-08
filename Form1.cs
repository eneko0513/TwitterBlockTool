using CoreTweet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlockSpecificUsersOnTwitter
{
    public partial class Form1 : Form
    {
        private DataTable _dataTable = null;
        private DataSet _dataSet = null;

        OAuth.OAuthSession session;
        public Tokens tokens;

        public Form1()
        {

            InitializeComponent();

            // データセットの初期化
            _dataTable = new DataTable();
            _dataSet = new DataSet();

            // データソースの初期化
            bindingSource_main.DataSource = _dataSet;

            // データグリッドの初期化
            advancedDataGridView_main.DataSource = bindingSource_main;

            // バインディングソースの設定
            SetHeaderData();
            session = OAuth.Authorize("apikey", "token");
            advancedDataGridView_main.Columns[3].Width = 470;
            GetPinCode(session);
            button2.Enabled = false;
        }


        private void SetHeaderData()
        {
            try
            {
                _dataTable = _dataSet.Tables.Add("Table");
                _dataTable.Columns.Add("ブロック対象", typeof(Boolean));
                _dataTable.Columns.Add("ユーザーID", typeof(string));
                _dataTable.Columns.Add("ユーザー名", typeof(string));
                _dataTable.Columns.Add("ツイート内容", typeof(string));
                bindingSource_main.DataMember = _dataTable.TableName;
            }
            catch
            {
            }
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            //トークンの発行
            string pincode = textBox1.Text;
            tokens = OAuth.GetTokens(session, pincode);

            label2.Text = "状態：" + tokens.ScreenName + "として認証済み";
        }

        private void GetPinCode(CoreTweet.OAuth.OAuthSession session)
        {
            System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            TweetSearch();
        }

        private void TweetSearch()
        {
            // グローバル検索
            string keyword = textBox2.Text;
            if (!keyword.Equals(""))
            {
                _dataTable.Clear();
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                var result = tokens.Search.Tweets(count => 100, q => keyword);
                // countは読み込み数。指定しなければDefoultの数値が入る。
                foreach (var value in result)
                {
                    string scrName = value.User.ScreenName;     // @User_ID
                    string name = value.User.Name;              // ユーザー名
                    string text = value.Text;                   // Tweet
                    AddUserData(false, scrName, name, text);
                }
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
        }


        private void AddUserData(Boolean check, string scrName, string name, string text)
        {
            try
            {
                object[] newrow = new object[] {
                        check,
                        scrName,
                        name,
                        text
                };
                _dataTable.Rows.Add(newrow);
                Application.DoEvents();
            }
            catch
            {
                // StatsBarDisplayChange(1);
            }
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox2.Text.Equals("") ? false : true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }

        private void AdvancedDataGridView_main_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == 0) && (e.RowIndex != (-1)))
            {
                advancedDataGridView_main.Rows[e.RowIndex].Cells[0].Value = !Convert.ToBoolean(advancedDataGridView_main.Rows[e.RowIndex].Cells[0].Value);
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < advancedDataGridView_main.RowCount; i++)
            {
                advancedDataGridView_main.Rows[i].Cells[0].Value = true;
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < advancedDataGridView_main.RowCount; i++)
            {
                advancedDataGridView_main.Rows[i].Cells[0].Value = false;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("ブロックしちゃいます？",
            "ブロック",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Exclamation,
            MessageBoxDefaultButton.Button2);

            //何が選択されたか調べる
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < advancedDataGridView_main.RowCount; i++)
                {
                    if (Convert.ToBoolean(advancedDataGridView_main.Rows[i].Cells[0].Value))
                    {
                        tokens.Blocks.Create(screen_name => advancedDataGridView_main.Rows[i].Cells[1].Value);
                    }
                }
                DialogResult result2 = MessageBox.Show("ブロックが完了しました",
                "完了",
                MessageBoxButtons.OK);
            }
            else if (result == DialogResult.No)
            {
            }
            else if (result == DialogResult.Cancel)
            {
            }
        }
    }
}
