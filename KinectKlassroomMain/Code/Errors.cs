using System;

namespace SeniorProject.Helpers
{
    internal static class Errors
    {
        public static void WriteError(Exception ex, String strDevMessage = "None")
        {
            Console.Write(ex.Message.ToString() + " - Target Site: '" + ex.TargetSite.ToString() + "' - Developer Message: '" + strDevMessage + "'");
        }
    }
}