namespace OSAInstallCustomActions
{
    using Microsoft.Deployment.WindowsInstaller;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Windows.Forms;

    public partial class DatabaseInstall : Form
    {
        private MySqlConnection connection;
        private string current = "0.1.0";
        private string newVersion = "0.4.3";
        private string ConnectionStringRoot;
        private string ConnectionStringOSAE;
        RegistrySettings regSettings;

        /// <summary>
        /// The path to the DB files
        /// </summary>
        private string directory;

        /// <summary>
        /// The install directory being used by the installer
        /// </summary>
        private string installDirectory;
        private string machine = string.Empty;

        /// <summary>
        /// Session from the installer used to access the installer log and variables
        /// </summary>
        private Session session;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ses">Installer session information</param>
        /// <param name="dir"></param>
        /// <param name="mach">Whether the installer is running on a client or server</param>
        public DatabaseInstall(Session ses, string dir, string mach)
        {
            session = ses;
            installDirectory = dir;
            directory = dir + @"DB\";
            machine = mach;
            InitializeComponent();
        }

        ~DatabaseInstall()
        {
            try
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
            catch (Exception)
            {
                // its not overly important if we fail to close so do nothing
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                regSettings = new RegistrySettings();
                regSettings.LoadCurrentValues();
                
                session.Log("Existing settings: DBPASSWORD " + regSettings.DbPassword + " DBUSERNAME " + regSettings.DbUsername + " DBPORT " + regSettings.DbPort + " DBCONNECTION " + regSettings.DbConnection);
                
                SetFormFields();

                if (regSettings.RequiredPresent())
                {
                    if (machine != "Client")
                    {
                        //Try to connect to MySql instance and check if osae database exists and if it needs to be upgraded
                        InstallStatus status = AttemptConnection();
                        if (status == InstallStatus.NEEDSUPGRADE)
                        {
                            upgrade();
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            this.Close();
                        }
                        else if (status == InstallStatus.UPTODATE)
                        {
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            btnClose.Visible = true;
                            this.Close();
                        }
                    }
                    else
                    {
                        // the keys are present and we are doing a client install so nothing to do
                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        btnClose.Visible = true;
                        this.Close();
                    }
                }              

                if (machine == "Client")
                {
                    lb_Progress.Visible = false;
                    installationProgressBar.Visible = false;
                    txbUsername.Visible = false;
                    txbPassword.Visible = false;
                    txbWcfAddress.Visible = true;
                    usernameLabel.Visible = false;
                    passwordLabel.Visible = false;
                    WCFlabel.Visible = true;
                    label1.Text = "Please enter the server address and MySql port of your Open Source Automation server.";
                }
                else
                {
                    lb_Progress.Visible = true;
                    installationProgressBar.Visible = true;
                }
            }
            catch (Exception ex)
            {
                session.Log("Error occured in Form_Load details:" + ex.Message);
            }
        }

