using System;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
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

        public Form1()
        {
            InitializeComponent();

            dt= new DataTable();
            dt.Columns.AddRange(new DataColumn[3]
{
                  new DataColumn("AccountName", typeof (string)),
                  new DataColumn("CompName", typeof (string)),
                  new DataColumn("Password", typeof (string))
           });

        }


        public static string GetPasswd(string computer)
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
            catch
            {
                return null;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[2].Visible = false;
        }

        private void BtnUsrSearch_Click(object sender, EventArgs e)
        {
                        

            SearchResultCollection results;
            DirectoryEntry de = new DirectoryEntry(GetCurrentDomainPath());

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
            if (txtUserName.Text != "" && txtUserPhone.Text != "")
            { ds.Filter = "(&(objectCategory=User)(objectClass=user)(name=" + txtUserName.Text + "*)(telephoneNumber=" + txtUserPhone.Text + "*))"; }
            else
            if (txtUserName.Text != "" && txtUserPhone.Text == "")
                   { ds.Filter = "(&(objectCategory=User)(objectClass=user)(name=" + txtUserName.Text + "*))"; }
                            else
                            if (txtUserName.Text == "" && txtUserPhone.Text != "")
                                { ds.Filter = "(&(objectCategory=User)(objectClass=user)(telephoneNumber=" + txtUserPhone.Text + "*))"; }
                                else return;
            // ds.Filter = "(&(objectCategory=User)(objectClass=person)(name=" + txtUserName.Text + "*)(| (telephoneNumber=" + txtUserPhone.Text + "*) (!(telephoneNumber=*) )))";

            results = ds.FindAll();

            lstBoxUsr.Items.Clear();
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

                lstBoxUsr.Items.Add(sr.Properties["userPrincipalName"][0].ToString());
                

            }
        }

        private string GetCurrentDomainPath()
        {
            DirectoryEntry de = new DirectoryEntry("LDAP://RootDSE");

            return "LDAP://" + de.Properties["defaultNamingContext"][0].ToString();
        }

        private void lstBoxUsr_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            string info = "";
            string usrName = lstBoxUsr.SelectedItem.ToString();

            try
            {
               
                //if (string.IsNullOrEmpty(txtUserName.Text.Trim()))

                using (PrincipalContext Context = new PrincipalContext(ContextType.Domain))
                using (UserPrincipal UP = UserPrincipal.FindByIdentity(Context, usrName))
                using (DirectoryEntry DE = (DirectoryEntry)UP.GetUnderlyingObject())
                {
                    if (DE.Properties.Contains("info")) info = DE.Properties["info"].Value.ToString();
                }
            }
            catch
            {
            }


            //if (string.IsNullOrEmpty(compName.Trim()))
            string[] compName = info.Split(';');

            txtUserName.Text = usrName;
            txtCompName.Text = compName[0];
            txtPasswd.Text = GetPasswd(compName[0]);

            dt.Rows.Add(txtUserName.Text, txtCompName.Text, txtPasswd.Text);


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
    }
}
