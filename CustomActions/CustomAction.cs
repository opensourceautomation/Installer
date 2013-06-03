namespace OSAInstallCustomActions
{
    using Microsoft.Deployment.WindowsInstaller;
    using OSAE;
    using System;

    public class CustomActions
    {
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
                session.Log("Begin Server CustomAction");

                DatabaseInstall databaseInstall = new DatabaseInstall(session, "", "Server");                

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

                DatabaseInstall databaseInstall = new DatabaseInstall(session, "", "Client");
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
    }
}
