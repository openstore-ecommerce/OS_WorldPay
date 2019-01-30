using System;
using System.Collections.Specialized;
using System.Runtime.Remoting.Contexts;
using System.Web;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;
using OS_WorldPay.Components;

namespace OS_WorldPay.DNN.NBrightStore
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class OS_WorldPayNotify : IHttpHandler
    {
        private String _lang = "";




        /// <summary>
        /// This function needs to process and returned message from the bank.
        /// This processing may vary widely between banks.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var modCtrl = new NBrightBuyController();
            var info = modCtrl.GetPluginSinglePageData("OS_WorldPaypayment", "OS_WorldPayPAYMENT", Utils.GetCurrentCulture());

            try
            {
                var MD5secretKey = info.GetXmlProperty("genxml/textbox/secretkey");
                var callbackpw = info.GetXmlProperty("genxml/textbox/callbackpw");

                var result = new CallbackResult(context.Request.Form, MD5secretKey, callbackpw);

                var debugMode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
                var debugMsg = "START CALL" + DateTime.Now.ToString("s") + " </br>";
                var rtnMsg = "WorldPay return process. Uncompleted" + Environment.NewLine;

                // ------------------------------------------------------------------------
                // In this case the payment provider passes back data via form POST.
                // Get the data we need.
                string returnmessage = "";
                int OS_WorldPayStoreOrderID = 0;
                string OS_WorldPayCartID = "";
                string OS_WorldPayClientLang = "";

                var orderid = result.cartId;

                debugMsg += "orderid: " + orderid + "</br>";

                if (Utils.IsNumeric(orderid))
                {

                    OS_WorldPayStoreOrderID = Convert.ToInt32(orderid);
                    // ------------------------------------------------------------------------

                    debugMsg += "OrderId: " + orderid + " </br>";
                    debugMsg += "StatusCode: " + result.transStatus + " </br>";

                    var orderData = new OrderData(OS_WorldPayStoreOrderID);
                    var transStatus = result.transStatus;

                    if (transStatus == "Y")
                        rtnMsg = CreateServerResponseString(TransactionStatus.Successful);
                    else
                        rtnMsg = CreateServerResponseString(TransactionStatus.NotSpecified);

                    if (transStatus != "Y")
                    {
                        orderData.PaymentFail();
                    }
                    else
                    {
                        if (transStatus == "Y")
                        {
                            orderData.PaymentOk();
                        }
                        else
                        {
                            orderData.PaymentFail();
                        }
                    }
                }
                if (debugMode)
                {
                    debugMsg += "Return Message: " + rtnMsg;
                    info.SetXmlProperty("genxml/debugmsg", debugMsg);
                    modCtrl.Update(info);
                }

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write(rtnMsg);
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.CacheControl = "no-cache";
                HttpContext.Current.Response.Expires = -1;
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
                if (!ex.ToString().StartsWith("System.Threading.ThreadAbortException")) // we expect a thread abort from the End response.
                {
                    info.SetXmlProperty("genxml/debugmsg", "OS_WorldPay ERROR: " + ex.ToString());
                    modCtrl.Update(info);
                }
            }


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Creates a response message confirming delivery of transaction result
        /// </summary>
        /// <param name="status">Result of delivery (note this is to confirm that you have received the result, not the result itself)</param>
        /// <param name="message">Optional message for example, any exceptions that may have occurred</param>
        /// <returns>String</returns>
        public string CreateServerResponseString(TransactionStatus status, string message = "")
        {

            var response = new NameValueCollection();
            response.Add("StatusCode", (int)status);
            if (!string.IsNullOrEmpty(message))
            {
                response.Add("Message", message);
            }

            return response.ToQueryString();
        }


    }
}