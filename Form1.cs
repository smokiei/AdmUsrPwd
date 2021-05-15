using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Windows.Forms;



namespace AdmUsrPwd


{
    //public class ClsUCP
    //{

    //    public string AccountName { get; set; }
    //    public string CompName { get; set; }
    //    public string Password { get; set; }
    //}

    public partial class Form1 : Form
    {
        public static DataTable dt;

        const string sBlocktext = "Заблокирован";

        const string sFilename = "history.txt";

        public Form1()
        {
            InitializeComponent();

            dt= new DataTable();
            dt.Columns.AddRange(new DataColumn[4]
{
                  new DataColumn("AccountName", typeof (string)),
                  new DataColumn("CompName", typeof (string)),
                  new DataColumn("Password", typeof (string)),
                  new DataColumn("Status", typeof (string))
           });

        }


        public string GetPasswd(string computer)
        {
            try
            {
                using (PrincipalContext Context = new PrincipalContext(ContextType.Domain))
                using (ComputerPrincipal CP = ComputerPrincipal.FindByIdentity(Context, computer))
                using (DirectoryEntry DE = (DirectoryEntry)CP.GetUnderlyingObject())
                {
                    if (DE.Properties.Contains("ms-Mcs-AdmPwd")) return DE.Properties["ms-Mcs-AdmPwd"].Value.ToString();
                }
                return null;
            }
            catch (Exception oe)
            {
                Console.WriteLine("Exception : " + oe.Message);
                return "";
            }
        }

        public void FillDT(string usrName)
        {
            string info = "";
            string usrStatus = "";
            try
            {
                using (PrincipalContext Context = new PrincipalContext(ContextType.Domain))
                using (UserPrincipal UP = UserPrincipal.FindByIdentity(Context, usrName))
                using (DirectoryEntry DE = (DirectoryEntry)UP.GetUnderlyingObject())
                {
                    if (DE.Properties.Contains("info")) info = DE.Properties["info"].Value.ToString();

                    if (DE.Properties.Contains("userAccountControl")) usrStatus = DE.Properties["userAccountControl"].Value.ToString();
                }
            }
            catch (Exception oe)
            {
                MessageBox.Show("Exception : " + oe.Message);
                Console.WriteLine("Exception : " + oe.Message);
            }

            if (!string.IsNullOrEmpty(usrStatus))
                if (isBlocked(usrStatus)) usrStatus = sBlocktext;
                    else usrStatus = "";

            string[] compName = info.Split(';');
            if (!string.IsNullOrEmpty(compName[0]) )
            {
                txtUserName.Text = usrName;
                txtCompName.Text = compName[0];
                txtPasswd.Text = GetPasswd(compName[0]);

                dt.Rows.Add(txtUserName.Text, txtCompName.Text, txtPasswd.Text, usrStatus);
            }
        }

        public bool isBlocked(string userAccountControl)
        {
            int iUAC = Int32.Parse(userAccountControl);
            iUAC = iUAC & 2;
            if (iUAC != 0)
            {
                return true;
            }

            return false;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[2].Visible = false;
        }

