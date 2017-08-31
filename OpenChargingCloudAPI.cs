/*
 * Copyright (c) 2014-2017, Achim 'ahzf' Friedland <achim@graphdefined.org>
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
using org.GraphDefined.WWCP.Net;
using org.GraphDefined.OpenData;
using org.GraphDefined.OpenData.Users;

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
        public  new static readonly IPPort          DefaultHTTPServerPort               = new IPPort(5500);

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
        private const               String          _LogoImage                          = "images/OpenChargingCloud_Logo2.png";


        public new const            String          DataRouterURIPrefix                 = "/datarouter";

        /// <summary>
        /// The name of the common log file.
        /// </summary>
        public new const            String          DefaultLogfileName                  = "OpenChargingCloud.log";

        /// <summary>
        /// The name of the default HTTP cookie.
        /// </summary>
        public new const            String          DefaultCookieName                   = "OpenChargingCloud";

        #endregion

        #region Properties

        public WWCP_HTTPAPI WWCP { get; }

        #endregion

        #region E-Mail delegates

        #region NewUserSignUpEMailCreatorDelegate

        private static Func<EMailAddress, String, NewUserSignUpEMailCreatorDelegate>

            __NewUserSignUpEMailCreator = (APIEMailAddress,
                                           APIPassphrase)

                => (Login,
                    EMailAddress,
                    Language,
                    VerificationToken)

                    => new HTMLEMailBuilder() {

                                       From           = APIEMailAddress,
                                       To             = EMailAddress,
                                       Passphrase     = APIPassphrase,
                                       Subject        = "Your new account at 'Open Charging Cloud'...",

                                       HTMLText       = "<!DOCTYPE html><html>" + Environment.NewLine +
                                                        "<head>" + Environment.NewLine +
                                                            "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" + Environment.NewLine +
                                                        "</head>" + Environment.NewLine +
                                                        "<body>" +
                                                        "<div style=\"width: 600px\">" + Environment.NewLine +
                                                            "<div style=\"border-bottom: 1px solid #AAAAAA; margin-bottom: 20px\">" +
                                                                "<h1 style=\"font-size: 120%\">" +
                                                                    "<img src=\"http://open.charging.cloud/images/OpenChargingCloud_Logo2.png\" style=\"padding-right: 10px\">" +
                                                                    "Open Charging Cloud" +
                                                                "</h1>" +
                                                            "</div>" + Environment.NewLine +
                                                            "<div style=\"border-bottom: 1px solid #AAAAAA; padding-left: 6px; padding-bottom: 40px; margin-bottom: 10px;\">" + Environment.NewLine +
                                                                "Hello " + Login + "!<br /><br />" +
                                                                "Wir freuen uns, Dich als neuen Mitstreitenden für <b>Offene Daten</b> in Jena begrüßen zu können! " +
                                                                "Um Deinen neuen Account gleich freizuschalten, klicke bitte auf folgenden Link...<br />" + Environment.NewLine +
                                                                "<a href=\"https://open.charging.cloud/verificationtokens/" + VerificationToken + "\" style=\"text-decoration: none; Color: #FFFFFF; Background-color: #348eda; Border: solid #348eda; border-width: 10px 20px; line-height: 2; font-weight: bold; text-align: center; cursor: pointer; display: inline-block; border-radius: 4px; margin-top: 20px; font-size: 70%\">Account bestätigen</a>" + Environment.NewLine +
                                                            "</div>" + Environment.NewLine +
                                                            "<img src=\"http://graphdefined.con/images/logo.png\"     style=\"width: 100px; padding-left: 10px\" alt=\"[ GraphDefined GmbH ]\">" + Environment.NewLine +
                                                        "</div></body></html>" + Environment.NewLine + Environment.NewLine,

                                       PlainText      = "Open Charging Cloud" + Environment.NewLine +
                                                        "===================" + Environment.NewLine + Environment.NewLine +
                                                        "Hallo " + Login + "!" + Environment.NewLine + Environment.NewLine +
                                                        "Wir freuen uns, Dich als neuen Mitstreitenden für *Offene Daten* in Jena" + Environment.NewLine +
                                                        "begrüßen zu können! Um Deinen neuen Account gleich freizuschalten, klicke" + Environment.NewLine +
                                                        "bitte auf folgenden Link:" + Environment.NewLine +
                                                        "https://open.charging.cloud/verificationtokens/" + VerificationToken + Environment.NewLine + Environment.NewLine +
                                                        "---------------------------------------------------------------" + Environment.NewLine +
                                                        "Fingerprint: AE0D 5C5C 4EB5 C3F0 683E 2173 B1EA 6EEA A89A 2896" + Environment.NewLine +
                                                        "[ GraphDefined GmbH ]" + Environment.NewLine + Environment.NewLine,

                                       SecurityLevel  = EMailSecurity.auto

                                   }.//AddAttachment("Hi there!".ToUTF8Bytes(), "welcome.txt", MailContentTypes.text_plain).
                                     AsImmutable;

        #endregion

        #region NewUserWelcomeEMailCreatorDelegate

        private static Func<EMailAddress, String, NewUserWelcomeEMailCreatorDelegate>

            __NewUserWelcomeEMailCreatorDelegate = (APIEMailAddress,
                                                    APIPassphrase)

                =>(Username,
                    EMailAddress,
                    Language) => new HTMLEMailBuilder() {

                                              From           = APIEMailAddress,
                                              To             = EMailAddress,
                                              Passphrase     = APIPassphrase,
                                              Subject        = "Welcome to the 'Open Charging Cloud'...",

                                              HTMLText       = "<!DOCTYPE html><html>" + Environment.NewLine +
                                                               "<head>" + Environment.NewLine +
                                                                   "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" + Environment.NewLine +
                                                               "</head>" + Environment.NewLine +
                                                               "<body>" +
                                                               "<div style=\"width: 600px\">" + Environment.NewLine +
                                                                   "<div style=\"border-bottom: 1px solid #AAAAAA; margin-bottom: 20px\">" +
                                                                       "<h1 style=\"font-size: 120%\">" +
                                                                           "<img src=\"http://offenes-jena.de/images/JenaLogo0001s2.png\" style=\"padding-right: 10px\">" +
                                                                           "Offenes Jena" +
                                                                       "</h1>" +
                                                                   "</div>" + Environment.NewLine +
                                                                   "<div style=\"border-bottom: 1px solid #AAAAAA; padding-left: 6px; padding-bottom: 40px; margin-bottom: 10px;\">" + Environment.NewLine +
                                                                       "Hallo " + Username + "!<br /><br />" +
                                                                       "Wir freuen uns, Dich als neuen Mitstreitenden für <b>Offene Daten</b> in Jena begrüßen zu können! " +
                                                                       "Dein Account ist nun freigeschaltet. Probier es am besten gleich mal aus!<br />" + Environment.NewLine +
                                                                   "</div>" + Environment.NewLine +
                                                                   "<img src=\"http://offenes-jena.de/local/images/OKFN/CFG_logo.png\" style=\"width: 100px; padding-left:  6px\" alt=\"[ Code for Germany ]\">" +
                                                                   "<img src=\"http://offenes-jena.de/local/images/OKFN/OKFN.png\"     style=\"width: 100px; padding-left: 10px\" alt=\"[ Open Knowledge Foundation ]\">" + Environment.NewLine +
                                                               "</div></body></html>" + Environment.NewLine + Environment.NewLine,

                                              PlainText      = "Offenes Jena" + Environment.NewLine +
                                                               "============" + Environment.NewLine + Environment.NewLine +
                                                               "Hallo " + Username + "!" + Environment.NewLine + Environment.NewLine +
                                                               "Wir freuen uns, Dich als neuen Mitstreitenden für *Offene Daten* in Jena" + Environment.NewLine +
                                                               "begrüßen zu können! Um Deinen neuen Account gleich freizuschalten, klicke" + Environment.NewLine +
                                                               "bitte auf folgenden Link:" + Environment.NewLine +
                                                               "---------------------------------------------------------------" + Environment.NewLine +
                                                               "Fingerprint: AE0D 5C5C 4EB5 C3F0 683E 2173 B1EA 6EEA A89A 2896" + Environment.NewLine +
                                                               "[ Code for Germany ] [ Open Knowledge Foundation ]" + Environment.NewLine + Environment.NewLine,

                                              SecurityLevel  = EMailSecurity.auto
                                          }.//AddAttachment("Hi there!".ToUTF8Bytes(), "welcome.txt", MailContentTypes.text_plain).
                                                                    AsImmutable;

        #endregion

        #region ResetPasswordEMailCreatorDelegate

        private static Func<EMailAddress, String, ResetPasswordEMailCreatorDelegate>

            __ResetPasswordEMailCreatorDelegate = (APIEMailAddress,
                                                   APIPassphrase)

                =>(Username,
                    EMailAddress,
                    Language) => new HTMLEMailBuilder() {

                                              From           = APIEMailAddress,
                                              To             = EMailAddress,
                                              Passphrase     = APIPassphrase,
                                              Subject        = "Dein Passwort...",

                                              HTMLText       = "<!DOCTYPE html><html>" + Environment.NewLine +
                                                               "<head>" + Environment.NewLine +
                                                                   "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />" + Environment.NewLine +
                                                               "</head>" + Environment.NewLine +
                                                               "<body>" +
                                                               "<div style=\"width: 600px\">" + Environment.NewLine +
                                                                   "<div style=\"border-bottom: 1px solid #AAAAAA; margin-bottom: 20px\">" +
                                                                       "<h1 style=\"font-size: 120%\">" +
                                                                           "<img src=\"http://offenes-jena.de/images/JenaLogo0001s2.png\" style=\"padding-right: 10px\">" +
                                                                           "Offenes Jena" +
                                                                       "</h1>" +
                                                                   "</div>" + Environment.NewLine +
                                                                   "<div style=\"border-bottom: 1px solid #AAAAAA; padding-left: 6px; padding-bottom: 40px; margin-bottom: 10px;\">" + Environment.NewLine +
                                                                       "Hallo " + Username + "!<br /><br />" +
                                                                       "Wir freuen uns, Dich als neuen Mitstreitenden für <b>Offene Daten</b> in Jena begrüßen zu können! " +
                                                                       "Dein Account ist nun freigeschaltet. Probier es am besten gleich mal aus!<br />" + Environment.NewLine +
                                                                   "</div>" + Environment.NewLine +
                                                                   "<img src=\"http://offenes-jena.de/local/images/OKFN/CFG_logo.png\" style=\"width: 100px; padding-left:  6px\" alt=\"[ Code for Germany ]\">" +
                                                                   "<img src=\"http://offenes-jena.de/local/images/OKFN/OKFN.png\"     style=\"width: 100px; padding-left: 10px\" alt=\"[ Open Knowledge Foundation ]\">" + Environment.NewLine +
                                                               "</div></body></html>" + Environment.NewLine + Environment.NewLine,

                                              PlainText      = "Offenes Jena" + Environment.NewLine +
                                                               "============" + Environment.NewLine + Environment.NewLine +
                                                               "Hallo " + Username + "!" + Environment.NewLine + Environment.NewLine +
                                                               "Wir freuen uns, Dich als neuen Mitstreitenden für *Offene Daten* in Jena" + Environment.NewLine +
                                                               "begrüßen zu können! Um Deinen neuen Account gleich freizuschalten, klicke" + Environment.NewLine +
                                                               "bitte auf folgenden Link:" + Environment.NewLine +
                                                               "---------------------------------------------------------------" + Environment.NewLine +
                                                               "Fingerprint: AE0D 5C5C 4EB5 C3F0 683E 2173 B1EA 6EEA A89A 2896" + Environment.NewLine +
                                                               "[ Code for Germany ] [ Open Knowledge Foundation ]" + Environment.NewLine + Environment.NewLine,

                                              SecurityLevel  = EMailSecurity.auto
                                          }.//AddAttachment("Hi there!".ToUTF8Bytes(), "welcome.txt", MailContentTypes.text_plain).
                                                                    AsImmutable;

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
        /// <param name="URIPrefix">A common prefix for all URIs.</param>
        /// 
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="APIEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        /// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        /// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        /// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        /// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="SkipURITemplates">Skip URI templates.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        /// <param name="DNSClient">The DNS client of the API.</param>
        /// <param name="Autostart">Whether to start the API automatically.</param>
        public OpenChargingCloudAPI(String                              HTTPServerName                     = DefaultHTTPServerName,
                                    IPPort                              HTTPServerPort                     = null,
                                    String                              HTTPHostname                       = null,
                                    String                              URIPrefix                          = "/",

                                    String                              ServiceName                        = DefaultServiceName,
                                    EMailAddress                        APIEMailAddress                    = null,
                                    PgpPublicKeyRing                    APIPublicKeyRing                   = null,
                                    PgpSecretKeyRing                    APISecretKeyRing                   = null,
                                    String                              APIPassphrase                      = null,
                                    EMailAddressList                    APIAdminEMails                     = null,
                                    SMTPClient                          APISMTPClient                      = null,

                                    String                              CookieName                         = DefaultCookieName,
                                    Languages?                          Language                           = DefaultLanguage,
                                    String                              LogoImage                          = null,
                                    NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator          = null,
                                    NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator         = null,
                                    ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator          = null,
                                    Byte?                               MinUserNameLenght                  = DefaultMinUserNameLenght,
                                    Byte?                               MinRealmLenght                     = DefaultMinRealmLenght,
                                    Byte?                               MinPasswordLenght                  = DefaultMinPasswordLenght,
                                    TimeSpan?                           SignInSessionLifetime              = null,

                                    String                              ServerThreadName                   = null,
                                    ThreadPriority                      ServerThreadPriority               = ThreadPriority.AboveNormal,
                                    Boolean                             ServerThreadIsBackground           = true,
                                    ConnectionIdBuilder                 ConnectionIdBuilder                = null,
                                    ConnectionThreadsNameBuilder        ConnectionThreadsNameBuilder       = null,
                                    ConnectionThreadsPriorityBuilder    ConnectionThreadsPriorityBuilder   = null,
                                    Boolean                             ConnectionThreadsAreBackground     = true,
                                    TimeSpan?                           ConnectionTimeout                  = null,
                                    UInt32                              MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

                                    Boolean                             SkipURITemplates                   = false,
                                    String                              LogfileName                        = DefaultLogfileName,
                                    DNSClient                           DNSClient                          = null)

            : base(HTTPServerName,
                   HTTPServerPort ?? DefaultHTTPServerPort,
                   HTTPHostname,
                   URIPrefix,

                   ServiceName,
                   APIEMailAddress,
                   APIPublicKeyRing != null ? APIPublicKeyRing : OpenPGP.ReadPublicKeyRing(typeof(OpenChargingCloudAPI).Assembly.GetManifestResourceStream(HTTPRoot + "GPGKeys.robot@open.charging.cloud_pubring.gpg")),
                   APISecretKeyRing,
                   APIPassphrase,
                   APIAdminEMails,
                   APISMTPClient,

                   CookieName.IsNotNullOrEmpty() ? CookieName : DefaultCookieName,
                   Language            ?? Languages.eng,
                   LogoImage                  ?? _LogoImage,
                   NewUserSignUpEMailCreator  ?? __NewUserSignUpEMailCreator         (APIEMailAddress, APIPassphrase),
                   NewUserWelcomeEMailCreator ?? __NewUserWelcomeEMailCreatorDelegate(APIEMailAddress, APIPassphrase),
                   ResetPasswordEMailCreator  ?? __ResetPasswordEMailCreatorDelegate (APIEMailAddress, APIPassphrase),
                   MinUserNameLenght          ?? 4,
                   MinRealmLenght             ?? 2,
                   MinPasswordLenght          ?? 8,
                   SignInSessionLifetime      ?? TimeSpan.FromDays(30),

                   ServerThreadName,
                   ServerThreadPriority,
                   ServerThreadIsBackground,
                   ConnectionIdBuilder,
                   ConnectionThreadsNameBuilder,
                   ConnectionThreadsPriorityBuilder,
                   ConnectionThreadsAreBackground,
                   ConnectionTimeout,
                   MaxClientConnections,

                   SkipURITemplates,
                   LogfileName ?? DefaultLogfileName,
                   DNSClient)

        {

            this.WWCP  = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

            RegisterURITemplates();

        }

        #endregion

        #region OpenChargingCloudAPI(HTTPServerName = DefaultHTTPServerName, ...)

        /// <summary>
        /// Create an instance of the Open Charging Cloud API.
        /// </summary>
        /// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        /// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="HTTPServerPort">A TCP port to listen on.</param>
        /// <param name="URIPrefix">A common prefix for all URIs.</param>
        /// 
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="APIEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        /// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        /// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        /// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        /// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="LogoImage">The logo of the website.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="MinUserNameLenght">The minimal user name length.</param>
        /// <param name="MinRealmLenght">The minimal realm length.</param>
        /// <param name="MinPasswordLenght">The minimal password length.</param>
        /// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        /// 
        /// <param name="SkipURITemplates">Skip URI templates.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        /// <param name="DNSClient">The DNS client of the API.</param>
        /// <param name="Autostart">Whether to start the API automatically.</param>
        public OpenChargingCloudAPI(String                              HTTPServerName                     = DefaultHTTPServerName,
                                    IPPort                              HTTPServerPort                     = null,
                                    String                              HTTPHostname                       = null,
                                    String                              URIPrefix                          = "/",

                                    String                              ServiceName                        = DefaultServiceName,
                                    EMailAddress                        APIEMailAddress                    = null,
                                    PgpPublicKeyRing                    APIPublicKeyRing                   = null,
                                    PgpSecretKeyRing                    APISecretKeyRing                   = null,
                                    String                              APIPassphrase                      = null,
                                    EMailAddressList                    APIAdminEMails                     = null,
                                    SMTPClient                          APISMTPClient                      = null,

                                    String                              CookieName                         = DefaultCookieName,
                                    Languages?                          Language                           = DefaultLanguage,
                                    String                              LogoImage                          = null,
                                    NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator          = null,
                                    NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator         = null,
                                    ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator          = null,
                                    Byte                                MinUserNameLenght                  = DefaultMinUserNameLenght,
                                    Byte                                MinRealmLenght                     = DefaultMinRealmLenght,
                                    Byte                                MinPasswordLenght                  = DefaultMinPasswordLenght,
                                    TimeSpan?                           SignInSessionLifetime              = null,

                                    String                              ServerThreadName                   = null,
                                    ThreadPriority                      ServerThreadPriority               = ThreadPriority.AboveNormal,
                                    Boolean                             ServerThreadIsBackground           = true,
                                    ConnectionIdBuilder                 ConnectionIdBuilder                = null,
                                    ConnectionThreadsNameBuilder        ConnectionThreadsNameBuilder       = null,
                                    ConnectionThreadsPriorityBuilder    ConnectionThreadsPriorityBuilder   = null,
                                    Boolean                             ConnectionThreadsAreBackground     = true,
                                    TimeSpan?                           ConnectionTimeout                  = null,
                                    UInt32                              MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

                                    Boolean                             SkipURITemplates                   = false,
                                    String                              LogfileName                        = DefaultLogfileName,
                                    DNSClient                           DNSClient                          = null,
                                    Boolean                             Autostart                          = false)

            : base(HTTPServerName,
                   HTTPServerPort ?? DefaultHTTPServerPort,
                   HTTPHostname,
                   URIPrefix,

                   ServiceName,
                   APIEMailAddress,
                   APIPublicKeyRing ?? OpenPGP.ReadPublicKeyRing(typeof(OpenChargingCloudAPI).Assembly.GetManifestResourceStream(HTTPRoot + "GPGKeys.robot@open.charging.cloud_pubring.gpg")),
                   APISecretKeyRing,
                   APIPassphrase,
                   APIAdminEMails,
                   APISMTPClient,

                   CookieName.IsNotNullOrEmpty() ? CookieName : DefaultCookieName,
                   Language ?? DefaultLanguage,
                   LogoImage,
                   NewUserSignUpEMailCreator,
                   NewUserWelcomeEMailCreator,
                   ResetPasswordEMailCreator,
                   MinUserNameLenght,
                   MinRealmLenght,
                   MinPasswordLenght,
                   SignInSessionLifetime,

                   ServerThreadName,
                   ServerThreadPriority,
                   ServerThreadIsBackground,
                   ConnectionIdBuilder,
                   ConnectionThreadsNameBuilder,
                   ConnectionThreadsPriorityBuilder,
                   ConnectionThreadsAreBackground,
                   ConnectionTimeout,
                   MaxClientConnections,

                   SkipURITemplates,
                   LogfileName,
                   DNSClient,
                   false)

        {

            this.WWCP  = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

            if (!SkipURITemplates)
                RegisterURITemplates();

            if (Autostart)
                Start();

        }

        #endregion

        #region (private) OpenChargingCloudAPI(HTTPServer, HTTPHostname = "*", URIPrefix = "/", ...)

        /// <summary>
        /// Attach this Open Charging Cloud API to the given HTTP server.
        /// </summary>
        /// <param name="HTTPServer">An existing HTTP server.</param>
        /// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="URIPrefix">A common prefix for all URIs.</param>
        /// 
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="APIEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        /// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        /// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        /// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        /// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="LogoImage">The logo of the website.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="MinUserNameLenght">The minimal user name length.</param>
        /// <param name="MinRealmLenght">The minimal realm length.</param>
        /// <param name="MinPasswordLenght">The minimal password length.</param>
        /// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        /// 
        /// <param name="SkipURITemplates">Skip URI templates.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        private OpenChargingCloudAPI(HTTPServer                          HTTPServer,
                                     String                              HTTPHostname                 = null,
                                     String                              URIPrefix                    = "/",

                                     String                              ServiceName                  = DefaultServiceName,
                                     EMailAddress                        APIEMailAddress              = null,
                                     PgpPublicKeyRing                    APIPublicKeyRing             = null,
                                     PgpSecretKeyRing                    APISecretKeyRing             = null,
                                     String                              APIPassphrase                = null,
                                     EMailAddressList                    APIAdminEMails               = null,
                                     SMTPClient                          APISMTPClient                = null,

                                     String                              CookieName                   = DefaultCookieName,
                                     Languages                           Language                     = DefaultLanguage,
                                     String                              LogoImage                    = null,
                                     NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator    = null,
                                     NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator   = null,
                                     ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator    = null,
                                     Byte                                MinUserNameLenght            = DefaultMinUserNameLenght,
                                     Byte                                MinRealmLenght               = DefaultMinRealmLenght,
                                     Byte                                MinPasswordLenght            = DefaultMinPasswordLenght,
                                     TimeSpan?                           SignInSessionLifetime        = null,

                                     Boolean                             SkipURITemplates             = false,
                                     String                              LogfileName                  = DefaultLogfileName)

            : base(HTTPServer,
                   HTTPHostname,
                   URIPrefix,

                   ServiceName,
                   APIEMailAddress,
                   APIPublicKeyRing ?? OpenPGP.ReadPublicKeyRing(typeof(OpenChargingCloudAPI).Assembly.GetManifestResourceStream(HTTPRoot + "GPGKeys.robot@open.charging.cloud_pubring.gpg")),
                   APISecretKeyRing,
                   APIPassphrase,
                   APIAdminEMails,
                   APISMTPClient,

                   CookieName.IsNotNullOrEmpty() ? CookieName : DefaultCookieName,
                   Language,
                   LogoImage                  ?? _LogoImage,
                   NewUserSignUpEMailCreator  ?? __NewUserSignUpEMailCreator         (APIEMailAddress, APIPassphrase),
                   NewUserWelcomeEMailCreator ?? __NewUserWelcomeEMailCreatorDelegate(APIEMailAddress, APIPassphrase),
                   ResetPasswordEMailCreator  ?? __ResetPasswordEMailCreatorDelegate (APIEMailAddress, APIPassphrase),
                   MinUserNameLenght,
                   MinRealmLenght,
                   MinPasswordLenght,
                   SignInSessionLifetime      ?? DefaultSignInSessionLifetime,

                   SkipURITemplates,
                   LogfileName)

        {

            #region Initial checks

            if (HTTPServer == null)
                throw new ArgumentNullException(nameof(HTTPServer),  "The given HTTP server must not be null!");

            if (URIPrefix == null)
                URIPrefix = "/";

            if (!URIPrefix.StartsWith("/", StringComparison.Ordinal))
                URIPrefix = "/" + URIPrefix;

            #endregion

            this.WWCP  = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

            if (!SkipURITemplates)
                RegisterURITemplates();

        }

        #endregion

        #endregion


        #region (static) AttachToHTTPAPI(HTTPServer, URIPrefix = "/", ...)

        /// <summary>
        /// Attach this HTTP API to the given HTTP server.
        /// </summary>
        /// <param name="HTTPServer">An existing HTTP server.</param>
        /// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="URIPrefix">A common prefix for all URIs.</param>
        /// 
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="APIEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIPublicKeyRing">A GPG public key for this API.</param>
        /// <param name="APISecretKeyRing">A GPG secret key for this API.</param>
        /// <param name="APIPassphrase">A GPG passphrase for this API.</param>
        /// <param name="APIAdminEMails">A list of admin e-mail addresses.</param>
        /// <param name="APISMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="LogoImage">The logo of the website.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="MinUserNameLenght">The minimal user name length.</param>
        /// <param name="MinRealmLenght">The minimal realm length.</param>
        /// <param name="MinPasswordLenght">The minimal password length.</param>
        /// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        /// 
        /// <param name="SkipURITemplates">Skip URI templates.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        public static OpenChargingCloudAPI AttachToHTTPAPI(HTTPServer                          HTTPServer,
                                                           String                              HTTPHostname                 = null,
                                                           String                              URIPrefix                    = "/",

                                                           String                              ServiceName                  = DefaultServiceName,
                                                           EMailAddress                        APIEMailAddress              = null,
                                                           PgpPublicKeyRing                    APIPublicKeyRing             = null,
                                                           PgpSecretKeyRing                    APISecretKeyRing             = null,
                                                           String                              APIPassphrase                = null,
                                                           EMailAddressList                    APIAdminEMails               = null,
                                                           SMTPClient                          APISMTPClient                = null,

                                                           String                              CookieName                   = DefaultCookieName,
                                                           Languages                           Language                     = DefaultLanguage,
                                                           String                              LogoImage                    = null,
                                                           NewUserSignUpEMailCreatorDelegate   NewUserSignUpEMailCreator    = null,
                                                           NewUserWelcomeEMailCreatorDelegate  NewUserWelcomeEMailCreator   = null,
                                                           ResetPasswordEMailCreatorDelegate   ResetPasswordEMailCreator    = null,
                                                           Byte                                MinUserNameLenght            = DefaultMinUserNameLenght,
                                                           Byte                                MinRealmLenght               = DefaultMinRealmLenght,
                                                           Byte                                MinPasswordLenght            = DefaultMinPasswordLenght,
                                                           TimeSpan?                           SignInSessionLifetime        = null,

                                                           Boolean                             SkipURITemplates             = false,
                                                           String                              LogfileName                  = DefaultLogfileName)


            => new OpenChargingCloudAPI(HTTPServer,
                                        HTTPHostname,
                                        URIPrefix,

                                        ServiceName,
                                        APIEMailAddress,
                                        APIPublicKeyRing,
                                        APISecretKeyRing,
                                        APIPassphrase,
                                        APIAdminEMails,
                                        APISMTPClient,

                                        CookieName.IsNotNullOrEmpty() ? CookieName : DefaultCookieName,
                                        Language,
                                        LogoImage,
                                        NewUserSignUpEMailCreator,
                                        NewUserWelcomeEMailCreator,
                                        ResetPasswordEMailCreator,
                                        MinUserNameLenght,
                                        MinRealmLenght,
                                        MinPasswordLenght,
                                        SignInSessionLifetime,

                                        SkipURITemplates,
                                        LogfileName);

        #endregion

        #region (private) RegisterURITemplates()

        private void RegisterURITemplates()
        {

            #region /shared/OpenChargingCloudAPI

            HTTPServer.RegisterResourcesFolder(HTTPHostname.Any, "/shared/OpenChargingCloudAPI", HTTPRoot.Substring(0, HTTPRoot.Length - 1));

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
//                                                     }.AsImmutable());

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
//                                                         }.AsImmutable());

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
//                                                     }.AsImmutable());

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
