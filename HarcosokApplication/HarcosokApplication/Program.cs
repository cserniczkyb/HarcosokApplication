using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HarcosokApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (MySqlConnection conn = new MySqlConnection("Server=localhost;Database=cs_harcosok;Uid=root;Pwd=;"))
                {
                    conn.Open();
                    Application.Run(new Form1(conn));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Hiba " + e);
            }       
             
        }
    }
}

