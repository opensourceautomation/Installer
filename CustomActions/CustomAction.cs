namespace OSAInstallCustomActions
{
    using Microsoft.Deployment.WindowsInstaller;
    using OSAE;
    using System;

    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckServerIp(Session session)
        {
            session.Log("Begin OSAInstallCustomActions CustomAction");
            
            ModifyRegistry registry = new ModifyRegistry();
            registry.SubKey = "SOFTWARE\\OSAE\\DBSETTINGS";

            string db = registry.Read("DBCONNECTION");

            if (string.IsNullOrEmpty(db) || db == "default")
            {
                ServerDetails details = new ServerDetails(session);

                if (details.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    registry.Write("DBCONNECTION", details.ServerIP());
                    return ActionResult.Success;
                }
                else
                {
                    session.Log("OSAInstallCustomActions CustomAction - User exited");
                    return ActionResult.UserExit;
                }
            }
            else
            {
                return ActionResult.Success;
            }
        }

        /// <summary>
        /// Depending on whether running the client installer or server installer
        /// will determine the aciton of doind a DB install upgrade or setting the IP
        /// </summary>
        /// <param name="session">The session information provided by WIX</param>
        /// <returns></returns>
        [CustomAction]        
        public static ActionResult DatabaseUpdate(Session session)
        {
            try
            {
                session.Log("Begin DatabaseUpdate CustomAction");

                DatabaseInstall databaseInstall = new DatabaseInstall("", "");
                session.Log("Session Property Value: " + session["OSAInstallType"].ToString());

                if (databaseInstall.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return ActionResult.Success;
                }
                else
                {
                    session.Log("OSAInstallCustomActions CustomAction - User exited");
                    return ActionResult.UserExit;
                }
            }
            catch (Exception ex)
            {
                session.Log("Exception Occured during custom action details:" + ex.Message);
                return ActionResult.Failure;
            }
        }
    }
}
