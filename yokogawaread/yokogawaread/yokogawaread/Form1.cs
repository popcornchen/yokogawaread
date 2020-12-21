using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TmctlAPINet;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace yokogawaread
{
    public partial class Form1 : Form
    {
        TMCTL cTmctl = new TMCTL();
        StringBuilder encode = new StringBuilder(100);
        StringBuilder buff = new StringBuilder(25600);
        StringBuilder buff_1 = new StringBuilder(25600);
        StringBuilder buff_2 = new StringBuilder(25600);
        StringBuilder buff_3 = new StringBuilder(25600);
        StringBuilder buff_4 = new StringBuilder(25600);
        int ret = 0;
        int id = 0;
        int rlen = 0;
        int currentcycletime = 0;
        int tag = 0;
        int recording_tag = 0;
        bool con_state = false;
        private System.Object lockThis = new System.Object();

        System.Threading.Timer threadTimer = null;

        public static string constr = "server=115.236.52.123; user=root; database=test; port=8332; pwd=gotmNAOL6^NcKJ9$";
        public Form1()
        {
            /*-----------------窗体初始化--------------------*/
            InitializeComponent();

            textBox1.Text = "192.168.0.100";//192.168.0.100
            textBox2.Text = "Epson_C4_V70_A90 ";
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
            this.comboBox3.SelectedIndex = 0;
            button2.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            db_initial();
            for (int i = 1; i <= 100; ++i)
            {
                MySqlConnection con = DataClass.ConnectionPool.getPool().getConnection();
                DataClass.ConnectionPool.getPool().closeConnection(con);
            }
            tag = get_tag();
        }

        /*-----------------开始采集按钮--------------------*/
        private void button1_Click(object sender, EventArgs e)
        {
            ExecuteCommunicate();
            int frequency = 0;
            if (comboBox2.SelectedIndex == 1)
            {
                frequency = 50;
            }
            else if (comboBox2.SelectedIndex == 0)
            {
                frequency = 20;
            }
            else if (comboBox2.SelectedIndex == 2)
            {
                frequency = 100;
            }
            else
            {
                frequency = 1000;
            }
            threadcycle(frequency);
            button2.Enabled = true;
            button1.Enabled = false;
        }

        /*-----------------设备参数初始化--------------------*/
        private int ExecuteCommunicate()
        {

            int ret = 0;
            int id = 0;
            int rlen = 0;


            DEVICELIST[] list = new DEVICELIST[10];


            StringBuilder encode = new StringBuilder(100);
            StringBuilder buff = new StringBuilder(25600);
            StringBuilder buff_1 = new StringBuilder(25600);
            StringBuilder buff_2 = new StringBuilder(25600);

            // ex9: VXI-11 IP = 192.168.0.100
            ret = cTmctl.Initialize(TMCTL.TM_CTL_VXI11, textBox1.Text.ToString(), ref id);

            ret = cTmctl.SetTerm(id, 2, 1);
            if (ret != 0)
            {
                return cTmctl.GetLastError(id);
            }

            ret = cTmctl.SetTimeout(id, 300);
            if (ret != 0)
            {
                return cTmctl.GetLastError(id);
            }

            ret = cTmctl.SetRen(id, 1);
            if (ret != 0)
            {
                return cTmctl.GetLastError(id);
            }

            // Send *RST
            ret = cTmctl.Send(id, ":Start");
            if (ret != 0)
            {
                return cTmctl.GetLastError(id);
            }
            return 0;
        }

        /*-----------------数据读写执行函数--------------------*/
        private void threadcycle(int frequency)
        {

            if (comboBox1.SelectedIndex == 1)
            {
                threadTimer = new System.Threading.Timer(new System.Threading.TimerCallback(read_2to3), null, 1000, frequency);
            }
            else if (comboBox1.SelectedIndex == 0)
            {
                threadTimer = new System.Threading.Timer(new System.Threading.TimerCallback(read_1to1), null, 1000, frequency);
            }
            else
            {

            }
        }

        /*-----------------数据读写函数--------------------*/
        private void read_1to1(object state)
        {

            //db_initial();
            lock (lockThis)
            {
                String timenow = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff");



                ret = cTmctl.Send(id, ":MEASure:CHANnel1:SCHannel1:AVERage:value?");
                ret = cTmctl.Receive(id, buff_1, buff.Capacity, ref rlen);

                ret = cTmctl.Send(id, ":MEASure:CHANnel1:SCHannel2:AVERage:value?");
                ret = cTmctl.Receive(id, buff_2, buff.Capacity, ref rlen);

                //Console.WriteLine(buff_1.ToString().Substring(26) + "++++1");
                //Console.WriteLine(buff_2.ToString().Substring(26) + "++++2");

                currentcycletime = currentcycletime + 1;
                SetMsg("当前记录时间:" + timenow + "    " + textBox2.Text.ToString() + "当前循环次数" + currentcycletime.ToString());
                SetMsg("\r\n");

                db_upload1to1((tag + currentcycletime).ToString(), buff_1.ToString().Substring(26), buff_2.ToString().Substring(26));
                //db_upload(buff_1.ToString().Substring(26), "current1");//26                
                SetMsg(buff_1.ToString().Substring(26) + "    ");

                //db_upload(buff_2.ToString().Substring(26), "voltage1");
                SetMsg(buff_2.ToString().Substring(26) + "    ");
                SetMsg("\r\n");
            }

            //threadTimer.Dispose();
            //System.Threading.Thread.CurrentThread.Abort();
        }
        private void read_2to3(object state)
        {

            //db_initial();
            lock (lockThis)
            {
                String timenow = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff");



                ret = cTmctl.Send(id, ":MEASure:CHANnel1:SCHannel1:AVERage:value?");
                ret = cTmctl.Receive(id, buff_1, buff.Capacity, ref rlen);

                ret = cTmctl.Send(id, ":MEASure:CHANnel1:SCHannel2:AVERage:value?");
                ret = cTmctl.Receive(id, buff_2, buff.Capacity, ref rlen);


                ret = cTmctl.Send(id, ":MEASure:CHANnel2:SCHannel1:AVERage:value?");
                ret = cTmctl.Receive(id, buff_3, buff.Capacity, ref rlen);

                ret = cTmctl.Send(id, ":MEASure:CHANnel2:SCHannel2:AVERage:value?");
                ret = cTmctl.Receive(id, buff_4, buff.Capacity, ref rlen);

                //Console.WriteLine(buff_3.ToString().Substring(26) + "++++3");
                //Console.WriteLine(buff_4.ToString().Substring(26) + "++++4");
                //Console.WriteLine(buff_1.ToString().Substring(26) + "++++1");
                //Console.WriteLine(buff_2.ToString().Substring(26) + "++++2");

                currentcycletime = currentcycletime + 1;
                SetMsg("当前记录时间:" + timenow + textBox2.Text.ToString() + "当前循环次数" + currentcycletime.ToString());
                SetMsg("\r\n");


                SetMsg(buff_1.ToString().Substring(26) + "    ");
                //db_upload(buff_1.ToString().Substring(26), "current1");

                SetMsg(buff_2.ToString().Substring(26) + "    ");
                //db_upload(buff_2.ToString().Substring(26), "voltage1");

                SetMsg(buff_3.ToString().Substring(26) + "    ");
                //db_upload(buff_3.ToString().Substring(26), "current2");

                SetMsg(buff_4.ToString().Substring(26) + "    ");
                //db_upload(buff_4.ToString().Substring(26), "voltage2");

                SetMsg("\r\n");
                db_upload2to3((tag + currentcycletime).ToString(), buff_1.ToString().Substring(26), buff_2.ToString().Substring(26), buff_3.ToString().Substring(26), buff_4.ToString().Substring(26));
            }

            //threadTimer.Dispose();
            //System.Threading.Thread.CurrentThread.Abort();
        }

        /*-----------------信息日志函数--------------------*/
        public void SetMsg(string msg)
        {
            richTextBox1.Invoke(new Action(() => { richTextBox1.AppendText(msg); }));
        }

        /*-----------------数据库信息初始化--------------------*/
        public int db_initial()
        {
            MySqlConnection con = new MySqlConnection(constr);
            try
            {
                con.Open();
                SetMsg("connection successful");
            }
            catch (Exception)
            {
                SetMsg("connection error");
                return -1;
            }

            CheckForIllegalCrossThreadCalls = false;
            string[] condition = { ";" };
            string[] constr_split = constr.Split(condition, StringSplitOptions.RemoveEmptyEntries);
            textBox3.Text = constr_split[0];
            textBox4.Text = constr_split[1];
            textBox5.Text = constr_split[4];

            //string command_init = "Truncate table yokogawa_record";
            //MySqlCommand delete = new MySqlCommand(command_init, con);
            //delete.ExecuteNonQuery();  //初始化清空一下表，是否需要保留数据？
            con.Close();

            return 0;
        }

        /*-----------------数据库相关操作--------------------*/
        public int db_upload(string data, string column)
        {
            MySqlConnection con = new MySqlConnection(constr);
            try
            {
                con.Open();
                SetMsg("connection successful");
            }
            catch (Exception)
            {
                SetMsg("connection error");
                return -1;
            }
            string content = "insert into yokogawa_record(" + column + ") values('" + data + "')";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            con.Close();

            return 0;
        }

        public void db_upload2to3(string number, string voltage1, string current1, string voltage2, string current2)
        {
            MySqlConnection con = DataClass.ConnectionPool.getPool().getConnection();
            string content = "insert into yokogawa_record(number, voltage1, current1, voltage2, current2) values('" + number + "', '" + voltage1 + "', '" + current1 + "', '" + voltage2 + "', '" + current2 + "')";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            DataClass.ConnectionPool.getPool().closeConnection(con);
        }
        public void db_upload1to1(string number, string voltage1, string current1)
        {
            MySqlConnection con = DataClass.ConnectionPool.getPool().getConnection();
            string content = "insert into yokogawa_record(number, voltage1, current1) values('" + number + "', '" + voltage1 + "', '" + current1 + "')";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            DataClass.ConnectionPool.getPool().closeConnection(con);
        }

        public void refresh_tag(string cycle)
        {
            MySqlConnection con = DataClass.ConnectionPool.getPool().getConnection();
            string content = "update yokogawa_monitor set SartPosition='" + cycle + "' where line=0";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            DataClass.ConnectionPool.getPool().closeConnection(con);
        }

        public int get_tag()
        {
            MySqlConnection con = DataClass.ConnectionPool.getPool().getConnection();
            string content = "select StartPosition from yokogawa_monitor where line=0";
            MySqlCommand cmd = new MySqlCommand(content, con);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            DataClass.ConnectionPool.getPool().closeConnection(con);
            return (Convert.ToInt32(dt.Rows[0]["StartPosition"]));
        }

        /*-----------------停止采集按钮--------------------*/
        private void button2_Click(object sender, EventArgs e)
        {

            MySqlConnection con = new MySqlConnection(constr);
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
            //结束数采过程
            try { threadTimer.Dispose(); }
            catch { }

            ret = cTmctl.Send(id, "Stop");
            ret = cTmctl.Finish(id);
            //ret = cTmctl.Finish(id);
            if (ret != 0)
            {
                MessageBox.Show(cTmctl.GetLastError(id).ToString());
            }

            refresh_tag(currentcycletime.ToString());
            con_state = false;
            button1.Enabled = true;
            button2.Enabled = false;
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        /*-----------------清空数据按钮--------------------*/
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            currentcycletime = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
