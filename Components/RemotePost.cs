﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OS_WorldPay
{
    public class RemotePost
    {
        private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
        public string Url = "";
        public string Method = "post";
        public string FormName = "CardsavePaymentForm";
        public void Add(string name, string value)
        {
            Inputs.Add(name, value);
        }

        public string GetPostHtml()
        {
            string sipsHtml = "";

            sipsHtml = "<html><head>";
            sipsHtml += "</head><body onload=\"document." + FormName + ".submit()\">";
            sipsHtml += "<form name=\"" + FormName + "\" method=\"" + Method + "\" action=\"" + Url + "\">";

            for (int i = 0; i < Inputs.Keys.Count; i++)
            {
                sipsHtml += "<input name=\"" + HttpUtility.HtmlEncode(Inputs.Keys[i]) + "\" type=\"hidden\" value=\"" + HttpUtility.HtmlEncode(Inputs[Inputs.Keys[i]]) + "\">";
            }

            sipsHtml += "</form>";

            sipsHtml += "  <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"100%\">";
            sipsHtml += "<tr><td width=\"100%\" height=\"100%\" valign=\"middle\" align=\"center\">";
            sipsHtml += "<font style=\"font-family: Trebuchet MS, Verdana, Helvetica;font-size: 14px;letter-spacing: 1px;font-weight: bold;\">";
            sipsHtml += "Processing...";
            sipsHtml += "</font><br /><br /><img src='/DesktopModules/NBright/OS_WorldPay/Themes/config/img/loading.gif' /> ";
            sipsHtml += "</td></tr>";
            sipsHtml += "</table>";

            sipsHtml += "</body></html>";

            return sipsHtml;

        }

    }

}
