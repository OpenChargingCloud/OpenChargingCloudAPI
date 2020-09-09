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
using System.Linq;
using System.Threading;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Authentication;

using Newtonsoft.Json.Linq;

using com.GraphDefined.SMSApi.API;

using cloud.charging.open.API;
using social.OpenData.UsersAPI;
using social.OpenData.UsersAPI.Notifications;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.SMTP;
using org.GraphDefined.Vanaheimr.Hermod.Mail;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.Sockets;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;

using org.GraphDefined.WWCP;
using org.GraphDefined.WWCP.Net.IO.JSON;
using org.GraphDefined.WWCP.Networking;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// WWCP HTTP API extention methods.
    /// </summary>
    public static class ExtentionMethods
    {

        // Used by multiple HTTP content types

        #region ParseRoamingNetwork                          (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork,                              out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network
        /// for the given HTTP hostname and HTTP query parameter
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetwork(this HTTPRequest      HTTPRequest,
                                                  OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                  out RoamingNetwork    RoamingNetwork,
                                                  out HTTPResponse      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 1)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            if (!RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURIParameters[0], out RoamingNetwork_Id RoamingNetworkId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetwork  = OpenChargingCloudAPI.
                                  GetAllRoamingNetworks(HTTPRequest.Host).
                                  FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            if (RoamingNetwork == null) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingStationOperator(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging station operator
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingStationOperator(this HTTPRequest             HTTPRequest,
                                                                            OpenChargingCloudAPI         OpenChargingCloudAPI,
                                                                            out RoamingNetwork           RoamingNetwork,
                                                                            out ChargingStationOperator  ChargingStationOperator,
                                                                            out HTTPResponse             HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            HTTPResponse             = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStationOperator_Id ChargingStationOperatorId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(ChargingStationOperatorId, out ChargingStationOperator))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingPool           (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool,            out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging pool
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingPool">The charging pool.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingPool(this HTTPRequest      HTTPRequest,
                                                                 OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                 out RoamingNetwork    RoamingNetwork,
                                                                 out ChargingPool      ChargingPool,
                                                                 out HTTPResponse      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            ChargingPool    = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingPool_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingPool_Id ChargingPoolId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingPoolById(ChargingPoolId, out ChargingPool))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingStation        (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStation,         out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging station
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStation">The charging station.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingStation(this HTTPRequest      HTTPRequest,
                                                                    OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                    out RoamingNetwork    RoamingNetwork,
                                                                    out ChargingStation   ChargingStation,
                                                                    out HTTPResponse      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ChargingStation  = null;
            HTTPResponse     = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            if (!RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURIParameters[0], out RoamingNetwork_Id RoamingNetworkId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetwork  = OpenChargingCloudAPI.
                                  GetAllRoamingNetworks(HTTPRequest.Host).
                                  FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            if (RoamingNetwork == null)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStation_Id ChargingStationId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationById(ChargingStationId, out ChargingStation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndEVSE                   (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EVSE,                    out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and EVSE
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EVSE">The EVSE.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndEVSE(this HTTPRequest      HTTPRequest,
                                                         OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                         out RoamingNetwork    RoamingNetwork,
                                                         out EVSE              EVSE,
                                                         out HTTPResponse      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            EVSE            = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            if (!RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURIParameters[0], out RoamingNetwork_Id RoamingNetworkId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetwork = OpenChargingCloudAPI.
                                 GetAllRoamingNetworks(HTTPRequest.Host).
                                 FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            if (RoamingNetwork == null)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            if (!EVSE_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out EVSE_Id EVSEId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetEVSEById(EVSEId, out EVSE))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingSession        (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingSession,         out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging session
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetworkId">The roaming network identification.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingSessionId">The charging session identification.</param>
        /// <param name="ChargingSession">The charging session.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingSession(this HTTPRequest         HTTPRequest,
                                                                    OpenChargingCloudAPI     OpenChargingCloudAPI,
                                                                    out RoamingNetwork_Id?   RoamingNetworkId,
                                                                    out RoamingNetwork       RoamingNetwork,
                                                                    out ChargingSession_Id?  ChargingSessionId,
                                                                    out ChargingSession      ChargingSession,
                                                                    out HTTPResponse         HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetworkId   = null;
            RoamingNetwork     = null;
            ChargingSessionId  = null;
            ChargingSession    = null;
            HTTPResponse       = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetworkId = RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURIParameters[0]);

            if (!RoamingNetworkId.HasValue)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetwork  = OpenChargingCloudAPI.GetRoamingNetwork(HTTPRequest.Host, RoamingNetworkId.Value);

            if (RoamingNetwork == null)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            ChargingSessionId = ChargingSession_Id.TryParse(HTTPRequest.ParsedURIParameters[1]);

            if (!ChargingSessionId.HasValue)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid charging session identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingSessionById(ChargingSessionId.Value, out ChargingSession))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown charging session identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndReservation            (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out Reservation,             out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging reservation
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="Reservation">The charging reservation.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndReservation(this HTTPRequest         HTTPRequest,
                                                                OpenChargingCloudAPI     OpenChargingCloudAPI,
                                                                out RoamingNetwork       RoamingNetwork,
                                                                out ChargingReservation  Reservation,
                                                                out HTTPResponse         HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            Reservation     = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }

            if (!RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURIParameters[0], out RoamingNetwork_Id RoamingNetworkId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            RoamingNetwork = OpenChargingCloudAPI.
                                 GetAllRoamingNetworks(HTTPRequest.Host).
                                 FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            if (RoamingNetwork == null)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown roaming network identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            if (!ChargingReservation_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingReservation_Id ReservationId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid reservation identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.ReservationsStore.TryGetLatest(ReservationId, out Reservation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown reservation identification!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndEMobilityProvider      (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EMobilityProvider,       out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and e-mobility provider
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EMobilityProvider">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndEMobilityProvider(this HTTPRequest       HTTPRequest,
                                                                      OpenChargingCloudAPI   OpenChargingCloudAPI,
                                                                      out RoamingNetwork     RoamingNetwork,
                                                                      out eMobilityProvider  EMobilityProvider,
                                                                      out HTTPResponse       HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork     = null;
            EMobilityProvider  = null;
            HTTPResponse       = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!eMobilityProvider_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out eMobilityProvider_Id EMobilityProviderId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EMobilityProviderId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetEMobilityProviderById(EMobilityProviderId, out EMobilityProvider))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EMobilityProviderId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion



        #region ParseRoamingNetworkAndParkingOperator(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ParkingOperator, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and parking operator
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ParkingOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndParkingOperator(this HTTPRequest     HTTPRequest,
                                                                    OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                    out RoamingNetwork   RoamingNetwork,
                                                                    out ParkingOperator  ParkingOperator,
                                                                    out HTTPResponse     HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ParkingOperator  = null;
            HTTPResponse     = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ParkingOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ParkingOperator_Id ParkingOperatorId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ParkingOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetParkingOperatorById(ParkingOperatorId, out ParkingOperator))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ParkingOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndSmartCity(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out SmartCity, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and smart city
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="SmartCity">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndSmartCity(this HTTPRequest    HTTPRequest,
                                                              OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                              out RoamingNetwork  RoamingNetwork,
                                                              out SmartCityProxy  SmartCity,
                                                              out HTTPResponse    HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            SmartCity       = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!SmartCity_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out SmartCity_Id SmartCityId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid SmartCityId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetSmartCityById(SmartCityId, out SmartCity))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown SmartCityId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndGridOperator(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out GridOperator, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and smart city
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="GridOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndGridOperator(this HTTPRequest    HTTPRequest,
                                                                 OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                 out RoamingNetwork  RoamingNetwork,
                                                                 out GridOperator    GridOperator,
                                                                 out HTTPResponse    HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            GridOperator    = null;
            HTTPResponse    = null;

            if (HTTPRequest.ParsedURIParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!GridOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out GridOperator_Id GridOperatorId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid GridOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetGridOperatorById(GridOperatorId, out GridOperator))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown GridOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion



        #region ParseRoamingNetworkAndChargingPoolAndChargingStation(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool, out ChargingStation, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network, charging pool
        /// and charging station for the given HTTP hostname and HTTP query
        /// parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingPool">The charging pool.</param>
        /// <param name="ChargingStation">The charging station.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        public static Boolean ParseRoamingNetworkAndChargingPoolAndChargingStation(this HTTPRequest     HTTPRequest,
                                                                                   OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                                   out RoamingNetwork   RoamingNetwork,
                                                                                   out ChargingPool     ChargingPool,
                                                                                   out ChargingStation  ChargingStation,
                                                                                   out HTTPResponse     HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ChargingPool     = null;
            ChargingStation  = null;
            HTTPResponse     = null;

            if (HTTPRequest.ParsedURIParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;

            #region Get charging pool...

            if (!ChargingPool_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingPool_Id ChargingPoolId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingPoolById(ChargingPoolId, out ChargingPool))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            #endregion

            #region Get charging station...

            if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURIParameters[2], out ChargingStation_Id ChargingStationId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationById(ChargingStationId, out ChargingStation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            #endregion

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool, out ChargingStation, out EVSE, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network, charging pool,
        /// charging station and EVSE for the given HTTP hostname and HTTP query
        /// parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingPool">The charging pool.</param>
        /// <param name="ChargingStation">The charging station.</param>
        /// <param name="EVSE">The EVSE.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        public static Boolean ParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this HTTPRequest     HTTPRequest,
                                                                                          OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                                                          out RoamingNetwork   RoamingNetwork,
                                                                                          out ChargingPool     ChargingPool,
                                                                                          out ChargingStation  ChargingStation,
                                                                                          out EVSE             EVSE,
                                                                                          out HTTPResponse     HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ChargingPool     = null;
            ChargingStation  = null;
            EVSE             = null;
            HTTPResponse     = null;

            if (HTTPRequest.ParsedURIParameters.Length < 4)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    Connection      = "close"
                };

                return false;

            }

            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;

            #region Get charging pool...

            if (!ChargingPool_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingPool_Id ChargingPoolId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingPoolById(ChargingPoolId, out ChargingPool))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            #endregion

            #region Get charging station...

            if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURIParameters[2], out ChargingStation_Id ChargingStationId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationById(ChargingStationId, out ChargingStation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            #endregion

            #region Get EVSE

            if (!EVSE_Id.TryParse(HTTPRequest.ParsedURIParameters[3], out EVSE_Id EVSEId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetEVSEById(EVSEId, out EVSE))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            #endregion

            return true;

        }

        #endregion


        #region ParseRoamingNetworkAndChargingStationOperatorAndBrand(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out Brand, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging station operator
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingStationOperatorAndBrand(this HTTPRequest             HTTPRequest,
                                                                                    OpenChargingCloudAPI                 OpenChargingCloudAPI,
                                                                                    out RoamingNetwork           RoamingNetwork,
                                                                                    out ChargingStationOperator  ChargingStationOperator,
                                                                                    out Brand                    Brand,
                                                                                    out HTTPResponse             HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            Brand                    = null;
            HTTPResponse             = null;

            if (HTTPRequest.ParsedURIParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStationOperator_Id ChargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(ChargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }



            if (!Brand_Id.TryParse(HTTPRequest.ParsedURIParameters[2], out Brand_Id BrandId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid BrandId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!ChargingStationOperator.TryGetBrand(BrandId, out Brand)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown BrandId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out ChargingStationGroup, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging station operator
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this HTTPRequest             HTTPRequest,
                                                                                                   OpenChargingCloudAPI                 OpenChargingCloudAPI,
                                                                                                   out RoamingNetwork           RoamingNetwork,
                                                                                                   out ChargingStationOperator  ChargingStationOperator,
                                                                                                   out ChargingStationGroup     ChargingStationGroup,
                                                                                                   out HTTPResponse             HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            ChargingStationGroup     = null;
            HTTPResponse             = null;

            if (HTTPRequest.ParsedURIParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStationOperator_Id ChargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(ChargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }



            if (!ChargingStationGroup_Id.TryParse(HTTPRequest.ParsedURIParameters[2], out ChargingStationGroup_Id ChargingStationGroupId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationGroupId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!ChargingStationOperator.TryGetChargingStationGroup(ChargingStationGroupId, out ChargingStationGroup)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationGroupId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion

        #region ParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out ChargingStationGroup, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging station operator
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this HTTPRequest             HTTPRequest,
                                                                                        OpenChargingCloudAPI                 OpenChargingCloudAPI,
                                                                                        out RoamingNetwork           RoamingNetwork,
                                                                                        out ChargingStationOperator  ChargingStationOperator,
                                                                                        out EVSEGroup                EVSEGroup,
                                                                                        out HTTPResponse             HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI == null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            EVSEGroup                = null;
            HTTPResponse             = null;

            if (HTTPRequest.ParsedURIParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }


            if (!HTTPRequest.ParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStationOperator_Id ChargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(ChargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }



            if (!EVSEGroup_Id.TryParse(HTTPRequest.ParsedURIParameters[2], out EVSEGroup_Id EVSEGroupId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EVSEGroupId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!ChargingStationOperator.TryGetEVSEGroup(EVSEGroupId, out EVSEGroup)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EVSEGroupId!"" }".ToUTF8Bytes(),
                    Connection      = "close"
                };

                return false;

            }

            return true;

        }

        #endregion


        // Additional HTTP methods for HTTP clients

        #region REMOTESTART(this HTTPClient, URI, BuilderAction = null)

        public static HTTPRequest.Builder REMOTESTART(this HTTPClient              HTTPClient,
                                                      HTTPPath                      URI,
                                                      Action<HTTPRequest.Builder>  BuilderAction  = null)
        {

            #region Initial checks

            if (HTTPClient == null)
                throw new ArgumentNullException(nameof(HTTPClient),  "The given HTTP client must not be null!");

            if (URI.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(URI),         "The given URI must not be null!");

            #endregion

            return HTTPClient.CreateRequest(OpenChargingCloudAPI.REMOTESTART, URI, BuilderAction);

        }

        #endregion

        #region REMOTESTOP (this HTTPClient, URI, BuilderAction = null)

        public static HTTPRequest.Builder REMOTESTOP(this HTTPClient              HTTPClient,
                                                     HTTPPath                      URI,
                                                     Action<HTTPRequest.Builder>  BuilderAction  = null)
        {

            #region Initial checks

            if (HTTPClient == null)
                throw new ArgumentNullException(nameof(HTTPClient),  "The given HTTP client must not be null!");

            if (URI.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(URI),         "The given URI must not be null!");

            #endregion

            return HTTPClient.CreateRequest(OpenChargingCloudAPI.REMOTESTOP, URI, BuilderAction);

        }

        #endregion


    }


    /// <summary>
    /// The common Open Charging Cloud API.
    /// </summary>
    public class OpenChargingCloudAPI : UsersAPI
    {

        #region Data

        /// <summary>
        /// The HTTP root for embedded ressources.
        /// </summary>
        public new const       String              HTTPRoot                                  = "cloud.charging.open.api.HTTPRoot.";

        public  const          String              DefaultOpenChargingCloudAPIDatabaseFile   = "OpenChargingCloudAPI.db";
        public  const          String              DefaultOpenChargingCloudAPILogFile        = "OpenChargingCloudAPI.log";

        public static readonly HTTPEventSource_Id  DebugLogId                                = HTTPEventSource_Id.Parse("DebugLog");
        public static readonly HTTPEventSource_Id  ImporterLogId                             = HTTPEventSource_Id.Parse("ImporterLog");
        public static readonly HTTPEventSource_Id  ForwardingInfosId                         = HTTPEventSource_Id.Parse("ForwardingInfos");

        #endregion

        #region Additional HTTP methods

        public readonly static HTTPMethod RESERVE      = HTTPMethod.Create("RESERVE",     IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod SETEXPIRED   = HTTPMethod.Create("SETEXPIRED",  IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod AUTHSTART    = HTTPMethod.Create("AUTHSTART",   IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod AUTHSTOP     = HTTPMethod.Create("AUTHSTOP",    IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod REMOTESTART  = HTTPMethod.Create("REMOTESTART", IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod REMOTESTOP   = HTTPMethod.Create("REMOTESTOP",  IsSafe: false, IsIdempotent: true);
        public readonly static HTTPMethod SENDCDR      = HTTPMethod.Create("SENDCDR",     IsSafe: false, IsIdempotent: true);

        #endregion

        #region Properties

        public HTTPServer<RoamingNetworks, RoamingNetwork>  WWCPHTTPServer    { get; }

        /// <summary>
        /// Send debug information via HTTP Server Sent Events.
        /// </summary>
        public HTTPEventSource<JObject>                     DebugLog          { get; }

        /// <summary>
        /// Send importer information via HTTP Server Sent Events.
        /// </summary>
        public HTTPEventSource<JObject>                     ImporterLog       { get; }

        #endregion

        #region Events

        #region (protected internal) CreateRoamingNetworkRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnCreateRoamingNetworkRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateRoamingNetworkRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnCreateRoamingNetworkRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) CreateRoamingNetworkResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnCreateRoamingNetworkResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateRoamingNetworkResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnCreateRoamingNetworkResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion


        #region (protected internal) DeleteRoamingNetworkRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnDeleteRoamingNetworkRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteRoamingNetworkRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnDeleteRoamingNetworkRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) DeleteRoamingNetworkResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnDeleteRoamingNetworkResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteRoamingNetworkResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnDeleteRoamingNetworkResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion



        #region (protected internal) CreateChargingPoolRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnCreateChargingPoolRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateChargingPoolRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnCreateChargingPoolRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) CreateChargingPoolResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnCreateChargingPoolResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateChargingPoolResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnCreateChargingPoolResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion


        #region (protected internal) DeleteChargingPoolRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnDeleteChargingPoolRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteChargingPoolRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnDeleteChargingPoolRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) DeleteChargingPoolResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnDeleteChargingPoolResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteChargingPoolResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnDeleteChargingPoolResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion



        #region (protected internal) CreateChargingStationRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnCreateChargingStationRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateChargingStationRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnCreateChargingStationRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) CreateChargingStationResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnCreateChargingStationResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateChargingStationResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnCreateChargingStationResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion


        #region (protected internal) DeleteChargingStationRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnDeleteChargingStationRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteChargingStationRequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnDeleteChargingStationRequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) DeleteChargingStationResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnDeleteChargingStationResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteChargingStationResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnDeleteChargingStationResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion



        #region (protected internal) SendGetEVSEsStatusRequest (Request)

        /// <summary>
        /// An event sent whenever a EVSEs->Status request was received.
        /// </summary>
        public HTTPRequestLogEvent OnGetEVSEsStatusRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a EVSEs->Status request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendGetEVSEsStatusRequest(DateTime     Timestamp,
                                                          HTTPAPI      API,
                                                          HTTPRequest  Request)

            => OnGetEVSEsStatusRequest?.WhenAll(Timestamp,
                                                API ?? this,
                                                Request);

        #endregion

        #region (protected internal) SendGetEVSEsStatusResponse(Response)

        /// <summary>
        /// An event sent whenever a EVSEs->Status response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnGetEVSEsStatusResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a EVSEs->Status response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendGetEVSEsStatusResponse(DateTime      Timestamp,
                                                           HTTPAPI       API,
                                                           HTTPRequest   Request,
                                                           HTTPResponse  Response)

            => OnGetEVSEsStatusResponse?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request,
                                                 Response);

        #endregion



        #region (protected internal) SendRemoteStartEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnSendRemoteStartEVSERequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendRemoteStartEVSERequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnSendRemoteStartEVSERequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) SendRemoteStartEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnSendRemoteStartEVSEResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendRemoteStartEVSEResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnSendRemoteStartEVSEResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion


        #region (protected internal) SendRemoteStopEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnSendRemoteStopEVSERequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendRemoteStopEVSERequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnSendRemoteStopEVSERequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) SendRemoteStopEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnSendRemoteStopEVSEResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendRemoteStopEVSEResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnSendRemoteStopEVSEResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion






        #region (protected internal) SendReserveEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnSendReserveEVSERequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendReserveEVSERequest(DateTime     Timestamp,
                                                       HTTPAPI      API,
                                                       HTTPRequest  Request)

            => OnSendReserveEVSERequest?.WhenAll(Timestamp,
                                                 API ?? this,
                                                 Request);

        #endregion

        #region (protected internal) SendReserveEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnSendReserveEVSEResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendReserveEVSEResponse(DateTime      Timestamp,
                                                        HTTPAPI       API,
                                                        HTTPRequest   Request,
                                                        HTTPResponse  Response)

            => OnSendReserveEVSEResponse?.WhenAll(Timestamp,
                                                  API ?? this,
                                                  Request,
                                                  Response);

        #endregion


        #region (protected internal) SendAuthStartEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnAuthStartEVSERequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendAuthStartEVSERequest(DateTime     Timestamp,
                                                         HTTPAPI      API,
                                                         HTTPRequest  Request)

            => OnAuthStartEVSERequest?.WhenAll(Timestamp,
                                               API ?? this,
                                               Request);

        #endregion

        #region (protected internal) SendAuthStartEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnAuthStartEVSEResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendAuthStartEVSEResponse(DateTime      Timestamp,
                                                          HTTPAPI       API,
                                                          HTTPRequest   Request,
                                                          HTTPResponse  Response)

            => OnAuthStartEVSEResponse?.WhenAll(Timestamp,
                                                API ?? this,
                                                Request,
                                                Response);

        #endregion


        #region (protected internal) SendAuthStopEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE request was received.
        /// </summary>
        public HTTPRequestLogEvent OnAuthStopEVSERequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendAuthStopEVSERequest(DateTime     Timestamp,
                                                        HTTPAPI      API,
                                                        HTTPRequest  Request)

            => OnAuthStopEVSERequest?.WhenAll(Timestamp,
                                              API ?? this,
                                              Request);

        #endregion

        #region (protected internal) SendAuthStopEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnAuthStopEVSEResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendAuthStopEVSEResponse(DateTime      Timestamp,
                                                         HTTPAPI       API,
                                                         HTTPRequest   Request,
                                                         HTTPResponse  Response)

            => OnAuthStopEVSEResponse?.WhenAll(Timestamp,
                                               API ?? this,
                                               Request,
                                               Response);

        #endregion


        #region (protected internal) SendCDRsRequest(Request)

        /// <summary>
        /// An event sent whenever a charge detail record was received.
        /// </summary>
        public HTTPRequestLogEvent OnSendCDRsRequest = new HTTPRequestLogEvent();

        /// <summary>
        /// An event sent whenever a charge detail record was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendCDRsRequest(DateTime     Timestamp,
                                                HTTPAPI      API,
                                                HTTPRequest  Request)

            => OnSendCDRsRequest?.WhenAll(Timestamp,
                                          API ?? this,
                                          Request);

        #endregion

        #region (protected internal) SendCDRsResponse(Response)

        /// <summary>
        /// An event sent whenever a charge detail record response was sent.
        /// </summary>
        public HTTPResponseLogEvent OnSendCDRsResponse = new HTTPResponseLogEvent();

        /// <summary>
        /// An event sent whenever a charge detail record response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendCDRsResponse(DateTime      Timestamp,
                                                 HTTPAPI       API,
                                                 HTTPRequest   Request,
                                                 HTTPResponse  Response)

            => OnSendCDRsResponse?.WhenAll(Timestamp,
                                           API ?? this,
                                           Request,
                                           Response);

        #endregion

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
                    ServiceName,
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
                    Language,
                    ServiceName,
                    Languages) => new HTMLEMailBuilder() {

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
                    ServiceName,
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
                    ServiceName,
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
        /// <param name="ServiceName">The name of the service.</param>
        /// <param name="HTTPServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        /// <param name="LocalHostname">The HTTP hostname for all URIs within this API.</param>
        /// <param name="LocalPort">A TCP port to listen on.</param>
        /// <param name="BaseURL">The base url of the service.</param>
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
        /// <param name="SMSSenderName">The (default) SMS sender name.</param>
        /// <param name="APIAdminSMS">A list of admin SMS phonenumbers.</param>
        /// 
        /// <param name="TelegramBotToken">The Telegram API access token of the bot.</param>
        /// 
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="UseSecureCookies">Force the web browser to send cookies only via HTTPS.</param>
        /// <param name="Language">The main language of the API.</param>
        /// <param name="LogoImage">The logo of the website.</param>
        /// <param name="NewUserSignUpEMailCreator">A delegate for sending a sign-up e-mail to a new user.</param>
        /// <param name="NewUserWelcomeEMailCreator">A delegate for sending a welcome e-mail to a new user.</param>
        /// <param name="ResetPasswordEMailCreator">A delegate for sending a reset password e-mail to a user.</param>
        /// <param name="PasswordChangedEMailCreator">A delegate for sending a password changed e-mail to a user.</param>
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
        public OpenChargingCloudAPI(String                               ServiceName                        = "GraphDefined Open Charging Cloud API",
                                    String                               HTTPServerName                     = "GraphDefined Open Charging Cloud API",
                                    HTTPHostname?                        LocalHostname                      = null,
                                    IPPort?                              LocalPort                          = null,
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
                                    String                               SMSSenderName                      = null,
                                    IEnumerable<PhoneNumber>             APIAdminSMS                        = null,

                                    String                               TelegramBotToken                   = null,

                                    HTTPCookieName?                      CookieName                         = null,
                                    Boolean                              UseSecureCookies                   = true,
                                    Languages?                           Language                           = null,
                                    String                               LogoImage                          = null,
                                    NewUserSignUpEMailCreatorDelegate    NewUserSignUpEMailCreator          = null,
                                    NewUserWelcomeEMailCreatorDelegate   NewUserWelcomeEMailCreator         = null,
                                    ResetPasswordEMailCreatorDelegate    ResetPasswordEMailCreator          = null,
                                    PasswordChangedEMailCreatorDelegate  PasswordChangedEMailCreator        = null,
                                    Byte?                                MinUserNameLenght                  = null,
                                    Byte?                                MinRealmLenght                     = null,
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
                                    String                               DatabaseFile                       = DefaultOpenChargingCloudAPIDatabaseFile,
                                    String                               LoggingPath                        = "default",
                                    String                               LogfileName                        = DefaultOpenChargingCloudAPILogFile,
                                    DNSClient                            DNSClient                          = null,
                                    Boolean                              Autostart                          = false)

            : base(ServiceName:                       ServiceName                 ?? "GraphDefined Open Charging Cloud API",
                   HTTPServerName:                    HTTPServerName              ?? "GraphDefined Open Charging Cloud API",
                   LocalHostname:                     LocalHostname,
                   LocalPort:                         LocalPort                   ?? IPPort.Parse(5500),
                   BaseURL:                           BaseURL,
                   URLPathPrefix:                     URLPathPrefix,

                   ServerCertificateSelector:         ServerCertificateSelector,
                   ClientCertificateValidator:        ClientCertificateValidator,
                   ClientCertificateSelector:         ClientCertificateSelector,
                   AllowedTLSProtocols:               AllowedTLSProtocols,

                   APIEMailAddress:                   APIEMailAddress,
                   APIPassphrase:                     APIPassphrase,
                   APIAdminEMails:                    APIAdminEMails,
                   APISMTPClient:                     APISMTPClient,

                   SMSAPICredentials:                 SMSAPICredentials,
                   SMSSenderName:                     SMSSenderName               ?? "Open Charging Cloud",
                   APIAdminSMS:                       APIAdminSMS,

                   TelegramBotToken:                  TelegramBotToken,

                   CookieName:                        CookieName                  ?? HTTPCookieName.Parse("OpenChargingCloudAPI"),
                   UseSecureCookies:                  UseSecureCookies,
                   Language:                          Language                    ?? Languages.eng,
                   LogoImage:                         LogoImage                   ?? "images/OpenChargingCloud_Logo2.png",
                   NewUserSignUpEMailCreator:         NewUserSignUpEMailCreator   ?? __NewUserSignUpEMailCreator          (BaseURL, APIEMailAddress, APIPassphrase),
                   NewUserWelcomeEMailCreator:        NewUserWelcomeEMailCreator  ?? __NewUserWelcomeEMailCreatorDelegate (BaseURL, APIEMailAddress, APIPassphrase),
                   ResetPasswordEMailCreator:         ResetPasswordEMailCreator   ?? __ResetPasswordEMailCreatorDelegate  (BaseURL, APIEMailAddress, APIPassphrase),
                   PasswordChangedEMailCreator:       PasswordChangedEMailCreator ?? __PasswordChangedEMailCreatorDelegate(BaseURL, APIEMailAddress, APIPassphrase),
                   MinUserNameLenght:                 MinUserNameLenght,
                   MinRealmLenght:                    MinRealmLenght,
                   PasswordQualityCheck:              PasswordQualityCheck,
                   SignInSessionLifetime:             SignInSessionLifetime       ?? TimeSpan.FromDays(30),

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
                   DatabaseFile:                      DatabaseFile                ?? DefaultOpenChargingCloudAPIDatabaseFile,
                   LoggingPath:                       LoggingPath                 ?? "default",
                   LogfileName:                       LogfileName                 ?? DefaultOpenChargingCloudAPILogFile,
                   DNSClient:                         DNSClient,
                   Autostart:                         Autostart)

        {

            Directory.CreateDirectory("WWCP");
            Directory.CreateDirectory("HTTPSSEs");
            Directory.CreateDirectory("Metrics");
            Directory.CreateDirectory("CI-Tests");

            Directory.CreateDirectory("OpenChargingCloudAPI");
            Directory.CreateDirectory("ChargingReservations");
            Directory.CreateDirectory("ChargingSessions");
            Directory.CreateDirectory("ChargeDetailRecords");

            //WWCP = OpenChargingCloudAPI.AttachToHTTPAPI(HTTPServer);

            this.WWCPHTTPServer = new HTTPServer<RoamingNetworks, RoamingNetwork>(HTTPServer);

            var LogfilePrefix = "HTTPSSEs" + Path.DirectorySeparatorChar;

            DebugLog     = HTTPServer.AddJSONEventSource(EventIdentification:      DebugLogId,
                                                         URLTemplate:              this.URLPathPrefix + "/" + DebugLogId.ToString(),
                                                         MaxNumberOfCachedEvents:  10000,
                                                         RetryIntervall:           TimeSpan.FromSeconds(5),
                                                         EnableLogging:            true,
                                                         LogfilePrefix:            LogfilePrefix);

            ImporterLog  = HTTPServer.AddJSONEventSource(EventIdentification:      ImporterLogId,
                                                         URLTemplate:              this.URLPathPrefix + "/" + ImporterLogId.ToString(),
                                                         MaxNumberOfCachedEvents:  1000,
                                                         RetryIntervall:           TimeSpan.FromSeconds(5),
                                                         EnableLogging:            true,
                                                         LogfilePrefix:            LogfilePrefix);


            RegisterURLTemplates();

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

        //    this.WWCP = OpenChargingCloudAPI.AttachToHTTPAPI(HTTPServer);

        //    if (!SkipURITemplates)
        //        RegisterURLTemplates();

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
        ///// <param name="URIPrefix">A common prefix for all URLs.</param>
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

        //    this.WWCP = OpenChargingCloudAPI.AttachToHTTPAPI(HTTPServer ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!"));

        //    if (!SkipURITemplates)
        //        RegisterURLTemplates();

        //}

        #endregion

        #endregion


        #region (static) AttachToHTTPAPI(HTTPServer, URIPrefix = "/", ...)

        ///// <summary>
        ///// Attach this HTTP API to the given HTTP server.
        ///// </summary>
        ///// <param name="HTTPServer">An existing HTTP server.</param>
        ///// <param name="HTTPHostname">The HTTP hostname for all URIs within this API.</param>
        ///// <param name="URIPrefix">A common prefix for all URLs.</param>
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

        #region (private) RegisterURLTemplates()

        private void RegisterURLTemplates()
        {

            #region /shared/OpenChargingCloudAPI

            HTTPServer.RegisterResourcesFolder(HTTPHostname.Any,
                                               HTTPPath.Parse("/shared/OpenChargingCloudAPI"),
                                               HTTPRoot.Substring(0, HTTPRoot.Length - 1));

            #endregion


            #region ~/RNs

            #region GET         ~/RNs

            // -----------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // -----------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     var AllRoamingNetworks  = GetAllRoamingNetworks(Request.Host);
                                                     var skip                = Request.QueryString.GetUInt64("skip");
                                                     var take                = Request.QueryString.GetUInt64("take");

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount       = AllRoamingNetworks.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = AllRoamingNetworks.
                                                                                                 ToJSON(skip, take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs

            // --------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // --------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "RNs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     var AllRoamingNetworks  = GetAllRoamingNetworks(Request.Host);

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  AllRoamingNetworks.ULongCount())
                                                                                            ).ToUTF8Bytes(),
                                                             Connection                   = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region OPTIONS     ~/RNs

            // ----------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // ----------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs",
                                                 HTTPDelegate: Request =>

                                                     Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.NoContent,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             Connection                 = "close"
                                                         }.AsImmutable)

                                                 );

            #endregion


            #region GET         ~/RNs->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs->Id
            // -------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs->Id",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     var AllRoamingNetworks  = GetAllRoamingNetworks(Request.Host);
                                                     var skip                = Request.QueryString.GetUInt64("skip");
                                                     var take                = Request.QueryString.GetUInt64("take");

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount       = AllRoamingNetworks.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = new JArray(AllRoamingNetworks.
                                                                                                             Select(rn => rn.Id.ToString()).
                                                                                                             Skip  (Request.QueryString.GetUInt64("skip")).
                                                                                                             Take  (Request.QueryString.GetUInt64("take"))).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs->AdminStatus

            // ------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs->AdminStatus
            // ------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     var AllRoamingNetworks  = GetAllRoamingNetworks(Request.Host);
                                                     var skip                = Request.QueryString.GetUInt64("skip");
                                                     var take                = Request.QueryString.GetUInt64("take");
                                                     var sinceFilter         = Request.QueryString.CreateDateTimeFilter<RoamingNetworkAdminStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter         = Request.QueryString.CreateStringFilter  <RoamingNetworkAdminStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount       = AllRoamingNetworks.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = AllRoamingNetworks.
                                                                                                  Select(rn => new RoamingNetworkAdminStatus(rn.Id, rn.AdminStatus)).
                                                                                                  Where (matchFilter).
                                                                                                  Where (sinceFilter).
                                                                                                  ToJSON(skip, take).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs->Status

            // -------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs->Status
            // -------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     var AllRoamingNetworks  = GetAllRoamingNetworks(Request.Host);
                                                     var skip                = Request.QueryString.GetUInt64("skip");
                                                     var take                = Request.QueryString.GetUInt64("take");
                                                     var sinceFilter         = Request.QueryString.CreateDateTimeFilter<RoamingNetworkStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter         = Request.QueryString.CreateStringFilter  <RoamingNetworkStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount      = AllRoamingNetworks.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = AllRoamingNetworks.
                                                                                                  Select(rn => new RoamingNetworkStatus(rn.Id, rn.Status)).
                                                                                                  Where (matchFilter).
                                                                                                  Where (sinceFilter).
                                                                                                  ToJSON(skip, take).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}

            #region GET         ~/RNs/{RoamingNetworkId}

            // ----------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            // ----------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork _RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, CREATE, DELETE",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _RoamingNetwork.ToJSON().ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region CREATE      ~/RNs/{RoamingNetworkId}

            // ---------------------------------------------------------------------------------
            // curl -v -X CREATE -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test2
            // ---------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.CREATE,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  CreateRoamingNetworkRequest,
                                                 HTTPResponseLogger: CreateRoamingNetworkResponse,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (Request.Authorization          == null      ||
                                                     //    Request.Authorization.Username != HTTPLogin ||
                                                     //    Request.Authorization.Password != HTTPPassword)
                                                     //    return SendEVSEStatusSetted(
                                                     //        new HTTPResponse.Builder(Request) {
                                                     //            HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //            WWWAuthenticate  = @"Basic realm=""WWCP""",
                                                     //            Server           = HTTPServer.DefaultServerName,
                                                     //            Date             = DateTime.UtcNow,
                                                     //            Connection       = "close"
                                                     //        });

                                                     #endregion

                                                     #region Check HTTP parameters

                                                     if (Request.ParsedURIParameters.Length < 1)
                                                         return Task.FromResult(new HTTPResponse.Builder(Request) {
                                                                     HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                     Server          = HTTPServer.DefaultServerName,
                                                                     Date            = DateTime.UtcNow,
                                                                 }.AsImmutable);


                                                     if (!RoamingNetwork_Id.TryParse(Request.ParsedURIParameters[0],
                                                                                     out RoamingNetwork_Id _RoamingNetworkId))
                                                         return Task.FromResult(new HTTPResponse.Builder(Request) {
                                                                     HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                     Server          = HTTPServer.DefaultServerName,
                                                                     Date            = DateTime.UtcNow,
                                                                     ContentType     = HTTPContentType.JSON_UTF8,
                                                                     Content         = HTTPExtentions.CreateError("Invalid roaming network identification!")
                                                                 }.AsImmutable);


                                                     if (TryGetRoamingNetwork(Request.Host,
                                                                              _RoamingNetworkId,
                                                                              out RoamingNetwork _RoamingNetwork))
                                                        return Task.FromResult(new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode  = HTTPStatusCode.Conflict,
                                                                    Server          = HTTPServer.DefaultServerName,
                                                                    Date            = DateTime.UtcNow,
                                                                    ContentType     = HTTPContentType.JSON_UTF8,
                                                                    Content         = HTTPExtentions.CreateError("RoamingNetworkId already exists!")
                                                                }.AsImmutable);

                                                     #endregion

                                                     #region Parse optional JSON

                                                     I18NString RoamingNetworkName         = I18NString.Empty;
                                                     I18NString RoamingNetworkDescription  = I18NString.Empty;

                                                     if (Request.TryParseJObjectRequestBody(out JObject       JSON,
                                                                                            out HTTPResponse  _HTTPResponse,
                                                                                            AllowEmptyHTTPBody: true))
                                                     {

                                                         if (!JSON.ParseMandatory("name",
                                                                                  "roaming network name",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  out RoamingNetworkName,
                                                                                  Request,
                                                                                  out _HTTPResponse))
                                                         {
                                                             return Task.FromResult(_HTTPResponse);
                                                         }

                                                         if (!JSON.ParseOptional("description",
                                                                                 "roaming network description",
                                                                                 HTTPServer.DefaultServerName,
                                                                                 out RoamingNetworkDescription,
                                                                                 Request,
                                                                                 out _HTTPResponse))
                                                         {
                                                             return Task.FromResult(_HTTPResponse);
                                                         }

                                                     }

                                                     #endregion


                                                     _RoamingNetwork = CreateNewRoamingNetwork(Request.Host,
                                                                                                  _RoamingNetworkId,
                                                                                                  RoamingNetworkName,
                                                                                                  Description: RoamingNetworkDescription ?? I18NString.Empty);


                                                     return Task.FromResult(new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.Created,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, CREATE, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ETag                       = "1",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = _RoamingNetwork.ToJSON().ToUTF8Bytes(),
                                                                 Connection                 = "close"
                                                             }.AsImmutable);

                                                 });

            #endregion

            #region DELETE      ~/RNs/{RoamingNetworkId}

            // ---------------------------------------------------------------------------------
            // curl -v -X DELETE -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test2
            // ---------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.DELETE,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  DeleteRoamingNetworkRequest,
                                                 HTTPResponseLogger: DeleteRoamingNetworkResponse,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (Request.Authorization          == null      ||
                                                     //    Request.Authorization.Username != HTTPLogin ||
                                                     //    Request.Authorization.Password != HTTPPassword)
                                                     //    return SendEVSEStatusSetted(
                                                     //        new HTTPResponse.Builder(Request) {
                                                     //            HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //            WWWAuthenticate  = @"Basic realm=""WWCP""",
                                                     //            Server           = HTTPServer.DefaultServerName,
                                                     //            Date             = DateTime.UtcNow,
                                                     //            Connection       = "close"
                                                     //        });

                                                     #endregion

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion


                                                     var RoamingNetwork = RemoveRoamingNetwork(Request.Host, _RoamingNetwork.Id);


                                                     return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.OK,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, CREATE, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ETag                       = "1",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = RoamingNetwork.ToJSON().ToUTF8Bytes()
                                                             }.AsImmutable);

                                                 });

            #endregion

            #region OPTIONS     ~/RNs/{RoamingNetworkId}

            // -----------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}
            // -----------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}",
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.NoContent,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             Connection                 = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/{PropertyKey}

            // ----------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            // ----------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/{PropertyKey}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     if (Request.ParsedURIParameters.Length < 2)
                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 Server          = HTTPServer.DefaultServerName,
                                                                 Date            = DateTime.UtcNow,
                                                             }.AsImmutable);

                                                     var PropertyKey = Request.ParsedURIParameters[1];

                                                     if (PropertyKey.IsNullOrEmpty())
                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 Server          = HTTPServer.DefaultServerName,
                                                                 Date            = DateTime.UtcNow,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = @"{ ""description"": ""Invalid property key!"" }".ToUTF8Bytes()
                                                             }.AsImmutable);


                                                     if (!_RoamingNetwork.TryGet(PropertyKey, out Object Value))
                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.NotFound,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, SET",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ETag                       = "1",
                                                                 Connection                 = "close"
                                                             }.AsImmutable);


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, SET",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = JSONObject.Create(
                                                                                              new JProperty(PropertyKey, Value)
                                                                                          ).ToUTF8Bytes(),
                                                             Connection                 = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/{PropertyKey}

            // ----------------------------------------------------------------------
            // curl -v -X SET -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            // ----------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.SET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/{PropertyKey}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     if (Request.ParsedURIParameters.Length < 2)
                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 Server          = HTTPServer.DefaultServerName,
                                                                 Date            = DateTime.UtcNow,
                                                                 Connection      = "close"
                                                             }.AsImmutable);

                                                     var PropertyKey = Request.ParsedURIParameters[1];

                                                     if (PropertyKey.IsNullOrEmpty())
                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 Server          = HTTPServer.DefaultServerName,
                                                                 Date            = DateTime.UtcNow,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = @"{ ""description"": ""Invalid property key!"" }".ToUTF8Bytes(),
                                                                 Connection      = "close"
                                                             }.AsImmutable);


                                                     #region Parse optional JSON

                                                     String OldValue  = null;
                                                     String NewValue  = null;

                                                     var DescriptionI18N = I18NString.Empty;

                                                     if (Request.TryParseJObjectRequestBody(out JObject      JSON,
                                                                                            out HTTPResponse HTTPResponse,
                                                                                            AllowEmptyHTTPBody: false))
                                                     {

                                                         #region Parse oldValue    [mandatory]

                                                         if (!JSON.ParseMandatoryText("oldValue",
                                                                                      "old value of the property",
                                                                                      HTTPServer.DefaultServerName,
                                                                                      out OldValue,
                                                                                      Request,
                                                                                      out HTTPResponse))
                                                         {
                                                             return Task.FromResult(HTTPResponse);
                                                         }

                                                         #endregion

                                                         #region Parse newValue    [mandatory]

                                                         if (!JSON.ParseMandatoryText("newValue",
                                                                                      "new value of the property",
                                                                                      HTTPServer.DefaultServerName,
                                                                                      out NewValue,
                                                                                      Request,
                                                                                      out HTTPResponse))
                                                         {
                                                             return Task.FromResult(HTTPResponse);
                                                         }

                                                         #endregion

                                                     }

                                                     #endregion


                                                     var result = _RoamingNetwork.Set(PropertyKey,
                                                                                      NewValue,
                                                                                      OldValue);

                                                     #region Choose HTTP status code

                                                     HTTPStatusCode _HTTPStatusCode;

                                                     switch (result)
                                                     {

                                                         case SetPropertyResult.Added:
                                                             _HTTPStatusCode = HTTPStatusCode.Created;
                                                             break;

                                                         case SetPropertyResult.Conflict:
                                                             _HTTPStatusCode = HTTPStatusCode.Conflict;
                                                             break;

                                                         default:
                                                             _HTTPStatusCode = HTTPStatusCode.OK;
                                                             break;

                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = _HTTPStatusCode,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, SET",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = JSONObject.Create(
                                                                                              new JProperty("oldValue",  OldValue),
                                                                                              new JProperty("newValue",  NewValue)
                                                                                          ).ToUTF8Bytes(),
                                                             Connection                 = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/ChargingPools

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools

            // ------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools
            // ------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")          ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators")         ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("-chargingstations") ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")            ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses      = expand.ContainsIgnoreCase("licenses")          ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount           = RoamingNetwork.ChargingPools.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = RoamingNetwork.ChargingPools.
                                                                                                ToJSON(skip,
                                                                                                       take,
                                                                                                       false,
                                                                                                       expandRoamingNetworks,
                                                                                                       expandOperators,
                                                                                                       expandChargingStations,
                                                                                                       expandBrands,
                                                                                                       expandDataLicenses).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingPools

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingPools
            // -----------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  RoamingNetwork.ChargingPools.ULongCount())
                                                                                            ).ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/ChargingPools

            // -----------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools
            // -----------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                                                 HTTPDelegate: Request =>

                                                     Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.NoContent,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                         }.AsImmutable)

                                                 );

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->Id
            // -------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->Id",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = new JArray(RoamingNetwork.ChargingPools.
                                                                                                            Select(pool => pool.Id.ToString()).
                                                                                                            Skip  (Request.QueryString.GetUInt64("skip")).
                                                                                                            Take  (Request.QueryString.GetUInt64("take"))).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = RoamingNetwork.ChargingPools.ULongCount()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->AdminStatus

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->AdminStatus
            // -------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64                           ("skip");
                                                     var take           = Request.QueryString.GetUInt64                           ("take");
                                                     var sinceFilter    = Request.QueryString.CreateDateTimeFilter<ChargingPoolAdminStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter    = Request.QueryString.CreateStringFilter  <ChargingPoolAdminStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.ChargingPoolAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.ChargingPoolAdminStatus().
                                                                                                  Where (matchFilter).
                                                                                                  Where (sinceFilter).
                                                                                                  ToJSON(skip, take).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->Status

            // --------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->Status
            // --------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64                      ("skip");
                                                     var take           = Request.QueryString.GetUInt64                      ("take");
                                                     var sinceFilter    = Request.QueryString.CreateDateTimeFilter<ChargingPoolStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter    = Request.QueryString.CreateStringFilter  <ChargingPoolStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.ChargingPoolStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.ChargingPoolStatus().
                                                                                                  Where (matchFilter).
                                                                                                  Where (sinceFilter).
                                                                                                  ToJSON(skip, take).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(

                                                                                                new JProperty("count",  RoamingNetwork.ChargingPools.Count()),

                                                                                                new JProperty("status", JSONObject.Create(
                                                                                                    RoamingNetwork.ChargingPools.GroupBy(pool => pool.Status.Value).Select(group =>
                                                                                                        new JProperty(group.Key.ToString().ToLower(),
                                                                                                                      group.Count()))
                                                                                                ))

                                                                                            ).ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/...
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(HTTPHostname.Any,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, SET",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _ChargingPool.ToJSON().ToUTF8Bytes()
                                                         }.AsImmutable);

                                           });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")      ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("chargingpools") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandEVSEs             = expand.ContainsIgnoreCase("-evses")        ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")        ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount   = _ChargingPool.ChargingStations.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _ChargingPool.ChargingStations.
                                                                                                OrderBy(station => station.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandEVSEs,
                                                                                                        expandOperators,
                                                                                                        expandChargingPools,
                                                                                                        expandBrands).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations->AdminStatus

            // -----------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/ChargingStations->AdminStatus
            // -----------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/ChargingStations->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingPool.ChargingStationAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingPool.ChargingStationAdminStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations->Status

            // ------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/ChargingStations->Status
            // ------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/ChargingStations->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingPool.ChargingStationStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingPool.ChargingStationStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPoolAndChargingStation(this,
                                                                                                                       out RoamingNetwork   _RoamingNetwork,
                                                                                                                       out ChargingPool     _ChargingPool,
                                                                                                                       out ChargingStation  _ChargingStation,
                                                                                                                       out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                   = Request.QueryString.GetUInt64("skip");
                                                     var take                   = Request.QueryString.GetUInt64("take");
                                                     var expand                 = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks  = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators        = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands           = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount   = _ChargingStation.EVSEs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _ChargingStation.EVSEs.
                                                                                                OrderBy(evse => evse.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandRoamingNetworks,
                                                                                                        expandOperators,
                                                                                                        expandBrands).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this,
                                                                                                                              out RoamingNetwork   _RoamingNetwork,
                                                                                                                              out ChargingPool     _ChargingPool,
                                                                                                                              out ChargingStation  _ChargingStation,
                                                                                                                              out EVSE             _EVSE,
                                                                                                                              out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _EVSE.ToJSON().ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../EVSEs
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount   = _ChargingPool.EVSEs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _ChargingPool.EVSEs.
                                                                                                OrderBy(evse => evse.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandRoamingNetworks,
                                                                                                        expandOperators,
                                                                                                        expandChargingPools,
                                                                                                        expandChargingStations,
                                                                                                        expandBrands).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus

            // ------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus
            // ------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingPool.EVSEAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingPool.EVSEAdminStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->Status

            // -------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/{ChargingPoolId}/EVSEs->Status
            // -------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork  _RoamingNetwork,
                                                                                                     out ChargingPool    _ChargingPool,
                                                                                                     out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingPool.EVSEStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingPool.EVSEStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/ChargingStations

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations
            // --------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip                   = Request.QueryString.GetUInt64("skip");
                                                     var take                   = Request.QueryString.GetUInt64("take");
                                                     var expand                 = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks  = expand.ContainsIgnoreCase("networks")      ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandOperators        = expand.ContainsIgnoreCase("operators")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools    = expand.ContainsIgnoreCase("chargingpools") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandEVSEs            = expand.ContainsIgnoreCase("-evses")        ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                                                     var expandBrands           = expand.ContainsIgnoreCase("brands")        ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses     = expand.ContainsIgnoreCase("licenses")      ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount   = RoamingNetwork.ChargingStations.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = RoamingNetwork.ChargingStations.
                                                                                                OrderBy(station => station.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandRoamingNetworks,
                                                                                                        expandOperators,
                                                                                                        expandChargingPools,
                                                                                                        expandEVSEs,
                                                                                                        expandBrands,
                                                                                                        expandDataLicenses).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingStations
            // --------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  RoamingNetwork.ChargingStations.ULongCount())
                                                                                            ).ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations
            // --------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations",
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.NoContent,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->Id
            // -------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->Id",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = new JArray(RoamingNetwork.ChargingStations.
                                                                                                            Select(station => station.Id.ToString()).
                                                                                                            Skip  (Request.QueryString.GetUInt64("skip")).
                                                                                                            Take  (Request.QueryString.GetUInt64("take"))).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = RoamingNetwork.ChargingStations.ULongCount()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->AdminStatus

            // ----------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->AdminStatus
            // ----------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64                              ("skip");
                                                     var take           = Request.QueryString.GetUInt64                              ("take");
                                                     var sinceFilter    = Request.QueryString.CreateDateTimeFilter<ChargingStationAdminStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter    = Request.QueryString.CreateStringFilter  <ChargingStationAdminStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.ChargingStationAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.ChargingStationAdminStatus().
                                                                                                             Where (matchFilter).
                                                                                                             Where (sinceFilter).
                                                                                                             ToJSON(skip, take).
                                                                                                             ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->Status

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->Status
            // -----------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64                         ("skip");
                                                     var take           = Request.QueryString.GetUInt64                         ("take");
                                                     var sinceFilter    = Request.QueryString.CreateDateTimeFilter<ChargingStationStatus>("since", (status, timestamp) => status.Status.Timestamp >= timestamp);
                                                     var matchFilter    = Request.QueryString.CreateStringFilter  <ChargingStationStatus>("match", (status, pattern)   => status.Id.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.ChargingStationStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.ChargingStationStatus().
                                                                                                             Where (matchFilter).
                                                                                                             Where (sinceFilter).
                                                                                                             ToJSON(skip, take).
                                                                                                             ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(

                                                                                                new JProperty("count",  RoamingNetwork.ChargingStations.Count()),

                                                                                                new JProperty("status", JSONObject.Create(
                                                                                                    RoamingNetwork.ChargingStations.GroupBy(station => station.Status.Value).Select(group =>
                                                                                                        new JProperty(group.Key.ToString().ToLower(),
                                                                                                                      group.Count()))
                                                                                                ))

                                                                                            ).ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/...
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(HTTPHostname.Any,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStation(this,
                                                                                                        out RoamingNetwork   _RoamingNetwork,
                                                                                                        out ChargingStation  _ChargingStation,
                                                                                                        out HTTPResponse     _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, SET",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _ChargingStation.ToJSON().ToUTF8Bytes()
                                                         }.AsImmutable);

                                           });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/.../EVSEs
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStation(this,
                                                                                                        out RoamingNetwork   _RoamingNetwork,
                                                                                                        out ChargingStation  _ChargingStation,
                                                                                                        out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;


                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount   = _ChargingStation.EVSEs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _ChargingStation.EVSEs.
                                                                                                OrderBy(evse => evse.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandRoamingNetworks,
                                                                                                        expandOperators,
                                                                                                        expandChargingPools,
                                                                                                        expandChargingStations,
                                                                                                        expandBrands).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus

            // ------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus
            // ------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStation(this,
                                                                                                        out RoamingNetwork   _RoamingNetwork,
                                                                                                        out ChargingStation  _ChargingStation,
                                                                                                        out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingStation.EVSEAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingStation.EVSEAdminStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->Status

            // -------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/EVSEs->Status
            // -------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStation(this,
                                                                                                        out RoamingNetwork   _RoamingNetwork,
                                                                                                        out ChargingStation  _ChargingStation,
                                                                                                        out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip            = Request.QueryString.GetUInt64("skip");
                                                     var take            = Request.QueryString.GetUInt64("take");
                                                     var historysize     = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount  = _ChargingStation.EVSEStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _ChargingStation.EVSEStatus().
                                                                                                 ToJSON(skip,
                                                                                                        take).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs

            // ----------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs
            // ----------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork  RoamingNetwork,
                                                                                      out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses      = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;


                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount          = RoamingNetwork.EVSEs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = RoamingNetwork.EVSEs.
                                                                                                OrderBy(evse => evse.Id).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        false,
                                                                                                        expandRoamingNetworks,
                                                                                                        expandOperators,
                                                                                                        expandChargingPools,
                                                                                                        expandChargingStations,
                                                                                                        expandBrands,
                                                                                                        expandDataLicenses).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/EVSEs

            // ---------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/EVSEs
            // ---------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  RoamingNetwork.EVSEs.ULongCount())
                                                                                            ).ToUTF8Bytes(),
                                                             Connection                   = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/EVSEs

            // ---------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs
            // ---------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.NoContent,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->Id

            // --------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->Id
            // --------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->Id",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork  RoamingNetwork,
                                                                                      out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = new JArray(RoamingNetwork.EVSEs.
                                                                                                            Select(evse => evse.Id.ToString()).
                                                                                                            Skip  (Request.QueryString.GetUInt64("skip")).
                                                                                                            Take  (Request.QueryString.GetUInt64("take"))).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = RoamingNetwork.EVSEs.ULongCount(),
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->AdminStatus

            // -----------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->AdminStatus
            // -----------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64("skip");
                                                     var take           = Request.QueryString.GetUInt64("take");
                                                     var historySize    = Request.QueryString.GetUInt64                     ("historySize",  1);
                                                     var afterFilter    = Request.QueryString.CreateDateTimeFilter<DateTime>("after",       (timestamp, pattern) => timestamp >= pattern);
                                                     var beforeFilter   = Request.QueryString.CreateDateTimeFilter<DateTime>("before",      (timestamp, pattern) => timestamp <= pattern);
                                                     var matchFilter    = Request.QueryString.CreateStringFilter  <EVSE_Id> ("match",       (evseId,    pattern) => evseId.ToString().Contains(pattern));

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.EVSEAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.EVSEAdminStatusSchedule(IncludeEVSEs:    evse      => matchFilter(evse.Id),
                                                                                                                                     TimestampFilter: timestamp => beforeFilter(timestamp) &&
                                                                                                                                                                   afterFilter (timestamp),
                                                                                                                                     HistorySize:     historySize).
                                                                                                             ToJSON(skip, take).
                                                                                                             ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->AdminStatusSchedule

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->AdminStatusSchedule
            // -------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->AdminStatusSchedule",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork  RoamingNetwork,
                                                                                      out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip           = Request.QueryString.GetUInt64("skip");
                                                     var take           = Request.QueryString.GetUInt64("take");
                                                     var historysize    = Request.QueryString.GetUInt64("historysize", 1);
                                                     var since          = Request.QueryString.CreateDateTimeFilter<EVSEAdminStatusSchedule>("since", (status, timestamp) => status.StatusSchedule.First().Timestamp >= timestamp);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var ExpectedCount  = RoamingNetwork.EVSEAdminStatus().ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.EVSEAdminStatus().
                                                                                                  ToJSON(skip,
                                                                                                         take).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->Status

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->Status
            // -----------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                         HTTPMethod.GET,
                                         URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->Status",
                                         HTTPContentType.JSON_UTF8,
                                         HTTPRequestLogger:  SendGetEVSEsStatusRequest,
                                         HTTPResponseLogger: SendGetEVSEsStatusResponse,
                                         HTTPDelegate:       Request => {

                                             #region Check parameters

                                             if (!Request.ParseRoamingNetwork(this,
                                                                              out RoamingNetwork RoamingNetwork,
                                                                              out HTTPResponse   _HTTPResponse))
                                             {
                                                 return Task.FromResult(_HTTPResponse);
                                             }

                                             #endregion

                                             var skip          = Request.QueryString.GetUInt64                     ("skip");
                                             var take          = Request.QueryString.GetUInt64                     ("take");
                                             var historySize   = Request.QueryString.GetUInt64                     ("historySize",  1);
                                             var afterFilter   = Request.QueryString.CreateDateTimeFilter<DateTime>("after",       (timestamp, pattern) => timestamp >= pattern);
                                             var beforeFilter  = Request.QueryString.CreateDateTimeFilter<DateTime>("before",      (timestamp, pattern) => timestamp <= pattern);
                                             var matchFilter   = Request.QueryString.CreateStringFilter  <EVSE_Id> ("match",       (evseId,    pattern) => evseId.ToString().Contains(pattern));

                                             //ToDo: Getting the expected total count might be very expensive!
                                             var ExpectedCount  = RoamingNetwork.EVSEStatus().ULongCount();

                                             return Task.FromResult(
                                                 new HTTPResponse.Builder(Request) {
                                                     HTTPStatusCode                 = HTTPStatusCode.OK,
                                                     Server                         = HTTPServer.DefaultServerName,
                                                     Date                           = DateTime.UtcNow,
                                                     AccessControlAllowOrigin       = "*",
                                                     AccessControlAllowMethods      = "GET",
                                                     AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                     ETag                           = "1",
                                                     ContentType                    = HTTPContentType.JSON_UTF8,
                                                     Content                        = RoamingNetwork.EVSEStatusSchedule(IncludeEVSEs:    evse      => matchFilter (evse.Id),
                                                                                                                        TimestampFilter: timestamp => beforeFilter(timestamp) &&
                                                                                                                                                      afterFilter (timestamp),
                                                                                                                        HistorySize:     historySize).
                                                                                                     ToJSON(skip, take).
                                                                                                     ToUTF8Bytes(),
                                                     X_ExpectedTotalNumberOfItems   = ExpectedCount,
                                                     Connection                     = "close"
                                                 }.AsImmutable);

                                              }, AllowReplacement: URIReplacement.Allow);

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(

                                                                                                new JProperty("count",  RoamingNetwork.EVSEs.Count()),

                                                                                                new JProperty("status", JSONObject.Create(
                                                                                                    RoamingNetwork.EVSEs.GroupBy(evse => evse.Status.Value).Select(group =>
                                                                                                        new JProperty(group.Key.ToString().ToLower(),
                                                                                                                      group.Count()))
                                                                                                ))

                                                                                            ).ToUTF8Bytes(),
                                                             Connection                   = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            // ---------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork _RoamingNetwork,
                                                                                             out EVSE           _EVSE,
                                                                                             out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _EVSE.ToJSON().ToUTF8Bytes(),
                                                             Connection                 = "close"
                                                         }.AsImmutable);

                                           });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus

            // -----------------------------------------------------------------------
            // curl -v -H "Accept:       application/json" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*E0001*1/AdminStatus
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async Request => {

                                                     #region Parse RoamingNetworkId and EVSEId parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork RoamingNetwork,
                                                                                             out EVSE           EVSE,
                                                                                             out HTTPResponse   _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     return new HTTPResponse.Builder(Request) {
                                                         HTTPStatusCode              = HTTPStatusCode.OK,
                                                         Server                      = HTTPServer.DefaultServerName,
                                                         Date                        = DateTime.UtcNow,
                                                         AccessControlAllowOrigin    = "*",
                                                         AccessControlAllowMethods   = "GET",
                                                         AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                         ETag                        = "1",
                                                         ContentType                 = HTTPContentType.JSON_UTF8,
                                                         Content                     = EVSE.AdminStatus.
                                                                                           ToJSON().
                                                                                           ToUTF8Bytes(),
                                                         Connection                  = "close"
                                                     };

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status

            // -----------------------------------------------------------------------
            // curl -v -H "Accept:       application/json" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*E0001*1/Status
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async Request => {

                                                     #region Parse RoamingNetworkId and EVSEId parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  RoamingNetwork,
                                                                                             out EVSE            EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     return new HTTPResponse.Builder(Request) {
                                                         HTTPStatusCode              = HTTPStatusCode.OK,
                                                         Server                      = HTTPServer.DefaultServerName,
                                                         Date                        = DateTime.UtcNow,
                                                         AccessControlAllowOrigin    = "*",
                                                         AccessControlAllowMethods   = "GET",
                                                         AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                         ETag                        = "1",
                                                         ContentType                 = HTTPContentType.JSON_UTF8,
                                                         Content                     = EVSE.Status.
                                                                                           ToJSON().
                                                                                           ToUTF8Bytes(),
                                                         Connection                  = "close"
                                                     };

                                                 });

            #endregion


            #region RESERVE     ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            #region Documentation

            // RESERVE ~/EVSEs/DE*GEF*E000001*1
            // 
            // {
            //     "ReservationId":      "5c24515b-0a88-1296-32ea-1226ce8a3cd0",               // optional
            //     "StartTime":          "2015-10-20T11:25:43.511Z",                           // optional; default: current timestamp
            //     "Duration":           3600,                                                 // optional; default: 900 [seconds]
            //     "IntendedCharging":   {                                                     // optional; (good for energy management)
            //                               "ProductId":   "AC1"                              // optional; default: "AC1"
            //                               "StartTime":   "2015-10-20T11:30:00.000Z",        // optional; default: reservation start time
            //                               "Duration":    1800,                              // optional; default: reservation duration [seconds]
            //                               "Plugs":       ["TypeFSchuko|Type2Outlet|..."],   // optional;
            //                               "MaxEnergy":   20,                                // optional; [kWh]
            //                               "MaxCosts":    [20, "Euro"],                      // optional;
            //                               "ChargePlan":  "fastest"                          // optional;
            //                           },
            //     "AuthorizedIds":      {                                                     // optional; List of authentication methods...
            //                               "AuthTokens",  ["012345ABCDEF", ...],                // optional; List of RFID Ids
            //                               "eMAIds",   ["DE*GDF*00112233*1", ...],           // optional; List of eMA Ids
            //                               "PINs",     ["123456", ...],                      // optional; List of keypad Pins
            //                               "Liste",    [...]                                 // optional; List of known (white-)lists
            //                           }
            // }

            #endregion

            // -----------------------------------------------------------------------
            // curl -v -X RESERVE -H "Content-Type: application/json" \
            //                    -H "Accept:       application/json"  \
            //      -d "{ \
            //            \"StartTime\":     \"2015-10-20T11:25:43.511Z\", \
            //            \"Duration\":        3600, \
            //            \"IntendedCharging\": { \
            //                                 \"Consumption\": 20, \
            //                                 \"Plug\":        \"TypeFSchuko\" \
            //                               }, \
            //            \"AuthorizedIds\": { \
            //                                 \"AuthTokens\": [\"1AA234BB\", \"012345ABCDEF\"], \
            //                                 \"eMAIds\":  [\"DE*GEF*00112233*1\"], \
            //                                 \"PINs\":    [\"1234\", \"6789\"] \
            //                               } \
            //          }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 RESERVE,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendReserveEVSERequest,
                                                 HTTPResponseLogger: SendReserveEVSEResponse,
                                                 HTTPDelegate:       async Request => {

                                                     #region Check RoamingNetworkId and EVSEId URI parameters

                                                     HTTPResponse    _HTTPResponse;
                                                     RoamingNetwork  RoamingNetwork;
                                                     EVSE            EVSE;

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork,
                                                                                             out EVSE,
                                                                                             out _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Define (optional) parameters

                                                     ChargingReservation_Id?  ReservationId       = null;
                                                     eMobilityProvider_Id?    ProviderId          = null;
                                                     eMobilityAccount_Id      eMAId               = default(eMobilityAccount_Id);
                                                     DateTime?                StartTime           = null;
                                                     TimeSpan?                Duration            = null;

                                                     // IntendedCharging
                                                     ChargingProduct_Id?      ChargingProductId   = null;
                                                     DateTime?                ChargingStartTime   = null;
                                                     TimeSpan?                CharingDuration     = null;
                                                     PlugTypes?               Plug                = null;
                                                     var                      Consumption         = 0U;

                                                     // AuthorizedIds
                                                     var                      AuthTokens          = new List<Auth_Token>();
                                                     var                      eMAIds              = new List<eMobilityAccount_Id>();
                                                     var                      PINs                = new List<UInt32>();

                                                     #endregion

                                                     #region Parse  (optional) JSON

                                                     if (Request.TryParseJObjectRequestBody(out JObject JSON,
                                                                                            out _HTTPResponse,
                                                                                            AllowEmptyHTTPBody: true))
                                                     {

                                                         #region Check ReservationId        [optional]

                                                         if (JSON.ParseOptionalStruct2("ReservationId",
                                                                                      "ReservationId",
                                                                                      HTTPServer.DefaultServerName,
                                                                                      ChargingReservation_Id.TryParse,
                                                                                      out ReservationId,
                                                                                      Request,
                                                                                      out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Check ProviderId           [optional]

                                                         if (JSON.ParseOptionalStruct2("ProviderId",
                                                                                      "ProviderId",
                                                                                      HTTPServer.DefaultServerName,
                                                                                      eMobilityProvider_Id.TryParse,
                                                                                      out ProviderId,
                                                                                      Request,
                                                                                      out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Check eMAId                [mandatory]

                                                         if (!JSON.ParseMandatory("eMAId",
                                                                                  "eMAId",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  eMobilityAccount_Id.TryParse,
                                                                                  out eMAId,
                                                                                  Request,
                                                                                  out _HTTPResponse))
                                                         {
                                                             return _HTTPResponse;
                                                         }

                                                         #endregion

                                                         #region Check StartTime            [optional]

                                                         if (JSON.ParseOptional("StartTime",
                                                                                "start time!",
                                                                                HTTPServer.DefaultServerName,
                                                                                out StartTime,
                                                                                Request,
                                                                                out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                             if (StartTime <= DateTime.Now)
                                                                 return new HTTPResponse.Builder(Request) {
                                                                            HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                            ContentType     = HTTPContentType.JSON_UTF8,
                                                                            Content         = new JObject(new JProperty("description", "The starting time must be in the future!")).ToUTF8Bytes()
                                                                        };

                                                         }

                                                         #endregion

                                                         #region Check Duration             [optional]

                                                         if (JSON.ParseOptional("Duration",
                                                                                "Duration",
                                                                                HTTPServer.DefaultServerName,
                                                                                out Duration,
                                                                                Request,
                                                                                out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Check IntendedCharging     [optional]

                                                         if (JSON.ParseOptional("IntendedCharging",
                                                                                "IntendedCharging",
                                                                                HTTPServer.DefaultServerName,
                                                                                out JObject IntendedChargingJSON,
                                                                                Request,
                                                                                out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                             #region Check ChargingStartTime    [optional]

                                                             if (IntendedChargingJSON.ParseOptional("StartTime",
                                                                                                    "IntendedCharging/StartTime",
                                                                                                    HTTPServer.DefaultServerName,
                                                                                                    out ChargingStartTime,
                                                                                                    Request,
                                                                                                    out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse != null)
                                                                     return _HTTPResponse;

                                                             }

                                                             #endregion

                                                             #region Check Duration             [optional]

                                                             if (IntendedChargingJSON.ParseOptional("Duration",
                                                                                                    "IntendedCharging/Duration",
                                                                                                    HTTPServer.DefaultServerName,
                                                                                                    out CharingDuration,
                                                                                                    Request,
                                                                                                    out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse != null)
                                                                     return _HTTPResponse;

                                                             }

                                                             #endregion

                                                             #region Check ChargingProductId    [optional]

                                                             if (!JSON.ParseOptional("ChargingProductId",
                                                                                     "IntendedCharging/ChargingProductId",
                                                                                     HTTPServer.DefaultServerName,
                                                                                     out ChargingProductId,
                                                                                     Request,
                                                                                     out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse != null)
                                                                     return _HTTPResponse;

                                                             }

                                                             #endregion

                                                             #region Check Plug                 [optional]

                                                             if (IntendedChargingJSON.ParseOptional("Plug",
                                                                                                    "IntendedCharging/ChargingProductId",
                                                                                                    HTTPServer.DefaultServerName,
                                                                                                    out Plug,
                                                                                                    Request,
                                                                                                    out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse != null)
                                                                     return _HTTPResponse;

                                                             }

                                                             #endregion

                                                             #region Check Consumption          [optional, kWh]

                                                             if (IntendedChargingJSON.ParseOptional("Consumption",
                                                                                                    "IntendedCharging/Consumption",
                                                                                                    HTTPServer.DefaultServerName,
                                                                                                    UInt32.Parse,
                                                                                                    out Consumption,
                                                                                                    Request,
                                                                                                    out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse != null)
                                                                     return _HTTPResponse;

                                                             }

                                                             #endregion

                                                         }

                                                         #endregion

                                                         #region Check AuthorizedIds        [optional]

                                                         if (JSON.ParseOptional("AuthorizedIds",
                                                                                "AuthorizedIds",
                                                                                HTTPServer.DefaultServerName,
                                                                                out JObject AuthorizedIdsJSON,
                                                                                Request,
                                                                                out _HTTPResponse))
                                                         {

                                                             if (_HTTPResponse != null)
                                                                 return _HTTPResponse;

                                                             #region Check AuthTokens   [optional]

                                                             if (AuthorizedIdsJSON.ParseOptional("AuthTokens",
                                                                                                 "AuthorizedIds/AuthTokens",
                                                                                                 HTTPServer.DefaultServerName,
                                                                                                 out JArray AuthTokensJSON,
                                                                                                 Request,
                                                                                                 out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse == null)
                                                                     return _HTTPResponse;

                                                                 foreach (var jtoken in AuthTokensJSON)
                                                                 {

                                                                     if (!Auth_Token.TryParse(jtoken.Value<String>(), out Auth_Token AuthToken))
                                                                         return new HTTPResponse.Builder(Request) {
                                                                                    HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                                    Date                       = DateTime.UtcNow,
                                                                                    AccessControlAllowOrigin   = "*",
                                                                                    AccessControlAllowMethods  = "RESERVE, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                                    Content                    = new JObject(new JProperty("description", "Invalid AuthorizedIds/RFIDId '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                                                                };

                                                                     AuthTokens.Add(AuthToken);

                                                                 }

                                                             }

                                                             #endregion

                                                             #region Check eMAIds       [optional]

                                                             if (AuthorizedIdsJSON.ParseOptional("eMAIds",
                                                                                                 "AuthorizedIds/eMAIds",
                                                                                                 HTTPServer.DefaultServerName,
                                                                                                 out JArray eMAIdsJSON,
                                                                                                 Request,
                                                                                                 out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse == null)
                                                                     return _HTTPResponse;


                                                                 eMobilityAccount_Id eMAId2;

                                                                 foreach (var jtoken in eMAIdsJSON)
                                                                 {

                                                                     if (!eMobilityAccount_Id.TryParse(jtoken.Value<String>(), out eMAId2))
                                                                         return new HTTPResponse.Builder(Request) {
                                                                                    HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                                    Date                       = DateTime.UtcNow,
                                                                                    AccessControlAllowOrigin   = "*",
                                                                                    AccessControlAllowMethods  = "RESERVE, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                                    Content                    = new JObject(new JProperty("description", "Invalid AuthorizedIds/eMAIds '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                                                                };

                                                                     eMAIds.Add(eMAId2);

                                                                 }

                                                             }

                                                             #endregion

                                                             #region Check PINs         [optional]

                                                             if (AuthorizedIdsJSON.ParseOptional("PINs",
                                                                                                 "AuthorizedIds/PINs",
                                                                                                 HTTPServer.DefaultServerName,
                                                                                                 out JArray PINsJSON,
                                                                                                 Request,
                                                                                                 out _HTTPResponse))
                                                             {

                                                                 if (_HTTPResponse == null)
                                                                     return _HTTPResponse;


                                                                 UInt32 PIN = 0;

                                                                 foreach (var jtoken in PINsJSON)
                                                                 {

                                                                     if (!UInt32.TryParse(jtoken.Value<String>(), out PIN))
                                                                         return new HTTPResponse.Builder(Request) {
                                                                                    HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                                    Date                       = DateTime.UtcNow,
                                                                                    AccessControlAllowOrigin   = "*",
                                                                                    AccessControlAllowMethods  = "RESERVE, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                                    Content                    = new JObject(new JProperty("description", "Invalid AuthorizedIds/PINs '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                                                                };

                                                                     PINs.Add(PIN);

                                                                 }

                                                             }

                                                             #endregion

                                                         }

                                                         #endregion

                                                     }

                                                     if (_HTTPResponse                != null &&
                                                         _HTTPResponse.HTTPStatusCode == HTTPStatusCode.BadRequest)
                                                     {
                                                         return _HTTPResponse;
                                                     }

                                                     #endregion


                                                     var result = await RoamingNetwork.
                                                                            Reserve(ChargingLocation.FromEVSEId(EVSE.Id),
                                                                                    ChargingReservationLevel.EVSE,
                                                                                    StartTime,
                                                                                    Duration,
                                                                                    ReservationId,
                                                                                    ProviderId,
                                                                                    RemoteAuthentication.FromRemoteIdentification(eMAId),
                                                                                    ChargingProductId.HasValue
                                                                                            ? new ChargingProduct(ChargingProductId.Value)
                                                                                            : null,
                                                                                    AuthTokens,
                                                                                    eMAIds,
                                                                                    PINs,

                                                                                    Request.Timestamp,
                                                                                    Request.CancellationToken,
                                                                                    Request.EventTrackingId);


                                                     var Now = DateTime.UtcNow;

                                                     #region Success

                                                     if (result.Result == ReservationResultType.Success)
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode             = HTTPStatusCode.Created,
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = Now,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    Location                   = HTTPPath.Parse("~/RNs/" + RoamingNetwork.Id + "/Reservations/" + result.Reservation.Id),
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = new JObject(new JProperty("ReservationId",           result.Reservation.Id.       ToString()),
                                                                                                             new JProperty("StartTime",               result.Reservation.StartTime.ToIso8601()),
                                                                                                             new JProperty("Duration",       (UInt32) result.Reservation.Duration. TotalSeconds)
                                                                                                            ).ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                     #region AlreadInUse

                                                     //else if (result.Result == ReservationResultType.ReservationId_AlreadyInUse)
                                                     //    return new HTTPResponse.Builder(HTTPRequest) {
                                                     //        HTTPStatusCode             = HTTPStatusCode.Conflict,
                                                     //        Server                     = API.HTTPServer.DefaultServerName,
                                                     //        Date                       = DateTime.UtcNow,
                                                     //        AccessControlAllowOrigin   = "*",
                                                     //        AccessControlAllowMethods  = "RESERVE, REMOTESTART, REMOTESTOP, SENDCDR",
                                                     //        AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                     //        ContentType                = HTTPContentType.JSON_UTF8,
                                                     //        Content                    = new JObject(new JProperty("description",  "ReservationId is already in use!")).ToUTF8Bytes(),
                                                     //        Connection                 = "close"
                                                     //    };

                                                     #endregion

                                                     #region ...or fail

                                                     else
                                                        return new HTTPResponse.Builder(Request) {
                                                                   HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                   Server                     = HTTPServer.DefaultServerName,
                                                                   Date                       = Now,
                                                                   AccessControlAllowOrigin   = "*",
                                                                   AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                   AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                   Connection                 = "close"
                                                               };

                                                     #endregion

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region AUTHSTART   ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X AUTHSTART -H "Content-Type: application/json" \
            //                      -H "Accept:       application/json" \
            //      -d "{ \"AuthToken\":  \"00112233\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 AUTHSTART,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendAuthStartEVSERequest,
                                                 HTTPResponseLogger: SendAuthStartEVSEResponse,
                                                 HTTPDelegate: async Request => {

                                                     #region Parse RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  RoamingNetwork,
                                                                                             out EVSE            EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse JSON

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #region Parse OperatorId         [optional]

                                                     ChargingStationOperator_Id OperatorId;

                                                     if (!JSON.ParseOptional("OperatorId",
                                                                             "Charging Station Operator identification",
                                                                             HTTPServer.DefaultServerName,
                                                                             ChargingStationOperator_Id.TryParse,
                                                                             out OperatorId,
                                                                             Request,
                                                                             out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse AuthToken          [mandatory]

                                                     if (!JSON.ParseMandatory("AuthToken",
                                                                              "authentication token",
                                                                              HTTPServer.DefaultServerName,
                                                                              Auth_Token.TryParse,
                                                                              out Auth_Token AuthToken,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse SessionId          [optional]

                                                     if (!JSON.ParseOptionalStruct2("SessionId",
                                                                                   "Charging session identification",
                                                                                   HTTPServer.DefaultServerName,
                                                                                   ChargingSession_Id.TryParse,
                                                                                   out ChargingSession_Id? SessionId,
                                                                                   Request,
                                                                                   out _HTTPResponse))
                                                     {

                                                         return _HTTPResponse;

                                                     }

                                                     #endregion

                                                     #region Parse ChargingProductId  [optional]

                                                     if (!JSON.ParseOptionalStruct2("ChargingProductId",
                                                                                    "Charging product identification",
                                                                                    HTTPServer.DefaultServerName,
                                                                                    ChargingProduct_Id.TryParse,
                                                                                    out ChargingProduct_Id? ChargingProductId,
                                                                                    Request,
                                                                                    out _HTTPResponse))
                                                     {

                                                         return _HTTPResponse;

                                                     }

                                                     #endregion

                                                     #endregion


                                                     var result = await RoamingNetwork.
                                                                            AuthorizeStart(LocalAuthentication.FromAuthToken(AuthToken),
                                                                                           ChargingLocation.FromEVSEId(EVSE.Id),
                                                                                           ChargingProductId.HasValue
                                                                                               ? new ChargingProduct(ChargingProductId.Value)
                                                                                               : null,
                                                                                           SessionId,
                                                                                           OperatorId,

                                                                                           Request.Timestamp,
                                                                                           Request.CancellationToken,
                                                                                           Request.EventTrackingId);


                                                     #region Authorized

                                                     if (result.Result == AuthStartResultTypes.Authorized)
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode             = HTTPStatusCode.OK,
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = DateTime.UtcNow,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = result.ToJSON().ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                     #region NotAuthorized

                                                     else if (result.Result == AuthStartResultTypes.Error)
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = DateTime.UtcNow,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = result.ToJSON().ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                     #region Forbidden

                                                     else
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode             = HTTPStatusCode.Forbidden, //ToDo: Is this smart?
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = DateTime.UtcNow,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = result.ToJSON().ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region AUTHSTOP    ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X AUTHSTOP -H "Content-Type: application/json" \
            //                     -H "Accept:       application/json" \
            //      -d "{ \"SessionId\":  \"60ce73f6-0a88-1296-3d3d-623fdd276ddc\", \
            //            \"AuthToken\":  \"00112233\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 AUTHSTOP,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendAuthStopEVSERequest,
                                                 HTTPResponseLogger: SendAuthStopEVSEResponse,
                                                 HTTPDelegate: async Request => {

                                                     #region Parse RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  RoamingNetwork,
                                                                                             out EVSE            EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse JSON

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #region Parse SessionId    [mandatory]

                                                     ChargingSession_Id SessionId = default(ChargingSession_Id);

                                                     if (!JSON.ParseMandatory("SessionId",
                                                                              "Charging session identification",
                                                                              HTTPServer.DefaultServerName,
                                                                              ChargingSession_Id.TryParse,
                                                                              out SessionId,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse AuthToken    [mandatory]

                                                     if (!JSON.ParseMandatory("AuthToken",
                                                                              "Authentication token",
                                                                              HTTPServer.DefaultServerName,
                                                                              Auth_Token.TryParse,
                                                                              out Auth_Token AuthToken,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse OperatorId   [optional]

                                                     ChargingStationOperator_Id OperatorId;

                                                     if (!JSON.ParseOptional("OperatorId",
                                                                             "Charging Station Operator identification",
                                                                             HTTPServer.DefaultServerName,
                                                                             ChargingStationOperator_Id.TryParse,
                                                                             out OperatorId,
                                                                             Request,
                                                                             out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #endregion


                                                     var result = await RoamingNetwork.
                                                                            AuthorizeStop(SessionId,
                                                                                          LocalAuthentication.FromAuthToken(AuthToken),
                                                                                          ChargingLocation.FromEVSEId(EVSE.Id),
                                                                                          OperatorId,

                                                                                          Request.Timestamp,
                                                                                          Request.CancellationToken,
                                                                                          Request.EventTrackingId);


                                                     #region Authorized

                                                     if (result.Result == AuthStopResultTypes.Authorized)
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode  = HTTPStatusCode.OK,
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = DateTime.UtcNow,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = result.ToJSON().ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                     #region NotAuthorized

                                                     else if (result.Result == AuthStopResultTypes.NotAuthorized)
                                                         return new HTTPResponse.Builder(Request) {
                                                                    HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                                                    Server                     = HTTPServer.DefaultServerName,
                                                                    Date                       = DateTime.UtcNow,
                                                                    AccessControlAllowOrigin   = "*",
                                                                    AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                    AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    ContentType                = HTTPContentType.JSON_UTF8,
                                                                    Content                    = result.ToJSON().ToUTF8Bytes()
                                                                };

                                                     #endregion

                                                     #region Forbidden

                                                     return new HTTPResponse.Builder(Request) {
                                                                HTTPStatusCode             = HTTPStatusCode.Forbidden, //ToDo: Is this smart?
                                                                Server                     = HTTPServer.DefaultServerName,
                                                                Date                       = DateTime.UtcNow,
                                                                AccessControlAllowOrigin   = "*",
                                                                AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                ContentType                = HTTPContentType.JSON_UTF8,
                                                                Content                    = result.ToJSON().ToUTF8Bytes()
                                                            };

                                                     #endregion

                                         }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region REMOTESTART ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X REMOTESTART -H "Content-Type: application/json" \
            //                        -H "Accept:       application/json"  \
            //      -d "{ \"ProviderId\":  \"DE*GDF\", \
            //            \"eMAId\":       \"DE*GDF*00112233*1\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 REMOTESTART,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendRemoteStartEVSERequest,
                                                 HTTPResponseLogger: SendRemoteStartEVSEResponse,
                                                 HTTPDelegate: async Request => {

                                                     #region Get RoamingNetwork and EVSE URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork RoamingNetwork,
                                                                                             out EVSE           EVSE,
                                                                                             out HTTPResponse   _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse JSON  [optional]

                                                     ChargingProduct_Id?      ChargingProductId   = null;
                                                     ChargingReservation_Id?  ReservationId       = null;
                                                     ChargingSession_Id?      SessionId           = null;
                                                     eMobilityProvider_Id?    ProviderId          = null;
                                                     eMobilityAccount_Id      eMAId               = default;

                                                     if (Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                     {

                                                         #region Check ChargingProductId  [optional]

                                                         if (!JSON.ParseOptionalStruct2("ChargingProductId",
                                                                                       "Charging product identification",
                                                                                       HTTPServer.DefaultServerName,
                                                                                       ChargingProduct_Id.TryParse,
                                                                                       out ChargingProductId,
                                                                                       Request,
                                                                                       out _HTTPResponse))
                                                         {

                                                             return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         // MaxKWh
                                                         // MaxPrice

                                                         #region Check ReservationId      [optional]

                                                         if (!JSON.ParseOptionalStruct2("ReservationId",
                                                                                       "Charging reservation identification",
                                                                                       HTTPServer.DefaultServerName,
                                                                                       ChargingReservation_Id.TryParse,
                                                                                       out ReservationId,
                                                                                       Request,
                                                                                       out _HTTPResponse))
                                                         {

                                                             return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Parse SessionId          [optional]

                                                         if (!JSON.ParseOptionalStruct2("SessionId",
                                                                                       "Charging session identification",
                                                                                       HTTPServer.DefaultServerName,
                                                                                       ChargingSession_Id.TryParse,
                                                                                       out SessionId,
                                                                                       Request,
                                                                                       out _HTTPResponse))
                                                         {

                                                             return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Parse ProviderId         [optional]

                                                         if (!JSON.ParseOptionalStruct2("ProviderId",
                                                                                       "EV service provider identification",
                                                                                       HTTPServer.DefaultServerName,
                                                                                       eMobilityProvider_Id.TryParse,
                                                                                       out ProviderId,
                                                                                       Request,
                                                                                       out _HTTPResponse))
                                                         {

                                                             return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Parse eMAId             [mandatory]

                                                         if (!JSON.ParseMandatory("eMAId",
                                                                                  "e-Mobility account identification",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  eMobilityAccount_Id.TryParse,
                                                                                  out eMAId,
                                                                                  Request,
                                                                                  out _HTTPResponse))

                                                             return _HTTPResponse;

                                                         #endregion

                                                     }

                                                     else
                                                         return _HTTPResponse;

                                                     #endregion


                                                     var result = await RoamingNetwork.
                                                                            RemoteStart(ChargingLocation.FromEVSEId(EVSE.Id),
                                                                                        ChargingProductId.HasValue
                                                                                            ? new ChargingProduct(ChargingProductId.Value)
                                                                                            : null,
                                                                                        ReservationId,
                                                                                        SessionId,
                                                                                        ProviderId,
                                                                                        RemoteAuthentication.FromRemoteIdentification(eMAId),
                                                                                      //  null,

                                                                                        Request.Timestamp,
                                                                                        Request.CancellationToken,
                                                                                        Request.EventTrackingId);


                                                     #region Success

                                                     if (result.Result == RemoteStartResultTypes.Success)
                                                         return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.Created,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(
                                                                                                  new JProperty("SessionId",  result.Session.Id.ToString())
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                     #region ...or fail!

                                                     else
                                                         return 
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(
                                                                                                  result.Session.Id != null
                                                                                                      ? new JProperty("SessionId",  result.Session.Id.ToString())
                                                                                                      : null,
                                                                                                  new JProperty("Result",       result.Result.ToString()),
                                                                                                  result.Description != null
                                                                                                      ? new JProperty("Description",  result.Description)
                                                                                                      : null
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region REMOTESTOP  ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X REMOTESTOP -H "Content-Type: application/json" \
            //                       -H "Accept:       application/json"  \
            //      -d "{ \"ProviderId\":  \"DE*8BD\", \
            //            \"SessionId\":   \"60ce73f6-0a88-1296-3d3d-623fdd276ddc\", \
            //            \"eMAId\":       \"DE*GDF*00112233*1\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 REMOTESTOP,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendRemoteStopEVSERequest,
                                                 HTTPResponseLogger: SendRemoteStopEVSEResponse,
                                                 HTTPDelegate: async Request => {

                                                     #region Get RoamingNetwork and EVSE URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork RoamingNetwork,
                                                                                             out EVSE           EVSE,
                                                                                             out HTTPResponse   _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse JSON

                                                     ChargingSession_Id     SessionId   = default(ChargingSession_Id);
                                                     eMobilityProvider_Id?  ProviderId  = null;
                                                     eMobilityAccount_Id?   eMAId       = null;

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON,
                                                                                             out _HTTPResponse,
                                                                                             AllowEmptyHTTPBody: false))

                                                     {

                                                         // Bypass SessionId check for remote safety admins
                                                         // coming from the same ev service provider

                                                         #region Parse SessionId         [mandatory]

                                                         if (!JSON.ParseMandatory("SessionId",
                                                                                  "Charging session identification",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  ChargingSession_Id.TryParse,
                                                                                  out SessionId,
                                                                                  Request,
                                                                                  out _HTTPResponse))

                                                             return _HTTPResponse;

                                                         #endregion

                                                         #region Parse ProviderId         [optional]

                                                         if (!JSON.ParseOptionalStruct2("ProviderId",
                                                                                        "EV service provider identification",
                                                                                        HTTPServer.DefaultServerName,
                                                                                        eMobilityProvider_Id.TryParse,
                                                                                        out ProviderId,
                                                                                        Request,
                                                                                        out _HTTPResponse))
                                                         {

                                                             return _HTTPResponse;

                                                         }

                                                         #endregion

                                                         #region Parse eMAId              [optional]

                                                         if (!JSON.ParseOptionalStruct2("eMAId",
                                                                                       "e-Mobility account identification",
                                                                                       HTTPServer.DefaultServerName,
                                                                                       eMobilityAccount_Id.TryParse,
                                                                                       out eMAId,
                                                                                       Request,
                                                                                       out _HTTPResponse))

                                                             return _HTTPResponse;

                                                         #endregion

                                                         // ReservationHandling

                                                     }

                                                     else
                                                         return _HTTPResponse;

                                                     #endregion


                                                     var result = await RoamingNetwork.RemoteStop(//EVSE.Id,
                                                                                                  SessionId,
                                                                                                  ReservationHandling.Close, // ToDo: Parse this property!
                                                                                                  ProviderId,
                                                                                                  RemoteAuthentication.FromRemoteIdentification(eMAId),

                                                                                                  Request.Timestamp,
                                                                                                  Request.CancellationToken,
                                                                                                  Request.EventTrackingId);


                                                     #region Success

                                                     if (result.Result == RemoteStopResultTypes.Success)
                                                     {

                                                         if (result.ReservationHandling.IsKeepAlive == false)
                                                             return new HTTPResponse.Builder(Request) {
                                                                        HTTPStatusCode             = HTTPStatusCode.NoContent,
                                                                        Server                     = HTTPServer.DefaultServerName,
                                                                        Date                       = DateTime.UtcNow,
                                                                        AccessControlAllowOrigin   = "*",
                                                                        AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                        AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                    };

                                                         else
                                                             return 
                                                                 new HTTPResponse.Builder(Request) {
                                                                     HTTPStatusCode             = HTTPStatusCode.OK,
                                                                     Server                     = HTTPServer.DefaultServerName,
                                                                     Date                       = DateTime.UtcNow,
                                                                     AccessControlAllowOrigin   = "*",
                                                                     AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                     AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                     ContentType                = HTTPContentType.JSON_UTF8,
                                                                     Content                    = new JObject(
                                                                                                      new JProperty("KeepAlive", (Int32) result.ReservationHandling.KeepAliveTime.Value.TotalSeconds)
                                                                                                  ).ToUTF8Bytes()
                                                                 };

                                                     }

                                                     #endregion

                                                     #region ...or fail

                                                     else
                                                         return 
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = new JObject(
                                                                                                  new JProperty("description",  result.Result.ToString())
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region SENDCDR     ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X SENDCDR -H "Content-Type: application/json" \
            //                    -H "Accept: application/json" \
            //      -d "{ \"SessionId\":        \"60ce73f6-0a88-1296-3d3d-623fdd276ddc\", \
            //            \"PartnerProductId\": \"Green Charging 11kWh\", \
            //            \"eMAId\":            \"DE*GDF*00112233*1\" \
            //            \"SessionStart\":     \"2014-08-18T13:12:34.641Z\", \
            //            \"ChargeStart\":      \"2014-08-18T13:12:35.853Z\", \
            //            \"MeterValueStart\":  1200.100, \
            //            \"MeterValueEnd\":    1200.110, \
            //            \"ChargeEnd\":        \"2014-08-18T14:36:11.351Z\", \
            //            \"SessionEnd\":       \"2014-08-18T14:36:12.662Z\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            HTTPServer.AddMethodCallback(Hostname,
                                                 SENDCDR,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  SendCDRsRequest,
                                                 HTTPResponseLogger: SendCDRsResponse,
                                                 HTTPDelegate: async Request => {

                                                     #region Check RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  RoamingNetwork,
                                                                                             out EVSE            EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return _HTTPResponse;
                                                     }

                                                     #endregion

                                                     #region Parse JSON

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #region Parse SessionId          [mandatory]

                                                     if (!JSON.ParseMandatory("SessionId",
                                                                              "charging session identification",
                                                                              HTTPServer.DefaultServerName,
                                                                              ChargingSession_Id.TryParse,
                                                                              out ChargingSession_Id SessionId,
                                                                              Request,
                                                                              out _HTTPResponse))
                                                     {
                                                         return _HTTPResponse;
                                                     }

                                                     #endregion

                                                     #region Parse ChargingProductId

                                                     if (JSON.ParseOptionalStruct2("ChargingProductId",
                                                                                  "charging product identification",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  ChargingProduct_Id.TryParse,
                                                                                  out ChargingProduct_Id? ChargingProductId,
                                                                                  Request,
                                                                                  out _HTTPResponse))
                                                     {

                                                         if (_HTTPResponse != null)
                                                            return _HTTPResponse;

                                                     }

                                                     #endregion

                                                     #region Parse AuthToken or eMAId

                                                     if (JSON.ParseOptional("AuthToken",
                                                                            "authentication token",
                                                                            HTTPServer.DefaultServerName,
                                                                            Auth_Token.TryParse,
                                                                            out Auth_Token AuthToken,
                                                                            Request,
                                                                            out _HTTPResponse))
                                                     {

                                                         if (_HTTPResponse != null)
                                                             return _HTTPResponse;

                                                     }

                                                     if (JSON.ParseOptionalStruct2("eMAId",
                                                                                  "e-mobility account identification",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  eMobilityAccount_Id.TryParse,
                                                                                  out eMobilityAccount_Id? eMAId,
                                                                                  Request,
                                                                                  out _HTTPResponse))
                                                     {

                                                         if (_HTTPResponse != null)
                                                             return _HTTPResponse;

                                                     }


                                                     if (AuthToken == null && eMAId == null)
                                                         return new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                             Server          = HTTPServer.DefaultServerName,
                                                             Date            = DateTime.UtcNow,
                                                             ContentType     = HTTPContentType.JSON_UTF8,
                                                             Content         = new JObject(new JProperty("description", "Missing authentication token or eMAId!")).ToUTF8Bytes()
                                                         };

                                                     #endregion

                                                     #region Parse ChargeStart/End...

                                                     if (!JSON.ParseMandatory("ChargeStart",
                                                                              "Charging start time",
                                                                              HTTPServer.DefaultServerName,
                                                                              out DateTime ChargingStart,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     if (!JSON.ParseMandatory("ChargeEnd",
                                                                              "Charging end time",
                                                                              HTTPServer.DefaultServerName,
                                                                              out DateTime ChargingEnd,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse SessionStart/End...

                                                     if (!JSON.ParseMandatory("SessionStart",
                                                                              "Charging start time",
                                                                              HTTPServer.DefaultServerName,
                                                                              out DateTime SessionStart,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     if (!JSON.ParseMandatory("SessionEnd",
                                                                              "Charging end time",
                                                                              HTTPServer.DefaultServerName,
                                                                              out DateTime SessionEnd,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #region Parse MeterValueStart/End...


                                                     if (!JSON.ParseMandatory("MeterValueStart",
                                                                              "Energy meter start value",
                                                                              HTTPServer.DefaultServerName,
                                                                              out Decimal MeterValueStart,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     if (!JSON.ParseMandatory("MeterValueEnd",
                                                                              "Energy meter end value",
                                                                              HTTPServer.DefaultServerName,
                                                                              out Decimal MeterValueEnd,
                                                                              Request,
                                                                              out _HTTPResponse))

                                                         return _HTTPResponse;

                                                     #endregion

                                                     #endregion


                                                     var _ChargeDetailRecord = new ChargeDetailRecord(Id:                    ChargeDetailRecord_Id.Parse(SessionId.ToString()),
                                                                                                      SessionId:             SessionId,
                                                                                                      EVSEId:                EVSE.Id,
                                                                                                      EVSE:                  EVSE,
                                                                                                      ChargingProduct:       ChargingProductId.HasValue
                                                                                                                                 ? new ChargingProduct(ChargingProductId.Value)
                                                                                                                                 : null,
                                                                                                      SessionTime:           new StartEndDateTime(SessionStart, SessionEnd),
                                                                                                      AuthenticationStart:   AuthToken != null
                                                                                                                                 ? (AAuthentication) LocalAuthentication. FromAuthToken(AuthToken)
                                                                                                                                 : (AAuthentication) RemoteAuthentication.FromRemoteIdentification(eMAId.Value),
                                                                                                      //ChargingTime:        new StartEndDateTime(ChargingStart.Value, ChargingEnd.Value),
                                                                                                      EnergyMeteringValues:  new List<Timestamped<Decimal>>() {
                                                                                                                                 new Timestamped<Decimal>(ChargingStart, MeterValueStart),
                                                                                                                                 new Timestamped<Decimal>(ChargingEnd,   MeterValueEnd)
                                                                                                                            });

                                                     var result = await RoamingNetwork.
                                                                            SendChargeDetailRecords(new ChargeDetailRecord[] { _ChargeDetailRecord },
                                                                                                    TransmissionTypes.Enqueue,

                                                                                                    Request.Timestamp,
                                                                                                    Request.CancellationToken,
                                                                                                    Request.EventTrackingId);


                                                     #region Forwarded

                                                     if (result.Result == SendCDRsResultTypes.Success)
                                                         return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.OK,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(
                                                                                                  new JProperty("Status",          "forwarded"),
                                                                                                  new JProperty("AuthorizatorId",  result.AuthorizatorId.ToString())
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                     #region NotForwared

                                                     else if (result.Result == SendCDRsResultTypes.Error)
                                                         return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.OK,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(
                                                                                                  new JProperty("Status",    "Not forwarded")
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                     #region ...or fail!

                                                     else
                                                         return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.NotFound,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(
                                                                                                  new JProperty("SessionId",       SessionId.ToString()),
                                                                                                  new JProperty("Description",     result.Description),
                                                                                                  new JProperty("AuthorizatorId",  result.AuthorizatorId.ToString())
                                                                                              ).ToUTF8Bytes()
                                                             };

                                                     #endregion

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // --------------------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            // --------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.OPTIONS,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                                                 HTTPDelegate: Request => {

                                                     #region Check RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  _RoamingNetwork,
                                                                                             out EVSE            _EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.NoContent,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, RESERVE, AUTHSTART, AUTHSTOP, REMOTESTART, REMOTESTOP, SENDCDR, OPTIONS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                         }.AsImmutable);

                                                 }, AllowReplacement: URIReplacement.Allow);

            #endregion


            #region SET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus

            // -----------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"CurrentStatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:5500/RNs/EST/EVSEs/DE*GEF*E0001*1/AdminStatus
            // -----------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"CurrentStatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Prod/EVSEs/DE*BDO*EVSE*CI*TESTS*A*1/AdminStatus
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.SET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async Request => {

                                                     #region Parse RoamingNetworkId and EVSEId parameters

                                                     if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                             out RoamingNetwork  RoamingNetwork,
                                                                                             out EVSE            EVSE,
                                                                                             out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return _HTTPResponse;
                                                     }

                                                     #endregion

                                                     #region Parse JSON

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #region Parse CurrentStatus  [optional]

                                                     if (JSON.ParseOptional("CurrentStatus",
                                                                            "EVSE admin status",
                                                                            HTTPServer.DefaultServerName,
                                                                            out EVSEAdminStatusTypes? CurrentStatus,
                                                                            Request,
                                                                            out _HTTPResponse))
                                                     {

                                                         if (_HTTPResponse != null)
                                                             return _HTTPResponse;

                                                     }

                                                     #endregion

                                                     #region Parse StatusList     [optional]

                                                     Timestamped<EVSEAdminStatusTypes>[] StatusList  = null;

                                                     if (JSON.ParseOptional("StatusList",
                                                                            "status list",
                                                                            HTTPServer.DefaultServerName,
                                                                            out JObject JSONStatusList,
                                                                            Request,
                                                                            out _HTTPResponse))
                                                     {

                                                         if (_HTTPResponse != null)
                                                             return _HTTPResponse;

                                                         if (JSONStatusList != null)
                                                         {

                                                             try
                                                             {

                                                                 StatusList = JSONStatusList.
                                                                                  Values<JProperty>().
                                                                                  Select(jproperty => new Timestamped<EVSEAdminStatusTypes>(
                                                                                                          DateTime.Parse(jproperty.Name),
                                                                                                          (EVSEAdminStatusTypes) Enum.Parse(typeof(EVSEAdminStatusTypes), jproperty.Value.ToString())
                                                                                                      )).
                                                                                  OrderBy(status   => status.Timestamp).
                                                                                  ToArray();

                                                             }
                                                             catch (Exception)
                                                             {
                                                                 // Will send the below BadRequest HTTP reply...
                                                             }

                                                         }

                                                         if (JSONStatusList == null || StatusList == null || !StatusList.Any())
                                                             return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 Server          = HTTPServer.DefaultServerName,
                                                                 Date            = DateTime.UtcNow,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = new JObject(
                                                                                       new JProperty("description", "Invalid status list!")
                                                                                   ).ToUTF8Bytes()
                                                             };

                                                     }

                                                     #endregion

                                                     #region Fail, if both CurrentStatus and StatusList are missing...

                                                     if (!CurrentStatus.HasValue && StatusList == null)
                                                     {

                                                         return new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                             Server          = HTTPServer.DefaultServerName,
                                                             Date            = DateTime.UtcNow,
                                                             ContentType     = HTTPContentType.JSON_UTF8,
                                                             Content         = new JObject(
                                                                                   new JProperty("description", "Either a 'CurrentStatus' or a 'StatusList' must be send!")
                                                                               ).ToUTF8Bytes()
                                                         };

                                                     }

                                                     #endregion

                                                     #endregion


                                                     if (StatusList == null)
                                                         StatusList = new Timestamped<EVSEAdminStatusTypes>[] {
                                                                          new Timestamped<EVSEAdminStatusTypes>(Request.Timestamp, CurrentStatus.Value)
                                                                      };

                                                     RoamingNetwork.SetEVSEAdminStatus(EVSE.Id, StatusList);


                                                     return new HTTPResponse.Builder(Request) {
                                                         HTTPStatusCode  = HTTPStatusCode.OK,
                                                         Server          = HTTPServer.DefaultServerName,
                                                         Date            = DateTime.UtcNow,
                                                         Connection      = "close"
                                                     };

                                                 });

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status

            // -----------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"currentStatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*EVSE*ALPHA*ONE*1/Status
            // -----------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"statusList\":  { \
            //              \"2014-10-13T22:14:01.862Z\": \"OutOfService\", \
            //              \"2014-10-13T21:32:15.386Z\": \"Charging\"  \
            //          }" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*EVSE*ALPHA*ONE*1/Status
            // -----------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"currentStatus\":  \"Charging\" }"     \
            //      http://127.0.0.1:3004/RNs/Prod/EVSEs/DE*BDO*EVSE*CI*TESTS*A*1/Status
            HTTPServer.AddMethodCallback(Hostname,
                                         HTTPMethod.SET,
                                         URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status",
                                         HTTPContentType.JSON_UTF8,
                                         HTTPDelegate: async Request => {

                                             #region Check RoamingNetworkId and EVSEId URI parameters

                                             if (!Request.ParseRoamingNetworkAndEVSE(this,
                                                                                     out RoamingNetwork  RoamingNetwork,
                                                                                     out EVSE            EVSE,
                                                                                     out HTTPResponse    _HTTPResponse))
                                             {
                                                 return _HTTPResponse;
                                             }

                                             #endregion

                                             #region Parse JSON

                                             if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                 return _HTTPResponse;

                                             #region Parse Current status  [optional]

                                             if (JSON.ParseOptional("currentStatus",
                                                                    "EVSE status",
                                                                    HTTPServer.DefaultServerName,
                                                                    out EVSEStatusTypes? CurrentStatus,
                                                                    Request,
                                                                    out _HTTPResponse))
                                             {

                                                 if (_HTTPResponse != null)
                                                     return _HTTPResponse;

                                             }

                                             #endregion

                                             #region Parse Status list     [optional]

                                             Timestamped<EVSEStatusTypes>[] StatusList = null;

                                             if (JSON.ParseOptional("statusList",
                                                                    "status list",
                                                                    HTTPServer.DefaultServerName,
                                                                    out JObject JSONStatusList,
                                                                    Request,
                                                                    out _HTTPResponse))
                                             {

                                                 if (_HTTPResponse != null)
                                                     return _HTTPResponse;

                                                 if (JSONStatusList != null)
                                                 {

                                                     try
                                                     {

                                                         StatusList = JSONStatusList.
                                                                          Values<JProperty>().
                                                                          Select(jproperty => new Timestamped<EVSEStatusTypes>(
                                                                                                  DateTime.Parse(jproperty.Name),
                                                                                                  (EVSEStatusTypes) Enum.Parse(typeof(EVSEStatusTypes), jproperty.Value.ToString())
                                                                                              )).
                                                                          OrderBy(status   => status.Timestamp).
                                                                          ToArray();

                                                     }
                                                     catch (Exception)
                                                     {
                                                         // Will send the below BadRequest HTTP reply...
                                                     }

                                                 }

                                                 if (JSONStatusList == null || StatusList == null || !StatusList.Any())
                                                     return new HTTPResponse.Builder(Request) {
                                                         HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                         Server          = HTTPServer.DefaultServerName,
                                                         Date            = DateTime.UtcNow,
                                                         ContentType     = HTTPContentType.JSON_UTF8,
                                                         Content         = new JObject(
                                                                             new JProperty("description", "Invalid status list!")
                                                                         ).ToUTF8Bytes()
                                                     };

                                             }

                                             #endregion

                                             #region Fail, if both CurrentStatus and StatusList are missing...

                                             if (!CurrentStatus.HasValue && StatusList == null)
                                             {

                                                 return new HTTPResponse.Builder(Request) {
                                                     HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                     Server          = HTTPServer.DefaultServerName,
                                                     Date            = DateTime.UtcNow,
                                                     ContentType     = HTTPContentType.JSON_UTF8,
                                                     Content         = new JObject(
                                                                         new JProperty("description", "Either a 'currentStatus' or a 'statusList' must be send!")
                                                                     ).ToUTF8Bytes()
                                                 };

                                             }

                                             #endregion

                                             #endregion


                                             if (StatusList == null)
                                             {
                                                 if (RoamingNetwork.TryGetEVSEById(EVSE.Id, out EVSE evse))
                                                 {
                                                     evse.Status = new Timestamped<EVSEStatusTypes>(Request.Timestamp,
                                                                                                    CurrentStatus.Value);
                                                 }
                                             }

                                             else
                                                 RoamingNetwork.SetEVSEStatus(EVSE.Id,
                                                                              StatusList);


                                             return new HTTPResponse.Builder(Request) {
                                                 HTTPStatusCode  = HTTPStatusCode.OK,
                                                 Server          = HTTPServer.DefaultServerName,
                                                 Date            = DateTime.UtcNow,
                                                 Connection      = "close"
                                             };

                                         });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/chargingSessions

            #region GET         ~/RNs/{RoamingNetworkId}/chargingSessions

            // ---------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/chargingSessions
            // ---------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/chargingSessions",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Get HTTP user and its organizations

                                                     // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                                                     if (!TryGetHTTPUser(Request,
                                                                         out User                   HTTPUser,
                                                                         out HashSet<Organization>  HTTPOrganizations,
                                                                         out HTTPResponse           Response,
                                                                         Recursive:                 true))
                                                     {
                                                         return Task.FromResult(Response);
                                                     }

                                                     #endregion

                                                     #region Get roaming network

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork  RoamingNetwork,
                                                                                      out HTTPResponse    HTTPResponse))
                                                     {
                                                         return Task.FromResult(HTTPResponse);
                                                     }

                                                     #endregion

                                                     //ToDo: Filter sessions by HTTPUser organization!

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");

                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses      = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;


                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount          = RoamingNetwork.ChargingSessions.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = RoamingNetwork.ChargingSessions.
                                                                                                 OrderBy(session => session.Id).
                                                                                                 ToJSON (skip,
                                                                                                         take,
                                                                                                         false).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/chargingSessions->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Prod/ChargingSessions->Id
            // -------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/chargingSessions->Id",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Get HTTP user and its organizations

                                                     // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                                                     if (!TryGetHTTPUser(Request,
                                                                         out User                   HTTPUser,
                                                                         out HashSet<Organization>  HTTPOrganizations,
                                                                         out HTTPResponse           Response,
                                                                         Recursive:                 true))
                                                     {
                                                         return Task.FromResult(Response);
                                                     }

                                                     #endregion

                                                     #region Get roaming network

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork  RoamingNetwork,
                                                                                      out HTTPResponse    HTTPResponse))
                                                     {
                                                         return Task.FromResult(HTTPResponse);
                                                     }

                                                     #endregion


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = new JArray(RoamingNetwork.ChargingSessions.
                                                                                                            Select(seession => seession.Id.ToString()).
                                                                                                            Skip  (Request.QueryString.GetUInt64("skip")).
                                                                                                            Take  (Request.QueryString.GetUInt64("take"))).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = RoamingNetwork.ChargingSessions.ULongCount(),
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/chargingSessions/{ChargingSession_Id}

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/chargingSessions/{ChargingSession_Id}
            // -----------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/chargingSessions/{ChargingSession_Id}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Get HTTP user and its organizations

                                                     // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                                                     if (!TryGetHTTPUser(Request,
                                                                         out User                   HTTPUser,
                                                                         out HashSet<Organization>  HTTPOrganizations,
                                                                         out HTTPResponse           Response,
                                                                         Recursive:                 true))
                                                     {
                                                         return Task.FromResult(Response);
                                                     }

                                                     #endregion

                                                     #region Get roaming network and charging session

                                                     if (!Request.ParseRoamingNetworkAndChargingSession(this,
                                                                                                        out RoamingNetwork_Id?   RoamingNetworkId,
                                                                                                        out RoamingNetwork       RoamingNetwork,
                                                                                                        out ChargingSession_Id?  ChargingSessionId,
                                                                                                        out ChargingSession      ChargingSession,
                                                                                                        out HTTPResponse         HTTPResponse))
                                                     {
                                                         return Task.FromResult(HTTPResponse);
                                                     }

                                                     #endregion

                                                     //ToDo: Filter sessions by HTTPUser organization!

                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses      = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET, COUNT, OPTIONS",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = ChargingSession.
                                                                                                 ToJSON(false).
                                                                                                 ToUTF8Bytes(),
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/ChargeDetailRecords

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargeDetailRecords/{ChargeDetailRecordId}

            #endregion

            #region ~/RNs/{RoamingNetworkId}/Reservations

            #region GET         ~/RNs/{RoamingNetworkId}/Reservations

            // ----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // ----------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "/RNs/{RoamingNetworkId}/Reservations",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async HTTPRequest => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (HTTPRequest.Authorization          == null        ||
                                                     //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                                                     //    HTTPRequest.Authorization.Password != HTTPPassword)
                                                     //    return new HTTPResponse.Builder(HTTPRequest) {
                                                     //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                                                     //        Server           = _API.HTTPServer.DefaultServerName,
                                                     //        Date             = DateTime.UtcNow,
                                                     //        Connection       = "close"
                                                     //    };

                                                     #endregion

                                                     #region Check parameters

                                                     if (!HTTPRequest.ParseRoamingNetwork(this, out RoamingNetwork RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     var skip                   = HTTPRequest.QueryString.GetUInt64("skip");
                                                     var take                   = HTTPRequest.QueryString.GetUInt32("take");

                                                     var _ChargingReservations  = RoamingNetwork.
                                                                                      ChargingReservations.
                                                                                      OrderBy(reservation => reservation.Id.ToString()).
                                                                                      Skip(skip).
                                                                                      Take(take).
                                                                                      ToArray();

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount         = RoamingNetwork.ChargingReservations.LongCount();

                                                     return new HTTPResponse.Builder(HTTPRequest) {
                                                         HTTPStatusCode             = _ChargingReservations.Any()
                                                                                          ? HTTPStatusCode.OK
                                                                                          : HTTPStatusCode.NoContent,
                                                         Server                     = HTTPServer.DefaultServerName,
                                                         Date                       = DateTime.UtcNow,
                                                         AccessControlAllowOrigin   = "*",
                                                         AccessControlAllowMethods  = "GET",
                                                         AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                         ETag                       = "1",
                                                         ContentType                = HTTPContentType.JSON_UTF8,
                                                         Content                    = (_ChargingReservations.Any()
                                                                                          ? _ChargingReservations.ToJSON()
                                                                                          : new JArray()
                                                                                      ).ToUTF8Bytes()
                                                     }.Set(new HTTPHeaderField("X-ExpectedTotalNumberOfItems", typeof(UInt64), HeaderFieldType.Response, RequestPathSemantic.EndToEnd),
                                                                               _ExpectedCount);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            #region GET         ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async HTTPRequest => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (HTTPRequest.Authorization          == null        ||
                                                     //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                                                     //    HTTPRequest.Authorization.Password != HTTPPassword)
                                                     //    return new HTTPResponse.Builder(HTTPRequest) {
                                                     //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                                                     //        Server           = _API.HTTPServer.DefaultServerName,
                                                     //        Date             = DateTime.UtcNow,
                                                     //        Connection       = "close"
                                                     //    };

                                                     #endregion

                                                     #region Check ChargingReservationId parameter

                                                     if (!HTTPRequest.ParseRoamingNetworkAndReservation(this,
                                                                                                        out RoamingNetwork       RoamingNetwork,
                                                                                                        out ChargingReservation  Reservation,
                                                                                                        out HTTPResponse         _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion

                                                     return new HTTPResponse.Builder(HTTPRequest) {
                                                         HTTPStatusCode             = HTTPStatusCode.OK,
                                                         Server                     = HTTPServer.DefaultServerName,
                                                         Date                       = DateTime.UtcNow,
                                                         AccessControlAllowOrigin   = "*",
                                                         AccessControlAllowMethods  = "GET, SETEXPIRED, DELETE",
                                                         AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                         ETag                       = "1",
                                                         ContentType                = HTTPContentType.JSON_UTF8,
                                                         Content                    = Reservation.ToJSON().ToUTF8Bytes()
                                                     };

                                                 });

            #endregion

            #region SETEXPIRED  ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 SETEXPIRED,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async Request => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (HTTPRequest.Authorization          == null        ||
                                                     //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                                                     //    HTTPRequest.Authorization.Password != HTTPPassword)
                                                     //    return new HTTPResponse.Builder(HTTPRequest) {
                                                     //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                                                     //        Server           = _API.HTTPServer.DefaultServerName,
                                                     //        Date             = DateTime.UtcNow,
                                                     //        Connection       = "close"
                                                     //    };

                                                     #endregion

                                                     #region Check ChargingReservationId parameter

                                                     if (!Request.ParseRoamingNetworkAndReservation(this,
                                                                                                    out RoamingNetwork       RoamingNetwork,
                                                                                                    out ChargingReservation  Reservation,
                                                                                                    out HTTPResponse         _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion


                                                     var response = RoamingNetwork.CancelReservation(Reservation.Id,
                                                                                                     ChargingReservationCancellationReason.Deleted,
                                                                                                  //   null, //ToDo: Refacor me to make use of the ProviderId!
                                                                                                 //    null,

                                                                                                     Request.Timestamp,
                                                                                                     Request.CancellationToken,
                                                                                                     Request.EventTrackingId,
                                                                                                     TimeSpan.FromSeconds(60)).Result;

                                                     switch (response.Result)
                                                     {

                                                         case CancelReservationResultTypes.Success:
                                                             return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.OK,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, SETEXPIRED, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(new JProperty("en", "Reservation removed. Additional costs may be charged!")).ToUTF8Bytes()
                                                             };

                                                         default:
                                                             return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.InternalServerError,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(new JProperty("description", "Could not remove reservation!")).ToUTF8Bytes()
                                                             };

                                                     }

                                                 });

            #endregion

            #region DELETE      ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.DELETE,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async Request => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (HTTPRequest.Authorization          == null        ||
                                                     //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                                                     //    HTTPRequest.Authorization.Password != HTTPPassword)
                                                     //    return new HTTPResponse.Builder(HTTPRequest) {
                                                     //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                                                     //        Server           = _API.HTTPServer.DefaultServerName,
                                                     //        Date             = DateTime.UtcNow,
                                                     //        Connection       = "close"
                                                     //    };

                                                     #endregion

                                                     #region Check ChargingReservationId parameter

                                                     if (!Request.ParseRoamingNetworkAndReservation(this,
                                                                                                        out RoamingNetwork       RoamingNetwork,
                                                                                                        out ChargingReservation  Reservation,
                                                                                                        out HTTPResponse         _HTTPResponse))
                                                         return _HTTPResponse;

                                                     #endregion


                                                     var response = RoamingNetwork.CancelReservation(Reservation.Id,
                                                                                                     ChargingReservationCancellationReason.Deleted,
                                                                                                 //    null, //ToDo: Refacor me to make use of the ProviderId!
                                                                                                 //    null,

                                                                                                     Request.Timestamp,
                                                                                                     Request.CancellationToken,
                                                                                                     Request.EventTrackingId,
                                                                                                     TimeSpan.FromSeconds(60)).Result;

                                                     switch (response.Result)
                                                     {

                                                         case CancelReservationResultTypes.Success:
                                                             return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.OK,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, SETEXPIRED, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(new JProperty("en", "Reservation removed. Additional costs may be charged!")).ToUTF8Bytes()
                                                             };

                                                         default:
                                                             return new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode             = HTTPStatusCode.InternalServerError,
                                                                 Server                     = HTTPServer.DefaultServerName,
                                                                 Date                       = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin   = "*",
                                                                 AccessControlAllowMethods  = "GET, DELETE",
                                                                 AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                                 ContentType                = HTTPContentType.JSON_UTF8,
                                                                 Content                    = JSONObject.Create(new JProperty("description", "Could not remove reservation!")).ToUTF8Bytes()
                                                             };

                                                     }

                                                 });

            #endregion

            #endregion



            #region ~/RNs/{RNId}/ChargingStationOperators

            #region GET         ~/RNs/{RNId}/ChargingStationOperators

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators
            // -----------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandRoamingNetworkId    = expand.ContainsIgnoreCase("network")           ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("chargingpools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("chargingstations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("EVSEs")             ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandBrandIds            = expand.ContainsIgnoreCase("brands")            ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenses        = expand.ContainsIgnoreCase("licenses")          ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.ChargingStationOperators.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _RoamingNetwork.ChargingStationOperators.
                                                                                                OrderBy(cso => cso.Id).
                                                                                                ToJSON(skip,
                                                                                                       take,
                                                                                                       false,
                                                                                                       expandRoamingNetworkId,
                                                                                                       expandChargingPoolIds,
                                                                                                       expandChargingStationIds,
                                                                                                       expandEVSEIds,
                                                                                                       expandBrandIds,
                                                                                                       expandDataLicenses).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs/{RNId}/ChargingStationOperators

            // ----------------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RNId}/ChargingStationOperators
            // ----------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  _RoamingNetwork.ChargingStationOperators.ULongCount())
                                                                                            ).ToUTF8Bytes(),
                                                             Connection                   = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RNId}/ChargingStationOperators->AdminStatus

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators->AdminStatus
            // -----------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip         = Request.QueryString.GetUInt64("skip");
                                                     var take         = Request.QueryString.GetUInt64("take");
                                                     var historysize  = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorAdminStatus.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _RoamingNetwork.ChargingStationOperatorAdminStatus.
                                                                                                 OrderBy(kvp => kvp.Key).
                                                                                                 ToJSON (skip,
                                                                                                         take,
                                                                                                         historysize).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RNId}/ChargingStationOperators->Status

            // -------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators->Status
            // -------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip         = Request.QueryString.GetUInt64("skip");
                                                     var take         = Request.QueryString.GetUInt64("take");
                                                     var historysize  = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorStatus.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                = HTTPStatusCode.OK,
                                                             Server                        = HTTPServer.DefaultServerName,
                                                             Date                          = DateTime.UtcNow,
                                                             AccessControlAllowOrigin      = "*",
                                                             AccessControlAllowMethods     = "GET",
                                                             AccessControlAllowHeaders     = "Content-Type, Accept, Authorization",
                                                             ETag                          = "1",
                                                             ContentType                   = HTTPContentType.JSON_UTF8,
                                                             Content                       = _RoamingNetwork.ChargingStationOperatorStatus.
                                                                                                 OrderBy(kvp => kvp.Key).
                                                                                                 ToJSON (skip,
                                                                                                         take,
                                                                                                         historysize).
                                                                                                 ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount,
                                                             Connection                    = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}

            HTTPServer.AddMethodCallback(HTTPHostname.Any,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork          _RoamingNetwork,
                                                                                                                out ChargingStationOperator _ChargingStationOperator,
                                                                                                                out HTTPResponse            _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode              = HTTPStatusCode.OK,
                                                             Server                      = HTTPServer.DefaultServerName,
                                                             Date                        = DateTime.UtcNow,
                                                             AccessControlAllowOrigin    = "*",
                                                             AccessControlAllowMethods   = "GET, CREATE, DELETE",
                                                             AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                             ETag                        = "1",
                                                             ContentType                 = HTTPContentType.JSON_UTF8,
                                                             Content                     = _ChargingStationOperator.ToJSON().ToUTF8Bytes(),
                                                             Connection                  = "close"
                                                         }.AsImmutable);

                                           });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools

            // -----------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators/{CSOId}/ChargingPools
            // -----------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     var expandChargingStations  = !expand.ContainsIgnoreCase("-chargingstations") ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                                                     var expandRoamingNetworks   =  expand.ContainsIgnoreCase("networks")          ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandOperators         =  expand.ContainsIgnoreCase("operators")         ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                                                     var expandBrands            =  expand.ContainsIgnoreCase("brands")            ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = RoamingNetwork.ChargingPools.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = RoamingNetwork.ChargingPools.
                                                                                                  ToJSON(skip,
                                                                                                         take,
                                                                                                         false,
                                                                                                         expandRoamingNetworks,
                                                                                                         expandOperators,
                                                                                                         expandChargingStations,
                                                                                                         expandBrands).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region CREATE      ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}

            // ----------------------------------------------------------------------------------------------------------------
            // curl -v -X CREATE -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test2/ChargingPools/{ChargingPoolId}
            // ----------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.CREATE,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingPools/{ChargingPoolId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPRequestLogger:  CreateChargingPoolRequest,
                                                 HTTPResponseLogger: CreateChargingPoolResponse,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP Basic Authentication

                                                     //if (Request.Authorization          == null      ||
                                                     //    Request.Authorization.Username != HTTPLogin ||
                                                     //    Request.Authorization.Password != HTTPPassword)
                                                     //    return SendEVSEStatusSetted(
                                                     //        new HTTPResponse.Builder(Request) {
                                                     //            HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                                                     //            WWWAuthenticate  = @"Basic realm=""WWCP""",
                                                     //            Server           = HTTPServer.DefaultServerName,
                                                     //            Date             = DateTime.UtcNow,
                                                     //            Connection       = "close"
                                                     //        });

                                                     #endregion

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingPool(this,
                                                                                                     out RoamingNetwork _RoamingNetwork,
                                                                                                     out ChargingPool   _ChargingPool,
                                                                                                     out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     #region Parse optional JSON

                                                     I18NString DescriptionI18N = null;

                                                     if (Request.TryParseJObjectRequestBody(out JObject JSON,
                                                                                            out _HTTPResponse,
                                                                                            AllowEmptyHTTPBody: true))
                                                     {

                                                         if (!JSON.ParseOptional("description",
                                                                                 "description",
                                                                                 HTTPServer.DefaultServerName,
                                                                                 out DescriptionI18N,
                                                                                 Request,
                                                                                 out _HTTPResponse))
                                                         {
                                                             return Task.FromResult(_HTTPResponse);
                                                         }

                                                     }

                                                     #endregion


                                                     _RoamingNetwork = CreateNewRoamingNetwork(Request.Host,
                                                                                               _RoamingNetwork.Id,
                                                                                               I18NString.Empty,
                                                                                               Description: DescriptionI18N ?? I18NString.Empty);


                                                     return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode              = HTTPStatusCode.Created,
                                                                 Server                      = HTTPServer.DefaultServerName,
                                                                 Date                        = DateTime.UtcNow,
                                                                 AccessControlAllowOrigin    = "*",
                                                                 AccessControlAllowMethods   = "GET, CREATE, DELETE",
                                                                 AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                                 ETag                        = "1",
                                                                 ContentType                 = HTTPContentType.JSON_UTF8,
                                                                 Content                     = _RoamingNetwork.ToJSON().ToUTF8Bytes(),
                                                                 Connection                  = "close"
                                                             }.AsImmutable);

                                                 });

            #endregion

            #region SET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}/AdminStatus

            // ------------------------------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*P000001*1/AdminStatus
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.SET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}/AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check RoamingNetworkId and EVSEId URI parameters

                                                     if (!Request.ParseRoamingNetwork(this,
                                                                                      out RoamingNetwork _RoamingNetwork,
                                                                                      out HTTPResponse   _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     #region Parse ChargingPoolId

                                                     if (!ChargingPool_Id.TryParse(Request.ParsedURIParameters[1],
                                                                                   out ChargingPool_Id ChargingPoolId))
                                                     {

                                                         //Log.Timestamp("Bad request: Invalid ChargingPoolId query parameter!");

                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = new JObject(//new JProperty("@context",    "http://wwcp.graphdefined.org/contexts/BadRequest.jsonld"),
                                                                                               new JProperty("description", "Invalid ChargingPoolId query parameter!")).ToUTF8Bytes()
                                                             }.AsImmutable);

                                                     }

                                                     if (!_RoamingNetwork.ContainsChargingPool(ChargingPoolId))
                                                     {

                                                         //Log.Timestamp("Bad request: Unknown ChargingPoolId query parameter!");

                                                         return Task.FromResult(
                                                             new HTTPResponse.Builder(Request) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = new JObject(//new JProperty("@context",    "http://wwcp.graphdefined.org/contexts/BadRequest.jsonld"),
                                                                                               new JProperty("description", "Unknown ChargingPoolId query parameter!")).ToUTF8Bytes()
                                                             }.AsImmutable);

                                                     }

                                                     #endregion

                                                     #region Parse JSON and new charging pool admin status

                                                     if (!Request.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     if (!JSON.ParseMandatoryEnum("newstatus",
                                                                                  "charging pool admin status",
                                                                                  HTTPServer.DefaultServerName,
                                                                                  out ChargingPoolAdminStatusTypes NewChargingPoolAdminStatus,
                                                                                  Request,
                                                                                  out _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     //Log.WriteLine("SetChargingPoolAdminStatus : " + RoamingNetwork.Id + " / " + ChargingPoolId + " => " + NewChargingPoolAdminStatus);

                                                     HTTPServer.Get<JObject>(DebugLogId).
                                                         SubmitEvent("SetChargingPoolAdminStatusRequest",
                                                                     new JObject(
                                                                         new JProperty("Timestamp",       DateTime.UtcNow.ToIso8601()),
                                                                         new JProperty("RoamingNetwork",  _RoamingNetwork.ToString()),
                                                                         new JProperty("ChargingPoolId",  ChargingPoolId.ToString()),
                                                                         new JProperty("NewStatus",       NewChargingPoolAdminStatus.ToString())
                                                                     )).Wait();


                                                     _RoamingNetwork.ChargingStationOperators.ForEach(evseoperator => {

                                                         if (evseoperator.ContainsChargingPool(ChargingPoolId))
                                                             evseoperator.SetChargingPoolAdminStatus(ChargingPoolId, new Timestamped<ChargingPoolAdminStatusTypes>(NewChargingPoolAdminStatus), SendUpstream: true);

                                                     });

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode  = HTTPStatusCode.OK,
                                                             Date            = DateTime.UtcNow,
                                                             Connection      = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}

            HTTPServer.AddMethodCallback(HTTPHostname.Any,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStation(this,
                                                                                                        out RoamingNetwork   _RoamingNetwork,
                                                                                                        out ChargingStation  _ChargingStation,
                                                                                                        out HTTPResponse     _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode              = HTTPStatusCode.OK,
                                                             Server                      = HTTPServer.DefaultServerName,
                                                             Date                        = DateTime.UtcNow,
                                                             AccessControlAllowOrigin    = "*",
                                                             AccessControlAllowMethods   = "GET, SET",
                                                             AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                             ETag                        = "1",
                                                             ContentType                 = HTTPContentType.JSON_UTF8,
                                                             Content                     = _ChargingStation.ToJSON().ToUTF8Bytes(),
                                                             Connection                  = "close"
                                                         }.AsImmutable);

                                           });

            #endregion

            #region SET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}/AdminStatus

            // ------------------------------------------------------------------------------------------
            //
            // ==== QA1 ===============================================================================
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/49*822*S013361835
            //
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/49*822*S013361835
            //
            // ==== PROD ==============================================================================
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*S000001*1
            //
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*S000001*1
            //
            // ------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.SET,
                                                 URLPathPrefix + "/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}/AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: async HTTPRequest => {

                                                     #region Parse Query/Request Parameters

                                                     if (!HTTPRequest.ParseRoamingNetwork(this,
                                                                                          out RoamingNetwork  RoamingNetwork,
                                                                                          out HTTPResponse    _HTTPResponse))
                                                     {
                                                         return _HTTPResponse;
                                                     }

                                                     var EventTrackingId = EventTracking_Id.New;

                                                     ChargingStationAdminStatusTypes NewChargingStationAdminStatus;
                                                     ChargingStation_Id ChargingStationId;

                                                     try
                                                     {

                                                         #region Parse ChargingStationId

                                                         if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURIParameters[1], out ChargingStationId))
                                                             return new HTTPResponse.Builder(HTTPRequest) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = new JObject(new JProperty("Description", "Invalid ChargingStationId query parameter!")).ToUTF8Bytes()
                                                             };

                                                         if (!RoamingNetwork.ContainsChargingStation(ChargingStationId))
                                                             return new HTTPResponse.Builder(HTTPRequest) {
                                                                 HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                 ContentType     = HTTPContentType.JSON_UTF8,
                                                                 Content         = new JObject(new JProperty("Description", "Unknown ChargingStationId query parameter!")).ToUTF8Bytes()
                                                             };

                                                         #endregion

                                                         if (!HTTPRequest.TryParseJObjectRequestBody(out JObject JSON, out _HTTPResponse))
                                                             return _HTTPResponse;

                                                         #region Parse newstatus

                                                         if (!JSON.ParseMandatoryEnum("newstatus",
                                                                                      "charging station admin status",
                                                                                      HTTPServer.DefaultServerName,
                                                                                      out NewChargingStationAdminStatus,
                                                                                      HTTPRequest,
                                                                                      out _HTTPResponse))
                                                         {
                                                             return _HTTPResponse;
                                                         }

                                                         #endregion

                                                     }
                                                     catch (Exception e)
                                                     {

                                                         return new HTTPResponse.Builder(HTTPRequest) {
                                                                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                                                    ContentType     = HTTPContentType.JSON_UTF8,
                                                                    Content         = new JObject(new JProperty("Description", "An exception occured: " + e.Message)).ToUTF8Bytes()
                                                                };

                                                     }

                                                     #endregion

                                                     try
                                                     {

                                                         HTTPServer.Get<JObject>(DebugLogId).
                                                             SubmitEvent("SetChargingStationAdminStatusRequest",
                                                                         new JObject(
                                                                             new JProperty("Timestamp",          DateTime.UtcNow.ToIso8601()),
                                                                             new JProperty("RoamingNetwork",     RoamingNetwork.ToString()),
                                                                             new JProperty("ChargingStationId",  ChargingStationId.ToString()),
                                                                             new JProperty("NewStatus",          NewChargingStationAdminStatus.ToString())
                                                                         )).Wait();


                                                         RoamingNetwork.SetChargingStationAdminStatus(ChargingStationId,
                                                                                                      new Timestamped<ChargingStationAdminStatusTypes>[1] {
                                                                                                          new Timestamped<ChargingStationAdminStatusTypes>(NewChargingStationAdminStatus)
                                                                                                      });

                                                         //GetEventSource(Semantics.DebugLog).
                                                         //        SubmitSubEvent("AUTHSTARTResponse",
                                                         //                       new JObject(
                                                         //                           new JProperty("Timestamp",         DateTime.UtcNow.ToIso8601()),
                                                         //                           new JProperty("RoamingNetwork",    RoamingNetwork.ToString()),
                                                         //                           new JProperty("SessionId",         AuthStartResult.SessionId.ToString()),
                                                         //                           new JProperty("PartnerSessionId",  PartnerSessionId.ToString()),
                                                         //                           new JProperty("ProviderId",        AuthStartResult.ProviderId.ToString()),
                                                         //                           new JProperty("AuthorizatorId",    AuthStartResult.AuthorizatorId.ToString()),
                                                         //                           new JProperty("Description",       "Authorized")
                                                         //                       ).ToString().
                                                         //                         Replace(Environment.NewLine, ""));


                                                         return new HTTPResponse.Builder(HTTPRequest) {
                                                             HTTPStatusCode  = HTTPStatusCode.OK,
                                                             Date            = DateTime.UtcNow,
                                                             ContentType     = HTTPContentType.JSON_UTF8,
                                                             Content         = new JObject(
                                                                                   new JProperty("Description",  "Ok")
                                                                               ).ToString().
                                                                                 Replace(Environment.NewLine, "").
                                                                                 ToUTF8Bytes(),
                                                             Connection      = "close"
                                                         };

                                                     }

                                                     #region Catch errors...

                                                     catch (Exception e)
                                                     {

                                                         //Log.Timestamp("Exception occured: " + e.Message);

                                                         return new HTTPResponse.Builder(HTTPRequest) {
                                                             HTTPStatusCode  = HTTPStatusCode.InternalServerError,
                                                             ContentType     = HTTPContentType.JSON_UTF8,
                                                             Content         = new JObject(new JProperty("@context",           "http://wwcp.graphdefined.org/contexts/SETSTATUS-Response.jsonld"),
                                                                                           new JProperty("RoamingNetwork",     RoamingNetwork.ToString()),
                                                                                           new JProperty("ChargingStationId",  ChargingStationId.ToString()),
                                                                                           new JProperty("Description",        "An exception occured: " + e.Message)).
                                                                                           ToString().ToUTF8Bytes()
                                                         };

                                                     }

                                                     #endregion

                                                 });

            #endregion

            #endregion



            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/ChargingStationGroups
            // ----------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork           RoamingNetwork,
                                                                                                                out ChargingStationOperator  ChargingStationOperator,
                                                                                                                out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount            = ChargingStationOperator.ChargingStationGroups.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = ChargingStationOperator.ChargingStationGroups.
                                                                                                  ToJSON(skip,
                                                                                                         take,
                                                                                                         false,
                                                                                                         expandChargingPoolIds,
                                                                                                         expandChargingStationIds,
                                                                                                         expandEVSEIds,
                                                                                                         expandDataLicenseIds).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this,
                                                                                                                                       out RoamingNetwork           RoamingNetwork,
                                                                                                                                       out ChargingStationOperator  ChargingStationOperator,
                                                                                                                                       out ChargingStationGroup     ChargingStationGroup,
                                                                                                                                       out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                       = Request.QueryString.GetUInt64("skip");
                                                     var take                       = Request.QueryString.GetUInt64("take");
                                                     var include                    = Request.QueryString.GetStrings("include");
                                                     var expand                     = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds      = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandChargingStationIds   = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandEVSEIds              = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandDataLicenseIds       = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                                                     var ChargingStationGroupJSON   = ChargingStationGroup.ToJSON(false,
                                                                                                                  expandChargingPoolIds,
                                                                                                                  expandChargingStationIds,
                                                                                                                  expandEVSEIds,
                                                                                                                  expandDataLicenseIds);

                                                     #region Include charging pools

                                                     //if (expandChargingPoolIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var pools  = ChargingStationOperator.
                                                     //                     ChargingPools.
                                                     //                     Where(pool => pool.ChargingStationGroup == ChargingStationGroup).
                                                     //                     ToArray();

                                                     //    if (pools.Length > 0)
                                                     //       ChargingStationGroupJSON["chargingPoolIds"] = new JArray(pools.Select(pool => pool.Id.ToString()));

                                                     //}

                                                     //else if (expandChargingPoolIds == InfoStatus.Expanded) {

                                                     //    var pools  = ChargingStationOperator.
                                                     //                     ChargingPools.
                                                     //                     Where(pool => pool.ChargingStationGroup == ChargingStationGroup).
                                                     //                     ToArray();

                                                     //    if (pools.Length > 0)
                                                     //        ChargingStationGroupJSON["chargingPools"]  = new JArray(pools.Select(pool => pool.ToJSON(Embedded:                        true,
                                                     //                                                                                  ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                                  ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                     //                                                                                  ExpandChargingStationIds:        InfoStatus.Hidden,
                                                     //                                                                                  ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                     //                                                                                  ExpandChargingStationGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     //#endregion

                                                     //#region Include charging stations

                                                     //if (expandChargingStationIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var stations  = ChargingStationOperator.
                                                     //                        ChargingStations.
                                                     //                        ToArray();

                                                     //    if (stations.Length > 0)
                                                     //       ChargingStationGroupJSON["chargingStationIds"] = new JArray(stations.Select(station => station.Id.ToString()));

                                                     //}

                                                     //else if (expandChargingStationIds == InfoStatus.Expanded) {

                                                     //    var stations  = ChargingStationOperator.
                                                     //                        ChargingStations.
                                                     //                        ToArray();

                                                     //    if (stations.Length > 0)
                                                     //        ChargingStationGroupJSON["chargingStations"]  = new JArray(ChargingStationOperator.
                                                     //                                                         ChargingStations.
                                                     //                                                         Select(station => station.ToJSON(Embedded:                        true,
                                                     //                                                                                          ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                                          ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                     //                                                                                          ExpandChargingPoolId:            InfoStatus.Hidden,
                                                     //                                                                                          ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                     //                                                                                          ExpandChargingStationGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     //#endregion

                                                     //#region Include EVSEs

                                                     //else if (expandEVSEIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var evses  = ChargingStationOperator.
                                                     //                     EVSEs.
                                                     //                     Where(evse => evse.ChargingStationGroup == ChargingStationGroup).
                                                     //                     ToArray();

                                                     //    if (evses.Length > 0)
                                                     //       ChargingStationGroupJSON["EVSEIds"] = new JArray(evses.Select(station => station.Id.ToString()));

                                                     //}

                                                     //else if (expandEVSEIds == InfoStatus.Expanded) {

                                                     //    var evses  = ChargingStationOperator.
                                                     //                     EVSEs.
                                                     //                     Where(evse => evse.ChargingStationGroup == ChargingStationGroup).
                                                     //                     ToArray();

                                                     //    if (evses.Length > 0)
                                                     //        ChargingStationGroupJSON["EVSEs"]   = new JArray(ChargingStationOperator.
                                                     //                                              EVSEs.
                                                     //                                              Where (evse => evse.ChargingStationGroup == ChargingStationGroup).
                                                     //                                              Select(evse => evse.ToJSON(Embedded:                        true,
                                                     //                                                                         ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                         ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                     //                                                                         ExpandChargingPoolId:            InfoStatus.Hidden,
                                                     //                                                                         ExpandChargingStationId:         InfoStatus.Hidden,
                                                     //                                                                         ExpandChargingStationGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     #endregion


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode              = HTTPStatusCode.OK,
                                                             Server                      = HTTPServer.DefaultServerName,
                                                             Date                        = DateTime.UtcNow,
                                                             AccessControlAllowOrigin    = "*",
                                                             AccessControlAllowMethods   = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                             ETag                        = "1",
                                                             ContentType                 = HTTPContentType.JSON_UTF8,
                                                             Content                     = ChargingStationGroupJSON.ToUTF8Bytes(),
                                                             Connection                  = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/EVSEGroups
            // ----------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork           RoamingNetwork,
                                                                                                                out ChargingStationOperator  ChargingStationOperator,
                                                                                                                out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount            = ChargingStationOperator.EVSEGroups.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = ChargingStationOperator.EVSEGroups.
                                                                                                  ToJSON(skip,
                                                                                                         take,
                                                                                                         false,
                                                                                                         expandChargingPoolIds,
                                                                                                         expandEVSEIds,
                                                                                                         expandEVSEIds,
                                                                                                         expandDataLicenseIds).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this,
                                                                                                                            out RoamingNetwork           RoamingNetwork,
                                                                                                                            out ChargingStationOperator  ChargingStationOperator,
                                                                                                                            out EVSEGroup                EVSEGroup,
                                                                                                                            out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var include                   = Request.QueryString.GetStrings("include");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandChargingStationIds  = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandEVSEIds             = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandDataLicenseIds      = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                                                     var EVSEGroupJSON             = EVSEGroup.ToJSON(false,
                                                                                                      expandChargingPoolIds,
                                                                                                      expandEVSEIds,
                                                                                                      expandEVSEIds,
                                                                                                      expandDataLicenseIds);

                                                     #region Include charging pools

                                                     //if (expandChargingPoolIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var pools  = EVSEOperator.
                                                     //                     ChargingPools.
                                                     //                     Where(pool => pool.EVSEGroup == EVSEGroup).
                                                     //                     ToArray();

                                                     //    if (pools.Length > 0)
                                                     //       EVSEGroupJSON["chargingPoolIds"] = new JArray(pools.Select(pool => pool.Id.ToString()));

                                                     //}

                                                     //else if (expandChargingPoolIds == InfoStatus.Expanded) {

                                                     //    var pools  = EVSEOperator.
                                                     //                     ChargingPools.
                                                     //                     Where(pool => pool.EVSEGroup == EVSEGroup).
                                                     //                     ToArray();

                                                     //    if (pools.Length > 0)
                                                     //        EVSEGroupJSON["chargingPools"]  = new JArray(pools.Select(pool => pool.ToJSON(Embedded:                        true,
                                                     //                                                                                  ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                                  ExpandEVSEOperatorId: InfoStatus.Hidden,
                                                     //                                                                                  ExpandEVSEIds:        InfoStatus.Hidden,
                                                     //                                                                                  ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                     //                                                                                  ExpandEVSEGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     //#endregion

                                                     //#region Include charging stations

                                                     //if (expandEVSEIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var stations  = EVSEOperator.
                                                     //                        EVSEs.
                                                     //                        ToArray();

                                                     //    if (stations.Length > 0)
                                                     //       EVSEGroupJSON["chargingStationIds"] = new JArray(stations.Select(station => station.Id.ToString()));

                                                     //}

                                                     //else if (expandEVSEIds == InfoStatus.Expanded) {

                                                     //    var stations  = EVSEOperator.
                                                     //                        EVSEs.
                                                     //                        ToArray();

                                                     //    if (stations.Length > 0)
                                                     //        EVSEGroupJSON["chargingStations"]  = new JArray(EVSEOperator.
                                                     //                                                         EVSEs.
                                                     //                                                         Select(station => station.ToJSON(Embedded:                        true,
                                                     //                                                                                          ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                                          ExpandEVSEOperatorId: InfoStatus.Hidden,
                                                     //                                                                                          ExpandChargingPoolId:            InfoStatus.Hidden,
                                                     //                                                                                          ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                     //                                                                                          ExpandEVSEGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     //#endregion

                                                     //#region Include EVSEs

                                                     //else if (expandEVSEIds == InfoStatus.ShowIdOnly)
                                                     //{

                                                     //    var evses  = EVSEOperator.
                                                     //                     EVSEs.
                                                     //                     Where(evse => evse.EVSEGroup == EVSEGroup).
                                                     //                     ToArray();

                                                     //    if (evses.Length > 0)
                                                     //       EVSEGroupJSON["EVSEIds"] = new JArray(evses.Select(station => station.Id.ToString()));

                                                     //}

                                                     //else if (expandEVSEIds == InfoStatus.Expanded) {

                                                     //    var evses  = EVSEOperator.
                                                     //                     EVSEs.
                                                     //                     Where(evse => evse.EVSEGroup == EVSEGroup).
                                                     //                     ToArray();

                                                     //    if (evses.Length > 0)
                                                     //        EVSEGroupJSON["EVSEs"]   = new JArray(EVSEOperator.
                                                     //                                              EVSEs.
                                                     //                                              Where (evse => evse.EVSEGroup == EVSEGroup).
                                                     //                                              Select(evse => evse.ToJSON(Embedded:                        true,
                                                     //                                                                         ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                     //                                                                         ExpandEVSEOperatorId: InfoStatus.Hidden,
                                                     //                                                                         ExpandChargingPoolId:            InfoStatus.Hidden,
                                                     //                                                                         ExpandEVSEId:         InfoStatus.Hidden,
                                                     //                                                                         ExpandEVSEGroupIds:                  InfoStatus.Hidden)));

                                                     //}

                                                     #endregion


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode              = HTTPStatusCode.OK,
                                                             Server                      = HTTPServer.DefaultServerName,
                                                             Date                        = DateTime.UtcNow,
                                                             AccessControlAllowOrigin    = "*",
                                                             AccessControlAllowMethods   = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                             ETag                        = "1",
                                                             ContentType                 = HTTPContentType.JSON_UTF8,
                                                             Content                     = EVSEGroupJSON.ToUTF8Bytes(),
                                                             Connection                  = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion


            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Brands
            // ----------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork           RoamingNetwork,
                                                                                                                out ChargingStationOperator  ChargingStationOperator,
                                                                                                                out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount            = ChargingStationOperator.Brands.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = ChargingStationOperator.Brands.
                                                                                                  ToJSON(skip,
                                                                                                         take,
                                                                                                         false,
                                                                                                         expandChargingPoolIds,
                                                                                                         expandChargingStationIds,
                                                                                                         expandEVSEIds,
                                                                                                         expandDataLicenseIds).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Brands/{BrandId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperatorAndBrand(this,
                                                                                                                        out RoamingNetwork           RoamingNetwork,
                                                                                                                        out ChargingStationOperator  ChargingStationOperator,
                                                                                                                        out Brand                    Brand,
                                                                                                                        out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                       = Request.QueryString.GetUInt64("skip");
                                                     var take                       = Request.QueryString.GetUInt64("take");
                                                     var include                    = Request.QueryString.GetStrings("include");
                                                     var expand                     = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds      = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandChargingStationIds   = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandEVSEIds              = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                                                     var expandDataLicenseIds       = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                                                     var BrandJSON                  = Brand.ToJSON(false,
                                                                                                   expandChargingPoolIds,
                                                                                                   expandChargingStationIds,
                                                                                                   expandEVSEIds,
                                                                                                   expandDataLicenseIds);

                                                     #region Include charging pools

                                                     if (expandChargingPoolIds == InfoStatus.ShowIdOnly)
                                                     {

                                                         var pools  = ChargingStationOperator.
                                                                          ChargingPools.
                                                                          Where(pool => pool.Brands.Contains(Brand)).
                                                                          ToArray();

                                                         if (pools.Length > 0)
                                                            BrandJSON["chargingPoolIds"] = new JArray(pools.Select(pool => pool.Id.ToString()));

                                                     }

                                                     else if (expandChargingPoolIds == InfoStatus.Expanded) {

                                                         var pools  = ChargingStationOperator.
                                                                          ChargingPools.
                                                                          Where(pool => pool.Brands.Contains(Brand)).
                                                                          ToArray();

                                                         if (pools.Length > 0)
                                                             BrandJSON["chargingPools"]  = new JArray(pools.Select(pool => pool.ToJSON(Embedded:                        true,
                                                                                                                                       ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                                                                                                       ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                                                                                                       ExpandChargingStationIds:        InfoStatus.Hidden,
                                                                                                                                       ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                                                                                                       ExpandBrandIds:                  InfoStatus.Hidden)));

                                                     }

                                                     #endregion

                                                     #region Include charging stations

                                                     if (expandChargingStationIds == InfoStatus.ShowIdOnly)
                                                     {

                                                         var stations  = ChargingStationOperator.
                                                                             ChargingStations.
                                                                             Where(station => station.Brands.Contains(Brand)).
                                                                             ToArray();

                                                         if (stations.Length > 0)
                                                            BrandJSON["chargingStationIds"] = new JArray(stations.Select(station => station.Id.ToString()));

                                                     }

                                                     else if (expandChargingStationIds == InfoStatus.Expanded) {

                                                         var stations  = ChargingStationOperator.
                                                                             ChargingStations.
                                                                             Where(station => station.Brands.Contains(Brand)).
                                                                             ToArray();

                                                         if (stations.Length > 0)
                                                             BrandJSON["chargingStations"]  = new JArray(ChargingStationOperator.
                                                                                                              ChargingStations.
                                                                                                              Where (station => station.Brands.SafeAny(brand => brand == Brand)).
                                                                                                              Select(station => station.ToJSON(Embedded:                        true,
                                                                                                                                               ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                                                                                                               ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                                                                                                               ExpandChargingPoolId:            InfoStatus.Hidden,
                                                                                                                                               ExpandEVSEIds:                   InfoStatus.ShowIdOnly,
                                                                                                                                               ExpandBrandIds:                  InfoStatus.Hidden)));

                                                     }

                                                     #endregion

                                                     #region Include EVSEs

                                                     else if (expandEVSEIds == InfoStatus.ShowIdOnly)
                                                     {

                                                         var evses  = ChargingStationOperator.
                                                                          EVSEs.
                                                                          Where(evse => evse.Brands.Contains(Brand)).
                                                                          ToArray();

                                                         if (evses.Length > 0)
                                                            BrandJSON["EVSEIds"] = new JArray(evses.Select(station => station.Id.ToString()));

                                                     }

                                                     else if (expandEVSEIds == InfoStatus.Expanded) {

                                                         var evses  = ChargingStationOperator.
                                                                          EVSEs.
                                                                          Where(evse => evse.Brands.Contains(Brand)).
                                                                          ToArray();

                                                         if (evses.Length > 0)
                                                             BrandJSON["EVSEs"]   = new JArray(ChargingStationOperator.
                                                                                                   EVSEs.
                                                                                                   Where (evse => evse.Brands.Contains(Brand)).
                                                                                                   Select(evse => evse.ToJSON(Embedded:                        true,
                                                                                                                              ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                                                                                              ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                                                                                              ExpandChargingPoolId:            InfoStatus.Hidden,
                                                                                                                              ExpandChargingStationId:         InfoStatus.Hidden,
                                                                                                                              ExpandBrandIds:                  InfoStatus.Hidden)));

                                                     }

                                                     #endregion


                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode              = HTTPStatusCode.OK,
                                                             Server                      = HTTPServer.DefaultServerName,
                                                             Date                        = DateTime.UtcNow,
                                                             AccessControlAllowOrigin    = "*",
                                                             AccessControlAllowMethods   = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders   = "Content-Type, Accept, Authorization",
                                                             ETag                        = "1",
                                                             ContentType                 = HTTPContentType.JSON_UTF8,
                                                             Content                     = BrandJSON.ToUTF8Bytes(),
                                                             Connection                  = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion



            #region ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Tariffs

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/Tariffs

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Tariffs
            // ----------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/Tariffs",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork           RoamingNetwork,
                                                                                                                out ChargingStationOperator  ChargingStationOperator,
                                                                                                                out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount            = ChargingStationOperator.ChargingTariffs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.JSON_UTF8,
                                                             Content                        = ChargingStationOperator.ChargingTariffs.
                                                                                                  ToJSON(skip,
                                                                                                         take,
                                                                                                         false,
                                                                                                         expandChargingPoolIds,
                                                                                                         expandChargingStationIds,
                                                                                                         expandEVSEIds,
                                                                                                         expandDataLicenseIds).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RNId}/ChargingStationOperators/{CSOId}/TariffOverview

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/TariffOverview
            // ----------------------------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RNId}/ChargingStationOperators/{CSOId}/TariffOverview",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetworkAndChargingStationOperator(this,
                                                                                                                out RoamingNetwork           RoamingNetwork,
                                                                                                                out ChargingStationOperator  ChargingStationOperator,
                                                                                                                out HTTPResponse             _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     var skip                      = Request.QueryString.GetUInt64("skip");
                                                     var take                      = Request.QueryString.GetUInt64("take");
                                                     var expand                    = Request.QueryString.GetStrings("expand");
                                                     var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                                                     var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount            = ChargingStationOperator.ChargingTariffs.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode                 = HTTPStatusCode.OK,
                                                             Server                         = HTTPServer.DefaultServerName,
                                                             Date                           = DateTime.UtcNow,
                                                             AccessControlAllowOrigin       = "*",
                                                             AccessControlAllowMethods      = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders      = "Content-Type, Accept, Authorization",
                                                             ETag                           = "1",
                                                             ContentType                    = HTTPContentType.TEXT_UTF8,
                                                             Content                        = ChargingStationOperator.ChargingStations.
                                                                                                  GetTariffs(skip,
                                                                                                             take,
                                                                                                             false,
                                                                                                             expandChargingPoolIds,
                                                                                                             expandChargingStationIds,
                                                                                                             expandEVSEIds,
                                                                                                             expandDataLicenseIds).
                                                                                                  Select(line => "\"" + line.AggregateWith("\";\"") + "\"").
                                                                                                  AggregateWith(Environment.NewLine).
                                                                                                  ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems   = _ExpectedCount,
                                                             Connection                     = "close"
                                                         }.AsImmutable);

                                                 });

            #endregion


            #endregion





            #region ~/RNs/{RoamingNetworkId}/eMobilityProviders

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders

            // -----------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders
            // -----------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork _RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip                    = Request.QueryString.GetUInt64("skip");
                                                     var take                    = Request.QueryString.GetUInt64("take");
                                                     var expand                  = Request.QueryString.GetStrings("expand");
                                                     //var expandChargingPools     = !expand.ContainsIgnoreCase("-chargingpools");
                                                     //var expandChargingStations  = !expand.ContainsIgnoreCase("-chargingstations");
                                                     //var expandBrands            = expand.ContainsIgnoreCase("brands");

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.eMobilityProviders.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _RoamingNetwork.eMobilityProviders.
                                                                                                ToJSON(skip,
                                                                                                       take,
                                                                                                       false).
                                                                                                       //expandChargingPools,
                                                                                                       //expandChargingStations,
                                                                                                       //expandBrands).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/eMobilityProviders

            // ----------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RoamingNetworkId}/eMobilityProviders
            // ----------------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.COUNT,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork _RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET, COUNT, STATUS",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = JSONObject.Create(
                                                                                                new JProperty("count",  _RoamingNetwork.ChargingStationOperators.ULongCount())
                                                                                            ).ToUTF8Bytes()
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders->AdminStatus

            // ------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders->AdminStatus
            // ------------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders->AdminStatus",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork _RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip         = Request.QueryString.GetUInt64("skip");
                                                     var take         = Request.QueryString.GetUInt64("take");
                                                     var historysize  = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorAdminStatus.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _RoamingNetwork.ChargingStationOperatorAdminStatus.
                                                                                                OrderBy(kvp => kvp.Key).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        historysize).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders->Status

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders->Status
            // -------------------------------------------------------------------------------------------------
            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders->Status",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(this, out RoamingNetwork _RoamingNetwork, out HTTPResponse _HTTPResponse))
                                                         return Task.FromResult(_HTTPResponse);

                                                     #endregion

                                                     var skip         = Request.QueryString.GetUInt64("skip");
                                                     var take         = Request.QueryString.GetUInt64("take");
                                                     var historysize  = Request.QueryString.GetUInt64("historysize", 1);

                                                     //ToDo: Getting the expected total count might be very expensive!
                                                     var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorStatus.ULongCount();

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode               = HTTPStatusCode.OK,
                                                             Server                       = HTTPServer.DefaultServerName,
                                                             Date                         = DateTime.UtcNow,
                                                             AccessControlAllowOrigin     = "*",
                                                             AccessControlAllowMethods    = "GET",
                                                             AccessControlAllowHeaders    = "Content-Type, Accept, Authorization",
                                                             ETag                         = "1",
                                                             ContentType                  = HTTPContentType.JSON_UTF8,
                                                             Content                      = _RoamingNetwork.ChargingStationOperatorStatus.
                                                                                                OrderBy(kvp => kvp.Key).
                                                                                                ToJSON (skip,
                                                                                                        take,
                                                                                                        historysize).
                                                                                                ToUTF8Bytes(),
                                                             X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                         }.AsImmutable);

                                                 });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}

            HTTPServer.AddMethodCallback(Hostname,
                                                 HTTPMethod.GET,
                                                 URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}",
                                                 HTTPContentType.JSON_UTF8,
                                                 HTTPDelegate: Request => {

                                                     #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndEMobilityProvider(this,
                                                                                                          out RoamingNetwork     _RoamingNetwork,
                                                                                                          out eMobilityProvider  _eMobilityProvider,
                                                                                                          out HTTPResponse       _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse);
                                                     }

                                                     #endregion

                                                     return Task.FromResult(
                                                         new HTTPResponse.Builder(Request) {
                                                             HTTPStatusCode             = HTTPStatusCode.OK,
                                                             Server                     = HTTPServer.DefaultServerName,
                                                             Date                       = DateTime.UtcNow,
                                                             AccessControlAllowOrigin   = "*",
                                                             AccessControlAllowMethods  = "GET, CREATE, DELETE",
                                                             AccessControlAllowHeaders  = "Content-Type, Accept, Authorization",
                                                             ETag                       = "1",
                                                             ContentType                = HTTPContentType.JSON_UTF8,
                                                             Content                    = _eMobilityProvider.ToJSON().ToUTF8Bytes()
                                                         }.AsImmutable);

                                           });

            #endregion

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





        #region CreateNewRoamingNetwork(Id, Name, Description = null, Configurator = null, ...)

        /// <summary>
        /// Create and register a new roaming network collection
        /// for the given HTTP hostname.
        /// </summary>
        /// <param name="Id">The unique identification of the new roaming network.</param>
        /// <param name="Name">The multi-language name of the roaming network.</param>
        /// <param name="Description">A multilanguage description of the roaming networks object.</param>
        /// <param name="Configurator">An optional delegate to configure the new roaming network after its creation.</param>
        /// <param name="AdminStatus">The initial admin status of the roaming network.</param>
        /// <param name="Status">The initial status of the roaming network.</param>
        /// <param name="MaxAdminStatusListSize">The maximum number of entries in the admin status history.</param>
        /// <param name="MaxStatusListSize">The maximum number of entries in the status history.</param>
        /// <param name="ChargingStationSignatureGenerator">A delegate to sign a charging station.</param>
        /// <param name="ChargingPoolSignatureGenerator">A delegate to sign a charging pool.</param>
        /// <param name="ChargingStationOperatorSignatureGenerator">A delegate to sign a charging station operator.</param>
        public RoamingNetwork CreateNewRoamingNetwork(RoamingNetwork_Id                         Id,
                                                      I18NString                                Name,
                                                      I18NString                                Description                                 = null,
                                                      Action<RoamingNetwork>                    Configurator                                = null,
                                                      RoamingNetworkAdminStatusTypes            AdminStatus                                 = RoamingNetworkAdminStatusTypes.Operational,
                                                      RoamingNetworkStatusTypes                 Status                                      = RoamingNetworkStatusTypes.Available,
                                                      UInt16                                    MaxAdminStatusListSize                      = RoamingNetwork.DefaultMaxAdminStatusListSize,
                                                      UInt16                                    MaxStatusListSize                           = RoamingNetwork.DefaultMaxStatusListSize,

                                                      ChargingStationSignatureDelegate          ChargingStationSignatureGenerator           = null,
                                                      ChargingPoolSignatureDelegate             ChargingPoolSignatureGenerator              = null,
                                                      ChargingStationOperatorSignatureDelegate  ChargingStationOperatorSignatureGenerator   = null,

                                                      IEnumerable<RoamingNetworkInfo>           RoamingNetworkInfos                         = null,
                                                      Boolean                                   DisableNetworkSync                          = false)


            => CreateNewRoamingNetwork(HTTPHostname.Any,
                                       Id,
                                       Name,
                                       Description,
                                       Configurator,
                                       AdminStatus,
                                       Status,
                                       MaxAdminStatusListSize,
                                       MaxStatusListSize,

                                       ChargingStationSignatureGenerator,
                                       ChargingPoolSignatureGenerator,
                                       ChargingStationOperatorSignatureGenerator,

                                       RoamingNetworkInfos,
                                       DisableNetworkSync);

        #endregion

        #region CreateNewRoamingNetwork(Hostname, Id, Name, Description = null, Configurator = null, ...)

        /// <summary>
        /// Create and register a new roaming network collection
        /// for the given HTTP hostname.
        /// </summary>
        /// <param name="Hostname">A HTTP hostname.</param>
        /// <param name="Id">The unique identification of the new roaming network.</param>
        /// <param name="Name">The multi-language name of the roaming network.</param>
        /// <param name="Description">A multilanguage description of the roaming networks object.</param>
        /// <param name="Configurator">An optional delegate to configure the new roaming network after its creation.</param>
        /// <param name="AdminStatus">The initial admin status of the roaming network.</param>
        /// <param name="Status">The initial status of the roaming network.</param>
        /// <param name="MaxAdminStatusListSize">The maximum number of entries in the admin status history.</param>
        /// <param name="MaxStatusListSize">The maximum number of entries in the status history.</param>
        /// <param name="ChargingStationSignatureGenerator">A delegate to sign a charging station.</param>
        /// <param name="ChargingPoolSignatureGenerator">A delegate to sign a charging pool.</param>
        /// <param name="ChargingStationOperatorSignatureGenerator">A delegate to sign a charging station operator.</param>
        public RoamingNetwork CreateNewRoamingNetwork(HTTPHostname                              Hostname,
                                                      RoamingNetwork_Id                         Id,
                                                      I18NString                                Name,
                                                      I18NString                                Description                                 = null,
                                                      Action<RoamingNetwork>                    Configurator                                = null,
                                                      RoamingNetworkAdminStatusTypes            AdminStatus                                 = RoamingNetworkAdminStatusTypes.Operational,
                                                      RoamingNetworkStatusTypes                 Status                                      = RoamingNetworkStatusTypes.Available,
                                                      UInt16                                    MaxAdminStatusListSize                      = RoamingNetwork.DefaultMaxAdminStatusListSize,
                                                      UInt16                                    MaxStatusListSize                           = RoamingNetwork.DefaultMaxStatusListSize,

                                                      ChargingStationSignatureDelegate          ChargingStationSignatureGenerator           = null,
                                                      ChargingPoolSignatureDelegate             ChargingPoolSignatureGenerator              = null,
                                                      ChargingStationOperatorSignatureDelegate  ChargingStationOperatorSignatureGenerator   = null,

                                                      IEnumerable<RoamingNetworkInfo>           RoamingNetworkInfos                         = null,
                                                      Boolean                                   DisableNetworkSync                          = false)

        {

            #region Initial checks

            if (Hostname == null)
                throw new ArgumentNullException(nameof(Hostname), "The given HTTP hostname must not be null!");

            #endregion

            var ExistingRoamingNetwork = WWCPHTTPServer.
                                             GetAllTenants(Hostname).
                                             FirstOrDefault(roamingnetwork => roamingnetwork.Id == Id);

            if (ExistingRoamingNetwork == null)
            {

                if (!WWCPHTTPServer.TryGetTenants(Hostname, out RoamingNetworks _RoamingNetworks))
                {

                    _RoamingNetworks = new RoamingNetworks();

                    if (!WWCPHTTPServer.TryAddTenants(Hostname, _RoamingNetworks))
                        throw new Exception("Could not add new roaming networks object to the HTTP host!");

                }

                var NewRoamingNetwork = _RoamingNetworks.
                                            CreateNewRoamingNetwork(Id,
                                                                    Name,
                                                                    Description,
                                                                    Configurator,
                                                                    AdminStatus,
                                                                    Status,
                                                                    MaxAdminStatusListSize,
                                                                    MaxStatusListSize,

                                                                    ChargingStationSignatureGenerator,
                                                                    ChargingPoolSignatureGenerator,
                                                                    ChargingStationOperatorSignatureGenerator,

                                                                    RoamingNetworkInfos,
                                                                    DisableNetworkSync);

                #region Link log events to HTTP-SSE...

                #region OnAuthorizeStartRequest/-Response

                NewRoamingNetwork.OnAuthorizeStartRequest += async (LogTimestamp,
                                                                    RequestTimestamp,
                                                                    Sender,
                                                                    SenderId,
                                                                    EventTrackingId,
                                                                    RoamingNetworkId,
                                                                    EMPRoamingProviderId,
                                                                    CSORoamingProviderId,
                                                                    OperatorId,
                                                                    Authentication,
                                                                    ChargingLocation,
                                                                    ChargingProduct,
                                                                    SessionId,
                                                                    ISendAuthorizeStartStop,
                                                                    RequestTimeout)

                    => await DebugLog.SubmitEvent("AUTHSTARTRequest",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   RequestTimestamp.    ToIso8601()),
                                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      OperatorId.    HasValue
                                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                                                          : null,
                                                      Authentication != null
                                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                                                          : null,
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      ChargingProduct != null
                                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                                                          : null,
                                                      SessionId.     HasValue
                                                          ? new JProperty("sessionId",             SessionId.           ToString())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null
                                               ));


                NewRoamingNetwork.OnAuthorizeStartResponse += async (LogTimestamp,
                                                                     RequestTimestamp,
                                                                     Sender,
                                                                     SenderId,
                                                                     EventTrackingId,
                                                                     RoamingNetworkId2,
                                                                     EMPRoamingProviderId,
                                                                     CSORoamingProviderId,
                                                                     OperatorId,
                                                                     Authentication,
                                                                     ChargingLocation,
                                                                     ChargingProduct,
                                                                     SessionId,
                                                                     ISendAuthorizeStartStop,
                                                                     RequestTimeout,
                                                                     Result,
                                                                     Runtime)

                    => await DebugLog.SubmitEvent("AUTHSTARTResponse",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   RequestTimestamp.    ToIso8601()),
                                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      OperatorId.HasValue
                                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                                                          : null,
                                                      new JProperty("authentication",              Authentication.      ToJSON()),
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      ChargingProduct != null
                                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                                                          : null,
                                                      SessionId.HasValue
                                                          ? new JProperty("sessionId",             SessionId.           ToString())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null,

                                                      new JProperty("result",                      Result.              ToJSON()),
                                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))

                                                  ));

                #endregion

                #region OnAuthorizeStopRequest/-Response

                NewRoamingNetwork.OnAuthorizeStopRequest += async (LogTimestamp,
                                                                   RequestTimestamp,
                                                                   Sender,
                                                                   SenderId,
                                                                   EventTrackingId,
                                                                   RoamingNetworkId2,
                                                                   EMPRoamingProviderId,
                                                                   CSORoamingProviderId,
                                                                   OperatorId,
                                                                   ChargingLocation,
                                                                   SessionId,
                                                                   Authentication,
                                                                   RequestTimeout)

                    => await DebugLog.SubmitEvent("AUTHSTOPRequest",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   RequestTimestamp.    ToIso8601()),
                                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      OperatorId != null
                                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                                                          : null,
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      new JProperty("sessionId",                   SessionId.           ToString()),
                                                      new JProperty("authentication",              Authentication.      ToString()),
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null
                                                  ));

                NewRoamingNetwork.OnAuthorizeStopResponse += async (LogTimestamp,
                                                                    RequestTimestamp,
                                                                    Sender,
                                                                    SenderId,
                                                                    EventTrackingId,
                                                                    RoamingNetworkId2,
                                                                    EMPRoamingProviderId,
                                                                    CSORoamingProviderId,
                                                                    OperatorId,
                                                                    ChargingLocation,
                                                                    SessionId,
                                                                    Authentication,
                                                                    RequestTimeout,
                                                                    Result,
                                                                    Runtime)

                    => await DebugLog.SubmitEvent("AUTHSTOPResponse",
                                                  JSONObject.Create(

                                                      new JProperty("timestamp",                   RequestTimestamp.    ToIso8601()),
                                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      OperatorId.HasValue
                                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                                                          : null,
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      SessionId.HasValue
                                                          ? new JProperty("sessionId",             SessionId.           ToString())
                                                          : null,
                                                      new JProperty("authentication",              Authentication.      ToString()),
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null,

                                                      new JProperty("result",                      Result.              ToJSON()),
                                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))

                                              ));

                #endregion


                #region OnReserveEVSERequest/-Response

                NewRoamingNetwork.OnReserveRequest += async (LogTimestamp,
                                                             Timestamp,
                                                             Sender,
                                                             EventTrackingId,
                                                             RoamingNetworkId2,
                                                             ReservationId,
                                                             EVSEId,
                                                             StartTime,
                                                             Duration,
                                                             ProviderId,
                                                             eMAId,
                                                             ChargingProduct,
                                                             AuthTokens,
                                                             eMAIds,
                                                             PINs,
                                                             RequestTimeout)

                    => await DebugLog.SubmitEvent("OnReserveRequest",
                                                  JSONObject.Create(
                                                      new JProperty("Timestamp",                 Timestamp.ToIso8601()),
                                                      EventTrackingId != null
                                                         ? new JProperty("EventTrackingId",      EventTrackingId.ToString())
                                                         : null,
                                                      new JProperty("RoamingNetwork",            Id.ToString()),
                                                      ReservationId != null
                                                         ? new JProperty("ReservationId",        ReservationId.ToString())
                                                         : null,
                                                      EVSEId     != null
                                                          ? new JProperty("EVSEId",              EVSEId.ToString())
                                                          : null,
                                                      StartTime.HasValue
                                                          ? new JProperty("StartTime",           StartTime.Value.ToIso8601())
                                                          : null,
                                                      Duration.HasValue
                                                          ? new JProperty("Duration",            Duration.Value.TotalSeconds.ToString())
                                                          : null,
                                                      ProviderId != null
                                                          ? new JProperty("ProviderId",          ProviderId.ToString())
                                                          : null,
                                                      eMAId != null
                                                          ? new JProperty("eMAId",               eMAId.ToString())
                                                          : null,
                                                      ChargingProduct != null
                                                          ? new JProperty("ChargingProduct",     JSONObject.Create(
                                                                new JProperty("Id",                              ChargingProduct.Id.ToString()),
                                                                ChargingProduct.MinDuration.HasValue
                                                                    ? new JProperty("MinDuration",               ChargingProduct.MinDuration.Value.TotalSeconds)
                                                                    : null,
                                                                ChargingProduct.StopChargingAfterTime.HasValue
                                                                    ? new JProperty("StopChargingAfterTime",     ChargingProduct.StopChargingAfterTime.Value.TotalSeconds)
                                                                    : null,
                                                                ChargingProduct.MinPower.HasValue
                                                                    ? new JProperty("MinPower",                  ChargingProduct.MinPower.Value)
                                                                    : null,
                                                                ChargingProduct.MaxPower.HasValue
                                                                    ? new JProperty("MaxPower",                  ChargingProduct.MaxPower.Value)
                                                                    : null,
                                                                ChargingProduct.MinEnergy.HasValue
                                                                    ? new JProperty("MinEnergy",                 ChargingProduct.MinEnergy.Value)
                                                                    : null,
                                                                ChargingProduct.StopChargingAfterKWh.HasValue
                                                                    ? new JProperty("StopChargingAfterKWh",      ChargingProduct.StopChargingAfterKWh.Value)
                                                                    : null
                                                               ))
                                                          : null,
                                                      AuthTokens != null
                                                          ? new JProperty("AuthTokens",          new JArray(AuthTokens.Select(_ => _.ToString())))
                                                          : null,
                                                      eMAIds != null
                                                          ? new JProperty("eMAIds",              new JArray(eMAIds.Select(_ => _.ToString())))
                                                          : null,
                                                      PINs != null
                                                          ? new JProperty("PINs",                new JArray(PINs.Select(_ => _.ToString())))
                                                          : null
                                                  ));

                NewRoamingNetwork.OnReserveResponse += async (LogTimestamp,
                                                              Timestamp,
                                                              Sender,
                                                              EventTrackingId,
                                                              RoamingNetworkId2,
                                                              ReservationId,
                                                              EVSEId,
                                                              StartTime,
                                                              Duration,
                                                              ProviderId,
                                                              eMAId,
                                                              ChargingProduct,
                                                              AuthTokens,
                                                              eMAIds,
                                                              PINs,
                                                              Result,
                                                              Runtime,
                                                              RequestTimeout)

                    => await DebugLog.SubmitEvent("OnReserveResponse",
                                                  JSONObject.Create(
                                                      new JProperty("Timestamp",                 Timestamp.ToIso8601()),
                                                        EventTrackingId != null
                                                           ? new JProperty("EventTrackingId",      EventTrackingId.ToString())
                                                           : null,
                                                        new JProperty("RoamingNetwork",            Id.ToString()),
                                                        ReservationId != null
                                                           ? new JProperty("ReservationId",        ReservationId.ToString())
                                                           : null,
                                                        EVSEId     != null
                                                            ? new JProperty("EVSEId",              EVSEId.ToString())
                                                            : null,
                                                        StartTime.HasValue
                                                            ? new JProperty("StartTime",           StartTime.Value.ToIso8601())
                                                            : null,
                                                        Duration.HasValue
                                                            ? new JProperty("Duration",            Duration.Value.TotalSeconds.ToString())
                                                            : null,
                                                        ProviderId != null
                                                            ? new JProperty("ProviderId",          ProviderId.ToString()+"X")
                                                            : null,
                                                        eMAId != null
                                                            ? new JProperty("eMAId",               eMAId.ToString())
                                                            : null,
                                                        ChargingProduct != null
                                                          ? new JProperty("ChargingProduct",     JSONObject.Create(
                                                                new JProperty("Id",                              ChargingProduct.Id.ToString()),
                                                                ChargingProduct.MinDuration.HasValue
                                                                    ? new JProperty("MinDuration",               ChargingProduct.MinDuration.Value.TotalSeconds)
                                                                    : null,
                                                                ChargingProduct.StopChargingAfterTime.HasValue
                                                                    ? new JProperty("StopChargingAfterTime",     ChargingProduct.StopChargingAfterTime.Value.TotalSeconds)
                                                                    : null,
                                                                ChargingProduct.MinPower.HasValue
                                                                    ? new JProperty("MinPower",                  ChargingProduct.MinPower.Value)
                                                                    : null,
                                                                ChargingProduct.MaxPower.HasValue
                                                                    ? new JProperty("MaxPower",                  ChargingProduct.MaxPower.Value)
                                                                    : null,
                                                                ChargingProduct.MinEnergy.HasValue
                                                                    ? new JProperty("MinEnergy",                 ChargingProduct.MinEnergy.Value)
                                                                    : null,
                                                                ChargingProduct.StopChargingAfterKWh.HasValue
                                                                    ? new JProperty("StopChargingAfterKWh",      ChargingProduct.StopChargingAfterKWh.Value)
                                                                    : null
                                                               ))
                                                          : null,
                                                        AuthTokens != null
                                                            ? new JProperty("AuthTokens",          new JArray(AuthTokens.Select(_ => _.ToString())))
                                                            : null,
                                                        eMAIds != null
                                                            ? new JProperty("eMAIds",              new JArray(eMAIds.Select(_ => _.ToString())))
                                                            : null,
                                                        PINs != null
                                                            ? new JProperty("PINs",                new JArray(PINs.Select(_ => _.ToString())))
                                                            : null,
                                                        new JProperty("Result",                    Result.Result.ToString()),
                                                        Result.Message.IsNotNullOrEmpty()
                                                            ? new JProperty("ErrorMessage",        Result.Message)
                                                            : null,
                                                        new JProperty("Runtime",                   Math.Round(Runtime.TotalMilliseconds, 0))
                                                  ));

                #endregion

                #region OnCancelReservationResponse

                NewRoamingNetwork.OnCancelReservationResponse += async (LogTimestamp,
                                                                   Timestamp,
                                                                   Sender,
                                                                   EventTrackingId,
                                                                   RoamingNetworkId,
                                                                   //ProviderId,
                                                                   ReservationId,
                                                                   Reservation,
                                                                   Reason,
                                                                   Result,
                                                                   Runtime,
                                                                   RequestTimeout)

                    => await DebugLog.SubmitEvent("OnCancelReservation",
                                                  JSONObject.Create(
                                                      new JProperty("Timestamp",                Timestamp.ToIso8601()),
                                                      EventTrackingId != null
                                                          ? new JProperty("EventTrackingId",    EventTrackingId.ToString())
                                                          : null,
                                                      new JProperty("ReservationId",            ReservationId.ToString()),

                                                      new JProperty("RoamingNetwork",           RoamingNetworkId.ToString()),

                                                      Reservation?.EVSEId != null
                                                          ? new JProperty("EVSEId",             Reservation.EVSEId.ToString())
                                                          : null,
                                                      Reservation?.ChargingStationId != null
                                                          ? new JProperty("ChargingStationId",  Reservation.ChargingStationId.ToString())
                                                          : null,
                                                      Reservation?.ChargingPoolId != null
                                                          ? new JProperty("ChargingPoolId",     Reservation.EVSEId.ToString())
                                                          : null,

                                                      new JProperty("Reason",                   Reason.ToString()),

                                                      new JProperty("Result",                   Result.Result.ToString()),
                                                      new JProperty("Message",                  Result.Message),
                                                      new JProperty("AdditionalInfo",           Result.AdditionalInfo),
                                                      new JProperty("Runtime",                  Result.Runtime)

                                                  ));

                //ToDo: OnCancelReservationResponse Result!

                #endregion


                #region OnRemoteStartRequest/-Response

                NewRoamingNetwork.OnRemoteStartRequest += async (LogTimestamp,
                                                                 Timestamp,
                                                                 Sender,
                                                                 EventTrackingId,
                                                                 RoamingNetworkId,
                                                                 ChargingLocation,
                                                                 ChargingProduct,
                                                                 ReservationId,
                                                                 SessionId,
                                                                 EMPRoamingProviderId,
                                                                 CSORoamingProviderId,
                                                                 ProviderId,
                                                                 Authentication,
                                                                 RequestTimeout)

                    => await DebugLog.SubmitEvent("OnRemoteStartRequest",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   Timestamp.           ToIso8601()),
                                                      EventTrackingId != null
                                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                                                          : null,
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      ChargingProduct != null
                                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                                                          : null,
                                                      ReservationId.HasValue
                                                          ? new JProperty("reservationId",         ReservationId.       ToString())
                                                          : null,
                                                      SessionId.HasValue
                                                          ? new JProperty("sessionId",             SessionId.           ToString())
                                                          : null,
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      ProviderId.HasValue
                                                          ? new JProperty("providerId",            ProviderId.          ToString())
                                                          : null,
                                                      Authentication != null
                                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null
                                                  ));

                NewRoamingNetwork.OnRemoteStartResponse += async (LogTimestamp,
                                                                  Timestamp,
                                                                  Sender,
                                                                  EventTrackingId,
                                                                  RoamingNetworkId,
                                                                  ChargingLocation,
                                                                  ChargingProduct,
                                                                  ReservationId,
                                                                  SessionId,
                                                                  EMPRoamingProviderId,
                                                                  CSORoamingProviderId,
                                                                  ProviderId,
                                                                  Authentication,
                                                                  RequestTimeout,
                                                                  Result,
                                                                  Runtime)

                    => await DebugLog.SubmitEvent("OnRemoteStartResponse",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   Timestamp.           ToIso8601()),
                                                      EventTrackingId      != null
                                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                                                          : null,
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                                                      ChargingLocation.IsDefined()
                                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                                                          : null,
                                                      ChargingProduct      != null
                                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                                                          : null,
                                                      ReservationId        != null
                                                          ? new JProperty("reservationId",         ReservationId.       ToString())
                                                          : null,
                                                      SessionId            != null
                                                          ? new JProperty("sessionId",             SessionId.           ToString())
                                                          : null,
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      ProviderId           != null
                                                          ? new JProperty("providerId",            ProviderId.          ToString())
                                                          : null,
                                                      Authentication != null
                                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null,
                                                      new JProperty("result",                      Result.              ToJSON()),
                                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))
                                                  ));

                #endregion

                #region OnRemoteStopRequest/-Response

                NewRoamingNetwork.OnRemoteStopRequest += async (LogTimestamp,
                                                                Timestamp,
                                                                Sender,
                                                                EventTrackingId,
                                                                RoamingNetworkId,
                                                                SessionId,
                                                                ReservationHandling,
                                                                EMPRoamingProviderId,
                                                                CSORoamingProviderId,
                                                                ProviderId,
                                                                Authentication,
                                                                RequestTimeout)

                    => await DebugLog.SubmitEvent("OnRemoteStopRequest",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   Timestamp.           ToIso8601()),
                                                      EventTrackingId != null
                                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                                                          : null,
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                                                      new JProperty("sessionId",                   SessionId.           ToString()),
                                                      ReservationHandling.HasValue
                                                          ? new JProperty("reservationHandling",   ReservationHandling. ToString())
                                                          : null,
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      ProviderId.HasValue
                                                          ? new JProperty("providerId",            ProviderId.          ToString())
                                                          : null,
                                                      Authentication != null
                                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null
                                                  ));

                NewRoamingNetwork.OnRemoteStopResponse += async (LogTimestamp,
                                                                 Timestamp,
                                                                 Sender,
                                                                 EventTrackingId,
                                                                 RoamingNetworkId,
                                                                 SessionId,
                                                                 ReservationHandling,
                                                                 EMPRoamingProviderId,
                                                                 CSORoamingProviderId,
                                                                 ProviderId,
                                                                 Authentication,
                                                                 RequestTimeout,
                                                                 Result,
                                                                 Runtime)

                    => await DebugLog.SubmitEvent("OnRemoteStopResponse",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                   Timestamp.           ToIso8601()),
                                                      EventTrackingId != null
                                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                                                          : null,
                                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                                                      new JProperty("sessionId",                   SessionId.           ToString()),
                                                      ReservationHandling.HasValue
                                                          ? new JProperty("reservationHandling",   ReservationHandling. ToString())
                                                          : null,
                                                      EMPRoamingProviderId.HasValue
                                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                                                          : null,
                                                      CSORoamingProviderId.HasValue
                                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                                                          : null,
                                                      ProviderId.HasValue
                                                          ? new JProperty("providerId",            ProviderId.          ToString())
                                                          : null,
                                                      Authentication != null
                                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                                                          : null,
                                                      RequestTimeout.HasValue
                                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                                                          : null,
                                                      new JProperty("result",                      Result.              ToJSON()),
                                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))
                                                  ));

                #endregion


                #region OnSendCDRsRequest/-Response

                NewRoamingNetwork.OnSendCDRsRequest += async (LogTimestamp,
                                                              RequestTimestamp,
                                                              Sender,
                                                              SenderId,
                                                              EventTrackingId,
                                                              RoamingNetworkId2,
                                                              ChargeDetailRecords,
                                                              RequestTimeout)


                    => await DebugLog.SubmitEvent("OnSendCDRsRequest",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",                RequestTimestamp.  ToIso8601()),
                                                      new JProperty("eventTrackingId",          EventTrackingId.   ToString()),
                                                      new JProperty("roamingNetworkId",         RoamingNetworkId2. ToString()),
                                                      //new JProperty("LogTimestamp",                     LogTimestamp.                                          ToIso8601()),
                                                      //new JProperty("RequestTimestamp",                 RequestTimestamp.                                      ToIso8601()),

                                                      new JProperty("chargeDetailRecords",              new JArray(
                                                          ChargeDetailRecords.Select(ChargeDetailRecord => JSONObject.Create(

                                                             new JProperty("@id",                              ChargeDetailRecord.Id.                                      ToString()),

                                                             new JProperty("sessionId",                        ChargeDetailRecord.SessionId.                               ToString()),

                                                             ChargeDetailRecord.SessionTime != null
                                                                 ? new JProperty("sessionStart",               ChargeDetailRecord.SessionTime.StartTime.                   ToIso8601())
                                                                 : null,
                                                             ChargeDetailRecord.SessionTime != null && ChargeDetailRecord.SessionTime.EndTime.HasValue
                                                                 ? new JProperty("sessionStop",                ChargeDetailRecord.SessionTime.EndTime.Value.               ToIso8601())
                                                                 : null,

                                                             ChargeDetailRecord.AuthenticationStart != null
                                                                 ? new JProperty("authenticationStart",        ChargeDetailRecord.AuthenticationStart.                     ToJSON())
                                                                 : null,
                                                             ChargeDetailRecord.AuthenticationStop  != null
                                                                 ? new JProperty("authenticationStop",         ChargeDetailRecord.AuthenticationStop.                      ToJSON())
                                                                 : null,
                                                             ChargeDetailRecord.ProviderIdStart.HasValue
                                                                 ? new JProperty("providerIdStart",            ChargeDetailRecord.ProviderIdStart.                         ToString())
                                                                 : null,
                                                             ChargeDetailRecord.ProviderIdStop.HasValue
                                                                 ? new JProperty("providerIdStop",             ChargeDetailRecord.ProviderIdStop.                          ToString())
                                                                 : null,

                                                             ChargeDetailRecord.ReservationId.HasValue
                                                                 ? new JProperty("reservationId",              ChargeDetailRecord.ReservationId.                           ToString())
                                                                 : null,
                                                             ChargeDetailRecord.ReservationTime != null
                                                                 ? new JProperty("reservationStart",           ChargeDetailRecord.ReservationTime.StartTime.               ToString())
                                                                 : null,
                                                             ChargeDetailRecord.ReservationTime != null && ChargeDetailRecord.ReservationTime.EndTime.HasValue
                                                                 ? new JProperty("reservationStop",            ChargeDetailRecord.ReservationTime.EndTime.Value.           ToIso8601())
                                                                 : null,
                                                             ChargeDetailRecord.Reservation             != null
                                                                 ? new JProperty("reservationLevel",           ChargeDetailRecord.Reservation.ReservationLevel.            ToString())
                                                                 : null,

                                                             ChargeDetailRecord.ChargingStationOperator != null
                                                                 ? new JProperty("chargingStationOperator",    ChargeDetailRecord.ChargingStationOperator.                 ToString())
                                                                 : null,

                                                             ChargeDetailRecord.EVSE != null
                                                                 ? new JProperty("EVSEId",                     ChargeDetailRecord.EVSE.Id.                                 ToString())
                                                                 : ChargeDetailRecord.EVSEId.HasValue
                                                                       ? new JProperty("EVSEId",               ChargeDetailRecord.EVSEId.                                  ToString())
                                                                       : null,

                                                             ChargeDetailRecord.ChargingProduct != null
                                                                 ? new JProperty("chargingProduct",            ChargeDetailRecord.ChargingProduct.ToJSON())
                                                                 : null,

                                                             ChargeDetailRecord.EnergyMeterId.HasValue
                                                                 ? new JProperty("energyMeterId",              ChargeDetailRecord.EnergyMeterId.                      ToString())
                                                                 : null,
                                                                   new JProperty("consumedEnergy",             ChargeDetailRecord.ConsumedEnergy),
                                                             ChargeDetailRecord.EnergyMeteringValues.Any()
                                                                 ? new JProperty("energyMeteringValues", JSONObject.Create(
                                                                       ChargeDetailRecord.EnergyMeteringValues.Select(metervalue => new JProperty(metervalue.Timestamp.ToIso8601(),
                                                                                                                                                  metervalue.Value)))
                                                                   )
                                                                 : null,
                                                             //ChargeDetailRecord.MeteringSignature.IsNotNullOrEmpty()
                                                             //    ? new JProperty("meteringSignature",          ChargeDetailRecord.MeteringSignature)
                                                             //    : null,

                                                             ChargeDetailRecord.ParkingSpaceId.HasValue
                                                                 ? new JProperty("parkingSpaceId",             ChargeDetailRecord.ParkingSpaceId.                      ToString())
                                                                 : null,
                                                             ChargeDetailRecord.ParkingTime != null
                                                                 ? new JProperty("parkingTimeStart",           ChargeDetailRecord.ParkingTime.StartTime.               ToIso8601())
                                                                 : null,
                                                             ChargeDetailRecord.ParkingTime != null && ChargeDetailRecord.ParkingTime.EndTime.HasValue
                                                                 ? new JProperty("parkingTimeEnd",             ChargeDetailRecord.ParkingTime.EndTime.Value.           ToString())
                                                                 : null,
                                                             ChargeDetailRecord.ParkingFee.HasValue
                                                                 ? new JProperty("parkingFee",                 ChargeDetailRecord.ParkingFee.                          ToString())
                                                                 : null)

                                                                 )
                                                         )
                                                     )

                                                  ));

                #endregion


                #region OnEVSEData/(Admin)StatusChanged

                NewRoamingNetwork.OnEVSEDataChanged += async (Timestamp,
                                                              EventTrackingId,
                                                              EVSE,
                                                              PropertyName,
                                                              OldValue,
                                                              NewValue)

                    => await DebugLog.SubmitEvent("OnEVSEDataChanged",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",        Timestamp.           ToIso8601()),
                                                      new JProperty("eventTrackingId",  EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",   NewRoamingNetwork.Id.ToString()),
                                                      new JProperty("EVSEId",           EVSE.Id.             ToString()),
                                                      new JProperty("propertyName",     PropertyName),
                                                      new JProperty("oldValue",         OldValue?.           ToString()),
                                                      new JProperty("newValue",         NewValue?.           ToString())
                                                  ));



                NewRoamingNetwork.OnEVSEStatusChanged += async (Timestamp,
                                                                EventTrackingId,
                                                                EVSE,
                                                                OldStatus,
                                                                NewStatus)

                    => await DebugLog.SubmitEvent("OnEVSEStatusChanged",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",        Timestamp.           ToIso8601()),
                                                      new JProperty("eventTrackingId",  EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",   NewRoamingNetwork.Id.ToString()),
                                                      new JProperty("EVSEId",           EVSE.Id.             ToString()),
                                                      new JProperty("oldStatus",        OldStatus.Value.     ToString()),
                                                      new JProperty("newStatus",        NewStatus.Value.     ToString())
                                                  ));



                NewRoamingNetwork.OnEVSEAdminStatusChanged += async (Timestamp,
                                                                     EventTrackingId,
                                                                     EVSE,
                                                                     OldStatus,
                                                                     NewStatus)

                    => await DebugLog.SubmitEvent("OnEVSEAdminStatusChanged",
                                                  JSONObject.Create(
                                                      new JProperty("timestamp",        Timestamp.           ToIso8601()),
                                                      new JProperty("eventTrackingId",  EventTrackingId.     ToString()),
                                                      new JProperty("roamingNetworkId",   NewRoamingNetwork.Id.ToString()),
                                                      new JProperty("EVSEId",           EVSE.Id.             ToString()),
                                                      new JProperty("oldStatus",        OldStatus.Value.     ToString()),
                                                      new JProperty("newStatus",        NewStatus.Value.     ToString())
                                                  ));

                #endregion

                #endregion

                return NewRoamingNetwork;

            }

            throw new RoamingNetworkAlreadyExists(Id);

        }

        #endregion

        #region GetAllRoamingNetworks(Hostname)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        public IEnumerable<RoamingNetwork> GetAllRoamingNetworks(HTTPHostname  Hostname)

            => WWCPHTTPServer.GetAllTenants(Hostname);

        #endregion

        #region GetRoamingNetwork(Hostname, RoamingNetworkId)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public RoamingNetwork GetRoamingNetwork(HTTPHostname       Hostname,
                                                RoamingNetwork_Id  RoamingNetworkId)

            => WWCPHTTPServer.GetAllTenants(Hostname).
                   FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

        #endregion

        #region TryGetRoamingNetwork(Hostname, RoamingNetworkId, out RoamingNetwork)

        /// <summary>
        ///Try to return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        /// <param name="RoamingNetwork">A roaming network.</param>
        public Boolean TryGetRoamingNetwork(HTTPHostname        Hostname,
                                            RoamingNetwork_Id   RoamingNetworkId,
                                            out RoamingNetwork  RoamingNetwork)
        {

            RoamingNetwork  = WWCPHTTPServer.GetAllTenants(Hostname).
                                  FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            return RoamingNetwork != null;

        }

        #endregion

        #region RoamingNetworkExists(Hostname, RoamingNetworkId)

        /// <summary>
        /// Check if a roaming networks exists for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public Boolean RoamingNetworkExists(HTTPHostname        Hostname,
                                            RoamingNetwork_Id   RoamingNetworkId)
        {

            var RoamingNetwork = WWCPHTTPServer.GetAllTenants(Hostname).
                                     FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            return RoamingNetwork != null;

        }

        #endregion

        #region RemoveRoamingNetwork(Hostname, RoamingNetworkId)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public RoamingNetwork RemoveRoamingNetwork(HTTPHostname       Hostname,
                                                   RoamingNetwork_Id  RoamingNetworkId)
        {

            if (!WWCPHTTPServer.TryGetTenants(Hostname, out RoamingNetworks _RoamingNetworks))
                return null;

            return _RoamingNetworks.RemoveRoamingNetwork(RoamingNetworkId);

        }

        #endregion

        #region TryRemoveRoamingNetwork(Hostname, RoamingNetworkId, out RoamingNetwork)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        /// <param name="RoamingNetwork">The removed roaming network.</param>
        public Boolean TryRemoveRoamingNetwork(HTTPHostname        Hostname,
                                               RoamingNetwork_Id   RoamingNetworkId,
                                               out RoamingNetwork  RoamingNetwork)
        {

            if (!WWCPHTTPServer.TryGetTenants(Hostname, out RoamingNetworks _RoamingNetworks))
            {
                RoamingNetwork = null;
                return false;
            }

            RoamingNetwork = _RoamingNetworks.RemoveRoamingNetwork(RoamingNetworkId);

            return RoamingNetwork != null;

        }

        #endregion

    }

}
