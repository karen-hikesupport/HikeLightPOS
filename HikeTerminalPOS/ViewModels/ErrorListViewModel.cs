using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using HikePOS.Helpers;
using HikePOS.Interfaces;
using HikePOS.Models;
using HikePOS.Models.Log;
using Newtonsoft.Json;

namespace HikePOS.ViewModels
{
    public class ErrorListViewModel: BaseViewModel
    {
        List<HikeAuditLog> _Errors { get; set; }
        public List<HikeAuditLog> Errors { get { return _Errors; } set { _Errors = value; SetPropertyChanged(nameof(Errors)); } }

        public ICommand ReportUsCommand { get; }

        public ErrorListViewModel()
        {
            ReportUsCommand = new Command(ReportUs);
        }

        public void ReportUs()
        {
            try
            {
                using (new Busy(this, true))
                {
                    IEmailComposer emailComposer = DependencyService.Get<IEmailComposer>();
                    List<string> ToEmails = new List<string>() { "hello@hikeup.com" };

                    var body = "<b>Dear Team,</b>" + "<br/>" +
                        "An exception occurred in a Application when synchronizing sale with cloud with following details: <br/>";

                    if (!string.IsNullOrEmpty(Settings.TenantName))
                    {
                        body = body + "<b>Tenant name:</b> " + Settings.TenantName + "<br/>";
                    }
                    if (!string.IsNullOrEmpty(Settings.SelectedOutletName))
                    {
                        body = body + "<b>Outlet name:</b> " + Settings.SelectedOutletName + "<br/>";
                    }

                    if (Settings.CurrentRegister != null && !string.IsNullOrEmpty(Settings.CurrentRegister.Name))
                    {
                        body = body + "<b>Register name:</b> " + Settings.CurrentRegister.Name + "<br/>";
                    }

                    if (Settings.CurrentUser != null && !string.IsNullOrEmpty(Settings.CurrentUser.FullName))
                    {
                        body = body + "<b>User name:</b> " + Settings.CurrentUser.FullName + "<br/><br/>";
                    }

                    body = body + "Please find attached error log. <br/>" +
                         "Thanks and Regards <br/>" +
                         "<b>Application Admin </b> <br/>";

                    emailComposer.SendEmail(ToEmails, "An exception occurred in a Application", body, JsonConvert.SerializeObject(Errors));
                }
            }
            catch(Exception ex)
            {
                ex.Track();
            }
        }
    }
}
