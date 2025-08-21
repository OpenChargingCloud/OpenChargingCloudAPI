/*
<<<<<<< HEAD
 * Copyright (c) 2014-2025 GraphDefined GmbH <achim.friedland@graphdefined.com>
=======
 * Copyright (c) 2014-2025 GraphDefined GmbH <achim.friedland@graphdefined.com>
>>>>>>> 29b17e9300782939c23d9211c6514cbb3282a990
 * This file is part of WWCP Net <https://github.com/GraphDefined/WWCP_Net>
 *
 * Licensed under the Affero GPL license, Version 3.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.gnu.org/licenses/agpl.html
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Logging;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// An OpenChargingCloud HTTP API logger.
    /// </summary>
    public class OpenChargingCloudAPILogger : HTTPServerLoggerX
    {

        #region Data

        /// <summary>
        /// The default context of this logger.
        /// </summary>
        public const String DefaultContext = "OpenChargingCloudAPI";

        #endregion

        #region Properties

        /// <summary>
        /// The linked OpenChargingCloud API.
        /// </summary>
        public OpenChargingCloudAPI  OpenChargingCloudAPI    { get; }

        #endregion

        #region Constructor(s)

        #region OpenChargingCloudAPILogger(OpenChargingCloudAPI, Context = DefaultContext, LogfileCreator = null)

        /// <summary>
        /// Create a new WWCP HTTP API logger using the default logging delegates.
        /// </summary>
        /// <param name="OpenChargingCloudAPI">A WWCP API.</param>
        /// <param name="LoggingPath">The logging path.</param>
        /// <param name="Context">A context of this API.</param>
        /// <param name="LogfileCreator">A delegate to create a log file from the given context and log file name.</param>
        public OpenChargingCloudAPILogger(OpenChargingCloudAPI     OpenChargingCloudAPI,
                                          String                   LoggingPath,
                                          String                   Context         = DefaultContext,
                                          LogfileCreatorDelegate?  LogfileCreator  = null)

            : this(OpenChargingCloudAPI,
                   LoggingPath,
                   Context,
                   null,
                   null,
                   null,
                   null,
                   LogfileCreator: LogfileCreator)

        { }

        #endregion

        #region OpenChargingCloudAPILogger(OpenChargingCloudAPI, Context, ... Logging delegates ...)

        /// <summary>
        /// Create a new WWCP HTTP API logger using the given logging delegates.
        /// </summary>
        /// <param name="OpenChargingCloudAPI">A WWCP API.</param>
        /// <param name="LoggingPath">The logging path.</param>
        /// <param name="Context">A context of this API.</param>
        /// 
        /// <param name="LogHTTPRequest_toConsole">A delegate to log incoming HTTP requests to console.</param>
        /// <param name="LogHTTPResponse_toConsole">A delegate to log HTTP requests/responses to console.</param>
        /// <param name="LogHTTPRequest_toDisc">A delegate to log incoming HTTP requests to disc.</param>
        /// <param name="LogHTTPResponse_toDisc">A delegate to log HTTP requests/responses to disc.</param>
        /// 
        /// <param name="LogHTTPRequest_toNetwork">A delegate to log incoming HTTP requests to a network target.</param>
        /// <param name="LogHTTPResponse_toNetwork">A delegate to log HTTP requests/responses to a network target.</param>
        /// <param name="LogHTTPRequest_toHTTPSSE">A delegate to log incoming HTTP requests to a HTTP server sent events source.</param>
        /// <param name="LogHTTPResponse_toHTTPSSE">A delegate to log HTTP requests/responses to a HTTP server sent events source.</param>
        /// 
        /// <param name="LogHTTPError_toConsole">A delegate to log HTTP errors to console.</param>
        /// <param name="LogHTTPError_toDisc">A delegate to log HTTP errors to disc.</param>
        /// <param name="LogHTTPError_toNetwork">A delegate to log HTTP errors to a network target.</param>
        /// <param name="LogHTTPError_toHTTPSSE">A delegate to log HTTP errors to a HTTP server sent events source.</param>
        /// 
        /// <param name="LogfileCreator">A delegate to create a log file from the given context and log file name.</param>
        public OpenChargingCloudAPILogger(OpenChargingCloudAPI         OpenChargingCloudAPI,
                                          String                       LoggingPath,
                                          String                       Context,

                                          HTTPRequestLoggerDelegate?   LogHTTPRequest_toConsole    = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPResponse_toConsole   = null,
                                          HTTPRequestLoggerDelegate?   LogHTTPRequest_toDisc       = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPResponse_toDisc      = null,

                                          HTTPRequestLoggerDelegate?   LogHTTPRequest_toNetwork    = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPResponse_toNetwork   = null,
                                          HTTPRequestLoggerDelegate?   LogHTTPRequest_toHTTPSSE    = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPResponse_toHTTPSSE   = null,

                                          HTTPResponseLoggerDelegate?  LogHTTPError_toConsole      = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPError_toDisc         = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPError_toNetwork      = null,
                                          HTTPResponseLoggerDelegate?  LogHTTPError_toHTTPSSE      = null,

                                          LogfileCreatorDelegate?      LogfileCreator              = null)

            : base(OpenChargingCloudAPI.HTTPTestServer,//.InternalHTTPServer,
                   LoggingPath,
                   Context,

                   LogHTTPRequest_toConsole,
                   LogHTTPResponse_toConsole,
                   LogHTTPRequest_toDisc,
                   LogHTTPResponse_toDisc,

                   LogHTTPRequest_toNetwork,
                   LogHTTPResponse_toNetwork,
                   LogHTTPRequest_toHTTPSSE,
                   LogHTTPResponse_toHTTPSSE,

                   LogHTTPError_toConsole,
                   LogHTTPError_toDisc,
                   LogHTTPError_toNetwork,
                   LogHTTPError_toHTTPSSE,

                   LogfileCreator)

        {

            #region Initial checks

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI), "The given WWCP HTTP API must not be null!");

            #endregion

            this.OpenChargingCloudAPI = OpenChargingCloudAPI;

            #region EVSEs

            RegisterEvent2("GetEVSEsStatusRequest",
                           handler => OpenChargingCloudAPI.OnGetEVSEsStatusRequest += handler,
                           handler => OpenChargingCloudAPI.OnGetEVSEsStatusRequest -= handler,
                           "EVSEStatus", "EVSE", "Status", "Request",  "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            //RegisterEvent2("GetEVSEsStatusResponse",
            //               handler => OpenChargingCloudAPI.OnGetEVSEsStatusResponse += handler,
            //               handler => OpenChargingCloudAPI.OnGetEVSEsStatusResponse -= handler,
            //               "EVSEStatus", "EVSE", "Status", "Response", "All").
            //    RegisterDefaultConsoleLogTarget(this).
            //    RegisterDefaultDiscLogTarget(this);

            #endregion


            #region Register auth start/stop log events

            RegisterEvent2("AuthEVSEStart",
                          handler => OpenChargingCloudAPI.OnAuthStartEVSERequest += handler,
                          handler => OpenChargingCloudAPI.OnAuthStartEVSERequest -= handler,
                          "Auth", "AuthEVSE", "AuthStart", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("AuthEVSEStarted",
                          handler => OpenChargingCloudAPI.OnAuthStartEVSEResponse += handler,
                          handler => OpenChargingCloudAPI.OnAuthStartEVSEResponse -= handler,
                          "Auth", "AuthEVSE", "AuthStarted", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("AuthEVSEStop",
                          handler => OpenChargingCloudAPI.OnAuthStopEVSERequest += handler,
                          handler => OpenChargingCloudAPI.OnAuthStopEVSERequest -= handler,
                          "Auth", "AuthEVSE", "AuthStop", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("AuthEVSEStopped",
                          handler => OpenChargingCloudAPI.OnAuthStopEVSEResponse += handler,
                          handler => OpenChargingCloudAPI.OnAuthStopEVSEResponse -= handler,
                          "Auth", "AuthEVSE", "AuthStopped", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            #endregion

            #region Register remote start/stop log events

            RegisterEvent2("RemoteEVSEStart",
                          handler => OpenChargingCloudAPI.OnSendRemoteStartEVSERequest += handler,
                          handler => OpenChargingCloudAPI.OnSendRemoteStartEVSERequest -= handler,
                          "Remote", "RemoteEVSE", "RemoteStart", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("RemoteEVSEStarted",
                          handler => OpenChargingCloudAPI.OnSendRemoteStartEVSEResponse += handler,
                          handler => OpenChargingCloudAPI.OnSendRemoteStartEVSEResponse -= handler,
                          "Remote", "RemoteEVSE", "RemoteStarted", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("RemoteEVSEStop",
                          handler => OpenChargingCloudAPI.OnSendRemoteStopEVSERequest += handler,
                          handler => OpenChargingCloudAPI.OnSendRemoteStopEVSERequest -= handler,
                          "Remote", "RemoteEVSE", "RemoteStop", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("RemoteEVSEStopped",
                          handler => OpenChargingCloudAPI.OnSendRemoteStopEVSEResponse += handler,
                          handler => OpenChargingCloudAPI.OnSendRemoteStopEVSEResponse -= handler,
                          "Remote", "RemoteEVSE", "RemoteStopped", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            #endregion

            #region Register CDR log events

            RegisterEvent2("SendCDR",
                          handler => OpenChargingCloudAPI.OnSendCDRsRequest += handler,
                          handler => OpenChargingCloudAPI.OnSendCDRsRequest -= handler,
                          "CDR", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            RegisterEvent2("CDRSent",
                          handler => OpenChargingCloudAPI.OnSendCDRsResponse += handler,
                          handler => OpenChargingCloudAPI.OnSendCDRsResponse -= handler,
                          "CDR", "All").
                RegisterDefaultConsoleLogTargetX(this).
                RegisterDefaultDiscLogTargetX(this);

            #endregion

        }

        #endregion

        #endregion

    }

}