        private void BtnUsrSearch_Click(object sender, EventArgs e)
        {
                        
            //вынести в инициализацию?
            SearchResultCollection results;
            DirectoryEntry de;

            string scurrentAD = GetCurrentDomainPath();
            if (scurrentAD == "")
            {   MessageBox.Show("НЕ НАЙДЕН AD");
                return;
            }


            de = new DirectoryEntry(scurrentAD);
            // Build User Searcher
            DirectorySearcher ds = null;

            ds = new DirectorySearcher(de);

            // Full Name
            //ds.PropertiesToLoad.Add("name");

            // Email Address
            //ds.PropertiesToLoad.Add("mail");

            // First Name
            //ds.PropertiesToLoad.Add("givenname");

            // Last Name (Surname)
            //ds.PropertiesToLoad.Add("sn");

            // Login Name
            ds.PropertiesToLoad.Add("userPrincipalName");
            ds.PropertiesToLoad.Add("userAccountControl");
            //(lockoutTime>=1)

            //
            if (txtUserName.Text.Trim() != "" )
            { ds.Filter = "(&(objectCategory=User)(objectClass=user)(name=" + txtUserName.Text.Trim() + "*))"; }

            // ds.Filter = "(&(objectCategory=User)(objectClass=person)(name=" + txtUserName.Text + "*)(| (telephoneNumber=" + txtUserPhone.Text + "*) (!(telephoneNumber=*) )))";
            try
            {
                results = ds.FindAll();
          

                lstViewUsr.Items.Clear();
                foreach (SearchResult sr in results)
                {

                    //MessageBox.Show(sr.Properties["name"][0].ToString());
                    //MessageBox.Show(sr.Properties["mail"][0].ToString());
                    //MessageBox.Show(sr.Properties["userPrincipalName"][0].ToString());
                    //MessageBox.Show(sr.Properties["userAccountControl"][0].ToString());

                    //512 = Enabled
                    //514 = Disabled
                    //66048 = Enabled, password never expires
                    //66050 = Disabled, password never expires
                    //https://winitpro.ru/index.php/2018/05/14/convertaciya-atributa-useraccountcontrol-v-ad/#:~:text=Каждый%20из%20этих%20атрибутов%20учетной,вместо%20этого%20используется%20атрибут%20UserAccountControl.


                    ListViewItem item = new ListViewItem( sr.Properties["userPrincipalName"][0].ToString());

                   if (isBlocked(sr.Properties["userAccountControl"][0].ToString())) { 
                        item.BackColor = Color.Red;
                        item.SubItems.Add(sBlocktext);
                    }
                    lstViewUsr.Items.Add(item);
                }

            }

            catch (Exception oe)
            {
                Console.WriteLine("Exception : " + oe.Message);
            }

        }

        private string GetCurrentDomainPath()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");

            try
            {
                return "LDAP://" + de.Properties["defaultNamingContext"][0].ToString();
            }
            catch (Exception oe)
            {
                Console.WriteLine("Exception : " + oe.Message);
                return "";
            }
        }

       

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            txtUserName.Text = Convert.ToString(dataGridView1.CurrentRow.Cells[0].Value);
            txtCompName.Text= Convert.ToString(dataGridView1.CurrentRow.Cells[1].Value);
            txtPasswd.Text = Convert.ToString(dataGridView1.CurrentRow.Cells[2].Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtPasswd.Text!="") Clipboard.SetText(txtPasswd.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtUserName.Text = "";
            lstViewUsr.Items.Clear();
        }

        private void lstViewUsr_SelectedIndexChanged(object sender, EventArgs e)
        {
           

            if (lstViewUsr.SelectedIndices.Count <= 0)
            {
                return;
            }

            //string usrName = lstViewUsr.Items[lstViewUsr.SelectedIndices[0]].Text;

            //if (  lstViewUsr.Items[lstViewUsr.SelectedIndices[0]].SubItems.Count >1 ) 
            //       usrStatus = lstViewUsr.Items[lstViewUsr.SelectedIndices[0]].SubItems[1].Text;

            FillDT(lstViewUsr.Items[lstViewUsr.SelectedIndices[0]].Text);


           
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if ( dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString() == sBlocktext ) 
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            //csv
            try
            {
                using (StreamWriter sw = new StreamWriter(sFilename, false, System.Text.Encoding.Default))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        sw.WriteLine(row[0].ToString());
                    }
                }
            }
            catch (Exception oe)
            {
                Console.WriteLine("Exception : " + oe.Message);
            }
        }
        

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader(sFilename, System.Text.Encoding.Default))
                {
                string line;
                while ((line =  sr.ReadLine()) != null) 
                    FillDT(line);
                }

            }
            catch (Exception oe)
            {
                Console.WriteLine("Exception : " + oe.Message);
            }
        }
    }
}
