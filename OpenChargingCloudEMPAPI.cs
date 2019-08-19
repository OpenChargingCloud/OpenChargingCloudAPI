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
using SMSApi.Api;
using System.Collections.Generic;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// The Open Charging Cloud EMP API.
    /// </summary>
    public class OpenChargingCloudEMPAPI : OpenChargingCloudAPI
    {

        #region Constructor(s)

        #region OpenChargingCloudEMPAPI(HTTPServerName = DefaultHTTPServerName, ...)

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
        /// <param name="DisableNotifications">Disable external notifications.</param>
        /// <param name="DisableLogfile">Disable the log file.</param>
        /// <param name="LogfileName">The name of the logfile for this API.</param>
        /// <param name="DNSClient">The DNS client of the API.</param>
        /// <param name="Autostart">Whether to start the API automatically.</param>
        public OpenChargingCloudEMPAPI(String                               HTTPServerName                     = DefaultHTTPServerName,
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
                                       Byte                                 MinPasswordLenght                  = DefaultMinPasswordLenght,
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

                                       Boolean                              SkipURITemplates                   = false,
                                       Boolean                              DisableNotifications               = false,
                                       Boolean                              DisableLogfile                     = false,
                                       String                               LoggingPath                        = null,
                                       String                               LogfileName                        = DefaultLogfileName,
                                       DNSClient                            DNSClient                          = null,
                                       Boolean                              Autostart                          = false)

            : base(HTTPServerName,
                   HTTPServerPort,
                   HTTPHostname,
                   ServiceName,
                   BaseURL,
                   URLPathPrefix,

                   ServerCertificateSelector,
                   ClientCertificateValidator,
                   ClientCertificateSelector,
                   AllowedTLSProtocols,

                   APIEMailAddress,
                   APIPassphrase,
                   APIAdminEMails,
                   APISMTPClient,

                   SMSAPICredentials,
                   APIAdminSMS,

                   CookieName,
                   Language,
                   LogoImage,
                   NewUserSignUpEMailCreator,
                   NewUserWelcomeEMailCreator,
                   ResetPasswordEMailCreator,
                   PasswordChangedEMailCreator,
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
                   DisableNotifications,
                   DisableLogfile,
                   LoggingPath,
                   LogfileName,
                   DNSClient,
                   Autostart: false)

        {

            //this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

            //RegisterURITemplates();

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
        //                               Byte                                 MinUserNameLenght                  = DefaultMinUserNameLenght,
        //                               Byte                                 MinRealmLenght                     = DefaultMinRealmLenght,
        //                               Byte                                 MinPasswordLenght                  = DefaultMinPasswordLenght,
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
        //           LoggingPath,
        //           LogfileName,
        //           DNSClient,
        //           Autostart: false)

        //{

        //    //this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer);

        //    //RegisterURITemplates();

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
        ///// <param name="URIPrefix">A common prefix for all URIs.</param>
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
        ///// <param name="MinUserNameLenght">The minimal user name length.</param>
        ///// <param name="MinRealmLenght">The minimal realm length.</param>
        ///// <param name="MinPasswordLenght">The minimal password length.</param>
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
        //                               Byte                                 MinUserNameLenght             = DefaultMinUserNameLenght,
        //                               Byte                                 MinRealmLenght                = DefaultMinRealmLenght,
        //                               Byte                                 MinPasswordLenght             = DefaultMinPasswordLenght,
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
        //           MinUserNameLenght,
        //           MinRealmLenght,
        //           MinPasswordLenght,
        //           SignInSessionLifetime,

        //           SkipURITemplates,
        //           DisableNotifications,
        //           DisableLogfile,
        //           LogfileName)

        //{

        //    //this.WWCP = WWCP_HTTPAPI.AttachToHTTPAPI(HTTPServer ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!"));

        //    //if (!SkipURITemplates)
        //    //    RegisterURITemplates();

        //}

        #endregion

        #endregion

    }

}
