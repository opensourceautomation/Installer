namespace OSAInstallCustomActions
{
    using Microsoft.Deployment.WindowsInstaller;
    using OSAE;
    using System;

    public class CustomActions
    {
        private const string INSTALLFOLDER = "INSTALLFOLDER"; 

        /// <summary>
        /// Performing an install or upgrade as part of a server installer
        /// </summary>
        /// <param name="session">The session information provided by WIX</param>
        /// <returns></returns>
        [CustomAction]        
        public static ActionResult Server(Session session)
        {
            try
            {
                var customActionData = new CustomActionData();
                customActionData.Add(INSTALLFOLDER, session[INSTALLFOLDER]);
                session.DoAction("DeferredServerAction", customActionData);
                return ActionResult.Success; 
            }
            catch (Exception ex)
            {
                session.Log("Exception Occured during custom action details:" + ex.Message);
                return ActionResult.Failure;
            }
        }

        /// <summary>
        /// Running in the context of a client installer
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult Client(Session session)
        {
            try
            {
                session.Log("Begin Client CustomAction");
                var installDir = session.CustomActionData[INSTALLFOLDER];
                session.Log("Custom Action Using install folder: " + installDir);
                DatabaseInstall databaseInstall = new DatabaseInstall(session, installDir, "Client");
                //session.Log("Session Property Value: " + session["OSAInstallType"].ToString());

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

        [CustomAction]
        public static ActionResult DeferredServerAction(Session session)
        {
            try
            {
                session.Log("Begin Server CustomAction");
                var installDir = session.CustomActionData[INSTALLFOLDER];
                session.Log("Custom Action Using install folder: " + installDir);


                DatabaseInstall databaseInstall = new DatabaseInstall(session, installDir, "Server");

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

        [CustomAction]
        public static ActionResult DeferredClientAction(Session session)
        {
            try
            {
                session.Log("Begin Server CustomAction");
                string installFolder = session["INSTALLFOLDER"];
                session.Log("Custom Action Using install folder: " + installFolder);


                DatabaseInstall databaseInstall = new DatabaseInstall(session, installFolder, "Client");

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
