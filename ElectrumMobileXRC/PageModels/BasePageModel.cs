using FreshMvvm;
using System;

namespace ElectrumMobileXRC.PageModels
{
    public class BasePageModel : FreshBasePageModel
    {
        internal static string _LoggedUserName;
        internal static DateTime _LoggedDateTimeUtc;

        public bool IsUserValid()
        {
            if (string.IsNullOrEmpty(_LoggedUserName))
            {
                return false;
            }

            if (_LoggedDateTimeUtc < DateTime.UtcNow.AddMinutes(-30))
            {
                return false;
            }

            return true;
        }

        public void SetValidUser(string userName)
        {
            _LoggedDateTimeUtc = DateTime.UtcNow;
            _LoggedUserName = userName;
        }
    }
}
