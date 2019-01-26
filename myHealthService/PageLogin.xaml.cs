using Newtonsoft.Json;
using OnlineHealthService.models;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace myHealthService
{
    /// <summary>
    /// Interaction logic for PageLogin.xaml
    /// </summary>
    public partial class PageLogin : Page
    {
        public PageLogin()
        {
            InitializeComponent();
        }

        Profile user = new Profile();

        string tokenResponse;
        string ProfileXML;

        public static string GPname;
        public static string GPemail;
        

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            
            if (textEmail.Text.Length == 0)
            {
                MessageBox.Show("Error: Enter Your Email");
                textEmail.Focus();
            }
            else if (!Regex.IsMatch(textEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                MessageBox.Show("Error: Enter Valid Email");
                textEmail.Select(0, textEmail.Text.Length);
                textEmail.Focus();
            }
            else
            {
                string email = textEmail.Text;
                string password = passwordbox.Password;
                SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=PatientDB;Integrated Security=True");
                con.Open();
                SqlCommand cmd = new SqlCommand("Select * from Registration where Email='" + email + "'  and password='" + password + "'", con);
                cmd.CommandType = CommandType.Text;
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    var DBtable = dataSet.Tables[0].Rows[0];
                    string username = DBtable["FirstName"].ToString() + " " + DBtable["LastName"].ToString();
                    string fulluserinfo = DBtable["FirstName"].ToString() + " " + DBtable["LastName"].ToString() + "\r\n" + DBtable["Email"].ToString() +  "\r\n" + DBtable["Gender"].ToString() + "\r\n" + DBtable["Birthday"].ToString() + "\r\n" + DBtable["PhoneNo"].ToString() + "\r\n" + DBtable["Address1"].ToString() + "\r\n" + DBtable["Address2"].ToString() + "\r\n" + DBtable["Postcode"].ToString() + "\r\n" + DBtable["State"].ToString();
                    MessageBox.Show("Welcome " + username);
                    PagePortal pagepor = new PagePortal();
                    pagepor.getGooglePlusUser(username);
                    pagepor.getuserinfo(fulluserinfo);
                    this.NavigationService.Navigate(pagepor);
                }
                else
                {
                    MessageBox.Show("Error: Email or Password incorrect");
                }
                con.Close();
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            PageRegister pagereg = new PageRegister();
            this.NavigationService.Navigate(pagereg);
        }

        private async void BtnGoogleP_Click(object sender, RoutedEventArgs e)
        {
            SociaLogin SL = new SociaLogin();
            SL.ShowDialog();
            tokenResponse = SL.access.Access_token;
            string userinfoRequestURI = "https://www.googleapis.com/plus/v1/people/me";

            try
            {
                // sends the request
                HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
                userinfoRequest.Method = "GET";
                userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}",SL.access.Access_token));
                userinfoRequest.ContentType = "application/x-www-form-urlencoded";
                userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
                using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
                {
                    // reads response body
                    string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                    var node = JsonConvert.DeserializeXNode(userinfoResponseText, "Profile");
                    ProfileXML = node.ToString();

                    XmlSerializer serializer = new XmlSerializer(typeof(Profile));
                    XmlReader reader = XmlReader.Create(new StringReader(ProfileXML));
                    user = (Profile)serializer.Deserialize(reader);

                    GPname = user.displayName;
                    GPemail = user.emails.myemail;

                    PagePortal pagepor = new PagePortal();
                    this.NavigationService.Navigate(pagepor);
                    pagepor.getGooglePlusUser(GPname);
                    //HP.ShowDialog();
                    //this.Hide();
                    //Por.Show();
                }
            }
            catch (System.NullReferenceException)
            {
                MessageBox.Show("Google+ Log In Error");
            }
        }

        private void BtnPint_Click(object sender, RoutedEventArgs e)
        {

        }
        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus;
        }
    }
}
