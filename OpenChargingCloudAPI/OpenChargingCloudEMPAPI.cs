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
using System.Threading;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Authentication;

using com.GraphDefined.SMSApi.API;

using social.OpenData.UsersAPI;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.SMTP;
using org.GraphDefined.Vanaheimr.Hermod.Mail;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.Sockets;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// The Open Charging Cloud EMP API.
    /// </summary>
    public class OpenChargingCloudEMPAPI : OpenChargingCloudAPI
    {

        #region Data

        public const String  DefaultOpenChargingCloudEMPAPIDatabaseFile   = "OpenChargingCloudEMPAPI.db";
        public const String  DefaultOpenChargingCloudEMPAPILogFile        = "OpenChargingCloudEMPAPI.log";

        #endregion

        #region Properties

        #endregion

        #region Constructor(s)

        #region OpenChargingCloudEMPAPI(HTTPServerName = DefaultHTTPServerName, ...)

        /// <summary>
        /// Create an instance of the Open Charging Cloud EMP API.
        /// </summary>
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        /// <param name="LocalHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="LocalPort">A TCP port to listen on.</param>
        /// <param name="ExternalDNSName">The offical URL/DNS name of this service, e.g. for sending e-mails.</param>
        /// <param name="URLPathPrefix">A common prefix for all URLs.</param>
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
        /// <param name="UseSecureCookies">Force the web browser to send cookies only via HTTPS.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="PasswordChangedEMailCreator">A delegate for sending a password changed e-mail to a user.</param>
        /// <param name="MinUserNameLength">The minimal user name length.</param>
        /// <param name="MinRealmLength">The minimal realm length.</param>
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
        public OpenChargingCloudEMPAPI(String                               ServiceName                        = "GraphDefined Open Charging Cloud EMP API",
                                       String                               HTTPServerName                     = "GraphDefined Open Charging Cloud EMP API",
                                       HTTPHostname?                        LocalHostname                      = null,
                                       IPPort?                              LocalPort                          = null,
                                       String                               ExternalDNSName                    = null,
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
                                       Boolean                              UseSecureCookies                   = true,
                                       Languages?                           Language                           = null,
                                       NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator          = null,
                                       NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator         = null,
                                       ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator          = null,
                                       PasswordChangedEMailCreatorDelegate  PasswordChangedEMailCreator        = null,
                                       Byte?                                MinUserNameLength                  = null,
                                       Byte?                                MinRealmLength                     = null,
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
                                       String                               DatabaseFile                       = DefaultOpenChargingCloudEMPAPIDatabaseFile,
                                       String                               LoggingPath                        = null,
                                       String                               LogfileName                        = DefaultOpenChargingCloudEMPAPILogFile,
                                       DNSClient                            DNSClient                          = null,
                                       Boolean                              Autostart                          = false)

            : base(ServiceName:                       ServiceName                 ?? "GraphDefined Open Charging Cloud EMP API",
                   HTTPServerName:                    HTTPServerName              ?? "GraphDefined Open Charging Cloud EMP API",
                   LocalHostname:                     LocalHostname,
                   LocalPort:                         LocalPort,
                   ExternalDNSName:                           ExternalDNSName,
                   URLPathPrefix:                     URLPathPrefix,
                   UseSecureCookies:                  UseSecureCookies,

                   ServerCertificateSelector:         ServerCertificateSelector,
                   ClientCertificateValidator:        ClientCertificateValidator,
                   ClientCertificateSelector:         ClientCertificateSelector,
                   AllowedTLSProtocols:               AllowedTLSProtocols,

                   APIEMailAddress:                   APIEMailAddress,
                   APIPassphrase:                     APIPassphrase,
                   APIAdminEMails:                    APIAdminEMails,
                   APISMTPClient:                     APISMTPClient,

                   SMSAPICredentials:                 SMSAPICredentials,
                   APIAdminSMS:                       APIAdminSMS,

                   CookieName:                        CookieName                  ?? HTTPCookieName.Parse("OpenChargingCloudEMPAPI"),
                   Language:                          Language,
                   NewUserSignUpEMailCreator:         NewUserSignUpEMailCreator,
                   NewUserWelcomeEMailCreator:        NewUserWelcomeEMailCreator,
                   ResetPasswordEMailCreator:         ResetPasswordEMailCreator,
                   PasswordChangedEMailCreator:       PasswordChangedEMailCreator,
                   MinUserNameLength:                 MinUserNameLength,
                   MinRealmLength:                    MinRealmLength,
                   PasswordQualityCheck:              PasswordQualityCheck,
                   SignInSessionLifetime:             SignInSessionLifetime,

                   ServerThreadName:                  ServerThreadName,
                   ServerThreadPriority:              ServerThreadPriority,
                   ServerThreadIsBackground:          ServerThreadIsBackground,
                   ConnectionIdBuilder:               ConnectionIdBuilder,
                   ConnectionThreadsNameBuilder:      ConnectionThreadsNameBuilder,
                   ConnectionThreadsPriorityBuilder:  ConnectionThreadsPriorityBuilder,
                   ConnectionThreadsAreBackground:    ConnectionThreadsAreBackground,
                   ConnectionTimeout:                 ConnectionTimeout,
                   MaxClientConnections:              MaxClientConnections,

                   SkipURLTemplates:                  SkipURLTemplates,
                   DisableNotifications:              DisableNotifications,
                   DisableLogfile:                    DisableLogfile,
                   DatabaseFile:                      DatabaseFile                ?? DefaultOpenChargingCloudEMPAPIDatabaseFile,
                   LoggingPath:                       LoggingPath                 ?? "default",
                   LogfileName:                       LogfileName                 ?? DefaultOpenChargingCloudEMPAPILogFile,
                   DNSClient:                         DNSClient,
                   Autostart:                         false)

        {

            //RegisterURLTemplates();

            if (Autostart)
                Start();

        }

        #endregion

        #region OpenChargingCloudEMPAPI(HTTPServerName = DefaultHTTPServerName, ...)

        ///// <summary>
        ///// Create an instance of the Open Charging Cloud API.
        ///// </summary>
        ///// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="HTTPServerPort">A TCP port to listen on.</param>
        ///// <param name="URIPrefix">A common prefix for all URLs.</param>
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
        ///// <param name="MinUserNameLength">The minimal user name length.</param>
        ///// <param name="MinRealmLength">The minimal realm length.</param>
        ///// <param name="MinPasswordLength">The minimal password length.</param>
        ///// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        ///// 
        ///// <param name="SkipURITemplates">Skip URI templates.</param>
        ///// <param name="DisableNotifications">Disable external notifications.</param>
        ///// <param name="DisableLogfile">Disable the log file.</param>
        ///// <param name="LogfileName">The name of the logfile for this API.</param>
        ///// <param name="DNSClient">The DNS client of the API.</param>
        ///// <param name="Autostart">Whether to start the API automatically.</param>
        //public OpenChargingCloudEMPAPI(String                               HTTPServerName                     = DefaultHTTPServerName,
        //                               IPPort?                              HTTPServerPort                     = null,
        //                               HTTPHostname?                        HTTPHostname                       = null,
        //                               HTTPPath?                             URIPrefix                          = null,

        //                               ServerCertificateSelectorDelegate    ServerCertificateSelector          = null,
        //                               RemoteCertificateValidationCallback  ClientCertificateValidator         = null,
        //                               LocalCertificateSelectionCallback    ClientCertificateSelector          = null,
        //                               SslProtocols                         AllowedTLSProtocols                = SslProtocols.Tls12,

        //                               String                               ServiceName                        = DefaultServiceName,
        //                               EMailAddress                         APIEMailAddress                    = null,
        //                               String                               APIPassphrase                      = null,
        //                               EMailAddressList                     APIAdminEMails                     = null,
        //                               SMTPClient                           APISMTPClient                      = null,

        //                               Credentials                          SMSAPICredentials                  = null,
        //                               IEnumerable<PhoneNumber>             APIAdminSMS                        = null,

        //                               HTTPCookieName?                      CookieName                         = null,
        //                               Languages?                           Language                           = DefaultLanguage,
        //                               String                               LogoImage                          = null,
        //                               NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator          = null,
        //                               NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator         = null,
        //                               ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator          = null,
        //                               PasswordChangedEMailCreatorDelegate  PasswordChangedEMailCreator        = null,
        //                               Byte                                 MinUserNameLength                  = DefaultMinUserNameLength,
        //                               Byte                                 MinRealmLength                     = DefaultMinRealmLength,
        //                               Byte                                 MinPasswordLength                  = DefaultMinPasswordLength,
        //                               TimeSpan?                            SignInSessionLifetime              = null,

        //                               String                               ServerThreadName                   = null,
        //                               ThreadPriority                       ServerThreadPriority               = ThreadPriority.AboveNormal,
        //                               Boolean                              ServerThreadIsBackground           = true,
        //                               ConnectionIdBuilder                  ConnectionIdBuilder                = null,
        //                               ConnectionThreadsNameBuilder         ConnectionThreadsNameBuilder       = null,
        //                               ConnectionThreadsPriorityBuilder     ConnectionThreadsPriorityBuilder   = null,
        //                               Boolean                              ConnectionThreadsAreBackground     = true,
        //                               TimeSpan?                            ConnectionTimeout                  = null,
        //                               UInt32                               MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

        //                               Boolean                              SkipURITemplates                   = false,
        //                               Boolean                              DisableNotifications               = false,
        //                               Boolean                              DisableLogfile                     = false,
        //                               String                               LoggingPath                        = null,
        //                               String                               LogfileName                        = DefaultLogfileName,
        //                               DNSClient                            DNSClient                          = null,
        //                               Boolean                              Autostart                          = false)

        //    : base(HTTPServerName,
        //           HTTPServerPort,
        //           HTTPHostname,
        //           URIPrefix,

        //           ServerCertificateSelector,
        //           ClientCertificateValidator,
        //           ClientCertificateSelector,
        //           AllowedTLSProtocols,

        //           ServiceName,
        //           APIEMailAddress,
        //           APIPassphrase,
        //           APIAdminEMails,
        //           APISMTPClient,

        //           SMSAPICredentials,
        //           APIAdminSMS,

        //           CookieName,
        //           Language,
        //           LogoImage,
        //           NewUserSignUpEMailCreator,
        //           NewUserWelcomeEMailCreator,
        //           ResetPasswordEMailCreator,
        //           PasswordChangedEMailCreator,
        //           MinUserNameLength,
        //           MinRealmLength,
        //           MinPasswordLength,
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
        //           LoggingPath,
        //           LogfileName,
        //           DNSClient,
        //           Autostart: false)

        //{

        //    //this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

        //    //RegisterURLTemplates();

        //    if (Autostart)
        //        Start();

        //}

        #endregion

        #region OpenChargingCloudEMPAPI(HTTPServer, HTTPHostname = null, URIPrefix = null, ...)

        ///// <summary>
        ///// Attach this Open Charging Cloud API to the given HTTP server.
        ///// </summary>
        ///// <param name="HTTPServer">An existing HTTP server.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="URIPrefix">A common prefix for all URLs.</param>
        ///// 
        ///// <param name="ServiceName">The name of the service.</param>
        ///// <param name="APIEMailAddress">An e-mail address for this API.</param>
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
        ///// <param name="MinUserNameLength">The minimal user name length.</param>
        ///// <param name="MinRealmLength">The minimal realm length.</param>
        ///// <param name="MinPasswordLength">The minimal password length.</param>
        ///// <param name="SignInSessionLifetime">The sign-in session lifetime.</param>
        ///// 
        ///// <param name="SkipURITemplates">Skip URI templates.</param>
        ///// <param name="DisableNotifications">Disable external notifications.</param>
        ///// <param name="DisableLogfile">Disable the log file.</param>
        ///// <param name="LogfileName">The name of the logfile for this API.</param>
        //public OpenChargingCloudEMPAPI(HTTPServer                           HTTPServer,
        //                               HTTPHostname?                        HTTPHostname                  = null,
        //                               HTTPPath?                             URIPrefix                     = null,

        //                               String                               ServiceName                   = DefaultServiceName,
        //                               EMailAddress                         APIEMailAddress               = null,
        //                               String                               APIPassphrase                 = null,
        //                               EMailAddressList                     APIAdminEMails                = null,
        //                               SMTPClient                           APISMTPClient                 = null,

        //                               Credentials                          SMSAPICredentials             = null,
        //                               IEnumerable<PhoneNumber>             APIAdminSMS                   = null,

        //                               HTTPCookieName?                      CookieName                    = null,
        //                               Languages                            Language                      = DefaultLanguage,
        //                               String                               LogoImage                     = null,
        //                               NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator     = null,
        //                               NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator    = null,
        //                               ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator     = null,
        //                               PasswordChangedEMailCreatorDelegate  PasswordChangedEMailCreator   = null,
        //                               Byte                                 MinUserNameLength             = DefaultMinUserNameLength,
        //                               Byte                                 MinRealmLength                = DefaultMinRealmLength,
        //                               Byte                                 MinPasswordLength             = DefaultMinPasswordLength,
        //                               TimeSpan?                            SignInSessionLifetime         = null,

        //                               Boolean                              SkipURITemplates              = false,
        //                               Boolean                              DisableNotifications          = false,
        //                               Boolean                              DisableLogfile                = false,
        //                               String                               LogfileName                   = DefaultLogfileName)

        //    : base(HTTPServer,
        //           HTTPHostname,
        //           URIPrefix,

        //           ServiceName,
        //           APIEMailAddress,
        //           APIPassphrase,
        //           APIAdminEMails,
        //           APISMTPClient,

        //           SMSAPICredentials,
        //           APIAdminSMS,

        //           CookieName,
        //           Language,
        //           LogoImage,
        //           NewUserSignUpEMailCreator,
        //           NewUserWelcomeEMailCreator,
        //           ResetPasswordEMailCreator,
        //           PasswordChangedEMailCreator,
        //           MinUserNameLength,
        //           MinRealmLength,
        //           MinPasswordLength,
        //           SignInSessionLifetime,

        //           SkipURITemplates,
        //           DisableNotifications,
        //           DisableLogfile,
        //           LogfileName)

        //{

        //    //this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!"));

        //    //if (!SkipURITemplates)
        //    //    RegisterURLTemplates();

        //}

        #endregion

        #endregion

    }

}
