/*
 * Copyright (c) 2014-2019, Achim 'ahzf' Friedland <achim@graphdefined.org>
 * This file is part of Open Charging Cloud API <http://www.github.com/OpenChargingCloud/OpenChargingCloudAPI>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using System.IO;
using System.Threading;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Authentication;

using Org.BouncyCastle.Bcpg.OpenPgp;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.SMTP;
using org.GraphDefined.Vanaheimr.Hermod.Mail;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.BouncyCastle;
using org.GraphDefined.Vanaheimr.Hermod.Sockets;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;
using org.GraphDefined.OpenData.Users;

using org.GraphDefined.WWCP.Net;
using com.GraphDefined.SMSApi.API;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// The common Open Charging Cloud API.
    /// </summary>
    public class OpenChargingCloudAPI : UsersAPI
    {

        #region Data

        /// <summary>
        /// The default HTTP server name.
        /// </summary>
        public  new const           String          DefaultHTTPServerName               = "GraphDefined Open Charging Cloud API";

        /// <summary>
        /// The default HTTP server port.
        /// </summary>
        public  new static readonly IPPort          DefaultHTTPServerPort               = IPPort.Parse(5500);

        /// <summary>
        /// The HTTP root for embedded ressources.
        /// </summary>
        public  new const           String          HTTPRoot                            = "cloud.charging.open.api.HTTPRoot.";

        /// <summary>
        /// The default service name.
        /// </summary>
        public  new const           String          DefaultServiceName                  = "GraphDefined Open Charging Cloud API";

        /// <summary>
        /// The default language of the API.
        /// </summary>
        public  new const           Languages       DefaultLanguage                     = Languages.eng;

        /// <summary>
        /// The logo of the website.
        /// </summary>
        public  new const           String          _LogoImage                          = "images/OpenChargingCloud_Logo2.png";


        public      static readonly HTTPPath         DataRouterURIPrefix                 = HTTPPath.Parse("/datarouter");

        /// <summary>
        /// The name of the common log file.
        /// </summary>
        public new const            String          DefaultLogfileName                  = "OpenChargingCloud.log";

        /// <summary>
        /// The name of the default HTTP cookie.
        /// </summary>
        public new static readonly  HTTPCookieName  DefaultCookieName                   = HTTPCookieName.Parse("OpenChargingCloud");

        private static readonly HTTPPath DefaultURIPrefix = HTTPPath.Parse("/");

        #endregion

        #region Properties

        public WWCP_HTTPAPI WWCP { get; }

        #endregion

        #region E-Mail delegates

        #region E-Mail headers / footers

        const String HTMLEMailHeader = "<!DOCTYPE html>\r\n" +
                                        "<html>\r\n" +
                                          "<head>\r\n" +
                                              "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />\r\n" +
                                          "</head>\r\n" +
                                          "<body style=\"background-color: #ececec\">\r\n" +
                                            "<div style=\"width: 600px\">\r\n" +
                                              "<div style=\"border-bottom: 1px solid #AAAAAA; margin-bottom: 20px\">\r\n" +
                                                  "<img src=\"https://cardi-link.cloud/login/CardiLink_Logo01.png\" style=\"width: 250px; padding-right: 10px\" alt=\"CardiLink\">\r\n" +
                                              "</div>\r\n" +
                                              "<div style=\"border-bottom: 1px solid #AAAAAA; padding-left: 6px; padding-bottom: 40px; margin-bottom: 10px;\">\r\n";

        const String HTMLEMailFooter = "</div>\r\n" +
                                              "<div style=\"color: #AAAAAA; font-size: 70%\">\r\n" +
                                                  "Fingerprint: CE12 96F1 74B3 75F8 0BE9&nbsp;&nbsp;0E54 289B 709A 9E53 A226<br />\r\n" +
                                                  "CardiLink GmbH, Henkestr. 91, 91052 Erlangen, Germany<br />\r\n" +
                                                  "Commercial Register Number: Amtsgericht Fürth HRB 15812<br />\r\n" +
                                                  "Managing Director: Lars Wassermann<br />\r\n" +
                                              "</div>\r\n" +
                                            "</div>\r\n" +
                                          "</body>\r\n" +
                                        "</html>\r\n\r\n";

        const String TextEMailHeader = "CardiLink\r\n" +
                                        "---------\r\n\r\n";

        const String TextEMailFooter = "\r\n\r\n---------------------------------------------------------------\r\n" +
                                        "Fingerprint: CE12 96F1 74B3 75F8 0BE9  0E54 289B 709A 9E53 A226\r\n" +
                                        "CardiLink GmbH, Henkestr. 91, 91052 Erlangen, Germany\r\n" +
                                        "Commercial Register Number: Amtsgericht Fürth HRB 15812\r\n" +
                                        "Managing Director: Lars Wassermann\r\n\r\n";

        #endregion


        #region NewUserSignUpEMailCreatorDelegate

        private static readonly Func<String, EMailAddress, String, NewUserSignUpEMailCreatorDelegate>

            __NewUserSignUpEMailCreator = (BaseURL,
                                           APIEMailAddress,
                                           APIPassphrase)

                => (UserId,
                    EMailAddress,
                    Username,
                    SecurityToken,
                    Use2FactorAuth,
                    DNSHostname,
                    Language) => new HTMLEMailBuilder() {

                         From       = APIEMailAddress,
                         To         = EMailAddress,
                         Passphrase = APIPassphrase,
                         Subject    = "Your CardiCloud account had been created",

                         HTMLText   = HTMLEMailHeader +
                                          "Dear " + Username + " (" + UserId + "),<br /><br />" + Environment.NewLine +
                                          "your CardiCloud account has been created!<br /><br />" + Environment.NewLine +
                                          "Please click the following link to set a new password for your account" + (Use2FactorAuth ? " and check your mobile phone for an additional security token" : "") + "...<br /><br />" + Environment.NewLine +
                                          "<a href=\"" + DNSHostname + "/setPassword?" + SecurityToken + (Use2FactorAuth ? "&2factor" : "") + "\" style=\"text-decoration: none; color: #FFFFFF; background-color: #ff7300; Border: solid #ff7300; border-width: 10px 20px; line-height: 2; font-weight: bold; text-align: center; cursor: pointer; display: inline-block; border-radius: 4px; margin-top: 20px; font-size: 70%\">Set a new password</a>" + Environment.NewLine +
                                      HTMLEMailFooter,

                         PlainText  = TextEMailHeader +
                                          "Dear " + Username + " (" + UserId + ")," + Environment.NewLine +
                                          "your CardiCloud account has been created!" + Environment.NewLine + Environment.NewLine +
                                          "Please click the following link to set a new password for your account" + (Use2FactorAuth ? " and check your mobile phone for an additional security token" : "") + "..." + Environment.NewLine + Environment.NewLine +
                                          DNSHostname + "/setPassword?" + SecurityToken + (Use2FactorAuth ? "&2factor" : "") +
                                      TextEMailFooter,

                         SecurityLevel = EMailSecurity.sign
                     }.AsImmutable;

        #endregion

        #region NewUserWelcomeEMailCreatorDelegate

        private static readonly Func<String, EMailAddress, String, NewUserWelcomeEMailCreatorDelegate>

            __NewUserWelcomeEMailCreatorDelegate = (BaseURL,
                                                    APIEMailAddress,
                                                    APIPassphrase)

                => (Username,
                     EMailAddress,
                     Language) => new HTMLEMailBuilder() {

                         From = APIEMailAddress,
                         To = EMailAddress,
                         Passphrase = APIPassphrase,
                         Subject = "Welcome to the 'CardiCloud'...",

                         HTMLText = "<!DOCTYPE html><html>" + Environment.NewLine +
                                                                "<head>" + Environment.NewLine +
                                                                    "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" + Environment.NewLine +
                                                                "</head>" + Environment.NewLine +
                                                                "<body style=\"background-color: #ececec\">" +
                                                                "<div style=\"width: 600px\">" + Environment.NewLine +
                                                                    "<div style=\"border-bottom: 1px solid #AAAAAA; margin-bottom: 20px\">" +
                                                                        "<img src=\"https://cardi-link.cloud/login/CardiLink_Logo01.png\" style=\"width: 250px; padding-right: 10px\" alt=\"CardiLink\">" +
                                                                    "</div>" + Environment.NewLine +
                                                                    "<div style=\"border-bottom: 1px solid #AAAAAA; padding-left: 6px; padding-bottom: 40px; margin-bottom: 10px;\">" + Environment.NewLine +
                                                                        "Dear " + Username + ",<br /><br />" + Environment.NewLine +
                                                                        "welcome to your new CardiCloud account!<br /><br />" + Environment.NewLine +
                                                                        "<a href=\"https://api.cardi-link.cloud/login\" style=\"text-decoration: none; color: #FFFFFF; background-color: #ff7300; Border: solid #ff7300; border-width: 10px 20px; line-height: 2; font-weight: bold; text-align: center; cursor: pointer; display: inline-block; border-radius: 4px; margin-top: 20px; font-size: 70%\">Login</a>" + Environment.NewLine +
                                                                    "</div>" + Environment.NewLine +
                                                                    "<div style=\"color: #AAAAAA; font-size: 70%\">" + Environment.NewLine +
                                                                        "Fingerprint: CE12 96F1 74B3 75F8 0BE9&nbsp;&nbsp;0E54 289B 709A 9E53 A226<br />" + Environment.NewLine +
                                                                        "CardiLink GmbH, Henkestr. 91, 91052 Erlangen, Germany<br />" + Environment.NewLine +
                                                                        "Commercial Register Number: Amtsgericht Fürth HRB 15812<br />" + Environment.NewLine +
                                                                        "Managing Director: Lars Wassermann<br />" + Environment.NewLine +
                                                                    "</div>" + Environment.NewLine +
                                                                "</div></body></html>" + Environment.NewLine + Environment.NewLine,

                         PlainText = "CardiLink" + Environment.NewLine +
                                                                "---------" + Environment.NewLine + Environment.NewLine +
                                                                "Dear " + Username + "," + Environment.NewLine +
                                                                "welcome to your new CardiCloud account!" + Environment.NewLine + Environment.NewLine +
                                                                "Please login via: https://api.cardi-link.cloud/login" + Environment.NewLine + Environment.NewLine +
                                                                "---------------------------------------------------------------" + Environment.NewLine +
                                                                "Fingerprint: CE12 96F1 74B3 75F8 0BE9  0E54 289B 709A 9E53 A226" + Environment.NewLine +
                                                                "CardiLink GmbH, Henkestr. 91, 91052 Erlangen, Germany" + Environment.NewLine +
                                                                "Commercial Register Number: Amtsgericht Fürth HRB 15812" + Environment.NewLine +
                                                                "Managing Director: Lars Wassermann" + Environment.NewLine + Environment.NewLine,

                         SecurityLevel = EMailSecurity.sign
                     }.//AddAttachment("Hi there!".ToUTF8Bytes(), "welcome.txt", MailContentTypes.text_plain).
                                                                     AsImmutable;

        #endregion

        #region ResetPasswordEMailCreatorDelegate

        private static readonly Func<String, EMailAddress, String, ResetPasswordEMailCreatorDelegate>

            __ResetPasswordEMailCreatorDelegate = (BaseURL,
                                                   APIEMailAddress,
                                                   APIPassphrase)

                => (UserId,
                    EMailAddress,
                    Username,
                    SecurityToken,
                    Use2FactorAuth,
                    DNSHostname,
                    Language) => new HTMLEMailBuilder() {

                         From       = APIEMailAddress,
                         To         = EMailAddress,
                         Passphrase = APIPassphrase,
                         Subject    = "CardiCloud password reset...",

                         HTMLText   = HTMLEMailHeader +
                                          "Dear " + Username + " (" + UserId + "),<br /><br />" + Environment.NewLine +
                                          "someone - hopefully you - requested us to change your password!<br />" + Environment.NewLine +
                                          "If this request was your intention, please click the following link to set a new password...<br /><br />" + Environment.NewLine +
                                          "<a href=\"" + DNSHostname + "/setPassword?" + SecurityToken + (Use2FactorAuth ? "&2factor" : "") + "\" style=\"text-decoration: none; color: #FFFFFF; background-color: #ff7300; Border: solid #ff7300; border-width: 10px 20px; line-height: 2; font-weight: bold; text-align: center; cursor: pointer; display: inline-block; border-radius: 4px; margin-top: 20px; font-size: 70%\">Set a new password</a>" + Environment.NewLine +
                                      HTMLEMailFooter,

                         PlainText  = TextEMailHeader +
                                          "Dear " + Username + " (" + UserId + ")," + Environment.NewLine +
                                          "someone - hopefully you - requested us to change your password!" + Environment.NewLine +
                                          "If this request was your intention, please click the following link to set a new password..." + Environment.NewLine + Environment.NewLine +
                                          DNSHostname + "/setPassword?" + SecurityToken + (Use2FactorAuth ? "&2factor" : "") +
                                      TextEMailFooter,

                         SecurityLevel = EMailSecurity.sign
                     }.AsImmutable;

        #endregion

        #region PasswordChangedEMailCreatorDelegate

        private static readonly Func<String, EMailAddress, String, PasswordChangedEMailCreatorDelegate>

            __PasswordChangedEMailCreatorDelegate = (BaseURL,
                                                     APIEMailAddress,
                                                     APIPassphrase)

                => (UserId,
                    EMailAddress,
                    Username,
                    DNSHostname,
                    Language) => new HTMLEMailBuilder() {

                         From       = APIEMailAddress,
                         To         = EMailAddress,
                         Passphrase = APIPassphrase,
                         Subject    = "CardiCloud password changed...",

                         HTMLText   = HTMLEMailHeader +
                                          "Dear " + Username + ",<br /><br />" + Environment.NewLine +
                                          "your password has successfully been changed!<br />" + Environment.NewLine +
                                          "<a href=\"" + DNSHostname + "/login?" + UserId + "\" style=\"text-decoration: none; color: #FFFFFF; background-color: #ff7300; Border: solid #ff7300; border-width: 10px 20px; line-height: 2; font-weight: bold; text-align: center; cursor: pointer; display: inline-block; border-radius: 4px; margin-top: 20px; font-size: 70%\">Login</a>" + Environment.NewLine +
                                      HTMLEMailFooter,

                         PlainText  = TextEMailHeader +
                                          "Dear " + Username + "," + Environment.NewLine +
                                          "your password has successfully been changed!" + Environment.NewLine +
                                          DNSHostname + "/login?" + UserId +
                                      TextEMailFooter,

                         SecurityLevel = EMailSecurity.sign
                     }.AsImmutable;

        #endregion

        #endregion

        #region Constructor(s)

        #region OpenChargingCloudAPI(HTTPServerName = DefaultHTTPServerName, ...)

        /// <summary>
        /// Create an instance of the Open Charging Cloud API.
        /// </summary>
        /// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        /// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="HTTPServerPort">A TCP port to listen on.</param>
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="BaseURL">The base url of the service.</param>
        /// <param name="URLPathPrefix">A common prefix for all URIs.</param>
        /// 
        /// <param name="ServerCertificateSelector">An optional delegate to select a SSL/TLS server certificate.</param>
        /// <param name="ClientCertificateValidator">An optional delegate to verify the SSL/TLS client certificate used for authentication.</param>
        /// <param name="ClientCertificateSelector">An optional delegate to select the SSL/TLS client certificate used for authentication.</param>
        /// <param name="AllowedTLSProtocols">The SSL/TLS protocol(s) allowed for this connection.</param>
        /// 
        /// <param name="APIEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        /// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        /// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="SMSAPICredentials">The credentials for the SMS API.</param>
        /// <param name="APIAdminSMS">A list of admin SMS phonenumbers.</param>
        /// 
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="LogoImage">The logo of the website.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="MinUserNameLenght">The minimal user name length.</param>
        /// <param name="MinRealmLenght">The minimal realm length.</param>
        /// <param name="PasswordQualityCheck">A delegate to ensure a minimal password quality.</param>
        /// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        /// 
        /// <param name="ServerThreadName">The optional name of the TCP server thread.</param>
        /// <param name="ServerThreadPriority">The optional priority of the TCP server thread.</param>
        /// <param name="ServerThreadIsBackground">Whether the TCP server thread is a background thread or not.</param>
        /// <param name="ConnectionIdBuilder">An optional delegate to build a connection identification based on IP socket information.</param>
        /// <param name="ConnectionThreadsNameBuilder">An optional delegate to set the name of the TCP connection threads.</param>
        /// <param name="ConnectionThreadsPriorityBuilder">An optional delegate to set the priority of the TCP connection threads.</param>
        /// <param name="ConnectionThreadsAreBackground">Whether the TCP connection threads are background threads or not (default: yes).</param>
        /// <param name="ConnectionTimeout">The TCP client timeout for all incoming client connections in seconds (default: 30 sec).</param>
        /// <param name="MaxClientConnections">The maximum number of concurrent TCP client connections (default: 4096).</param>
        /// 
        /// <param name="SkipURLTemplates">Skip URI templates.</param>
        /// <param name="DisableNotifications">Disable external notifications.</param>
        /// <param name="DisableLogfile">Disable the log file.</param>
        /// <param name="LoggingPath">The path for all logfiles.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        /// <param name="DNSClient">The DNS client of the API.</param>
        /// <param name="Autostart">Whether to start the API automatically.</param>
        public OpenChargingCloudAPI(String                               HTTPServerName                     = DefaultHTTPServerName,
                                    IPPort?                              HTTPServerPort                     = null,
                                    HTTPHostname?                        HTTPHostname                       = null,
                                    String                               ServiceName                        = DefaultServiceName,
                                    String                               BaseURL                            = "",
                                    HTTPPath?                            URLPathPrefix                      = null,

                                    ServerCertificateSelectorDelegate    ServerCertificateSelector          = null,
                                    RemoteCertificateValidationCallback  ClientCertificateValidator         = null,
                                    LocalCertificateSelectionCallback    ClientCertificateSelector          = null,
                                    SslProtocols                         AllowedTLSProtocols                = SslProtocols.Tls12,

                                    EMailAddress                         APIEMailAddress                    = null,
                                    String                               APIPassphrase                      = null,
                                    EMailAddressList                     APIAdminEMails                     = null,
                                    SMTPClient                           APISMTPClient                      = null,

                                    Credentials                          SMSAPICredentials                  = null,
                                    IEnumerable<PhoneNumber>             APIAdminSMS                        = null,

                                    HTTPCookieName?                      CookieName                         = null,
                                    Languages?                           Language                           = DefaultLanguage,
                                    String                               LogoImage                          = null,
                                    NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator          = null,
                                    NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator         = null,
                                    ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator          = null,
                                    PasswordChangedEMailCreatorDelegate  PasswordChangedEMailCreator        = null,
                                    Byte                                 MinUserNameLenght                  = DefaultMinUserNameLenght,
                                    Byte                                 MinRealmLenght                     = DefaultMinRealmLenght,
                                    PasswordQualityCheckDelegate         PasswordQualityCheck               = null,
                                    TimeSpan?                            SignInSessionLifetime              = null,

                                    String                               ServerThreadName                   = null,
                                    ThreadPriority                       ServerThreadPriority               = ThreadPriority.AboveNormal,
                                    Boolean                              ServerThreadIsBackground           = true,
                                    ConnectionIdBuilder                  ConnectionIdBuilder                = null,
                                    ConnectionThreadsNameBuilder         ConnectionThreadsNameBuilder       = null,
                                    ConnectionThreadsPriorityBuilder     ConnectionThreadsPriorityBuilder   = null,
                                    Boolean                              ConnectionThreadsAreBackground     = true,
                                    TimeSpan?                            ConnectionTimeout                  = null,
                                    UInt32                               MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

                                    Boolean                              SkipURLTemplates                   = false,
                                    Boolean                              DisableNotifications               = false,
                                    Boolean                              DisableLogfile                     = false,
                                    String                               LoggingPath                        = null,
                                    String                               LogfileName                        = DefaultLogfileName,
                                    DNSClient                            DNSClient                          = null,
                                    Boolean                              Autostart                          = false)

            : base(HTTPServerName:               HTTPServerName,
                   HTTPServerPort:               HTTPServerPort ?? DefaultHTTPServerPort,
                   HTTPHostname:                 HTTPHostname,
                   ServiceName:                  ServiceName,
                   BaseURL:                      BaseURL,
                   URLPathPrefix:                URLPathPrefix,

                   ServerCertificateSelector:    ServerCertificateSelector,
                   ClientCertificateValidator:   ClientCertificateValidator,
                   ClientCertificateSelector:    ClientCertificateSelector,
                   AllowedTLSProtocols:          AllowedTLSProtocols,

                   APIEMailAddress:              APIEMailAddress,
                   APIPassphrase:                APIPassphrase,
                   APIAdminEMails:               APIAdminEMails,
                   APISMTPClient:                APISMTPClient,

                   SMSAPICredentials:            SMSAPICredentials,
                   APIAdminSMS:                  APIAdminSMS,

                   CookieName:                   CookieName,
                   Language:                     Languages.eng,
                   LogoImage:                    _LogoImage,
                   NewUserSignUpEMailCreator:    __NewUserSignUpEMailCreator          (BaseURL, APIEMailAddress, APIPassphrase),
                   NewUserWelcomeEMailCreator:   __NewUserWelcomeEMailCreatorDelegate (BaseURL, APIEMailAddress, APIPassphrase),
                   ResetPasswordEMailCreator:    __ResetPasswordEMailCreatorDelegate  (BaseURL, APIEMailAddress, APIPassphrase),
                   PasswordChangedEMailCreator:  __PasswordChangedEMailCreatorDelegate(BaseURL, APIEMailAddress, APIPassphrase),
                   MinUserNameLenght:            4,
                   MinRealmLenght:               2,
                   PasswordQualityCheck:         PasswordQualityCheck,
                   SignInSessionLifetime:        TimeSpan.FromDays(30),

                   SkipURLTemplates:             false,
                   DisableNotifications:         DisableNotifications,
                   DisableLogfile:               DisableLogfile,
                   LoggingPath:                  LoggingPath,
                   LogfileName:                  LogfileName,
                   DNSClient:                    DNSClient,
                   Autostart:                    Autostart)

        {

            this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

            RegisterURITemplates();

            if (Autostart)
                Start();

        }

        #endregion

        #region OpenChargingCloudAPI(HTTPServer, HTTPHostname = null, URIPrefix = null, ...)

        ///// <summary>
        ///// Create an instance of the Open Charging Cloud API.
        ///// </summary>
        ///// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="HTTPServerPort">A TCP port to listen on.</param>
        ///// <param name="URIPrefix">A common prefix for all URIs.</param>
        ///// 
        ///// <param name="ServerCertificateSelector">An optional delegate to select a SSL/TLS server certificate.</param>
        ///// <param name="ClientCertificateValidator">An optional delegate to verify the SSL/TLS client certificate used for authentication.</param>
        ///// <param name="ClientCertificateSelector">An optional delegate to select the SSL/TLS client certificate used for authentication.</param>
        ///// <param name="AllowedTLSProtocols">The SSL/TLS protocol(s) allowed for this connection.</param>
        ///// 
        ///// <param name="ServiceName">The name of the service.</param>
        ///// <param name="APIEMailAddress">An e-mail address for this API.</param>
        ///// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        ///// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        ///// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        ///// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        ///// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        ///// 
        ///// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        ///// <param name="Language">The main language of the API.</param>
        ///// <param name="LogoImage">The logo of the website.</param>
        ///// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        ///// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        ///// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        ///// <param name="MinUserNameLenght">The minimal user name length.</param>
        ///// <param name="MinRealmLenght">The minimal realm length.</param>
        ///// <param name="MinPasswordLenght">The minimal password length.</param>
        ///// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        ///// 
        ///// <param name="SkipURITemplates">Skip URI templates.</param>
        ///// <param name="DisableNotifications">Disable external notifications.</param>
        ///// <param name="DisableLogfile">Disable the log file.</param>
        ///// <param name="LogfileName">The name of the logfile for this API.</param>
        ///// <param name="DNSClient">The DNS client of the API.</param>
        ///// <param name="Autostart">Whether to start the API automatically.</param>
        //public OpenChargingCloudAPI(String                               HTTPServerName                     = DefaultHTTPServerName,
        //                            IPPort?                              HTTPServerPort                     = null,
        //                            HTTPHostname?                        HTTPHostname                       = null,
        //                            HTTPPath?                             URIPrefix                          = null,

        //                            ServerCertificateSelectorDelegate    ServerCertificateSelector          = null,
        //                            RemoteCertificateValidationCallback  ClientCertificateValidator         = null,
        //                            LocalCertificateSelectionCallback    ClientCertificateSelector          = null,
        //                            SslProtocols                         AllowedTLSProtocols                = SslProtocols.Tls12,

        //                            String                               ServiceName                        = DefaultServiceName,
        //                            EMailAddress                         APIEMailAddress                    = null,
        //                            PgpPublicKeyRing                     APIPublicKeyRing                   = null,
        //                            PgpSecretKeyRing                     APISecretKeyRing                   = null,
        //                            String                               APIPassphrase                      = null,
        //                            EMailAddressList                     APIAdminEMails                     = null,
        //                            SMTPClient                           APISMTPClient                      = null,

        //                            HTTPCookieName?                      CookieName                         = null,
        //                            Languages?                           Language                           = DefaultLanguage,
        //                            String                               LogoImage                          = null,
        //                            NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator          = null,
        //                            NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator         = null,
        //                            ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator          = null,
        //                            Byte                                 MinUserNameLenght                  = DefaultMinUserNameLenght,
        //                            Byte                                 MinRealmLenght                     = DefaultMinRealmLenght,
        //                            Byte                                 MinPasswordLenght                  = DefaultMinPasswordLenght,
        //                            TimeSpan?                            SignInSessionLifetime              = null,

        //                            String                               ServerThreadName                   = null,
        //                            ThreadPriority                       ServerThreadPriority               = ThreadPriority.AboveNormal,
        //                            Boolean                              ServerThreadIsBackground           = true,
        //                            ConnectionIdBuilder                  ConnectionIdBuilder                = null,
        //                            ConnectionThreadsNameBuilder         ConnectionThreadsNameBuilder       = null,
        //                            ConnectionThreadsPriorityBuilder     ConnectionThreadsPriorityBuilder   = null,
        //                            Boolean                              ConnectionThreadsAreBackground     = true,
        //                            TimeSpan?                            ConnectionTimeout                  = null,
        //                            UInt32                               MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

        //                            Boolean                              SkipURITemplates                   = false,
        //                            Boolean                              DisableNotifications               = false,
        //                            Boolean                              DisableLogfile                     = false,
        //                            String                               LogfileName                        = DefaultLogfileName,
        //                            DNSClient                            DNSClient                          = null,
        //                            Boolean                              Autostart                          = false)

        //    : base(HTTPServerName,
        //           HTTPServerPort ?? DefaultHTTPServerPort,
        //           HTTPHostname,
        //           URIPrefix      ?? DefaultURIPrefix,

        //           ServerCertificateSelector,
        //           ClientCertificateValidator,
        //           ClientCertificateSelector,
        //           AllowedTLSProtocols,

        //           ServiceName,
        //           APIEMailAddress,
        //           APIPublicKeyRing ?? OpenPGP.ReadPublicKeyRing(typeof(OpenChargingCloudAPI).Assembly.GetManifestResourceStream(HTTPRoot + "GPGKeys.robot@open.charging.cloud_pubring.gpg")),
        //           APISecretKeyRing,
        //           APIPassphrase,
        //           APIAdminEMails,
        //           APISMTPClient,

        //           CookieName ?? DefaultCookieName,
        //           Language   ?? DefaultLanguage,
        //           LogoImage,
        //           NewUserSignUpEMailCreator,
        //           NewUserWelcomeEMailCreator,
        //           ResetPasswordEMailCreator,
        //           MinUserNameLenght,
        //           MinRealmLenght,
        //           MinPasswordLenght,
        //           SignInSessionLifetime,

        //           ServerThreadName,
        //           ServerThreadPriority,
        //           ServerThreadIsBackground,
        //           ConnectionIdBuilder,
        //           ConnectionThreadsNameBuilder,
        //           ConnectionThreadsPriorityBuilder,
        //           ConnectionThreadsAreBackground,
        //           ConnectionTimeout,
        //           MaxClientConnections,

        //           SkipURITemplates,
        //           DisableNotifications,
        //           DisableLogfile,
        //           LogfileName,
        //           DNSClient,
        //           false)

        //{

        //    this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

        //    if (!SkipURITemplates)
        //        RegisterURITemplates();

        //    if (Autostart)
        //        Start();

        //}

        #endregion

        #region OpenChargingCloudAPI(HTTPServer, HTTPHostname = null, URIPrefix = null, ...)

        ///// <summary>
        ///// Attach this Open Charging Cloud API to the given HTTP server.
        ///// </summary>
        ///// <param name="HTTPServer">An existing HTTP server.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="URIPrefix">A common prefix for all URIs.</param>
        ///// 
        ///// <param name="ServiceName">The name of the service.</param>
        ///// <param name="APIEMailAddress">An e-mail address for this API.</param>
        ///// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        ///// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        ///// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        ///// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        ///// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        ///// 
        ///// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        ///// <param name="Language">The main language of the API.</param>
        ///// <param name="LogoImage">The logo of the website.</param>
        ///// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        ///// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        ///// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        ///// <param name="MinUserNameLenght">The minimal user name length.</param>
        ///// <param name="MinRealmLenght">The minimal realm length.</param>
        ///// <param name="MinPasswordLenght">The minimal password length.</param>
        ///// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        ///// 
        ///// <param name="SkipURITemplates">Skip URI templates.</param>
        ///// <param name="DisableNotifications">Disable external notifications.</param>
        ///// <param name="DisableLogfile">Disable the log file.</param>
        ///// <param name="LogfileName">The name of the logfile for this API.</param>
        //public OpenChargingCloudAPI(HTTPServer                          HTTPServer,
        //                            HTTPHostname?                       HTTPHostname                 = null,
        //                            HTTPPath?                            URIPrefix                    = null,

        //                            String                              ServiceName                  = DefaultServiceName,
        //                            EMailAddress                        APIEMailAddress              = null,
        //                            PgpPublicKeyRing                    APIPublicKeyRing             = null,
        //                            PgpSecretKeyRing                    APISecretKeyRing             = null,
        //                            String                              APIPassphrase                = null,
        //                            EMailAddressList                    APIAdminEMails               = null,
        //                            SMTPClient                          APISMTPClient                = null,

        //                            HTTPCookieName?                     CookieName                   = null,
        //                            Languages                           Language                     = DefaultLanguage,
        //                            String                              LogoImage                    = null,
        //                            NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator    = null,
        //                            NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator   = null,
        //                            ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator    = null,
        //                            Byte                                MinUserNameLenght            = DefaultMinUserNameLenght,
        //                            Byte                                MinRealmLenght               = DefaultMinRealmLenght,
        //                            Byte                                MinPasswordLenght            = DefaultMinPasswordLenght,
        //                            TimeSpan?                           SignInSessionLifetime        = null,

        //                            Boolean                             SkipURITemplates             = false,
        //                            Boolean                             DisableNotifications         = false,
        //                            Boolean                             DisableLogfile               = false,
        //                            String                              LogfileName                  = DefaultLogfileName)



        //    : base(HTTPServer,
        //           HTTPHostname,
        //           URIPrefix ?? DefaultURIPrefix,

        //           ServiceName,
        //           APIEMailAddress,
        //           APIPublicKeyRing ?? OpenPGP.ReadPublicKeyRing(typeof(OpenChargingCloudAPI).Assembly.GetManifestResourceStream(HTTPRoot + "GPGKeys.robot@open.charging.cloud_pubring.gpg")),
        //           APISecretKeyRing,
        //           APIPassphrase,
        //           APIAdminEMails,
        //           APISMTPClient,

        //           CookieName ?? DefaultCookieName,
        //           Language,
        //           LogoImage                  ?? _LogoImage,
        //           NewUserSignUpEMailCreator  ?? __NewUserSignUpEMailCreator         (APIEMailAddress, APIPassphrase),
        //           NewUserWelcomeEMailCreator ?? __NewUserWelcomeEMailCreatorDelegate(APIEMailAddress, APIPassphrase),
        //           ResetPasswordEMailCreator  ?? __ResetPasswordEMailCreatorDelegate (APIEMailAddress, APIPassphrase),
        //           MinUserNameLenght,
        //           MinRealmLenght,
        //           MinPasswordLenght,
        //           SignInSessionLifetime      ?? DefaultSignInSessionLifetime,

        //           SkipURITemplates,
        //           DisableNotifications,
        //           DisableLogfile,
        //           LogfileName)

        //{

        //    this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!"));

        //    if (!SkipURITemplates)
        //        RegisterURITemplates();

        //}

        #endregion

        #endregion


        #region (static) AttachToHTTPAPI(HTTPServer, URIPrefix = "/", ...)

        ///// <summary>
        ///// Attach this HTTP API to the given HTTP server.
        ///// </summary>
        ///// <param name="HTTPServer">An existing HTTP server.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="URIPrefix">A common prefix for all URIs.</param>
        ///// 
        ///// <param name="ServiceName">The name of the service.</param>
        ///// <param name="APIEMailAddress">An e-mail address for this API.</param>
        ///// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        ///// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        ///// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        ///// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        ///// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        ///// 
        ///// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        ///// <param name="Language">The main language of the API.</param>
        ///// <param name="LogoImage">The logo of the website.</param>
        ///// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        ///// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        ///// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        ///// <param name="MinUserNameLenght">The minimal user name length.</param>
        ///// <param name="MinRealmLenght">The minimal realm length.</param>
        ///// <param name="MinPasswordLenght">The minimal password length.</param>
        ///// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        ///// 
        ///// <param name="SkipURITemplates">Skip URI templates.</param>
        ///// <param name="DisableNotifications">Disable external notifications.</param>
        ///// <param name="DisableLogfile">Disable the log file.</param>
        ///// <param name="LogfileName">The name of the logfile for this API.</param>
        //public static OpenChargingCloudAPI AttachToHTTPAPI(HTTPServer                          HTTPServer,
        //                                                   HTTPHostname?                       HTTPHostname                 = null,
        //                                                   HTTPPath?                            URIPrefix                    = null,

        //                                                   String                              ServiceName                  = DefaultServiceName,
        //                                                   EMailAddress                        APIEMailAddress              = null,
        //                                                   PgpPublicKeyRing                    APIPublicKeyRing             = null,
        //                                                   PgpSecretKeyRing                    APISecretKeyRing             = null,
        //                                                   String                              APIPassphrase                = null,
        //                                                   EMailAddressList                    APIAdminEMails               = null,
        //                                                   SMTPClient                          APISMTPClient                = null,

        //                                                   HTTPCookieName?                     CookieName                   = null,
        //                                                   Languages                           Language                     = DefaultLanguage,
        //                                                   String                              LogoImage                    = null,
        //                                                   NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator    = null,
        //                                                   NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator   = null,
        //                                                   ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator    = null,
        //                                                   Byte                                MinUserNameLenght            = DefaultMinUserNameLenght,
        //                                                   Byte                                MinRealmLenght               = DefaultMinRealmLenght,
        //                                                   Byte                                MinPasswordLenght            = DefaultMinPasswordLenght,
        //                                                   TimeSpan?                           SignInSessionLifetime        = null,

        //                                                   Boolean                             SkipURITemplates             = false,
        //                                                   Boolean                             DisableNotifications         = false,
        //                                                   Boolean                             DisableLogfile               = false,
        //                                                   String                              LogfileName                  = DefaultLogfileName)


        //    => new OpenChargingCloudAPI(HTTPServer,
        //                                HTTPHostname,
        //                                URIPrefix ?? DefaultURIPrefix,

        //                                ServiceName,
        //                                APIEMailAddress,
        //                                APIPublicKeyRing,
        //                                APISecretKeyRing,
        //                                APIPassphrase,
        //                                APIAdminEMails,
        //                                APISMTPClient,

        //                                CookieName ?? DefaultCookieName,
        //                                Language,
        //                                LogoImage,
        //                                NewUserSignUpEMailCreator,
        //                                NewUserWelcomeEMailCreator,
        //                                ResetPasswordEMailCreator,
        //                                MinUserNameLenght,
        //                                MinRealmLenght,
        //                                MinPasswordLenght,
        //                                SignInSessionLifetime,

        //                                SkipURITemplates,
        //                                DisableNotifications,
        //                                DisableLogfile,
        //                                LogfileName);

        #endregion

        #region (private) RegisterURITemplates()

        private void RegisterURITemplates()
        {

            #region /shared/OpenChargingCloudAPI

            HTTPServer.RegisterResourcesFolder(HTTPHostname.Any,
                                               HTTPPath.Parse("/shared/OpenChargingCloudAPI"),
                                               HTTPRoot.Substring(0, HTTPRoot.Length - 1));

            #endregion

            //HTTPServer.RegisterResourcesFolder(HTTPHostname.Any,
            //                                   "/shared/OpenClouds",
            //                                   "com.GraphDefined.OpenClouds.HTTPRoot",
            //                                   typeof(OpenCloudsDummy).Assembly);

            #region / (HTTPRoot)

//            HTTPServer.AddMethodCallback(HTTPHostname.Any,
//                                         HTTPMethod.GET,
//                                         new String[] { "/index.html",
//                                                        "/",
//                                                        "/{FileName}"},
//                                         HTTPContentType.HTML_UTF8,
//                                         HTTPDelegate: Request => {

//                                             var FilePath = (Request.ParsedURIParameters != null && Request.ParsedURIParameters.Length > 0)
//                                                                ? Request.ParsedURIParameters.Last().Replace("/", ".")
//                                                                : "index.html";


//                                             if (FilePath.EndsWith(".html23"))
//                                             {

//                                                 var _MemoryStream1 = new MemoryStream();
//                                                 this.GetType().Assembly.GetManifestResourceStream(HTTPRoot + "template.html").SeekAndCopyTo(_MemoryStream1, 0);
//                                                 var Template = _MemoryStream1.ToArray().ToUTF8String();

//                                                 var _MemoryStream2 = new MemoryStream();
//                                                 this.GetType().Assembly.GetManifestResourceStream(HTTPRoot + "" + FilePath).SeekAndCopyTo(_MemoryStream2, 3);

//                                                 return Task.FromResult(
//                                                     new HTTPResponseBuilder(Request) {
//                                                         HTTPStatusCode  = HTTPStatusCode.OK,
//                                                         ContentType     = HTTPContentType.HTML_UTF8,
//                                                         Content         = Template.Replace("<%= content %>",   _MemoryStream2.ToArray().ToUTF8String()).
//                                                                                    Replace("<%= logoimage %>", String.Concat(@"<img src=""", LogoImage, @""" /> ")).
//                                                                                    ToUTF8Bytes(),
//                                                         Connection      = "close"
//                                                     }.AsImmutable);

//                                             }

//                                             else
//                                             {

//                                                 var _MemoryStream = new MemoryStream();
//                                                 var _FileStream   = this.GetType().Assembly.GetManifestResourceStream(HTTPRoot + "" + FilePath);

//                                                 #region File not found!

//                                                 if (_FileStream == null)
//                                                     return Task.FromResult(
//                                                         new HTTPResponseBuilder(Request) {
//                                                             HTTPStatusCode  = HTTPStatusCode.NotFound,
//                                                             Server          = HTTPServer.DefaultServerName,
//                                                             Date            = DateTime.Now,
//                                                             CacheControl    = "public, max-age=300",
//                                                             Connection      = "close"
//                                                         }.AsImmutable);

//                                                 #endregion

//                                                 _FileStream.SeekAndCopyTo(_MemoryStream, 0);

//                                                 #region Choose HTTP Content Type based on the file name extention...

//                                                 HTTPContentType ResponseContentType = null;

//                                                 var FileName = FilePath.Substring(FilePath.LastIndexOf("/") + 1);

//                                                 // Get the appropriate content type based on the suffix of the requested resource
//                                                 switch (FileName.Remove(0, FileName.LastIndexOf(".") + 1))
//                                                 {
//                                                     case "htm" : ResponseContentType = HTTPContentType.HTML_UTF8;       break;
//                                                     case "html": ResponseContentType = HTTPContentType.HTML_UTF8;       break;
//                                                     case "css" : ResponseContentType = HTTPContentType.CSS_UTF8;        break;
//                                                     case "gif" : ResponseContentType = HTTPContentType.GIF;             break;
//                                                     case "jpg" : ResponseContentType = HTTPContentType.JPEG;            break;
//                                                     case "jpeg": ResponseContentType = HTTPContentType.JPEG;            break;
//                                                     case "svg" : ResponseContentType = HTTPContentType.SVG;             break;
//                                                     case "png" : ResponseContentType = HTTPContentType.PNG;             break;
//                                                     case "ico" : ResponseContentType = HTTPContentType.ICO;             break;
//                                                     case "swf" : ResponseContentType = HTTPContentType.SWF;             break;
//                                                     case "js"  : ResponseContentType = HTTPContentType.JAVASCRIPT_UTF8; break;
//                                                     case "txt" : ResponseContentType = HTTPContentType.TEXT_UTF8;       break;
//                                                     case "xml" : ResponseContentType = HTTPContentType.XMLTEXT_UTF8;    break;
//                                                     default:     ResponseContentType = HTTPContentType.OCTETSTREAM;     break;
//                                                 }

//                                                 #endregion

//                                                 #region Create HTTP Response

//                                                 return Task.FromResult(
//                                                     new HTTPResponseBuilder(Request) {
//                                                         HTTPStatusCode  = HTTPStatusCode.OK,
//                                                         Server          = HTTPServer.DefaultServerName,
//                                                         Date            = DateTime.Now,
//                                                         ContentType     = ResponseContentType,
//                                                         Content         = _MemoryStream.ToArray(),
////                                                         CacheControl    = "public, max-age=300",
//                                                         //Expires          = "Mon, 25 Jun 2015 21:31:12 GMT",
////                                                         KeepAlive       = new KeepAliveType(TimeSpan.FromMinutes(5), 500),
////                                                         Connection      = "Keep-Alive",
//                                                         Connection = "close"
//                                                     }.AsImmutable);

//                                                 #endregion

//                                             }

//                                         });

            #endregion

        }

        #endregion


        #region (protected) GetOpenChargingCloudAPIRessource(Ressource)

        /// <summary>
        /// Get an embedded ressource of the Open Charging Cloud API.
        /// </summary>
        /// <param name="Ressource">The path and name of the ressource to load.</param>
        protected Stream GetOpenChargingCloudAPIRessource(String Ressource)
            => GetType().Assembly.GetManifestResourceStream(HTTPRoot + Ressource);

        #endregion


    }

}