        private void SetFormFields()
        {
            if (!string.IsNullOrEmpty(regSettings.DbPassword))
            {
                txbPassword.Text = regSettings.DbPassword;
            }

            if(!string.IsNullOrEmpty(regSettings.WcfServer))
            {
                txbWcfAddress.Text = regSettings.WcfServer;
            }

            if (!string.IsNullOrEmpty(regSettings.DbUsername))
            {
                txbUsername.Text = regSettings.DbUsername;
            }
            else
            {
                txbUsername.Text = "root";
            }

            if (!string.IsNullOrEmpty(regSettings.DbPort))
            {
                txbPort.Text = regSettings.DbPort;
            }
            else
            {
                txbPort.Text = "3306";
            }

            if (!string.IsNullOrEmpty(regSettings.DbConnection))
            {
                txbServer.Text = regSettings.DbConnection;
            }
            else
            {
                txbServer.Text = "localhost";
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnClose.Visible && btnInstall.Text != "Close")
            {
                MessageBox.Show("Database did not install successfully.  Please make sure your MySql instance is running and re-run the Open Source Automation installer again.");
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                if(string.IsNullOrEmpty(regSettings.WcfServer))
                {
                    OSAE.ModifyRegistry myRegistry = new OSAE.ModifyRegistry();
                    myRegistry.SubKey = @"SOFTWARE\OSAE";
                    myRegistry.Write("WcfServer", txbWcfAddress.Text);
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (machine == "Client")
            {
                ConnectionStringRoot = string.Format("Uid=osae;Password=osaePass;Server={0};Port={1};", txbServer.Text, txbPort.Text);
                connection = new MySqlConnection(ConnectionStringRoot);

                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    session.Log("Exception trying to connect to DB: " + ex.Message);
                    lblConnectResult.ForeColor = System.Drawing.Color.Red;
                    lblConnectResult.Text = "Connection failed. \nPlease make sure the OSA server is running.";
                    return;
                }

                lblConnectResult.ForeColor = System.Drawing.Color.Green;
                lblConnectResult.Text = "Connection Successful.";
                btnInstall.Text = "Close";
                btnInstall.Visible = true;
                btnConnect.Enabled = false;
                SetRegistryKeys();
            }
            else
            {
                InstallStatus status = AttemptConnection();
                               
                if (status == InstallStatus.ERROR)
                {
                    session.Log("Connection failed");
                    ShowConnectionError();
                    return;
                }
                else if (status == InstallStatus.NOTINSTALLED)
                {
                    lblConnectResult.ForeColor = System.Drawing.Color.Green;
                    lblConnectResult.Text = "Connection Successful. \nClick to install.";
                    btnInstall.Text = "Install";
                    btnInstall.Visible = true;
                    btnConnect.Enabled = false;
                }
                else if (status == InstallStatus.NEEDSUPGRADE)
                {
                    lblConnectResult.ForeColor = System.Drawing.Color.Green;
                    lblConnectResult.Text = "Connection Successful. \nClick to upgrade.";
                    btnInstall.Text = "Upgrade";
                    btnInstall.Visible = true;
                    btnConnect.Enabled = false;
                }
                else if (status == InstallStatus.UPTODATE)
                {
                    lblConnectResult.ForeColor = System.Drawing.Color.Green;
                    lblConnectResult.Text = "Connection Successful. \nDatabase is up to date.";
                    btnInstall.Text = "Close";
                    btnInstall.Visible = true;
                    btnConnect.Enabled = false;
                }

                SetRegistryKeys();
            }
        }

        private void ShowConnectionError()
        {
            DialogResult result = MessageBox.Show("Please check the settings and make sure MySql is running.\n\n" +
                "If you are installing the database on a remote server ensure the account is configured to allow remote connections\n\n" +
                "Do you want to view the help page for more information?",
                "Connection failed",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("http://www.opensourceautomation.com/wiki/index.php?title=Installation_Guide");
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (btnInstall.Text == "Install")
            {
                try
                {
                    btnInstall.Enabled = false;
                    Install();
                }
                catch (Exception ex)
                {
                    session.Log("Error installing database!: " + ex.Message);

                    lblError.ForeColor = System.Drawing.Color.Red;
                    lblError.Text = "Error installing database!";
                    installationProgressBar.Style = ProgressBarStyle.Blocks;
                }
            }
            else if (btnInstall.Text == "Upgrade")
            {
                upgrade();
            }
            else if (btnInstall.Text == "Close")
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void Install()
        {
            // DB not found.  Need to install.
            session.Log("No OSA database found.  We need to install it.");

            lb_Progress.Visible = true;
            installationProgressBar.Visible = true;

            MySqlCommand command;
            MySqlDataAdapter adapter;
            DataSet dataset = new DataSet();
            MySqlScript script;
            command = new MySqlCommand("select User from mysql.user where User = 'osae'", connection);
            adapter = new MySqlDataAdapter(command);
            adapter.Fill(dataset);
            connection.Close();
            int count = dataset.Tables[0].Rows.Count;
            installationProgressBar.Value = 25;

            if (count == 0)
            {
                script = new MySqlScript(connection, "CREATE USER `osae`@`%` IDENTIFIED BY 'osaePass';");
                script.Execute();
            }
            installationProgressBar.Value = 50;
            script = new MySqlScript(connection, "GRANT SUPER ON *.* TO `osae`@`%`;GRANT ALL PRIVILEGES ON *.* TO 'osae'@'%' WITH GRANT OPTION;");
            script.Execute();
            installationProgressBar.Value = 75;
            script = new MySqlScript(connection, File.ReadAllText(directory + @"osae.sql"));
            script.Execute();
            connection.Close();

            installationProgressBar.Style = ProgressBarStyle.Blocks;
            installationProgressBar.Value = 100;

            lblError.ForeColor = System.Drawing.Color.Green;
            lblError.Text = "Success!";
            btnClose.Visible = true;

            txbUsername.Text = "osae";
            txbPassword.Text = "osaePass";
            SetRegistryKeys();
        }

        private void SetRegistryKeys()
        {
            OSAE.ModifyRegistry myRegistry = new OSAE.ModifyRegistry();
            myRegistry.SubKey = @"SOFTWARE\OSAE\DBSETTINGS";
            myRegistry.Write("DBUSERNAME", "osae");
            myRegistry.Write("DBPASSWORD", "osaePass");
            myRegistry.Write("DBCONNECTION", txbServer.Text);
            myRegistry.Write("DBPORT", txbPort.Text);
            myRegistry.Write("DBNAME", "osae");
            myRegistry.Write("INSTALLDIR", installDirectory);
        }      

        private void upgrade()
        {
            lb_Progress.Visible = true;
            installationProgressBar.Visible = true;

            session.Log("Upgrading...");
            string[] version = current.Split('.');
            int major = Int32.Parse(version[0]);
            int minor = Int32.Parse(version[1]);
            int bug = Int32.Parse(version[2]);

            string[] updateScripts = Directory.GetFiles(directory, @"*.sql", SearchOption.TopDirectoryOnly);
            List<string> scripts = new List<string>();
            
            foreach (string s in updateScripts)
            {
                if (s.Substring(directory.Length + 1).Contains("-"))
                {
                    string[] vers = s.Substring(directory.Length).Split('-');
                    string[] nums = vers[0].Split('.');

                    if (Int32.Parse(nums[0]) >= major)
                    {
                        if (Int32.Parse(nums[1]) >= minor)
                        {
                            if (Int32.Parse(nums[2]) >= bug)
                            {
                                scripts.Add(s.Substring(directory.Length));
                                session.Log("Found upgrade script: " + s.Substring(directory.Length));
                            }
                        }
                    }
                }
            }
            scripts.Sort();
            int max = scripts.Count;
            int count = 1;
            try
            {
                foreach (string s in scripts)
                {
                    MySqlScript script = new MySqlScript(connection, File.ReadAllText(directory + "\\" + s));
                    //script.Delimiter = "$$";
                    script.Execute();
                    decimal percent = count / max;
                    installationProgressBar.Value = (int)Math.Round(percent, 0);
                    count++;
                    session.Log("Upgrade script executed successfully: " + s);
                }
                connection.Close();
                lblError.Text = "Database upgraded successfully.";
                btnClose.Visible = true;

            }
            catch (Exception ex)
            {
                lblError.Text = "Database upgrade failed.";
                MessageBox.Show("Upgrade script failed: " + ex.Message);
                session.Log("Upgrade script failed: " + ex.Message);
            }
            installationProgressBar.Value = 100;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private InstallStatus AttemptConnection()
        {
            ConnectionStringRoot = string.Format("Uid={0};Password={1};Server={2};Port={3};", txbUsername.Text, txbPassword.Text, txbServer.Text, txbPort.Text);
            connection = new MySqlConnection(ConnectionStringRoot);

            try
            {
                connection.Open();

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
                    session.Log("Found OSA database.  Checking to see if we need to upgrade.");
                    dataset = new DataSet();
                    ConnectionStringOSAE = string.Format("Uid={0};Pwd={1};Server={2};Port={3};Database={4};allow user variables=true", "osae", "osaePass", txbServer.Text, txbPort.Text, "osae");
                    connection = new MySqlConnection(ConnectionStringOSAE);
                    connection.Open();
                    command = new MySqlCommand("select property_value from osae_object_property p inner join osae_object_type_property tp on p.object_type_property_id = tp.property_id inner join osae_object o on o.object_id = p.object_id where object_name = 'SYSTEM' and property_name = 'DB Version'", connection);
                    adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataset);
                    if (dataset.Tables[0].Rows[0][0].ToString() == string.Empty)
                        current = "0.1.0";
                    else
                        current = dataset.Tables[0].Rows[0][0].ToString();
                    if (current == newVersion)
                    {
                        return InstallStatus.UPTODATE;
                    }
                    else
                    {
                        return InstallStatus.NEEDSUPGRADE;
                    }
                }
                else
                {
                    return InstallStatus.NOTINSTALLED;
                }
            }
            catch (Exception ex)
            {
                session.Log("Connection failed: " + ex.Message);
                return InstallStatus.ERROR;
            }
        }

        
    }
}
