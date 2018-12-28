using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace HarcosokApplication
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;

        public Form1(MySqlConnection conn)
        {
            InitializeComponent();
            this.conn = conn;
            CreateTables();
            ComboBoxUpdate();
            HarcosListBoxUpdate();
        }

        public void CreateTables()
        {
            var command = conn.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS harcosok(
                id INTEGER PRIMARY KEY AUTO_INCREMENT,
                nev VARCHAR(128) NOT NULL UNIQUE,
                letrejott DATE NOT NULL        
            )";
            command.ExecuteNonQuery();

            var command2 = conn.CreateCommand();
            command2.CommandText = @"
            CREATE TABLE IF NOT EXISTS kepessegek(
                id INTEGER PRIMARY KEY AUTO_INCREMENT,
                nev VARCHAR(128) NOT NULL,
                leiras VARCHAR(128) NOT NULL,
                harcos_id INTEGER NOT NULL,
                FOREIGN KEY (harcos_id) REFERENCES harcosok(id)    
            )";
            command2.ExecuteNonQuery();
        }

        public void letrehozButton_Click(object sender, EventArgs e)
        {
            if (harcosNeveTextBox.Text == "" || harcosNeveTextBox.Text == null)
            {
                MessageBox.Show("Adjon meg egy nevet");
            }
            else
            {
                string nev = harcosNeveTextBox.Text;
                DateTime letrejott = DateTime.Now;

                var ellenorzes = conn.CreateCommand();
                ellenorzes.CommandText = "SELECT COUNT(*) FROM harcosok WHERE nev = @nev";
                ellenorzes.Parameters.AddWithValue("@nev", nev);
                var darab = (long)ellenorzes.ExecuteScalar();
                if (darab != 0)
                {
                    MessageBox.Show("A username mar letezik");
                    return;
                }
                var command = conn.CreateCommand();
                command.CommandText = "INSERT INTO harcosok (nev,letrejott) VALUES (@nev,@letrejott)";
                command.Parameters.AddWithValue("@nev", nev);
                command.Parameters.AddWithValue("@letrejott", letrejott);
                int erintettSorok = command.ExecuteNonQuery();
                ComboBoxUpdate();
                HarcosListBoxUpdate();
            }          
        }
        public void ComboBoxUpdate()
        {
            var command = conn.CreateCommand();
            command.CommandText = "SELECT nev, letrejott FROM harcosok ORDER BY nev";
            using (var reader = command.ExecuteReader())
            {
                hasznaloComboBox.Items.Clear();
                while (reader.Read())
                {
                    var nev = reader.GetString("nev");
                    hasznaloComboBox.Items.Add(nev);
                }
            }
        }
        public void HarcosListBoxUpdate()
        {
            var command = conn.CreateCommand();
            command.CommandText = "SELECT nev, letrejott FROM harcosok ORDER BY nev";
            using (var reader = command.ExecuteReader())
            {
                harcosokListBox.Items.Clear();
                while (reader.Read())
                {
                    var nev = reader.GetString("nev");
                    var letrejott = reader.GetDateTime("letrejott");
                    harcosokListBox.Items.Add(string.Format("{0} - {1:yyyy.MM.dd.}",nev,letrejott));
                }
            }
        }

        private void hozzaadButton_Click(object sender, EventArgs e)
        {
            if (kepessegNeveTextBox.Text == "" || kepessegNeveTextBox.Text == null || leirasTextBox.Text == "" || leirasTextBox.Text == null)
            {
                MessageBox.Show("Töltsön ki minden mezőt");
            }
            else {              
                var command = conn.CreateCommand();
                string harcosnev = hasznaloComboBox.SelectedItem.ToString();
                var harcos_id = 0;
                command.CommandText = "SELECT id FROM harcosok where nev = @nev";
                command.Parameters.AddWithValue("@nev", harcosnev);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        harcos_id = reader.GetInt32("id");
                    }
                }

                string nev = kepessegNeveTextBox.Text;
                string leiras = leirasTextBox.Text;

                var command2 = conn.CreateCommand();
                command2.CommandText = "INSERT INTO kepessegek (nev,leiras,harcos_id) VALUES (@nev, @leiras, @harcos_id)";
                command2.Parameters.AddWithValue("@nev", nev);
                command2.Parameters.AddWithValue("@leiras", leiras);
                command2.Parameters.AddWithValue("@harcos_id", harcos_id);
                int erintettSorok = command2.ExecuteNonQuery();
                leirasTextBox.Clear();
                kepessegNeveTextBox.Clear();
                kepessegekListBox.Refresh();
            }         
        }

        private void harcosokListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            kepessegLeirasaTextBox.Clear();
            kepessegekListBox.Items.Clear();
            string nev = harcosokListBox.SelectedItem.ToString();
            string[] split = nev.Split();
            string harcosnev = split[0];
            

            var command = conn.CreateCommand();
            command.CommandText = "SELECT kepessegek.nev FROM kepessegek INNER JOIN harcosok ON kepessegek.harcos_id = harcosok.id where harcosok.nev = @nev";
            command.Parameters.AddWithValue("@nev", harcosnev);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    kepessegekListBox.Items.Add(reader.GetString("nev"));
                }
            }
            int erintettSorok = command.ExecuteNonQuery();
        }

        private void kepessegekListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string kepessegnev = kepessegekListBox.SelectedItem.ToString();
            var command = conn.CreateCommand();
            command.CommandText = "SELECT leiras FROM kepessegek where nev = @nev";
            command.Parameters.AddWithValue("@nev", kepessegnev);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    kepessegLeirasaTextBox.Text = reader.GetString("leiras");
                }
            }
            int erintettSorok = command.ExecuteNonQuery();
        }

        private void torlesButton_Click(object sender, EventArgs e)
        {
            if (kepessegekListBox.SelectedItem == null || kepessegekListBox.SelectedItem.Equals(""))
            {
                MessageBox.Show("Nincs kiválasztott képesség");
            }
            else
            {
                string nev = harcosokListBox.SelectedItem.ToString();
                string[] split = nev.Split();
                string harcosnev = split[0];
                var harcos_id = 0;
                var command = conn.CreateCommand();
                command.CommandText = "SELECT id FROM harcosok where nev = @nev";
                command.Parameters.AddWithValue("@nev", harcosnev);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        harcos_id = reader.GetInt32("id");
                    }
                }
                string kepessegnev = kepessegekListBox.SelectedItem.ToString();
                var command2 = conn.CreateCommand();
                command2.CommandText = "DELETE FROM kepessegek where nev = @nev AND harcos_id = @id";
                command2.Parameters.AddWithValue("@nev", kepessegnev);
                command2.Parameters.AddWithValue("@id", harcos_id);
                int erintettSorok = command2.ExecuteNonQuery();
                kepessegekListBox.Refresh();
            }
        }

        private void modositButton_Click(object sender, EventArgs e)
        {
            if (kepessegekListBox.SelectedItem == null || kepessegekListBox.SelectedItem.Equals(""))
            {
                MessageBox.Show("Nincs kiválasztott képesség");
            }
            else
            {
                string leiras = kepessegLeirasaTextBox.Text;
                string kepessegnev = kepessegekListBox.SelectedItem.ToString();
                var command = conn.CreateCommand();
                command.CommandText = "UPDATE kepessegek SET leiras = @leiras where nev = @nev";
                command.Parameters.AddWithValue("@leiras", leiras);
                command.Parameters.AddWithValue("@nev", kepessegnev);
                int erintettSorok = command.ExecuteNonQuery();
                kepessegLeirasaTextBox.Refresh();
            }
        }
    }
}
