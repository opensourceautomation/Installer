﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace DBInstall
{
    public partial class Form1 : Form 
    {
        string directory = "";
        string existing = "";
        string current = "0.4.7";
        string newVersion = "0.4.8";
        string machine = "";
        MySqlConnection connection;

        public Form1(string dir, string mach)
        {
            directory = dir;
            machine = mach;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.IO.FileInfo file = new System.IO.FileInfo("install_log.log");
            file.Directory.Create();
            btnOpenFile.Visible = false;

            if (machine == "Server")
            {
                if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\MySQL Server 5.7\\bin\\mysql.exe"))
                {
                    lblMySQL.Text = "MySQL Found at:";
                    txbxLocation.Text = Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\MySQL Server 5.7\\bin\\mysql.exe";
                }
                else if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\MySQL Server 5.6\\bin\\mysql.exe"))
                {
                    lblMySQL.Text = "MySQL Found at:";
                    txbxLocation.Text = Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\MySQL Server 5.6\\bin\\mysql.exe";
                }
                else if (File.Exists(Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\bin\\mysql.exe"))
                {
                    lblMySQL.Text = "MySQL Found at:";
                    txbxLocation.Text = Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\bin\\mysql.exe";
                }
                else
                {
                    lblMySQL.Text = "MySQL NOT Found! You must Browse for it.";
                    txbxLocation.Text = Environment.GetEnvironmentVariable("ProgramFiles");
                    btnOpenFile.Visible = true;
                }

                string ConnectionString = string.Format("Uid={0};Password={1};Server={2};Port={3};", "osae", "osaePass", "localhost", "3306");

                    connection = new MySqlConnection(ConnectionString);
                    try
                    {
                        connection.Open();
                        // osae user found and connection succeeded.  Now Check for DB.
                        addToLog("Connected to MySQL Server.");
                        MySqlCommand command;
                        MySqlDataAdapter adapter;
                        DataSet dataset = new DataSet();
                        command = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'osae'", connection);
                        adapter = new MySqlDataAdapter(command);
                        adapter.Fill(dataset);
                        connection.Close();
                        int count = dataset.Tables[0].Rows.Count;

                        if (count == 1)
                        {
                            // DB found.  Need to upgrade.  First find out current version.
                            addToLog("Found OSA database.  Checking to see if we need to upgrade.");
                            dataset = new DataSet();
                            ConnectionString = string.Format("Uid={0};Pwd={1};Server={2};Port={3};Database={4};allow user variables=true", "osae", "osaePass", "localhost", "3306", "osae");
                            connection = new MySqlConnection(ConnectionString);
                            connection.Open();
                            command = new MySqlCommand("select property_value from osae_object_property p inner join osae_object_type_property tp on p.object_type_property_id = tp.property_id inner join osae_object o on o.object_id = p.object_id where object_name = 'SYSTEM' and property_name = 'DB Version'", connection);
                            adapter = new MySqlDataAdapter(command);
                            adapter.Fill(dataset);
                            if (dataset.Tables[0].Rows[0][0].ToString() == "") current = "0.4.7";
                            else current = dataset.Tables[0].Rows[0][0].ToString();

                            if (current == newVersion)
                                Close();
                            else
                            {
                                lblFoundDB.Text = "Found version " + current + "\nClick button to upgrade to v" + newVersion;
                                btnInstall.Text = "Upgrade";
                            }

                        }
                        else
                        {
                            // DB not found.  Need to install.
                            addToLog("No OSA database found.  We need to install it.");
                            MySqlScript script = new MySqlScript(connection, File.ReadAllText(directory + "\\osae.sql"));
                            script.Execute();
                            connection.Close();
                            Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        // osae user not found.  Must be an existing mySql install without OSA installed.  Need to ask for root password.
                        addToLog("Unable to connect with osae account: " + ex.Message);

                        ServiceController sc = new ServiceController();
                        if (IsServiceInstalled("MySQL56")) sc = new ServiceController("MySQL56");
                        else if (IsServiceInstalled("MySQL57")) sc = new ServiceController("MySQL57");
                        else if (IsServiceInstalled("MySQL")) sc = new ServiceController("MySQL");

                    if (sc.Status != ServiceControllerStatus.Running)
                        {
                            addToLog("MySql service is not running!");
                            lblFoundDB.Text = "The MySql service is not running.  \nPlease make sure it is running and run the \nOpen Source Automation installer again.";
                            btnInstall.Text = "Close";
                        }
                        else
                        {
                            btnInstall.Text = "OK";
                            lbl1.Visible = true;
                            lbl2.Visible = true;
                            lblMySQL.Visible = true;
                            txbPassword.Visible = true;
                            txbUsername.Visible = true;
                            txbxLocation.Visible = true;
                            btnOpenFile.Visible = true;
                            lblFoundDB.Text = "Existing MySql Server found. \nPlease enter your root password and \nverify location of mysql.exe.";
                        }
                    }
                }
            else
            {
                lblMySQL.Visible = false;
                txbxLocation.Visible = false;
                btnOpenFile.Visible = false;
                lbl1.Visible = true;
                txbUsername.Text = "";
                lbl2.Visible = true;
                txbPassword.Visible = true;
                txbPassword.UseSystemPasswordChar = false;
                txbUsername.Visible = true;
                lbl1.Text = "Server:";
                lbl2.Text = "Port:";

                ModifyRegistry myRegistry = new ModifyRegistry();
                myRegistry.SubKey = "SOFTWARE\\OSAE\\DBSETTINGS";
                txbPassword.Text = myRegistry.Read("DBPORT");
                txbUsername.Text = myRegistry.Read("DBCONNECTION");

                lblFoundDB.Text = "Please enter the network name or IP \nof the OSA server and click OK.";
                btnInstall.Text = "OK";
            }
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            // get list of Windows services
            ServiceController[] services = ServiceController.GetServices();

            // try to find service name
            foreach (ServiceController service in services)
            {
                if (service.ServiceName.ToLower() == serviceName.ToLower())
                {
                    //service can we pull a path here?;
                    return true;
                }
            }
            return false;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (machine == "Server")
            {
                if (btnInstall.Text == "Upgrade")
                {
                    upgrade();
                    Close();
                }
                else if (btnInstall.Text == "OK")
                {
                    string mysqlDir = "", iniDir = "";
                    if (txbxLocation.Text != "Unknown, you must browse for it!" && txbxLocation.Text != "")
                        mysqlDir = txbxLocation.Text;
                    else
                        mysqlDir = Environment.GetEnvironmentVariable("ProgramFiles") + "\\MySQL\\MySQL Server 5.7\\bin\\mysql";

                    //iniDir = mysqlDir.Substring(0, mysqlDir.IndexOf("bin")-1);
                    //if (File.Exists(iniDir + "my.ini"))
                    //{
                    //    StreamWriter sw = File.AppendText(iniDir + "my.ini");
                    //    sw.WriteLine("event_scheduler=ON");
                    //    sw.Close();
                    //}

                    string args = " -u" + txbUsername.Text + " -p" + txbPassword.Text + " --execute \"CREATE USER `osae`@`%` IDENTIFIED BY \'osaePass\'\";";
                    Process p = Process.Start(mysqlDir, args);
                    p.WaitForExit();

                    string ConnectionString = string.Format("Uid={0};Password={1};Server={2};Port={3}", txbUsername.Text, txbPassword.Text, "localhost", "3306");
                    connection = new MySqlConnection(ConnectionString);
                    MySqlCommand command;
                    MySqlDataAdapter adapter;
                    DataSet dataset = new DataSet();
                    command = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'osae'", connection);
                    adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataset);
                    connection.Close();
                    int count = dataset.Tables[0].Rows.Count;

                    args = " -u" + txbUsername.Text + " -p" + txbPassword.Text + " --execute \"GRANT ALL ON osae.* TO `osae`@`%`\";";
                    p = Process.Start(mysqlDir, args);
                    p.WaitForExit();

                    args = " -u" + txbUsername.Text + " -p" + txbPassword.Text + " --execute \"UPDATE mysql.user SET Super_Priv=\'Y\' WHERE user=\'osae\'\";";
                    p = Process.Start(mysqlDir, args);
                    p.WaitForExit();

                    args = " -u" + txbUsername.Text + " -p" + txbPassword.Text + " --execute \"FLUSH PRIVILEGES\";";
                    p = Process.Start(mysqlDir, args);
                    p.WaitForExit();

                    if (count == 1)
                    {
                        try
                        {
                            ConnectionString = string.Format("Uid={0};Password={1};Server={2};Port={3};Database={4};allow user variables=true", txbUsername.Text, txbPassword.Text, "localhost", "3306", "osae");
                            connection = new MySqlConnection(ConnectionString);

                            connection.Open();
                            dataset = new DataSet();
                            command = new MySqlCommand("select property_value from osae_object_property p inner join osae_object_type_property tp on p.object_type_property_id = tp.property_id inner join osae_object o on o.object_id = p.object_id where object_name = 'SYSTEM' and property_name = 'DB Version'", connection);
                            adapter = new MySqlDataAdapter(command);
                            adapter.Fill(dataset);
                            if (dataset.Tables[0].Rows[0][0].ToString() == "") current = "0.4.7";
                            else current = dataset.Tables[0].Rows[0][0].ToString();

                            if (current != newVersion) upgrade();
                            connection.Close();
                            Close();
                        }
                        catch
                        { MessageBox.Show("Connection error.  Please check password."); }
                    }
                    else
                    {
                        MySqlScript script = new MySqlScript(connection, File.ReadAllText(directory + "\\osae.sql"));
                        script.Execute();

                        script = new MySqlScript(connection, "SET GLOBAL event_scheduler=ON;");
                        script.Execute();

                        connection.Close();

                        MessageBox.Show("Database installed successfully.");
                        Close();
                    }
                }
                else
                    Close();
            }
            else if (btnInstall.Text == "OK")
            {
                ModifyRegistry myRegistry = new ModifyRegistry();
                myRegistry.SubKey = "SOFTWARE\\OSAE\\DBSETTINGS";
                myRegistry.Write("DBCONNECTION", txbUsername.Text);
                myRegistry.Write("DBPORT", txbPassword.Text);
                Close();
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            // Show the dialog and get result.
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) txbxLocation.Text = openFileDialog1.FileName;
        }

        #region Methods
        
        private void upgrade()
        {
            addToLog("Upgrading...");
            string[] version = current.Split('.');
            int major = Int32.Parse(version[0]);
            int minor = Int32.Parse(version[1]);
            int bug = Int32.Parse(version[2]);

            string[] updateScripts = Directory.GetFiles(directory, "*.sql", SearchOption.TopDirectoryOnly);
            List<string> scripts = new List<string>();
            foreach (string s in updateScripts)
            {
                if (s.Substring(directory.Length + 1).Contains("-"))
                {
                    string[] vers = s.Substring(directory.Length + 1).Split('-');
                    string[] nums = vers[0].Split('.');

                    if (Int32.Parse(nums[0]) >= major)
                    {
                        if (Int32.Parse(nums[1]) >= minor)
                        {
                            if (Int32.Parse(nums[2]) >= bug)
                            {
                                scripts.Add(s.Substring(directory.Length + 1));
                                addToLog("Found upgrade script: " + s.Substring(directory.Length + 1));
                            }
                        }
                    }
                }
            }
            scripts.Sort();
            foreach (string s in scripts)
            {
                try
                {
                    MySqlScript script = new MySqlScript(connection, File.ReadAllText(directory + "\\" + s));
                    //script.Delimiter = "$$";
                    script.Execute();
                }
                catch (Exception ex)
                {
                    addToLog("Upgrade script failed: " + ex.Message);
                    //MySqlCommand upgCommand = new MySqlCommand();
                    //upgCommand.Connection = connection;
                    //upgCommand.CommandText = File.ReadAllText(directory + "\\" + s);
                    //upgCommand.ExecuteNonQuery();
                }
            }
            connection.Close();
            MessageBox.Show("Database upgraded successfully.");
        }

        private void addToLog(string log)
        {
            StreamWriter sw = File.AppendText("install_log.log");
            sw.WriteLine(System.DateTime.Now.ToString() + " - " + log);
            sw.Close();
        }

        #endregion
    }


    public class ModifyRegistry
    {
        private string subKey;

        public string SubKey
        {
            get { return subKey; }
            set { subKey = value; }
        }

        private RegistryKey baseRegistryKey = Registry.LocalMachine;
        /// <summary>
        /// A property to set the BaseRegistryKey value.
        /// (default = Registry.LocalMachine)
        /// </summary>
        public RegistryKey BaseRegistryKey
        {
            get { return baseRegistryKey; }
            set { baseRegistryKey = value; }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To read a registry key.
        /// input: KeyName (string)
        /// output: value (string) 
        /// </summary>
        public string Read(string KeyName)
        {
            RegistryKey rk = baseRegistryKey;  // Opening the registry key
            RegistryKey sk1 = rk.OpenSubKey(subKey);  // Open a subKey as read-only
            // If the RegistrySubKey doesn't exist -> (null)
            if (sk1 == null)
                return null;
            else
            {
                try
                {
                    // If the RegistryKey exists I get its value
                    // or null is returned.
                    return (string)sk1.GetValue(KeyName.ToUpper());
                }
                catch (Exception e)
                {
                    //AddToLog("Registery Read error: " + e.Message);
                    return null;
                }
            }
        }

        /* **************************************************************************
         * **************************************************************************/

        /// <summary>
        /// To write into a registry key.
        /// input: KeyName (string) , Value (object)
        /// output: true or false 
        /// </summary>
        public bool Write(string KeyName, object Value)
        {
            try
            {
                // Setting
                RegistryKey rk = baseRegistryKey;
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // 'cause OpenSubKey open a subKey as read-only
                RegistryKey sk1 = rk.CreateSubKey(subKey);
                // Save the value
                sk1.SetValue(KeyName.ToUpper(), Value);

                return true;
            }
            catch (Exception e)
            {
                // AAAAAAAAAAARGH, an error!
                //ShowErrorMessage(e, "Writing registry " + KeyName.ToUpper());
                return false;
            }
        }
    }
}
