using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yokogawaread.DataClass
{
    /*-----------------数据库信息上传--------------------*/
    class dataAdapter
    {
        public void db_upload2to3(string number, string voltage1, string current1, string voltage2, string current2)
        {
            MySqlConnection con = ConnectionPool.getPool().getConnection();
            string content = "insert into yokogawa_record(number, voltage1, current1, voltage2, current2) values('" + number + "', '" + voltage1 + "', '" + current1 + "', '" + voltage2 + "', '" + current2 + "')";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            ConnectionPool.getPool().closeConnection(con);
        }

        public void db_upload1to1(string number, string voltage1, string current1)
        {
            MySqlConnection con = ConnectionPool.getPool().getConnection();
            string content = "insert into yokogawa_record(number, voltage1, current1) values('" + number + "', '" + voltage1 + "', '" + current1 + "')";
            MySqlCommand insert = new MySqlCommand(content, con);
            insert.ExecuteNonQuery();
            ConnectionPool.getPool().closeConnection(con);
        }
    }
}
