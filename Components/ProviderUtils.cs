using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DotNetNuke.Common.Utilities;
using System.Security.Cryptography;
using OS_WorldPay.Components;
using System.Collections.Specialized;
using System.Globalization;

namespace OS_WorldPay
{
    public class ProviderUtils
    {

        public static bool AreNullOrEmpty(params string[] stringsToValidate)
        {
            bool result = false;
            Array.ForEach(stringsToValidate, str => {
                if (string.IsNullOrEmpty(str)) result = true;
            });
            return result;
        }

        public static string stripGWInvalidChars(string strIn)
        {
            string[] toReplace = new string[] { "\\", "<", ">", "#", "]", "[" };

            string strOut = strIn;

            foreach (string charToReplace in toReplace)
            {
                strOut.Replace(charToReplace, "");
            }

            return strOut;
        }

        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_WorldPaypayment", "OS_WorldPayPAYMENT", Utils.GetCurrentCulture());
            return info;
        }


        public static String GetBankRemotePost(OrderData orderData)
        {

            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_WorldPaypayment", "OS_WorldPayPAYMENT", orderData.Lang);

            var MD5secretKey = info.GetXmlProperty("genxml/textbox/secretkey");
            var installid = info.GetXmlPropertyInt("genxml/textbox/installid");

            var request = new HostedTransactionRequest();

            var appliedtotal =  orderData.PurchaseInfo.GetXmlPropertyRaw("genxml/appliedtotal");
            request.amount = appliedtotal;
            request.currency = info.GetXmlProperty("genxml/textbox/currencycode");
            request.testMode = 0;  // Not sure what this should be.  Only example I found was 100 for test system??
            request.instId = installid;
            request.cartId = orderData.GetInfo().ItemID.ToString();

            var postUrl = info.GetXmlProperty("genxml/textbox/liveurl");
            if (info.GetXmlPropertyBool("genxml/checkbox/preproduction"))
            {
                request.testMode = 100;
                postUrl = info.GetXmlProperty("genxml/textbox/testurl");
            }

            var requestInputs = request.ToNameValueCollection();

            var rPost = new RemotePostPay();
            rPost.Url = postUrl;

            var callbackhashInputs = new StringBuilder();
            callbackhashInputs.Append(MD5secretKey);
            callbackhashInputs.Append(":");
            callbackhashInputs.Append(request.currency);
            callbackhashInputs.Append(":");
            callbackhashInputs.Append(request.cartId);
            callbackhashInputs.Append(":");
            callbackhashInputs.Append(appliedtotal);

            var signaturehashInputs = new StringBuilder();
            signaturehashInputs.Append(MD5secretKey);
            signaturehashInputs.Append(":");
            signaturehashInputs.Append(request.currency);
            signaturehashInputs.Append(":");
            signaturehashInputs.Append(request.cartId);
            signaturehashInputs.Append(":");
            signaturehashInputs.Append(appliedtotal);

            byte[] callbackhashDigest = new MD5CryptoServiceProvider().ComputeHash(StringToByteArray(callbackhashInputs.ToString()));

            byte[] signaturehashDigest = new MD5CryptoServiceProvider().ComputeHash(StringToByteArray(signaturehashInputs.ToString()));

            rPost.Add("signature", ByteArrayToHexString(signaturehashDigest));
            rPost.Add("MC_callbacksignature", ByteArrayToHexString(callbackhashDigest));

            // add the rest of the form variables
            foreach (var k in requestInputs.AllKeys)
            {
                rPost.Add(k, requestInputs.GetValues(k)[0]);
            }


            //Build the re-direct html 
            var rtnStr = "";
            rtnStr = rPost.GetPostHtml();

            if (info.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_OS_WorldPaypost.html", rtnStr + " signaturehashInputs:" + signaturehashInputs + " request.amount:" + request.amount + " appliedtotal:" + appliedtotal);
            }
            return rtnStr;
        }

        public static byte[] StringToByteArray(string source, bool useASCII = true)
        {
            Encoding e;
            if (useASCII)
                e = new ASCIIEncoding();
            else
                e = new UTF8Encoding();
            return e.GetBytes(source);
        }

        public static string ByteArrayToHexString(byte[] source)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i].ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Performs basic validation of the transaction result (you should also implement your own e.g. check amounts against order)
        /// </summary>
        /// <param name="result">Transaction result</param>
        public void ValidateResult(ServerTransactionResult result, String MerchantId, String MerchantPassword, String PreSharedKey)
        {
            NameValueCollection nameValueCollection = new NameValueCollection();
            HashMethod hashMethod = HashMethod.SHA1;
            nameValueCollection.Add("PreSharedKey", PreSharedKey);
            nameValueCollection.Add("MerchantID", MerchantId);
            nameValueCollection.Add("Password", MerchantPassword);
            nameValueCollection.Add("StatusCode", Convert.ToInt32(result.StatusCode));
            nameValueCollection.Add("Message", result.Message);
            if (result.StatusCode == TransactionStatus.DuplicateTransaction)
            {
                nameValueCollection.Add("PreviousStatusCode", Convert.ToInt32(result.PreviousStatusCode));
            }
            else
            {
                nameValueCollection.Add("PreviousStatusCode", "");
            }
            nameValueCollection.Add("PreviousMessage", result.PreviousMessage);
            nameValueCollection.Add("CrossReference", result.CrossReference);
            nameValueCollection.Add("AddressNumericCheckResult", result.AddressNumericCheckResult);
            nameValueCollection.Add("PostCodeCheckResult", result.PostCodeCheckResult);
            nameValueCollection.Add("CV2CheckResult", result.CV2CheckResult);
            nameValueCollection.Add("ThreeDSecureAuthenticationCheckResult", result.ThreeDSecureAuthenticationCheckResult);
            nameValueCollection.Add("CardType", result.CardType);
            nameValueCollection.Add("CardClass", result.CardClass);
            nameValueCollection.Add("CardIssuer", result.CardIssuer);
            nameValueCollection.Add("CardIssuerCountryCode", result.CardIssuerCountryCode);
            nameValueCollection.Add("Amount", result.Amount);
            nameValueCollection.Add("CurrencyCode", Convert.ToString(result.CurrencyCode));
            nameValueCollection.Add("OrderID", result.OrderID);
            nameValueCollection.Add("TransactionType", result.TransactionType);
            nameValueCollection.Add("TransactionDateTime", Convert.ToString(result.TransactionDateTime));
            nameValueCollection.Add("OrderDescription", result.OrderDescription);
            nameValueCollection.Add("CustomerName", result.CustomerName);
            nameValueCollection.Add("Address1", result.Address1);
            nameValueCollection.Add("Address2", result.Address2);
            nameValueCollection.Add("Address3", result.Address3);
            nameValueCollection.Add("Address4", result.Address4);
            nameValueCollection.Add("City", result.City);
            nameValueCollection.Add("State", result.State);
            nameValueCollection.Add("PostCode", result.PostCode);
            nameValueCollection.Add("CountryCode", Convert.ToString(result.CountryCode));
            nameValueCollection.Add("EmailAddress", result.EmailAddress);
            nameValueCollection.Add("PhoneNumber", result.PhoneNumber);
            bool flag = false;
            string queryString = nameValueCollection.ToQueryString("&", false, flag);
            string str = HashUtil.ComputeHashDigest(queryString, PreSharedKey, hashMethod);
            if (result.HashDigest != str)
            {
                throw new Exception("Hash Check Failed");
            }
        }



    }
}
