/*
 * Copyright (c) 2015-2026 GraphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of Open Charging Cloud API <http://www.github.com/OpenChargingCloud/OpenChargingCloudAPI>
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

using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.HTTPTest;
using org.GraphDefined.Vanaheimr.Hermod.SMTP;
using org.GraphDefined.Vanaheimr.Hermod.Mail;
using org.GraphDefined.Vanaheimr.Hermod.Logging;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.WWCP.Networking;

#endregion

namespace cloud.charging.open.API
{

    /// <summary>
    /// Extension methods for the Open Charging Cloud HTTP API.
    /// </summary>
    public static class OpenChargingCloudAPIExtensions
    {

        // Used by multiple HTTP content types

        public const String RoamingNetworkId           = "RoamingNetworkId";
        public const String ChargingStationOperatorId  = "ChargingStationOperatorId";
        public const String ChargingPoolId             = "ChargingPoolId";
        public const String ChargingStationId          = "ChargingStationId";
        public const String EVSEId                     = "EVSEId";
        public const String ChargingSessionId          = "ChargingSessionId";
        public const String ChargingReservationId      = "ChargingReservationId";

        public const String EMobilityProviderId        = "EMobilityProviderId";


        #region TryParseRoamingNetworkId                           (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetworkId,                              out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and roaming network identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetworkId">The roaming network identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkId(this HTTPRequest                                HTTPRequest,
                                                       OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                       [NotNullWhen(true)]  out RoamingNetwork_Id      RoamingNetworkId,
                                                       [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            HTTPResponseBuilder = null;

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.RoamingNetworkId,
                RoamingNetwork_Id.TryParse,
                out RoamingNetworkId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid roaming network identification '{RoamingNetworkId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetwork                             (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork,                                out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and roaming network.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetwork(this HTTPRequest                                HTTPRequest,
                                                     OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                     [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                     [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkId(
                OpenChargingCloudAPI,
                out var roamingNetworkId,
                out HTTPResponseBuilder))
            {
                RoamingNetwork = null;
                return false;
            }

            if (!OpenChargingCloudAPI.TryGetRoamingNetwork(HTTPRequest.Host,
                                                           roamingNetworkId,
                                                           out RoamingNetwork))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown roaming network identification '{roamingNetworkId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion


        #region TryParseRoamingNetworkAndChargingStationOperatorId (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperatorId, out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging station operator identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperatorId">The charging station operator identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingStationOperatorId(this HTTPRequest                                     HTTPRequest,
                                                                                 OpenChargingCloudAPI                                 OpenChargingCloudAPI,
                                                                                 [NotNullWhen(true)]  out IRoamingNetwork?            RoamingNetwork,
                                                                                 [NotNullWhen(true)]  out ChargingStationOperator_Id  ChargingStationOperatorId,
                                                                                 [NotNullWhen(false)] out HTTPResponse.Builder?       HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                ChargingStationOperatorId = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.ChargingStationOperatorId,
                ChargingStationOperator_Id.TryParse,
                out ChargingStationOperatorId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid charging station operator identification '{ChargingStationOperatorId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingStationOperator   (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator,   out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging station operator.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationOperator">The charging station operator.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingStationOperator(this HTTPRequest                                    HTTPRequest,
                                                                               OpenChargingCloudAPI                                OpenChargingCloudAPI,
                                                                               [NotNullWhen(true)]  out IRoamingNetwork?           RoamingNetwork,
                                                                               [NotNullWhen(true)]  out IChargingStationOperator?  ChargingStationOperator,
                                                                               [NotNullWhen(false)] out HTTPResponse.Builder?      HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndChargingStationOperatorId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingStationOperatorId,
                out HTTPResponseBuilder))
            {
                ChargingStationOperator = null;
                return false;
            }

            if (!RoamingNetwork.TryGetChargingStationOperatorById(chargingStationOperatorId, out ChargingStationOperator))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown charging station operator identification '{chargingStationOperatorId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingPoolId            (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPoolId,            out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging pool identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingPoolId">The charging pool identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingPoolId(this HTTPRequest                                HTTPRequest,
                                                                      OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                      [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                      [NotNullWhen(true)]  out ChargingPool_Id        ChargingPoolId,
                                                                      [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                ChargingPoolId = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.ChargingPoolId,
                ChargingPool_Id.TryParse,
                out ChargingPoolId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid charging pool identification '{ChargingPoolId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingPool              (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool,              out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging pool.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingPool">The charging pool.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingPool(this HTTPRequest                                HTTPRequest,
                                                                    OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                    [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                    [NotNullWhen(true)]  out IChargingPool?         ChargingPool,
                                                                    [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndChargingPoolId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingPoolId,
                out HTTPResponseBuilder))
            {
                ChargingPool = null;
                return false;
            }

            if (!RoamingNetwork.TryGetChargingPoolById(chargingPoolId, out ChargingPool))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown charging pool identification '{chargingPoolId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingStationId         (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationId,         out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging station identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStationId">The charging station identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingStationId(this HTTPRequest                                HTTPRequest,
                                                                         OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                         [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                         [NotNullWhen(true)]  out ChargingStation_Id     ChargingStationId,
                                                                         [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                ChargingStationId  = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.ChargingStationId,
                ChargingStation_Id.TryParse,
                out ChargingStationId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid charging station identification '{ChargingStationId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingStation           (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStation,           out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging station.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingStation">The charging station.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingStation(this HTTPRequest                                HTTPRequest,
                                                                       OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                       [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                       [NotNullWhen(true)]  out IChargingStation?      ChargingStation,
                                                                       [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndChargingStationId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingStationId,
                out HTTPResponseBuilder))
            {
                ChargingStation = null;
                return false;
            }

            if (!RoamingNetwork.TryGetChargingStationById(chargingStationId, out ChargingStation))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown charging station identification '{chargingStationId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndEVSEId                    (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EVSEId,                    out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and EVSE identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EVSEId">The EVSE identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndEVSEId(this HTTPRequest                                HTTPRequest,
                                                              OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                              [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                              [NotNullWhen(true)]  out EVSE_Id                EVSEId,
                                                              [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                EVSEId = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.EVSEId,
                EVSE_Id.TryParse,
                out EVSEId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid EVSE identification '{EVSEId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndEVSE                      (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EVSE,                      out HTTPResponseBuilder)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and EVSE
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EVSE">The EVSE.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndEVSE(this HTTPRequest                                HTTPRequest,
                                                            OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                            [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                            [NotNullWhen(true)]  out IEVSE?                 EVSE,
                                                            [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndEVSEId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var evseId,
                out HTTPResponseBuilder))
            {
                EVSE = null;
                return false;
            }

            if (!RoamingNetwork.TryGetEVSEById(evseId, out EVSE))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown EVSE identification '{evseId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion


        #region TryParseRoamingNetworkAndChargingSessionId         (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingSessionId,         out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging session identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingSessionId">The charging session identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingSessionId(this HTTPRequest                                HTTPRequest,
                                                                         OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                         [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                         [NotNullWhen(true)]  out ChargingSession_Id     ChargingSessionId,
                                                                         [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                ChargingSessionId  = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.ChargingSessionId,
                ChargingSession_Id.TryParse,
                out ChargingSessionId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid charging session identification '{ChargingSessionId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingSession           (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingSession,           out HTTPResponseBuilder)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging session
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingSession">The charging session.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndChargingSession(this HTTPRequest                                HTTPRequest,
                                                                       OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                       [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                       [NotNullWhen(true)]  out ChargingSession?       ChargingSession,
                                                                       [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndChargingSessionId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingSessionId,
                out HTTPResponseBuilder))
            {
                ChargingSession = null;
                return false;
            }

            if (!RoamingNetwork.TryGetChargingSessionById(chargingSessionId, out ChargingSession))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown charging session identification '{chargingSessionId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion


        #region TryParseRoamingNetworkAndReservationId             (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingReservationId,     out HTTPResponseBuilder)

        /// <summary>
        /// Try to parse the given HTTP request and return the roaming network and charging reservation identification.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingReservationId">The charging reservation identification.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndReservationId(this HTTPRequest                                 HTTPRequest,
                                                                     OpenChargingCloudAPI                             OpenChargingCloudAPI,
                                                                     [NotNullWhen(true)]  out IRoamingNetwork?        RoamingNetwork,
                                                                     [NotNullWhen(true)]  out ChargingReservation_Id  ChargingReservationId,
                                                                     [NotNullWhen(false)] out HTTPResponse.Builder?   HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetwork(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out HTTPResponseBuilder))
            {
                ChargingReservationId = default;
                return false;
            }

            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.ChargingReservationId,
                ChargingReservation_Id.TryParse,
                out ChargingReservationId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Invalid charging reservation identification '{ChargingReservationId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndReservation               (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingReservation,       out HTTPResponseBuilder)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging reservation
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="ChargingReservation">The charging reservation.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndReservation(this HTTPRequest                                HTTPRequest,
                                                                   OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                   [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                   [NotNullWhen(true)]  out ChargingReservation?   ChargingReservation,
                                                                   [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            if (!HTTPRequest.TryParseRoamingNetworkAndReservationId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingReservationId,
                out HTTPResponseBuilder))
            {
                ChargingReservation = null;
                return false;
            }

            if (!RoamingNetwork.TryGetChargingReservationById(chargingReservationId, out ChargingReservation))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown charging reservation identification '{chargingReservationId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            return true;

        }

        #endregion


        #region TryParseRoamingNetworkAndEMobilityProvider       (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EMobilityProvider,       out HTTPResponseBuilder)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and e-mobility provider
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EMobilityProvider">The charging station operator.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndEMobilityProvider(this HTTPRequest                                HTTPRequest,
                                                                      OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                      [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                      [NotNullWhen(true)]  out IEMobilityProvider?    EMobilityProvider,
                                                                      [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            RoamingNetwork       = null;
            EMobilityProvider    = null;

            if (HTTPRequest.ParsedURLParameters.Length < 2)
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponseBuilder))
                return false;


            if (!EMobilityProvider_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var eMobilityProviderId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EMobilityProviderId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetEMobilityProviderById(eMobilityProviderId, out EMobilityProvider))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EMobilityProviderId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndGridOperator(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out GridOperator, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and smart city
        /// for the given HTTP hostname and HTTP query parameters or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="GridOperator">The charging station operator.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkAndGridOperator(this HTTPRequest                                HTTPRequest,
                                                                 OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                 [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                 [NotNullWhen(true)]  out IGridOperator?         GridOperator,
                                                                 [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponse)
        {

            RoamingNetwork  = null;
            GridOperator    = null;

            if (HTTPRequest.ParsedURLParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!GridOperator_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var gridOperatorId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid GridOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetGridOperatorById(gridOperatorId, out GridOperator))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown GridOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion



        #region TryParseRoamingNetworkAndChargingSession           (this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out EMobilityProviderId, out ChargingSession,         out HTTPResponseBuilder)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network and charging session
        /// for the given HTTP hostname and HTTP query parameters
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="OpenChargingCloudAPI">The OpenChargingCloud API.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="EMobilityProviderId">The e-mobility provider identification.</param>
        /// <param name="ChargingSession">The charging session.</param>
        /// <param name="HTTPResponseBuilder">A HTTP error response.</param>
        public static Boolean TryParseRoamingNetworkEMobilityProviderIdAndChargingSession(this HTTPRequest                                HTTPRequest,
                                                                                          OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                                          [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                                          [NotNullWhen(true)]  out EMobilityProvider_Id   EMobilityProviderId,
                                                                                          [NotNullWhen(true)]  out ChargingSession?       ChargingSession,
                                                                                          [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponseBuilder)
        {

            EMobilityProviderId  = default;
            ChargingSession      = null;

            if (!HTTPRequest.TryParseRoamingNetworkAndChargingSessionId(
                OpenChargingCloudAPI,
                out RoamingNetwork,
                out var chargingSessionId,
                out HTTPResponseBuilder))
            {
                return false;
            }


            if (!HTTPRequest.TryParseURLParameter(
                OpenChargingCloudAPIExtensions.EMobilityProviderId,
                EMobilityProvider_Id.TryParse,
                out EMobilityProviderId))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown e-mobility provider identification '{EMobilityProviderId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }

            if (!RoamingNetwork.TryGetEMobilityProviderById(EMobilityProviderId, out var eMobilityProvider))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                                          HTTPStatusCode  = HTTPStatusCode.NotFound,
                                          Server          = OpenChargingCloudAPI.HTTPServerName,
                                          Date            = Timestamp.Now,
                                          ContentType     = HTTPContentType.Application.JSON_UTF8,
                                          Content         = $"{{ \"description\": \"Unknown e-mobility provider identification '{EMobilityProviderId}'!\" }}".ToUTF8Bytes(),
                                          Connection      = ConnectionType.KeepAlive
                                      };

                return false;

            }


            if (!eMobilityProvider.TryGetChargingSessionById(chargingSessionId, out ChargingSession))
            {

                HTTPResponseBuilder = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = $"{{ \"description\": \"Unknown charging session identification '{chargingSessionId}'!\" }}".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion



        #region TryParseRoamingNetworkAndParkingOperator(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ParkingOperator, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndParkingOperator(this HTTPRequest           HTTPRequest,
                                                                    OpenChargingCloudAPI       OpenChargingCloudAPI,
                                                                    out IRoamingNetwork?       RoamingNetwork,
                                                                    out ParkingOperator?       ParkingOperator,
                                                                    out HTTPResponse.Builder?  HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ParkingOperator  = null;

            if (HTTPRequest.ParsedURLParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI,
                                                 out RoamingNetwork,
                                                 out HTTPResponse))
            {
                return false;
            }


            if (!ParkingOperator_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var parkingOperatorId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ParkingOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetParkingOperatorById(parkingOperatorId, out ParkingOperator))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ParkingOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndSmartCity(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out SmartCity, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndSmartCity(this HTTPRequest           HTTPRequest,
                                                              OpenChargingCloudAPI       OpenChargingCloudAPI,
                                                              out IRoamingNetwork?       RoamingNetwork,
                                                              out SmartCityProxy?        SmartCity,
                                                              out HTTPResponse.Builder?  HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork  = null;
            SmartCity       = null;

            if (HTTPRequest.ParsedURLParameters.Length < 2)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!SmartCity_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out SmartCity_Id SmartCityId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid SmartCityId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetSmartCityById(SmartCityId, out SmartCity))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown SmartCityId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion




        #region TryParseRoamingNetworkAndChargingPoolAndChargingStation(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool, out ChargingStation, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndChargingPoolAndChargingStation(this HTTPRequest                                HTTPRequest,
                                                                                   OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                                   [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                                   [NotNullWhen(true)]  out IChargingPool?         ChargingPool,
                                                                                   [NotNullWhen(true)]  out IChargingStation?      ChargingStation,
                                                                                   [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponse)
        {

            RoamingNetwork   = null;
            ChargingPool     = null;
            ChargingStation  = null;

            if (HTTPRequest.ParsedURLParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;

            #region Get charging pool...

            if (!ChargingPool_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var chargingPoolId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingPoolById(chargingPoolId, out ChargingPool))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            #endregion

            #region Get charging station...

            if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURLParameters[2], out var chargingStationId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationById(chargingStationId, out ChargingStation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            #endregion

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingPool, out ChargingStation, out EVSE, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this HTTPRequest                                HTTPRequest,
                                                                                          OpenChargingCloudAPI                            OpenChargingCloudAPI,
                                                                                          [NotNullWhen(true)]  out IRoamingNetwork?       RoamingNetwork,
                                                                                          [NotNullWhen(true)]  out IChargingPool?         ChargingPool,
                                                                                          [NotNullWhen(true)]  out IChargingStation?      ChargingStation,
                                                                                          [NotNullWhen(true)]  out IEVSE?                 EVSE,
                                                                                          [NotNullWhen(false)] out HTTPResponse.Builder?  HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),      "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork   = null;
            ChargingPool     = null;
            ChargingStation  = null;
            EVSE             = null;

            if (HTTPRequest.ParsedURLParameters.Length < 4)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;

            #region Get charging pool...

            if (!ChargingPool_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var chargingPoolId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingPoolById(chargingPoolId, out ChargingPool))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingPoolId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            #endregion

            #region Get charging station...

            if (!ChargingStation_Id.TryParse(HTTPRequest.ParsedURLParameters[2], out var chargingStationId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationById(chargingStationId, out ChargingStation))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            #endregion

            #region Get EVSE

            if (!EVSE_Id.TryParse(HTTPRequest.ParsedURLParameters[3], out var evseId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetEVSEById(evseId, out EVSE))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EVSEId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            #endregion

            return true;

        }

        #endregion


        #region TryParseRoamingNetworkAndChargingStationOperatorAndBrand(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out Brand, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndChargingStationOperatorAndBrand(this HTTPRequest                                    HTTPRequest,
                                                                                    OpenChargingCloudAPI                                OpenChargingCloudAPI,
                                                                                    [NotNullWhen(true)]  out IRoamingNetwork?           RoamingNetwork,
                                                                                    [NotNullWhen(true)]  out IChargingStationOperator?  ChargingStationOperator,
                                                                                    [NotNullWhen(true)]  out Brand?                     Brand,
                                                                                    [NotNullWhen(false)] out HTTPResponse.Builder?      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            Brand                    = null;
            HTTPResponse             = null;

            if (HTTPRequest.ParsedURLParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var chargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(chargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }



            if (!Brand_Id.TryParse(HTTPRequest.ParsedURLParameters[2], out var brandId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid BrandId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            var brand = ChargingStationOperator.Brands.FirstOrDefault(brand => brand.Id == brandId);
            if (brand is null) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown BrandId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out ChargingStationGroup, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this HTTPRequest                                    HTTPRequest,
                                                                                                   OpenChargingCloudAPI                                OpenChargingCloudAPI,
                                                                                                   [NotNullWhen(true)]  out IRoamingNetwork?           RoamingNetwork,
                                                                                                   [NotNullWhen(true)]  out IChargingStationOperator?  ChargingStationOperator,
                                                                                                   [NotNullWhen(true)]  out ChargingStationGroup?      ChargingStationGroup,
                                                                                                   [NotNullWhen(false)] out HTTPResponse.Builder?      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),  "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            ChargingStationGroup     = null;

            if (HTTPRequest.ParsedURLParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = OpenChargingCloudAPI.HTTPServerName,
                                   Date            = Timestamp.Now,
                               };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var chargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(chargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }



            if (!ChargingStationGroup_Id.TryParse(HTTPRequest.ParsedURLParameters[2], out var chargingStationGroupId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationGroupId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!ChargingStationOperator.TryGetChargingStationGroup(chargingStationGroupId, out ChargingStationGroup)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationGroupId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion

        #region TryParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this HTTPRequest, OpenChargingCloudAPI, out RoamingNetwork, out ChargingStationOperator, out ChargingStationGroup, out HTTPResponse)

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
        public static Boolean TryParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this HTTPRequest                                    HTTPRequest,
                                                                                        OpenChargingCloudAPI                                OpenChargingCloudAPI,
                                                                                        [NotNullWhen(true)]  out IRoamingNetwork?           RoamingNetwork,
                                                                                        [NotNullWhen(true)]  out IChargingStationOperator?  ChargingStationOperator,
                                                                                        [NotNullWhen(true)]  out EVSEGroup?                 EVSEGroup,
                                                                                        [NotNullWhen(false)] out HTTPResponse.Builder?      HTTPResponse)
        {

            #region Initial checks

            if (HTTPRequest is null)
                throw new ArgumentNullException(nameof(HTTPRequest),           "The given HTTP request must not be null!");

            if (OpenChargingCloudAPI is null)
                throw new ArgumentNullException(nameof(OpenChargingCloudAPI),  "The given OpenChargingCloud API must not be null!");

            #endregion

            RoamingNetwork           = null;
            ChargingStationOperator  = null;
            EVSEGroup                = null;

            if (HTTPRequest.ParsedURLParameters.Length < 3)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                };

                return false;

            }


            if (!HTTPRequest.TryParseRoamingNetwork(OpenChargingCloudAPI, out RoamingNetwork, out HTTPResponse))
                return false;


            if (!ChargingStationOperator_Id.TryParse(HTTPRequest.ParsedURLParameters[1], out var chargingStationOperatorId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!RoamingNetwork.TryGetChargingStationOperatorById(chargingStationOperatorId, out ChargingStationOperator)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown ChargingStationOperatorId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }



            if (!EVSEGroup_Id.TryParse(HTTPRequest.ParsedURLParameters[2], out var evseGroupId)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid EVSEGroupId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            //ToDo: May fail for empty sequences!
            if (!ChargingStationOperator.TryGetEVSEGroup(evseGroupId, out EVSEGroup)) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = OpenChargingCloudAPI.HTTPServerName,
                    Date            = Timestamp.Now,
                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown EVSEGroupId!"" }".ToUTF8Bytes(),
                    Connection      = ConnectionType.KeepAlive
                };

                return false;

            }

            return true;

        }

        #endregion


        // Additional HTTP methods for HTTP clients

        #region RESERVE     (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP RESERVE request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> RESERVE(this AHTTPClient              HTTPClient,
                                                 HTTPPath                      Path,
                                                 Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                 IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.RESERVE,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region SETEXPIRED  (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP SETEXPIRED request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> SETEXPIRED(this AHTTPClient              HTTPClient,
                                                    HTTPPath                      Path,
                                                    Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                    IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.SETEXPIRED,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region AUTHSTART   (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP AUTHSTART request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> AUTHSTART(this AHTTPClient              HTTPClient,
                                                   HTTPPath                      Path,
                                                   Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                   IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.AUTHSTART,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region AUTHSTOP    (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP REMOTESTOP request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> AUTHSTOP(this AHTTPClient              HTTPClient,
                                                  HTTPPath                      Path,
                                                  Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                  IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.AUTHSTOP,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region REMOTESTART (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP REMOTESTART request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> REMOTESTART(this AHTTPClient              HTTPClient,
                                                     HTTPPath                      Path,
                                                     Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                     IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.REMOTESTART,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region REMOTESTOP  (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP REMOTESTOP request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> REMOTESTOP(this AHTTPClient              HTTPClient,
                                                    HTTPPath                      Path,
                                                    Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                    IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.REMOTESTOP,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion

        #region SENDCDR     (this HTTPClient, Path, ...)

        /// <summary>
        /// Create a new HTTP SENDCDR request.
        /// </summary>
        /// <param name="HTTPClient">A HTTP client.</param>
        /// <param name="Path">A HTTP path.</param>
        /// <param name="RequestBuilder">A delegate to configure the new HTTP request builder.</param>
        /// <param name="Authentication">An optional HTTP authentication.</param>
        public static Task<HTTPResponse> SENDCDR(this AHTTPClient              HTTPClient,
                                                 HTTPPath                      Path,
                                                 Action<HTTPRequest.Builder>?  RequestBuilder   = null,
                                                 IHTTPAuthentication?          Authentication   = null)

            => HTTPClient.Execute(
                   client => client.CreateRequest(
                                 OpenChargingCloudAPI.SENDCDR,
                                 Path,
                                 Authentication:  Authentication,
                                 RequestBuilder:  RequestBuilder
                             )
               );

        #endregion


    }


    /// <summary>
    /// The common Open Charging Cloud HTTP API.
    /// </summary>
    public class OpenChargingCloudAPI : HTTPExtAPIX
    {

        #region Data

        /// <summary>
        /// The default HTTP server name.
        /// </summary>
        public new const       String               DefaultHTTPServerName                          = "Open Charging Cloud API";

        /// <summary>
        /// The default HTTP service name.
        /// </summary>
        public new const       String               DefaultHTTPServiceName                         = "Open Charging Cloud API";

        /// <summary>
        /// The HTTP root for embedded resources.
        /// </summary>
        public new const       String               HTTPRoot                                       = "cloud.charging.open.API.HTTPRoot.";

        public const           String               DefaultOpenChargingCloudAPI_DatabaseFileName   = "OpenChargingCloudAPI.db";
        public const           String               DefaultOpenChargingCloudAPI_LogfileName        = "OpenChargingCloudAPI.log";

        public static readonly HTTPEventSource_Id   DebugLogId                                     = HTTPEventSource_Id.Parse("DebugLog");
        public static readonly HTTPEventSource_Id   ImporterLogId                                  = HTTPEventSource_Id.Parse("ImporterLog");
        public static readonly HTTPEventSource_Id   ForwardingInfosId                              = HTTPEventSource_Id.Parse("ForwardingInfos");

        public                 WWWAuthenticate      WWWAuthenticateDefaults                        = WWWAuthenticate.Basic("Open Charging Cloud");

        #endregion

        #region Additional HTTP methods

        public readonly static HTTPMethod RESERVE      = HTTPMethod.TryParse("RESERVE",     IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod SETEXPIRED   = HTTPMethod.TryParse("SETEXPIRED",  IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod AUTHSTART    = HTTPMethod.TryParse("AUTHSTART",   IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod AUTHSTOP     = HTTPMethod.TryParse("AUTHSTOP",    IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod REMOTESTART  = HTTPMethod.TryParse("REMOTESTART", IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod REMOTESTOP   = HTTPMethod.TryParse("REMOTESTOP",  IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod SENDCDR      = HTTPMethod.TryParse("SENDCDR",     IsSafe: false, IsIdempotent: true)!;
        public readonly static HTTPMethod RESENDCDR    = HTTPMethod.TryParse("RESENDCDR",   IsSafe: false, IsIdempotent: true)!;

        #endregion

        #region Properties

        /// <summary>
        /// The API version hash (git commit hash value).
        /// </summary>
        public new String                                   APIVersionHash                { get; }

        public String                                       OpenChargingCloudAPIPath      { get; }

        //public String                                       ChargingReservationsPath      { get; }
        //public String                                       ChargingSessionsPath          { get; }
        //public String                                       ChargeDetailRecordsPath       { get; }


        //public HTTPServer<RoamingNetworks, RoamingNetwork>  WWCPHTTPServer                { get; }

        /// <summary>
        /// Debug information via HTTP Server Sent Events.
        /// </summary>
    //    public HTTPEventSource<JObject>                     DebugLog                      { get; }

        /// <summary>
        /// Send importer information via HTTP Server Sent Events.
        /// </summary>
        public HTTPEventSource<JObject>                     ImporterLog                   { get; }

        /// <summary>
        /// Whether this API allows anonymous read access.
        /// </summary>
        public Boolean                                      AllowsAnonymousReadAccesss    { get; }

        #endregion

        #region Events

        #region (protected internal) CreateRoamingNetworkRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnCreateRoamingNetworkRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateRoamingNetworkRequest(DateTimeOffset     Timestamp,
                                                            HTTPAPIX           API,
                                                            HTTPRequest        Request,
                                                            CancellationToken  CancellationToken)

            => OnCreateRoamingNetworkRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) CreateRoamingNetworkResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnCreateRoamingNetworkResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateRoamingNetworkResponse(DateTimeOffset     Timestamp,
                                                             HTTPAPIX           API,
                                                             HTTPRequest        Request,
                                                             HTTPResponse       Response,
                                                             CancellationToken  CancellationToken)

            => OnCreateRoamingNetworkResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) DeleteRoamingNetworkRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnDeleteRoamingNetworkRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteRoamingNetworkRequest(DateTimeOffset     Timestamp,
                                                            HTTPAPIX           API,
                                                            HTTPRequest        Request,
                                                            CancellationToken  CancellationToken)

            => OnDeleteRoamingNetworkRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) DeleteRoamingNetworkResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnDeleteRoamingNetworkResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteRoamingNetworkResponse(DateTimeOffset     Timestamp,
                                                             HTTPAPIX           API,
                                                             HTTPRequest        Request,
                                                             HTTPResponse       Response,
                                                             CancellationToken  CancellationToken)

            => OnDeleteRoamingNetworkResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) GetChargingPoolsRequest   (Request)

        /// <summary>
        /// An event sent whenever a GetChargingPools request was received.
        /// </summary>
        public HTTPRequestLogEventX OnGetChargingPoolsRequest = new();

        /// <summary>
        /// An event sent whenever a GetChargingPools request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task GetChargingPoolsRequest(DateTimeOffset     Timestamp,
                                                        HTTPAPIX           API,
                                                        HTTPRequest        Request,
                                                        CancellationToken  CancellationToken)

            => OnGetChargingPoolsRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) GetChargingPoolsResponse  (Response)

        /// <summary>
        /// An event sent whenever a GetChargingPools response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnGetChargingPoolsResponse = new();

        /// <summary>
        /// An event sent whenever a GetChargingPools response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task GetChargingPoolsResponse(DateTimeOffset     Timestamp,
                                                         HTTPAPIX           API,
                                                         HTTPRequest        Request,
                                                         HTTPResponse       Response,
                                                         CancellationToken  CancellationToken)

            => OnGetChargingPoolsResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) CreateChargingPoolRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnCreateChargingPoolRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateChargingPoolRequest(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          CancellationToken  CancellationToken)

            => OnCreateChargingPoolRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) CreateChargingPoolResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnCreateChargingPoolResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateChargingPoolResponse(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           HTTPResponse       Response,
                                                           CancellationToken  CancellationToken)

            => OnCreateChargingPoolResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) DeleteChargingPoolRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnDeleteChargingPoolRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteChargingPoolRequest(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          CancellationToken  CancellationToken)

            => OnDeleteChargingPoolRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) DeleteChargingPoolResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnDeleteChargingPoolResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteChargingPoolResponse(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           HTTPResponse       Response,
                                                           CancellationToken  CancellationToken)

            => OnDeleteChargingPoolResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) GetChargingStationsRequest   (Request)

        /// <summary>
        /// An event sent whenever a GetChargingStations request was received.
        /// </summary>
        public HTTPRequestLogEventX OnGetChargingStationsRequest = new();

        /// <summary>
        /// An event sent whenever a GetChargingStations request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task GetChargingStationsRequest(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           CancellationToken  CancellationToken)

            => OnGetChargingStationsRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) GetChargingStationsResponse  (Response)

        /// <summary>
        /// An event sent whenever a GetChargingStations response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnGetChargingStationsResponse = new();

        /// <summary>
        /// An event sent whenever a GetChargingStations response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task GetChargingStationsResponse(DateTimeOffset     Timestamp,
                                                            HTTPAPIX           API,
                                                            HTTPRequest        Request,
                                                            HTTPResponse       Response,
                                                            CancellationToken  CancellationToken)

            => OnGetChargingStationsResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) CreateChargingStationRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnCreateChargingStationRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task CreateChargingStationRequest(DateTimeOffset     Timestamp,
                                                             HTTPAPIX           API,
                                                             HTTPRequest        Request,
                                                             CancellationToken  CancellationToken)

            => OnCreateChargingStationRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) CreateChargingStationResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnCreateChargingStationResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task CreateChargingStationResponse(DateTimeOffset     Timestamp,
                                                              HTTPAPIX           API,
                                                              HTTPRequest        Request,
                                                              HTTPResponse       Response,
                                                              CancellationToken  CancellationToken)

            => OnCreateChargingStationResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) DeleteChargingStationRequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnDeleteChargingStationRequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task DeleteChargingStationRequest(DateTimeOffset     Timestamp,
                                                             HTTPAPIX           API,
                                                             HTTPRequest        Request,
                                                             CancellationToken  CancellationToken)

            => OnDeleteChargingStationRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) DeleteChargingStationResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnDeleteChargingStationResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task DeleteChargingStationResponse(DateTimeOffset     Timestamp,
                                                              HTTPAPIX           API,
                                                              HTTPRequest        Request,
                                                              HTTPResponse       Response,
                                                              CancellationToken  CancellationToken)

            => OnDeleteChargingStationResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) GetEVSEsRequest   (Request)

        /// <summary>
        /// An event sent whenever a GetEVSEs request was received.
        /// </summary>
        public HTTPRequestLogEventX OnGetEVSEsRequest = new();

        /// <summary>
        /// An event sent whenever a GetEVSEs request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task GetEVSEsRequest(DateTimeOffset     Timestamp,
                                                HTTPAPIX           API,
                                                HTTPRequest        Request,
                                                CancellationToken  CancellationToken)

            => OnGetEVSEsRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) GetEVSEsResponse  (Response)

        /// <summary>
        /// An event sent whenever a GetEVSEs response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnGetEVSEsResponse = new();

        /// <summary>
        /// An event sent whenever a GetEVSEs response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task GetEVSEsResponse(DateTimeOffset     Timestamp,
                                                 HTTPAPIX           API,
                                                 HTTPRequest        Request,
                                                 HTTPResponse       Response,
                                                 CancellationToken  CancellationToken)

            => OnGetEVSEsResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) SendGetEVSEsStatusRequest (Request)

        /// <summary>
        /// An event sent whenever an EVSEs->Status request was received.
        /// </summary>
        public HTTPRequestLogEventX OnGetEVSEsStatusRequest = new();

        /// <summary>
        /// An event sent whenever an EVSEs->Status request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendGetEVSEsStatusRequest(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          CancellationToken  CancellationToken)

            => OnGetEVSEsStatusRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendGetEVSEsStatusResponse(Response)

        /// <summary>
        /// An event sent whenever an EVSEs->Status response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnGetEVSEsStatusResponse = new();

        /// <summary>
        /// An event sent whenever an EVSEs->Status response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendGetEVSEsStatusResponse(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           HTTPResponse       Response,
                                                           CancellationToken  CancellationToken)

            => OnGetEVSEsStatusResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) SendRemoteStartEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnSendRemoteStartEVSERequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendRemoteStartEVSERequest(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           CancellationToken  CancellationToken)

            => OnSendRemoteStartEVSERequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendRemoteStartEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSendRemoteStartEVSEResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendRemoteStartEVSEResponse(DateTimeOffset     Timestamp,
                                                            HTTPAPIX           API,
                                                            HTTPRequest        Request,
                                                            HTTPResponse       Response,
                                                            CancellationToken  CancellationToken)

            => OnSendRemoteStartEVSEResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) SendRemoteStopEVSERequest (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnSendRemoteStopEVSERequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendRemoteStopEVSERequest(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          CancellationToken  CancellationToken)

            => OnSendRemoteStopEVSERequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendRemoteStopEVSEResponse(Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSendRemoteStopEVSEResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendRemoteStopEVSEResponse(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           HTTPResponse       Response,
                                                           CancellationToken  CancellationToken)

            => OnSendRemoteStopEVSEResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) SendReserveEVSERequest     (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnSendReserveEVSERequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendReserveEVSERequest(DateTimeOffset     Timestamp,
                                                       HTTPAPIX           API,
                                                       HTTPRequest        Request,
                                                       CancellationToken  CancellationToken)

            => OnSendReserveEVSERequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendReserveEVSEResponse    (Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSendReserveEVSEResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendReserveEVSEResponse(DateTimeOffset     Timestamp,
                                                        HTTPAPIX           API,
                                                        HTTPRequest        Request,
                                                        HTTPResponse       Response,
                                                        CancellationToken  CancellationToken)

            => OnSendReserveEVSEResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) SendTokenAuthRequest   (Request)

        /// <summary>
        /// An event sent whenever a TokenAuth request was received.
        /// </summary>
        public HTTPRequestLogEventX OnTokenAuthRequest = new();

        /// <summary>
        /// An event sent whenever a TokenAuth request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendTokenAuthRequest(DateTimeOffset     Timestamp,
                                                     HTTPAPIX           API,
                                                     HTTPRequest        Request,
                                                     CancellationToken  CancellationToken)

            => OnTokenAuthRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendTokenAuthResponse  (Response)

        /// <summary>
        /// An event sent whenever a TokenAuth response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnTokenAuthResponse = new();

        /// <summary>
        /// An event sent whenever a TokenAuth response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendTokenAuthResponse(DateTimeOffset     Timestamp,
                                                      HTTPAPIX           API,
                                                      HTTPRequest        Request,
                                                      HTTPResponse       Response,
                                                      CancellationToken  CancellationToken)

            => OnTokenAuthResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion



        #region (protected internal) SendAuthStartEVSERequest   (Request)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnAuthStartEVSERequest = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendAuthStartEVSERequest(DateTimeOffset     Timestamp,
                                                         HTTPAPIX           API,
                                                         HTTPRequest        Request,
                                                         CancellationToken  CancellationToken)

            => OnAuthStartEVSERequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendAuthStartEVSEResponse  (Response)

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnAuthStartEVSEResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate start EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendAuthStartEVSEResponse(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          HTTPResponse       Response,
                                                          CancellationToken  CancellationToken)

            => OnAuthStartEVSEResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) SendAuthStopEVSERequest    (Request)

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE request was received.
        /// </summary>
        public HTTPRequestLogEventX OnAuthStopEVSERequest = new();

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE request was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendAuthStopEVSERequest(DateTimeOffset     Timestamp,
                                                        HTTPAPIX           API,
                                                        HTTPRequest        Request,
                                                        CancellationToken  CancellationToken)

            => OnAuthStopEVSERequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendAuthStopEVSEResponse   (Response)

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnAuthStopEVSEResponse = new();

        /// <summary>
        /// An event sent whenever a authenticate stop EVSE response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendAuthStopEVSEResponse(DateTimeOffset     Timestamp,
                                                         HTTPAPIX           API,
                                                         HTTPRequest        Request,
                                                         HTTPResponse       Response,
                                                         CancellationToken  CancellationToken)

            => OnAuthStopEVSEResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) SendCDRsRequest            (Request)

        /// <summary>
        /// An event sent whenever a charge detail record was received.
        /// </summary>
        public HTTPRequestLogEventX OnSendCDRsRequest = new();

        /// <summary>
        /// An event sent whenever a charge detail record was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SendCDRsRequest(DateTimeOffset     Timestamp,
                                                HTTPAPIX           API,
                                                HTTPRequest        Request,
                                                CancellationToken  CancellationToken)

            => OnSendCDRsRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SendCDRsResponse           (Response)

        /// <summary>
        /// An event sent whenever a charge detail record response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSendCDRsResponse = new();

        /// <summary>
        /// An event sent whenever a charge detail record response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SendCDRsResponse(DateTimeOffset     Timestamp,
                                                 HTTPAPIX           API,
                                                 HTTPRequest        Request,
                                                 HTTPResponse       Response,
                                                 CancellationToken  CancellationToken)

            => OnSendCDRsResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion


        #region (protected internal) SetEVSEAdminStatusRequest            (Request)

        /// <summary>
        /// An event sent whenever a Set EVSE Admin Status was received.
        /// </summary>
        public HTTPRequestLogEventX OnSetEVSEAdminStatusRequest = new();

        /// <summary>
        /// An event sent whenever a Set EVSE Admin Status was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SetEVSEAdminStatusRequest(DateTimeOffset     Timestamp,
                                                          HTTPAPIX           API,
                                                          HTTPRequest        Request,
                                                          CancellationToken  CancellationToken)

            => OnSetEVSEAdminStatusRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SetEVSEAdminStatusResponse           (Response)

        /// <summary>
        /// An event sent whenever a Set EVSE Admin Status response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSetEVSEAdminStatusResponse = new();

        /// <summary>
        /// An event sent whenever a Set EVSE Admin Status response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SetEVSEAdminStatusResponse(DateTimeOffset     Timestamp,
                                                           HTTPAPIX           API,
                                                           HTTPRequest        Request,
                                                           HTTPResponse       Response,
                                                           CancellationToken  CancellationToken)

            => OnSetEVSEAdminStatusResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SetEVSEStatusRequest                 (Request)

        /// <summary>
        /// An event sent whenever a Set EVSE Status was received.
        /// </summary>
        public HTTPRequestLogEventX OnSetEVSEStatusRequest = new();

        /// <summary>
        /// An event sent whenever a Set EVSE Status was received.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        protected internal Task SetEVSEStatusRequest(DateTimeOffset     Timestamp,
                                                     HTTPAPIX           API,
                                                     HTTPRequest        Request,
                                                     CancellationToken  CancellationToken)

            => OnSetEVSEStatusRequest.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   CancellationToken
               );

        #endregion

        #region (protected internal) SetEVSEStatusResponse                (Response)

        /// <summary>
        /// An event sent whenever a Set EVSE Status response was sent.
        /// </summary>
        public HTTPResponseLogEventX OnSetEVSEStatusResponse = new();

        /// <summary>
        /// An event sent whenever a Set EVSE Status response was sent.
        /// </summary>
        /// <param name="Timestamp">The timestamp of the request.</param>
        /// <param name="API">The HTTP API.</param>
        /// <param name="Request">A HTTP request.</param>
        /// <param name="Response">A HTTP response.</param>
        protected internal Task SetEVSEStatusResponse(DateTimeOffset     Timestamp,
                                                      HTTPAPIX           API,
                                                      HTTPRequest        Request,
                                                      HTTPResponse       Response,
                                                      CancellationToken  CancellationToken)

            => OnSetEVSEStatusResponse.WhenAll(
                   Timestamp,
                   API,
                   Request,
                   Response,
                   CancellationToken
               );

        #endregion

        #endregion

        #region Custom JSON serializers

        public CustomJObjectSerializerDelegate<ReceivedCDRInfo>?     CustomCDRReceivedInfoSerializer       { get; set; }
        public CustomJObjectSerializerDelegate<ChargeDetailRecord>?  CustomChargeDetailRecordSerializer    { get; set; }
        public CustomJObjectSerializerDelegate<SendCDRResult>?       CustomSendCDRResultSerializer         { get; set; }
        public CustomJObjectSerializerDelegate<ChargingSession>?     CustomChargingSessionSerializer       { get; set; }

        #endregion

        #region E-Mail delegates

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an instance of the Open Charging Cloud API.
        /// </summary>
        /// <param name="HTTPHostname">The HTTP hostname for all URLs within this API.</param>
        /// <param name="ExternalDNSName">The official URL/DNS name of this service, e.g. for sending e-mails.</param>
        /// <param name="BasePath">When the API is served from an optional subdirectory path.</param>
        /// <param name="HTTPServerName">The default HTTP server name, used whenever no HTTP Host-header has been given.</param>
        /// 
        /// <param name="URLPathPrefix">A common prefix for all URLs.</param>
        /// <param name="HTTPServiceName">The name of the HTTP service.</param>
        /// <param name="APIVersionHashes">The API version hashes (git commit hash values).</param>
        /// 
        /// <param name="APIRobotEMailAddress">An e-mail address for this API.</param>
        /// <param name="APIRobotGPGPassphrase">A GPG passphrase for this API.</param>
        /// <param name="SMTPClient">A SMTP client for sending e-mails.</param>
        /// 
        /// <param name="PasswordQualityCheck">A delegate to ensure a minimal password quality.</param>
        /// <param name="CookieName">The name of the HTTP Cookie for authentication.</param>
        /// <param name="UseSecureCookies">Force the web browser to send cookies only via HTTPS.</param>
        /// 
        /// <param name="RemoteAuthServers">Servers for remote authorization.</param>
        /// <param name="RemoteAuthAPIKeys">API keys for incoming remote authorizations.</param>
        /// 
        /// <param name="IsDevelopment">This HTTP API runs in development mode.</param>
        /// <param name="DevelopmentServers">An enumeration of server names which will imply to run this service in development mode.</param>
        /// <param name="SkipURLTemplates">Skip URL templates.</param>
        /// <param name="DatabaseFileName">The name of the database file for this API.</param>
        /// <param name="DisableNotifications">Disable external notifications.</param>
        /// <param name="DisableLogging">Disable the log file.</param>
        /// <param name="LoggingPath">The path for all logfiles.</param>
        /// <param name="LogfileName">The name of the logfile.</param>
        /// <param name="LogfileCreator">A delegate for creating the name of the logfile for this API.</param>
        public OpenChargingCloudAPI(HTTPTestServerX?               HTTPTestServer              = null,
                                    IEnumerable<HTTPHostname>?     Hostnames                   = null,
                                    HTTPPath?                      RootPath                    = null,
                                    IEnumerable<HTTPContentType>?  HTTPContentTypes            = null,
                                    I18NString?                    Description                 = null,

                                    String?                        ExternalDNSName             = null,
                                    HTTPPath?                      BasePath                    = null,

                                    String?                        HTTPServerName              = DefaultHTTPServerName,
                                    String?                        HTTPServiceName             = DefaultHTTPServiceName,
                                    String?                        APIVersionHash              = null,
                                    JObject?                       APIVersionHashes            = null,

                                    Organization_Id?               AdminOrganizationId         = null,
                                    EMailAddress?                  APIRobotEMailAddress        = null,
                                    String?                        APIRobotGPGPassphrase       = null,
                                    ISMTPClient?                   SMTPClient                  = null,

                                    PasswordQualityCheckDelegate?  PasswordQualityCheck        = null,
                                    HTTPCookieName?                CookieName                  = null,
                                    Boolean                        UseSecureCookies            = true,
                                    Languages?                     DefaultLanguage             = null,

                                    IEnumerable<URLWithAPIKey>?    RemoteAuthServers           = null,
                                    IEnumerable<APIKey_Id>?        RemoteAuthAPIKeys           = null,

                                    Boolean?                       AllowsAnonymousReadAccess   = true,

                                    ServiceCheckKeys?              ServiceCheckKeys            = null,

                                    Boolean?                       IsDevelopment               = null,
                                    IEnumerable<String>?           DevelopmentServers          = null,
                                    Boolean                        SkipURLTemplates            = false,
                                    String                         DatabaseFileName            = DefaultOpenChargingCloudAPI_DatabaseFileName,
                                    Boolean                        DisableNotifications        = false,
                                    Boolean                        DisableLogging              = false,
                                    String                         LoggingPath                 = "", //DefaultHTTPAPI_LoggingPath,
                                    String                         LoggingContext              = "", //DefaultLoggingContext,
                                    String                         LogfileName                 = DefaultOpenChargingCloudAPI_LogfileName,
                                    LogfileCreatorDelegate?        LogfileCreator              = null)

            : base(HTTPTestServer,
                   Hostnames,
                   RootPath,
                   HTTPContentTypes,
                   Description,

                   ExternalDNSName,
                   BasePath,

                   HTTPServerName       ?? DefaultHTTPServerName,
                   HTTPServiceName      ?? DefaultHTTPServiceName,
                   APIVersionHash,
                   APIVersionHashes,

                   AdminOrganizationId,
                   APIRobotEMailAddress,
                   APIRobotGPGPassphrase,
                   SMTPClient,
                   //SMSClient,
                   //SMSSenderName,
                   //TelegramClient,

                   PasswordQualityCheck,
                   CookieName           ?? HTTPCookieName.Parse(nameof(OpenChargingCloudAPI)),
                   UseSecureCookies,
                   TimeSpan.FromDays(30),
                   DefaultLanguage      ?? Languages.en,
                   4,
                   4,
                   4,
                   5,
                   20,
                   8,
                   4,
                   4,

                   RemoteAuthServers,
                   RemoteAuthAPIKeys,

                   ServiceCheckKeys,

                   IsDevelopment,
                   DevelopmentServers,
                   SkipURLTemplates,
                   DatabaseFileName     ?? DefaultOpenChargingCloudAPI_DatabaseFileName,
                   DisableNotifications,
                   DisableLogging,
                   LoggingPath,
                   LoggingContext,
                   LogfileName          ?? DefaultOpenChargingCloudAPI_LogfileName,
                   LogfileCreator)

        {

            this.APIVersionHash              = APIVersionHashes?[nameof(OpenChargingCloudAPI)]?.Value<String>()?.Trim() ?? "";
            this.AllowsAnonymousReadAccesss  = AllowsAnonymousReadAccess ?? true;

            this.OpenChargingCloudAPIPath    = Path.Combine(this.LoggingPath, "OpenChargingCloudAPI");
            //this.ChargingReservationsPath    = Path.Combine(OpenChargingCloudAPIPath, "ChargingReservations");
            //this.ChargingSessionsPath        = Path.Combine(OpenChargingCloudAPIPath, "ChargingSessions");
            //this.ChargeDetailRecordsPath     = Path.Combine(OpenChargingCloudAPIPath, "ChargeDetailRecords");

            if (!DisableLogging)
            {
                Directory.CreateDirectory(OpenChargingCloudAPIPath);
                //Directory.CreateDirectory(ChargingReservationsPath);
                //Directory.CreateDirectory(ChargingSessionsPath);
                //Directory.CreateDirectory(ChargeDetailRecordsPath);
            }

            //WWCP = OpenChargingCloudAPI.AttachToHTTPAPI(HTTPServer);

            //this.WWCPHTTPServer = new HTTPServer<RoamingNetworks, RoamingNetwork>(HTTPServer);

            //DebugLog     = HTTPServer.AddJSONEventSource(EventIdentification:      DebugLogId,
            //                                             HTTPAPI:                  this,
            //                                             URLTemplate:              this.URLPathPrefix + DebugLogId.ToString(),
            //                                             MaxNumberOfCachedEvents:  1000,
            //                                             RetryInterval :           TimeSpan.FromSeconds(5),
            //                                             EnableLogging:            true,
            //                                             LogfilePath:              this.OpenChargingCloudAPIPath);

            //ImporterLog  = HTTPServer.AddJSONEventSource(EventIdentification:      ImporterLogId,
            //                                             HTTPAPI:                  this,
            //                                             URLTemplate:              this.URLPathPrefix + ImporterLogId.ToString(),
            //                                             MaxNumberOfCachedEvents:  1000,
            //                                             RetryInterval :           TimeSpan.FromSeconds(5),
            //                                             EnableLogging:            true,
            //                                             LogfilePath:              this.OpenChargingCloudAPIPath);

            //RegisterNotifications().Wait();
            RegisterURLTemplates();

            //this.HTMLTemplate = HTMLTemplate ?? GetResourceString("template.html");

            DebugX.Log($"{nameof(OpenChargingCloudAPI)} version '{APIVersionHash}' initialized...");

        }

        #endregion


        #region (private) RegisterURLTemplates()

        #region Manage HTTP Resources

        #region (protected override) GetResourceStream      (ResourceName)

        protected override Stream? GetResourceStream(String ResourceName)

            => GetResourceStream(ResourceName,
                                 new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                 //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                 new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly));

        #endregion

        #region (protected override) GetResourceMemoryStream(ResourceName)

        protected override MemoryStream? GetResourceMemoryStream(String ResourceName)

            => GetResourceMemoryStream(ResourceName,
                                       new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                       //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                       new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly));

        #endregion

        #region (protected override) GetResourceString      (ResourceName)

        protected override String GetResourceString(String ResourceName)

            => GetResourceString(ResourceName,
                                 new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                 //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                 new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly));

        #endregion

        #region (protected override) GetResourceBytes       (ResourceName)

        protected override Byte[] GetResourceBytes(String ResourceName)

            => GetResourceBytes(ResourceName,
                                new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly));

        #endregion

        #region (protected override) MixWithHTMLTemplate    (ResourceName)

        protected override String MixWithHTMLTemplate(String ResourceName)

            => MixWithHTMLTemplate(ResourceName,
                                   new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                   //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                   new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly));

        #endregion

        #region (protected override) MixWithHTMLTemplate    (Template, ResourceName, Content = null)

        protected override String MixWithHTMLTemplate(String   Template,
                                                      String   ResourceName,
                                                      String?  Content   = null)

            => MixWithHTMLTemplate(Template,
                                   ResourceName,
                                   [
                                       new Tuple<String, Assembly>(OpenChargingCloudAPI.HTTPRoot, typeof(OpenChargingCloudAPI).Assembly),
                                       //new Tuple<String, Assembly>(UsersAPI.            HTTPRoot, typeof(UsersAPI).            Assembly),
                                       new Tuple<String, Assembly>(HTTPAPI.             HTTPRoot, typeof(HTTPAPI).             Assembly)
                                   ],
                                   Content);

        #endregion

        #endregion

        private void RegisterURLTemplates()
        {

            //HTTPServer.AddAuth(request => {

            //    #region Allow some URLs for anonymous access...

            //    if (request.Path.Equals    (URLPathPrefix)                      ||
            //        request.Path.Equals    (URLPathPrefix + "/impress")         ||
            //        request.Path.StartsWith(URLPathPrefix + "/GPGKeys")         ||
            //        request.Path.StartsWith(URLPathPrefix + "/chargy/versions") ||
            //        request.Path.StartsWith(URLPathPrefix + "/chargy/issues")   ||
            //        request.Path.StartsWith(URLPathPrefix + "/shared/OpenChargingCloudAPI/libs/leaflet") ||
            //        request.Path.StartsWith(URLPathPrefix + "/RNs"))
            //    {
            //        return Anonymous;
            //    }

            //    #endregion

            //    return null;

            //});

            ////HTTPServer.AddFilter(request => {
            ////    return null;
            ////});

            //HTTPServer.Rewrite  (request => {

            //    #region /               => /dashboard/index.shtml

            //    //if ((request.Path == URLPathPrefix || request.Path == (URLPathPrefix + "/")) &&
            //    //    request.HTTPMethod == HTTPMethod.GET &&
            //    //    TryGetSecurityTokenFromCookie(request, out SecurityToken_Id SecurityToken) &&
            //    //    _HTTPCookies.ContainsKey(SecurityToken))
            //    //{

            //    //    return new HTTPRequest.Builder(request) {
            //    //        Path = URLPathPrefix + HTTPPath.Parse("/dashboard/index.shtml")
            //    //    };

            //    //}

            //    #endregion

            //    #region /profile        => /profile/profile.shtml

            //    //if ((request.Path == URLPathPrefix + "/profile" ||
            //    //     request.Path == URLPathPrefix + "/profile/") &&
            //    //     request.HTTPMethod == HTTPMethod.GET)
            //    //{

            //    //    return new HTTPRequest.Builder(request) {
            //    //        Path = URLPathPrefix + HTTPPath.Parse("/profile/profile.shtml")
            //    //    };

            //    //}

            //    #endregion

            //    #region /admin          => /admin/index.shtml

            //    //if (request.Path == URLPathPrefix + "/admin" &&
            //    //    request.HTTPMethod == HTTPMethod.GET)
            //    //{

            //    //    return new HTTPRequest.Builder(request) {
            //    //        Path = URLPathPrefix + HTTPPath.Parse("/admin/index.shtml")
            //    //    };

            //    //}

            //    #endregion

            //    return request;

            //});



            #region /shared/OpenChargingCloudAPI

            //this.MapResourceAssemblyFolder(HTTPHostname.Any,
            //                               HTTPPath.Parse("/shared/OpenChargingCloudAPI"),
            //                               HTTPRoot[..^1]);

            #endregion

            var URLPathPrefix = HTTPPath.Root;

            #region / (HTTPRoot)

//            AddHandler(

//                HTTPHostname.Any,
//                HTTPMethod.GET,
//                [
//                    URLPathPrefix + HTTPPath.Parse("/index.html"),
//                    URLPathPrefix + HTTPPath.Parse("/"),
//                    URLPathPrefix + HTTPPath.Parse("/{FileName}")
//                ],
//                OpenEnd:       true,
//                HTTPDelegate:  request => {

//                    #region Get file path

//                    var filePath = (request.ParsedURLParameters is not null && request.ParsedURLParameters.Length > 0)
//                                        ? request.ParsedURLParameters.Last().Replace('/', '.')
//                                        : "index.html";

//                    if (filePath.EndsWith('.'))
//                        filePath += "index.shtml";

//                    #endregion

//                    #region The resource is a templated HTML file...

//                    if (filePath.EndsWith(".shtml", StringComparison.Ordinal))
//                    {

//                        var file = MixWithHTMLTemplate(filePath);

//                        if (file.IsNullOrEmpty())
//                            return Task.FromResult(
//                                new HTTPResponse.Builder(request) {
//                                    HTTPStatusCode  = HTTPStatusCode.NotFound,
//                                    Server          = HTTPTestServer?.HTTPServerName,
//                                    Date            = Timestamp.Now,
//                                    CacheControl    = "public, max-age=300",
//                                    Connection      = ConnectionType.KeepAlive
//                                }.AsImmutable);

//                        else
//                            return Task.FromResult(
//                                new HTTPResponse.Builder(request) {
//                                    HTTPStatusCode  = HTTPStatusCode.OK,
//                                    ContentType     = HTTPContentType.Text.HTML_UTF8,
//                                    Content         = file.ToUTF8Bytes(),
//                                    CacheControl    = "public, max-age=300",
//                                    Connection      = ConnectionType.KeepAlive
//                                }.AsImmutable);

//                    }

//                    #endregion

//                    else
//                    {

//                        var resourceStream = GetResourceStream(filePath);

//                        #region File not found!

//                        if (resourceStream is null)
//                            return Task.FromResult(
//                                new HTTPResponse.Builder(request) {
//                                    HTTPStatusCode  = HTTPStatusCode.NotFound,
//                                    Server          = HTTPTestServer?.HTTPServerName,
//                                    Date            = Timestamp.Now,
//                                    CacheControl    = "public, max-age=300",
//                                    Connection      = ConnectionType.KeepAlive
//                                }.AsImmutable);

//                        #endregion

//                        #region Choose HTTP content type based on the file name extension of the requested resource...

//                        var fileName             = filePath[(filePath.LastIndexOf("/") + 1)..];

//                        var responseContentType  = fileName.Remove(0, fileName.LastIndexOf(".") + 1) switch {

//                            "htm"   => HTTPContentType.Text.HTML_UTF8,
//                            "html"  => HTTPContentType.Text.HTML_UTF8,
//                            "css"   => HTTPContentType.Text.CSS_UTF8,
//                            "gif"   => HTTPContentType.Image.GIF,
//                            "jpg"   => HTTPContentType.Image.JPEG,
//                            "jpeg"  => HTTPContentType.Image.JPEG,
//                            "svg"   => HTTPContentType.Image.SVG,
//                            "png"   => HTTPContentType.Image.PNG,
//                            "ico"   => HTTPContentType.Image.ICO,
//                            "js"    => HTTPContentType.Text.JAVASCRIPT_UTF8,
//                            "txt"   => HTTPContentType.Text.PLAIN,
//                            "xml"   => HTTPContentType.Text.XML_UTF8,

//                            _       => HTTPContentType.Application.OCTETSTREAM,

//                        };

//                        #endregion

//                        #region Create HTTP response

//                        return Task.FromResult(
//                            new HTTPResponse.Builder(request) {
//                                HTTPStatusCode  = HTTPStatusCode.OK,
//                                Server          = HTTPTestServer?.HTTPServerName,
//                                Date            = Timestamp.Now,
//                                ContentType     = responseContentType,
//                                ContentStream   = resourceStream,
//                                CacheControl    = "public, max-age=300",
//                                //Expires          = "Mon, 25 Jun 2015 21:31:12 GMT",
////                                              KeepAlive       = new KeepAliveType(TimeSpan.FromMinutes(5), 500),
////                                              Connection      = "Keep-Alive",
//                                Connection      = ConnectionType.KeepAlive
//                            }.AsImmutable);

//                        #endregion

//                    }

//                },
//                AllowReplacement: URLReplacement.Allow

//            );

            #endregion


            #region ~/impress

            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "impress",
                HTTPContentType.Text.HTML_UTF8,
                HTTPDelegate: request =>

                    Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ContentType                 = HTTPContentType.Text.HTML_UTF8,
                            Content                     = GetResourceBytes("legal.impress.html"),
                            Connection                  = ConnectionType.KeepAlive,
                            Vary                        = "Accept"
                        }.AsImmutable),

                AllowReplacement: URLReplacement.Allow

            );

            #endregion




            #region ~/dashboard

            #region GET         ~/dashboard

            // ----------------------------------------------------------------
            // curl -v -H "Accept: text/html" http://127.0.0.1:3001/dashboard
            // ----------------------------------------------------------------
            AddHandler(
                              HTTPMethod.GET,
                              URLPathPrefix + "dashboard",
                              HTTPContentType.Text.HTML_UTF8,
                              HTTPDelegate: Request => {

                                  #region Get HTTP user and its organizations

                                  // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                                  if (!TryGetHTTPUser(Request,
                                                      out var HTTPUser,
                                                      out var HTTPOrganizations,
                                                      out var Response,
                                                      Recursive: true))
                                  {
                                      return Task.FromResult(Response.AsImmutable);
                                  }

                                  #endregion


                                  return Task.FromResult(
                                      new HTTPResponse.Builder(Request) {
                                          HTTPStatusCode              = HTTPStatusCode.OK,
                                          Server                      = HTTPServer.HTTPServerName,
                                          Date                        = Timestamp.Now,
                                          AccessControlAllowOrigin    = "*",
                                          AccessControlAllowMethods   = [ "GET" ],
                                          AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                                          ContentType                 = HTTPContentType.Text.HTML_UTF8,
                                          Content                     = MixWithHTMLTemplate("dashboard.dashboard2.shtml").ToUTF8Bytes(),
                                          Connection                  = ConnectionType.KeepAlive,
                                          Vary                        = "Accept"
                                      }.AsImmutable);

                              }, AllowReplacement: URLReplacement.Allow);

            #endregion

            #endregion


            #region ~/chargy/versions

            #region GET         ~/chargy/versions

            // -----------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/chargy/versions
            // -----------------------------------------------------------------------------
            AddHandler(
                              HTTPMethod.GET,
                              URLPathPrefix + "chargy/versions",
                              HTTPContentType.Application.JSON_UTF8,
                              HTTPDelegate: Request => {

                                  var skip                    = Request.QueryString.GetUInt64("skip");
                                  var take                    = Request.QueryString.GetUInt64("take");

                                  return Task.FromResult(
                                      new HTTPResponse.Builder(Request) {
                                          HTTPStatusCode                = HTTPStatusCode.OK,
                                          Server                        = HTTPServer.HTTPServerName,
                                          Date                          = Timestamp.Now,
                                          AccessControlAllowOrigin      = "*",
                                          AccessControlAllowMethods     = [ "GET", "OPTIONS" ],
                                          AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                          ETag                          = "1",
                                          ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                          Content                       = new JArray().
                                                                              ToUTF8Bytes(),
                                          X_ExpectedTotalNumberOfItems  = 0
                                      }.AsImmutable);

                              });

            #endregion

            #region OPTIONS     ~/chargy/versions

            // ----------------------------------------------------------
            // curl -v -X OPTIONS http://127.0.0.1:5500/chargy/versions
            // ----------------------------------------------------------
            AddHandler(
                              HTTPMethod.OPTIONS,
                              URLPathPrefix + "chargy/versions",
                              HTTPDelegate: Request =>

                                  Task.FromResult(
                                      new HTTPResponse.Builder(Request) {
                                          HTTPStatusCode               = HTTPStatusCode.NoContent,
                                          Server                       = HTTPServer.HTTPServerName,
                                          Date                         = Timestamp.Now,
                                          AccessControlAllowOrigin     = "*",
                                          AccessControlAllowMethods    = [ "GET", "OPTIONS" ],
                                          AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                                      }.AsImmutable)

                              );

            #endregion

            #endregion

            #region ~/chargy/issues

            #region POST        ~/chargy/issues

            // -----------------------------------------------------------------------------------------------------------------------
            // curl -v -X POST -H "Content-Type: application/json" -d "{ \"hello\": \"world\" }" http://127.0.0.1:5500/chargy/issues
            // -----------------------------------------------------------------------------------------------------------------------
            AddHandler(
                              HTTPMethod.POST,
                              URLPathPrefix + "chargy/issues",
                              HTTPContentType.Application.JSON_UTF8,
                              //HTTPRequestLogger:  PostChargyIssueRequest,
                              //HTTPResponseLogger: PostChargyIssueResponse,
                              HTTPDelegate: async Request => {

                                  #region Parse JSON

                                  if (!Request.TryParseJSONObjectRequestBody(out var json,
                                                                             out var httpResponse))
                                  {
                                      return httpResponse!;
                                  }

                                  json ??= [];

                                  #endregion

                                  return new HTTPResponse.Builder(Request) {
                                             HTTPStatusCode              = HTTPStatusCode.Created,
                                             Server                      = HTTPServer.HTTPServerName,
                                             Date                        = Timestamp.Now,
                                             AccessControlAllowOrigin    = "*",
                                             AccessControlAllowMethods   = [ "OPTIONS", "POST" ],
                                             AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                                             ETag                        = "1",
                                             ContentType                 = HTTPContentType.Application.JSON_UTF8,
                                             Content                     = json.ToUTF8Bytes()
                                         }.AsImmutable;

                              });

            #endregion

            #region OPTIONS     ~/chargy/issues

            // --------------------------------------------------------
            // curl -v -X OPTIONS http://127.0.0.1:5500/chargy/issues
            // --------------------------------------------------------
            AddHandler(
                              HTTPMethod.OPTIONS,
                              URLPathPrefix + "chargy/issues",
                              HTTPDelegate: Request =>

                                  Task.FromResult(
                                      new HTTPResponse.Builder(Request) {
                                          HTTPStatusCode               = HTTPStatusCode.NoContent,
                                          Server                       = HTTPServer.HTTPServerName,
                                          Date                         = Timestamp.Now,
                                          AccessControlAllowOrigin     = "*",
                                          AccessControlAllowMethods    = [ "OPTIONS", "POST" ],
                                          AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                                      }.AsImmutable)

                              );

            #endregion

            #endregion



            // Be aware of multi-tenancy!

            #region ~/RNs

            #region OPTIONS     ~/RNs

            // ----------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // ----------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs",
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs

            // ---------------------------------------------------------------------------
            // curl -v -X "HEAD" -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // ---------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "RNs",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs

            // -----------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // -----------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    var withMetadata                      = request.QueryString.GetBoolean("withMetadata", false);

                    var matchFilter                       = request.QueryString.CreateStringFilter<IRoamingNetwork>(
                                                                "match",
                                                                (roamingNetwork, pattern) => roamingNetwork.ToString()?.Contains(pattern) == true
                                                                                             //roamingNetwork.Name?.             Contains(pattern) == true ||
                                                                                             //roamingNetwork.Address.           Contains(pattern)         ||
                                                                                             //roamingNetwork.City.              Contains(pattern)         ||
                                                                                             //roamingNetwork.PostalCode.        Contains(pattern)         ||
                                                                                             //roamingNetwork.Country.ToString()?.Contains(pattern) == true         ||
                                                                                             //roamingNetwork.Directions.        Matches (pattern)         ||
                                                                                             //roamingNetwork.Operator?.   Name. Contains(pattern) == true ||
                                                                                             //roamingNetwork.SubOperator?.Name. Contains(pattern) == true ||
                                                                                             //roamingNetwork.Owner?.      Name. Contains(pattern) == true ||
                                                                                             //roamingNetwork.Facilities.        Matches (pattern)         ||
                                                                                             //roamingNetwork.EVSEUIds.          Matches (pattern)         ||
                                                                                             //roamingNetwork.EVSEIds.           Matches (pattern)         ||
                                                                                             //roamingNetwork.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                            );
                    var skip                              = request.QueryString.GetUInt64("skip");
                    var take                              = request.QueryString.GetUInt64("take");

                    var expand                            = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworkIds           = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStationOperatorIds  = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingPoolIds             = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds          = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEVSEIds                     = expand.ContainsIgnoreCase("-evses")    ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandBrandIds                    = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds              = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEMobilityProviderIds        = expand.ContainsIgnoreCase("providers") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;


                    var allResults                        = GetAllRoamingNetworks(request.Host);
                    var totalCount                        = allResults.ULongCount();

                    var filteredResults                   = allResults.Where(matchFilter).ToArray();
                    var filteredCount                     = filteredResults.ULongCount();

                    var jsonResults                       = filteredResults.
                                                                OrderBy(roamingNetwork => roamingNetwork.Id).
                                                                ToJSON (skip,
                                                                        take,
                                                                        Embedded:  false,

                                                                        expandChargingStationOperatorIds,
                                                                        expandRoamingNetworkIds,
                                                                        expandChargingPoolIds,
                                                                        expandChargingStationIds,
                                                                        expandEVSEIds,
                                                                        expandBrandIds,
                                                                        expandDataLicenseIds,
                                                                        expandEMobilityProviderIds,

                                                                        null,  // CustomRoamingNetworkSerializer,
                                                                        null,  // CustomChargingStationOperatorSerializer,
                                                                        null,  // CustomChargingPoolSerializer,
                                                                        null,  // CustomChargingStationSerializer,
                                                                        null); // CustomEVSESerializer

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode                = HTTPStatusCode.OK,
                                   Server                        = DefaultHTTPServerName,
                                   Date                          = Timestamp.Now,
                                   AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                   AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                   Content                       = withMetadata
                                                                       ? JSONObject.Create(
                                                                             new JProperty("totalCount",     totalCount),
                                                                             new JProperty("filteredCount",  filteredCount),
                                                                             new JProperty("data",           jsonResults)
                                                                         ).ToUTF8Bytes()
                                                                       : new JArray(jsonResults).ToUTF8Bytes(),
                                   ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                   X_ExpectedTotalNumberOfItems  = filteredCount,
                                   Connection                    = ConnectionType.KeepAlive,
                                   Vary                          = "Accept"
                               }.AsImmutable
                           );

                });

            #endregion

            #region COUNT       ~/RNs

            // --------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs
            // --------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "RNs",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    var matchFilter      = request.QueryString.CreateStringFilter<IRoamingNetwork>(
                                               "match",
                                               (roamingNetwork, pattern) => roamingNetwork.ToString()?.Contains(pattern) == true
                                                                            //roamingNetwork.Name?.             Contains(pattern) == true ||
                                                                            //roamingNetwork.Address.           Contains(pattern)         ||
                                                                            //roamingNetwork.City.              Contains(pattern)         ||
                                                                            //roamingNetwork.PostalCode.        Contains(pattern)         ||
                                                                            //roamingNetwork.Country.ToString()?.Contains(pattern) == true         ||
                                                                            //roamingNetwork.Directions.        Matches (pattern)         ||
                                                                            //roamingNetwork.Operator?.   Name. Contains(pattern) == true ||
                                                                            //roamingNetwork.SubOperator?.Name. Contains(pattern) == true ||
                                                                            //roamingNetwork.Owner?.      Name. Contains(pattern) == true ||
                                                                            //roamingNetwork.Facilities.        Matches (pattern)         ||
                                                                            //roamingNetwork.EVSEUIds.          Matches (pattern)         ||
                                                                            //roamingNetwork.EVSEIds.           Matches (pattern)         ||
                                                                            //roamingNetwork.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                           );

                    var allResults       = GetAllRoamingNetworks(request.Host);
                    var totalCount       = allResults.ULongCount();

                    var filteredResults  = allResults.Where(matchFilter).ToArray();
                    var filteredCount    = filteredResults.ULongCount();


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(
                                                               new JProperty("totalCount",     totalCount),
                                                               new JProperty("filteredCount",  filteredCount)
                                                           ).ToUTF8Bytes(),
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs->Id
            // -------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs->Id",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "GET", "HEAD" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    var allRoamingNetworks  = GetAllRoamingNetworks(request.Host);
                    var skip                = request.QueryString.GetUInt64("skip");
                    var take                = request.QueryString.GetUInt64("take");

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = new JArray(
                                                                 allRoamingNetworks.
                                                                     Select(roamingNetwork => roamingNetwork.Id.ToString()).
                                                                     Skip  (request.QueryString.GetUInt64("skip")).
                                                                     Take  (request.QueryString.GetUInt64("take"))
                                                             ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = allRoamingNetworks.ULongCount(),
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs->AdminStatus

            // ------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs->AdminStatus
            // ------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "GET", "HEAD" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    var allRoamingNetworks  = GetAllRoamingNetworks(request.Host);
                    var skip                = request.QueryString.GetUInt64("skip");
                    var take                = request.QueryString.GetUInt64("take");
                    var sinceFilter         = request.QueryString.CreateDateTimeFilter<RoamingNetworkAdminStatus>("since", (adminStatus, timestamp) => adminStatus.Timestamp >= timestamp);
                    var matchFilter         = request.QueryString.CreateStringFilter  <RoamingNetworkAdminStatus>("match", (adminStatus, pattern)   => adminStatus.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = allRoamingNetworks.
                                                                Select(roamingNetwork => new RoamingNetworkAdminStatus(
                                                                                             roamingNetwork.Id,
                                                                                             roamingNetwork.AdminStatus
                                                                                         )).
                                                                Where (matchFilter).
                                                                Where (sinceFilter).
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = allRoamingNetworks.ULongCount(),
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs->Status

            // -------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs->Status
            // -------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs->Status",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "GET", "HEAD" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    var allRoamingNetworks  = GetAllRoamingNetworks(request.Host);
                    var skip                = request.QueryString.GetUInt64("skip");
                    var take                = request.QueryString.GetUInt64("take");
                    var sinceFilter         = request.QueryString.CreateDateTimeFilter<RoamingNetworkStatus>("since", (status, timestamp) => status.Timestamp >= timestamp);
                    var matchFilter         = request.QueryString.CreateStringFilter  <RoamingNetworkStatus>("match", (status, pattern)   => status.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = allRoamingNetworks.
                                                                 Select(roamingNetwork => new RoamingNetworkStatus(
                                                                                             roamingNetwork.Id,
                                                                                             roamingNetwork.Status
                                                                                         )).
                                                                 Where (matchFilter).
                                                                 Where (sinceFilter).
                                                                 ToJSON(skip, take).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = allRoamingNetworks.ULongCount(),
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}

            #region OPTIONS     ~/RNs/{RoamingNetworkId}

            // -----------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId..}",
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}

            // ----------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            // ----------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var expand                            = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworkIds           = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStationOperatorIds  = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingPoolIds             = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds          = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEVSEIds                     = expand.ContainsIgnoreCase("-evses")    ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandBrandIds                    = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds              = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEMobilityProviderIds        = expand.ContainsIgnoreCase("providers") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = roamingNetwork.GetHashCode().ToString(),
                            ContentType                = HTTPContentType.Application.JSON_UTF8,
                            Content                    = roamingNetwork.ToJSON(

                                                             Embedded:  false,

                                                             expandRoamingNetworkIds,
                                                             expandChargingStationOperatorIds,
                                                             expandChargingPoolIds,
                                                             expandChargingStationIds,
                                                             expandEVSEIds,
                                                             expandBrandIds,
                                                             expandDataLicenseIds,
                                                             expandEMobilityProviderIds,

                                                             null,  // CustomRoamingNetworkSerializer,
                                                             null,  // CustomChargingStationOperatorSerializer,
                                                             null,  // CustomChargingPoolSerializer,
                                                             null,  // CustomChargingStationSerializer,
                                                             null   // CustomEVSESerializer

                                                         ).ToUTF8Bytes(),
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs/{RoamingNetworkId}

            // --------------------------------------------------------------------------------
            // curl -v -X "HEAD" -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            // --------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "RNs/{RoamingNetworkId}",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = roamingNetwork.GetHashCode().ToString(),
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region CREATE      ~/RNs/{RoamingNetworkId}

            // ---------------------------------------------------------------------------------
            // curl -v -X CREATE -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test2
            // ---------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.CREATE,
                URLPathPrefix + "RNs/{RoamingNetworkId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   CreateRoamingNetworkRequest,
                HTTPResponseLogger:  CreateRoamingNetworkResponse,
                HTTPDelegate:        request => {

                    #region Try to get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var       httpUser,
                                        out var       httpOrganizations,
                                        out var       httpResponseBuilder,
                                        AccessLevel:  Access_Levels.Admin,
                                        Recursive:    true))
                    {
                        return Task.FromResult(httpResponseBuilder!.AsImmutable);
                    }

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkId(this,
                                                          out var roamingNetworkId,
                                                          out     httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    if (TryGetRoamingNetwork(request.Host,
                                             roamingNetworkId,
                                             out var roamingNetwork))
                    {

                        return Task.FromResult(new HTTPResponse.Builder(request) {
                                    HTTPStatusCode  = HTTPStatusCode.Conflict,
                                    Server          = HTTPServer.HTTPServerName,
                                    Date            = Timestamp.Now,
                                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                                    Content         = HTTPResponseExtensions.CreateError($"A roaming networkId with name '{roamingNetworkId}' already exists!"),
                               }.AsImmutable);

                    }

                    #endregion

                    #region Parse optional JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder,
                                                               AllowEmptyHTTPBody: true))
                    {
                        return Task.FromResult(httpResponseBuilder!.AsImmutable);
                    }

                    if (!json.ParseMandatory("name",
                                             "roaming network name",
                                             HTTPServer.HTTPServerName,
                                             out I18NString roamingNetworkName,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    if (json.ParseOptionalJSON("description",
                                               "roaming network description",
                                               I18NString.TryParse,
                                               out I18NString? roamingNetworkDescription,
                                               out var         errorResponse))
                    {
                        if (errorResponse is not null)
                            return Task.FromResult(new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);
                    }

                    #endregion


                    roamingNetwork = CreateNewRoamingNetwork(
                                         request.Host,
                                         roamingNetworkId,
                                         roamingNetworkName,
                                         Description: roamingNetworkDescription ?? I18NString.Empty
                                     );


                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode             = HTTPStatusCode.Created,
                                   Server                     = HTTPServer.HTTPServerName,
                                   Date                       = Timestamp.Now,
                                   AccessControlAllowOrigin   = "*",
                                   AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                                   AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                   ETag                       = "1",
                                   ContentType                = HTTPContentType.Application.JSON_UTF8,
                                   Content                    = roamingNetwork.ToJSON().ToUTF8Bytes(),
                                   Connection                 = ConnectionType.KeepAlive
                               }.AsImmutable
                           );

                });

            #endregion

            #region DELETE      ~/RNs/{RoamingNetworkId}

            // ---------------------------------------------------------------------------------
            // curl -v -X DELETE -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test2
            // ---------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.DELETE,
                URLPathPrefix + "RNs/{RoamingNetworkId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   DeleteRoamingNetworkRequest,
                HTTPResponseLogger:  DeleteRoamingNetworkResponse,
                HTTPDelegate:        request => {

                    #region Try to get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var       httpUser,
                                        out var       httpOrganizations,
                                        out var       httpResponseBuilder,
                                        AccessLevel:  Access_Levels.Admin,
                                        Recursive:    true))
                    {
                        return Task.FromResult(httpResponseBuilder!.AsImmutable);
                    }

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out     httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var deletedRoamingNetwork = RemoveRoamingNetwork(
                                                    request.Host,
                                                    roamingNetwork.Id
                                                );


                    return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ETag                       = "1",
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = deletedRoamingNetwork?.ToJSON().ToUTF8Bytes()
                            }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/{PropertyKey}

            //// ----------------------------------------------------------------------
            //// curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            //// ----------------------------------------------------------------------
            //AddHandler(
            //                  HTTPMethod.GET,
            //                  URLPathPrefix + "RNs/{RoamingNetworkId}/{PropertyKey}",
            //                  HTTPContentType.Application.JSON_UTF8,
            //                  HTTPDelegate: Request => {

            //                      #region Check anonymous access

            //                      if (!AllowsAnonymousReadAccesss)
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode             = HTTPStatusCode.Unauthorized,
            //                                  Server                     = HTTPTestServer?.HTTPServerName,
            //                                  Date                       = Timestamp.Now,
            //                                  AccessControlAllowOrigin   = "*",
            //                                  AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
            //                                  AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
            //                                  WWWAuthenticate            = WWWAuthenticateDefaults,
            //                                  Connection                 = ConnectionType.KeepAlive
            //                              }.AsImmutable);

            //                      #endregion

            //                      #region Check HTTP parameters

            //                      if (!Request.ParseRoamingNetwork(this,
            //                                                       out var roamingNetwork,
            //                                                       out var httpResponseBuilder) ||
            //                           roamingNetwork is null)
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }

            //                      #endregion

            //                      if (Request.ParsedURLParameters.Length < 2)
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode  = HTTPStatusCode.BadRequest,
            //                                  Server          = HTTPTestServer?.HTTPServerName,
            //                                  Date            = Timestamp.Now,
            //                              }.AsImmutable);

            //                      var PropertyKey = Request.ParsedURLParameters[1];

            //                      if (PropertyKey.IsNullOrEmpty())
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode  = HTTPStatusCode.BadRequest,
            //                                  Server          = HTTPTestServer?.HTTPServerName,
            //                                  Date            = Timestamp.Now,
            //                                  ContentType     = HTTPContentType.Application.JSON_UTF8,
            //                                  Content         = @"{ ""description"": ""Invalid property key!"" }".ToUTF8Bytes()
            //                              }.AsImmutable);


            //                      if (!roamingNetwork.TryGetInternalData(PropertyKey, out var Value))
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode             = HTTPStatusCode.NotFound,
            //                                  Server                     = HTTPTestServer?.HTTPServerName,
            //                                  Date                       = Timestamp.Now,
            //                                  AccessControlAllowOrigin   = "*",
            //                                  AccessControlAllowMethods  = [ "GET", "SET" ],
            //                                  AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
            //                                  ETag                       = "1",
            //                                  Connection                 = ConnectionType.KeepAlive
            //                              }.AsImmutable);


            //                      return Task.FromResult(
            //                          new HTTPResponse.Builder(Request) {
            //                              HTTPStatusCode             = HTTPStatusCode.OK,
            //                              Server                     = HTTPTestServer?.HTTPServerName,
            //                              Date                       = Timestamp.Now,
            //                              AccessControlAllowOrigin   = "*",
            //                              AccessControlAllowMethods  = [ "GET", "SET" ],
            //                              AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
            //                              ETag                       = "1",
            //                              ContentType                = HTTPContentType.Application.JSON_UTF8,
            //                              Content                    = JSONObject.Create(
            //                                                               new JProperty(PropertyKey, Value)
            //                                                           ).ToUTF8Bytes(),
            //                              Connection                 = ConnectionType.KeepAlive
            //                          }.AsImmutable);

            //                  });

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/{PropertyKey}

            //// -----------------------------------------------------------------------------
            //// curl -v -X SET -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test
            //// -----------------------------------------------------------------------------
            //AddHandler(
            //                  HTTPMethod.SET,
            //                  URLPathPrefix + "RNs/{RoamingNetworkId}/{PropertyKey}",
            //                  HTTPContentType.Application.JSON_UTF8,
            //                  HTTPDelegate: Request => {

            //                      #region Try to get HTTP user and its organizations

            //                      // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
            //                      if (!TryGetHTTPUser(Request,
            //                                          out var       httpUser,
            //                                          out var       httpOrganizations,
            //                                          out var       httpResponseBuilder,
            //                                          AccessLevel:  Access_Levels.Admin,
            //                                          Recursive:    true))
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }

            //                      #endregion

            //                      #region Check HTTP parameters

            //                      if (!Request.ParseRoamingNetwork(this,
            //                                                       out var roamingNetwork,
            //                                                       out httpResponseBuilder) ||
            //                           roamingNetwork is null)
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }

            //                      #endregion


            //                      if (Request.ParsedURLParameters.Length < 2)
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode  = HTTPStatusCode.BadRequest,
            //                                  Server          = HTTPTestServer?.HTTPServerName,
            //                                  Date            = Timestamp.Now,
            //                                  Connection      = ConnectionType.KeepAlive
            //                              }.AsImmutable);

            //                      var PropertyKey = Request.ParsedURLParameters[1];

            //                      if (PropertyKey.IsNullOrEmpty())
            //                          return Task.FromResult(
            //                              new HTTPResponse.Builder(Request) {
            //                                  HTTPStatusCode  = HTTPStatusCode.BadRequest,
            //                                  Server          = HTTPTestServer?.HTTPServerName,
            //                                  Date            = Timestamp.Now,
            //                                  ContentType     = HTTPContentType.Application.JSON_UTF8,
            //                                  Content         = @"{ ""description"": ""Invalid property key!"" }".ToUTF8Bytes(),
            //                                  Connection      = ConnectionType.KeepAlive
            //                              }.AsImmutable);


            //                      #region Parse optional JSON

            //                      if (Request.TryParseJSONObjectRequestBody(out var json,
            //                                                             out httpResponseBuilder,
            //                                                             AllowEmptyHTTPBody: false) ||
            //                          json is null)
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }


            //                      #region Parse oldValue    [mandatory]

            //                      if (!json.ParseMandatoryText("oldValue",
            //                                                   "old value of the property",
            //                                                   HTTPTestServer?.HTTPServerName,
            //                                                   out String OldValue,
            //                                                   Request,
            //                                                   out httpResponseBuilder))
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }

            //                      #endregion

            //                      #region Parse newValue    [mandatory]

            //                      if (!json.ParseMandatoryText("newValue",
            //                                                   "new value of the property",
            //                                                   HTTPTestServer?.HTTPServerName,
            //                                                   out String NewValue,
            //                                                   Request,
            //                                                   out httpResponseBuilder))
            //                      {
            //                          return Task.FromResult(httpResponseBuilder!.AsImmutable);
            //                      }

            //                      #endregion

            //                      #endregion


            //                      var result = roamingNetwork.SetInternalData(PropertyKey,
            //                                                                  NewValue,
            //                                                                  OldValue);

            //                      #region Choose HTTP status code

            //                      HTTPStatusCode _HTTPStatusCode;

            //                      switch (result)
            //                      {

            //                          case SetPropertyResult.Added:
            //                              _HTTPStatusCode = HTTPStatusCode.Created;
            //                              break;

            //                          case SetPropertyResult.Conflict:
            //                              _HTTPStatusCode = HTTPStatusCode.Conflict;
            //                              break;

            //                          default:
            //                              _HTTPStatusCode = HTTPStatusCode.OK;
            //                              break;

            //                      }

            //                      #endregion

            //                      return Task.FromResult(
            //                          new HTTPResponse.Builder(Request) {
            //                              HTTPStatusCode             = _HTTPStatusCode,
            //                              Server                     = HTTPTestServer?.HTTPServerName,
            //                              Date                       = Timestamp.Now,
            //                              AccessControlAllowOrigin   = "*",
            //                              AccessControlAllowMethods  = [ "GET", "SET" ],
            //                              AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
            //                              ETag                       = "1",
            //                              ContentType                = HTTPContentType.Application.JSON_UTF8,
            //                              Content                    = JSONObject.Create(
            //                                                               new JProperty("oldValue",  OldValue),
            //                                                               new JProperty("newValue",  NewValue)
            //                                                           ).ToUTF8Bytes(),
            //                              Connection                 = ConnectionType.KeepAlive
            //                          }.AsImmutable);

            //                  });

            #endregion

            #endregion



            // Charging Station Operators

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/ChargingStationOperators

            // ----------------------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators
            // ----------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators",
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs/{RoamingNetworkId}/ChargingStationOperators

            // -------------------------------------------------------------------------------------------------------
            // curl -v -X HEAD -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators
            // -------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators",
                request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var withMetadata            = request.QueryString.GetBoolean("withMetadata", false);

                    var matchFilter             = request.QueryString.CreateStringFilter<IChargingStationOperator>(
                                                      "match",
                                                      (chargingStationOperator, pattern) => chargingStationOperator.ToString()?.Contains(pattern) == true
                                                                                 //chargingPool.Name?.             Contains(pattern) == true ||
                                                                                 //chargingPool.Address.           Contains(pattern)         ||
                                                                                 //chargingPool.City.              Contains(pattern)         ||
                                                                                 //chargingPool.PostalCode.        Contains(pattern)         ||
                                                                                 //chargingPool.Country.ToString()?.Contains(pattern) == true         ||
                                                                                 //chargingPool.Directions.        Matches (pattern)         ||
                                                                                 //chargingPool.Operator?.   Name. Contains(pattern) == true ||
                                                                                 //chargingPool.SubOperator?.Name. Contains(pattern) == true ||
                                                                                 //chargingPool.Owner?.      Name. Contains(pattern) == true ||
                                                                                 //chargingPool.Facilities.        Matches (pattern)         ||
                                                                                 //chargingPool.EVSEUIds.          Matches (pattern)         ||
                                                                                 //chargingPool.EVSEIds.           Matches (pattern)         ||
                                                                                 //chargingPool.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                  );
                    var skip                    = request.QueryString.GetUInt64("skip");
                    var take                    = request.QueryString.GetUInt64("take");

                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEVSEs             = expand.ContainsIgnoreCase("-evses")    ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses      = expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;


                    var allResults              = roamingNetwork.ChargingStationOperators;
                    var totalCount              = allResults.ULongCount();

                    var filteredResults         = allResults.Where(matchFilter).ToArray();
                    var filteredCount           = filteredResults.ULongCount();

                    var jsonResults             = filteredResults.
                                                      OrderBy(chargingPool => chargingPool.Id).
                                                      ToJSON (skip,
                                                              take,
                                                              Embedded:                            false,
                                                              ExpandRoamingNetworkId:              expandRoamingNetworks,
                                                         //     ExpandChargingStationOperatorId:     expandOperators,
                                                              ExpandChargingStationIds:            expandChargingStations,
                                                              ExpandEVSEIds:                       expandEVSEs,
                                                              ExpandBrandIds:                      expandBrands,
                                                              ExpandDataLicenses:                  expandDataLicenses,
                                                              CustomChargingPoolSerializer:        null,
                                                              CustomChargingStationSerializer:     null,
                                                              CustomEVSESerializer:                null
                                                            //  CustomChargingConnectorSerializer:   null
                                                            );

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode                = HTTPStatusCode.OK,
                                   Server                        = DefaultHTTPServerName,
                                   Date                          = Timestamp.Now,
                                   AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                   AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                   Content                       = withMetadata
                                                                       ? JSONObject.Create(
                                                                             new JProperty("totalCount",     totalCount),
                                                                             new JProperty("filteredCount",  filteredCount),
                                                                             new JProperty("data",           jsonResults)
                                                                         ).ToUTF8Bytes()
                                                                       : new JArray(jsonResults).ToUTF8Bytes(),
                                   ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                   X_ExpectedTotalNumberOfItems  = filteredCount,
                                   Connection                    = ConnectionType.KeepAlive,
                                   Vary                          = "Accept"
                               }.AsImmutable
                           );

                });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingStationOperators

            // ----------------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RoamingNetworkId}/ChargingStationOperators
            // ----------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = JSONObject.Create(
                                                              new JProperty("count",  roamingNetwork.ChargingStationOperators.ULongCount())
                                                          ).ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators->AdminStatus

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators->AdminStatus
            // -----------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.ChargingStationOperatorAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingStationOperatorAdminStatus().
                                                                OrderBy(adminStatus => adminStatus.Id).
                                                                ToJSON (skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount,
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators->Status

            // -------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators->Status
            // -------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.ChargingStationOperatorStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingStationOperatorStatus().
                                                                OrderBy(status => status.Id).
                                                                ToJSON (skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount,
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}

            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "CREATE", "DELETE" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = chargingStationOperator.ToJSON().ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools

            // -----------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/ChargingStationOperators/{CSOId}/ChargingPools
            // -----------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var skip                    = request.QueryString.GetUInt64 ("skip");
                    var take                    = request.QueryString.GetUInt64 ("take");

                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   =  expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperators         =  expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = !expand.ContainsIgnoreCase("-stations") ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandEVSEs             =  expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded   : InfoStatus.Hidden;
                    var expandBrands            =  expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses      =  expand.ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                    var showIds                 = request.QueryString.GetStrings("showIds");
                    var showEVSEIds             = showIds.ContainsIgnoreCase("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showBrandIds            = showIds.ContainsIgnoreCase("brands")    ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showDataLicenseIds      = showIds.ContainsIgnoreCase("licenses")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount           = roamingNetwork.ChargingPools.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = roamingNetwork.ChargingPools.
                                                                 ToJSON(Skip:                                skip,
                                                                        Take:                                take,
                                                                        Embedded:                            false,
                                                                        IncludeRemoved:                      null,
                                                                        ExpandRoamingNetworkId:              expandRoamingNetworks,
                                                                        ExpandChargingStationOperatorId:     expandOperators,
                                                                        ExpandChargingStationIds:            expandChargingStations,
                                                                        ExpandEVSEIds:                       expandEVSEs,
                                                                        ExpandBrandIds:                      expandBrands,
                                                                        ExpandDataLicenses:                  expandDataLicenses,
                                                                        IncludeRemovedChargingStations:      null,
                                                                        IncludeCustomData:                   null,
                                                                        CustomChargingPoolSerializer:        null,
                                                                        CustomChargingStationSerializer:     null,
                                                                        CustomEVSESerializer:                null,
                                                                        CustomChargingConnectorSerializer:   null).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                }
            );

            #endregion

            #region CREATE      ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}

            // ----------------------------------------------------------------------------------------------------------------
            // curl -v -X CREATE -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test2/ChargingPools/{ChargingPoolId}
            // ----------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.CREATE,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:  CreateChargingPoolRequest,
                HTTPResponseLogger: CreateChargingPoolResponse,
                HTTPDelegate: request => {

                    #region Check HTTP Basic Authentication

                    //if (Request.Authorization          is null      ||
                    //    Request.Authorization.Username != HTTPLogin ||
                    //    Request.Authorization.Password != HTTPPassword)
                    //    return SendEVSEStatusSetted(
                    //        new HTTPResponse.Builder(Request) {
                    //            HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                    //            WWWAuthenticate  = @"Basic realm=""WWCP""",
                    //            Server           = HTTPTestServer?.HTTPServerName,
                    //            Date             = Timestamp.Now,
                    //            Connection       = ConnectionType.KeepAlive
                    //        });

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    #region Parse optional JSON

                    I18NString? DescriptionI18N = null;

                    if (request.TryParseJSONObjectRequestBody(out var JSON,
                                                              out httpResponse,
                                                              AllowEmptyHTTPBody: true))
                    {

                        if (!JSON.ParseOptional("description",
                                                "description",
                                                HTTPServer.HTTPServerName,
                                                out DescriptionI18N,
                                                request,
                                                out httpResponse))
                        {
                            return Task.FromResult(httpResponse.AsImmutable);
                        }

                    }

                    #endregion


                    roamingNetwork = CreateNewRoamingNetwork(request.Host,
                                                             roamingNetwork.Id,
                                                             I18NString.Empty,
                                                             Description: DescriptionI18N ?? I18NString.Empty);


                    return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode              = HTTPStatusCode.Created,
                                Server                      = HTTPServer.HTTPServerName,
                                Date                        = Timestamp.Now,
                                AccessControlAllowOrigin    = "*",
                                AccessControlAllowMethods   = [ "GET", "CREATE", "DELETE" ],
                                AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                                ETag                        = "1",
                                ContentType                 = HTTPContentType.Application.JSON_UTF8,
                                Content                     = roamingNetwork.ToJSON().ToUTF8Bytes(),
                                Connection                  = ConnectionType.KeepAlive
                            }.AsImmutable);

                }
            );

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}/AdminStatus

            // ------------------------------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*P000001*1/AdminStatus
            AddHandler(

                HTTPMethod.SET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingPools/{ChargingPoolId}/AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    #region Parse ChargingPoolId

                    if (!ChargingPool_Id.TryParse(request.ParsedURLParameters[1],
                                                  out ChargingPool_Id ChargingPoolId))
                    {

                        //Log.Timestamp("Bad request: Invalid ChargingPoolId query parameter!");

                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                ContentType     = HTTPContentType.Application.JSON_UTF8,
                                Content         = new JObject(//new JProperty("@context",    "http://wwcp.graphdefined.org/contexts/BadRequest.jsonld"),
                                                            new JProperty("description", "Invalid ChargingPoolId query parameter!")).ToUTF8Bytes()
                            }.AsImmutable);

                    }

                    if (!roamingNetwork.ContainsChargingPool(ChargingPoolId))
                    {

                        //Log.Timestamp("Bad request: Unknown ChargingPoolId query parameter!");

                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                ContentType     = HTTPContentType.Application.JSON_UTF8,
                                Content         = new JObject(//new JProperty("@context",    "http://wwcp.graphdefined.org/contexts/BadRequest.jsonld"),
                                                            new JProperty("description", "Unknown ChargingPoolId query parameter!")).ToUTF8Bytes()
                            }.AsImmutable);

                    }

                    #endregion

                    #region Parse JSON and new charging pool admin status

                    if (!request.TryParseJSONObjectRequestBody(out var JSON, out httpResponse))
                        return Task.FromResult(httpResponse.AsImmutable);

                    if (!JSON.ParseMandatoryEnum("newstatus",
                                                "charging pool admin status",
                                                HTTPServer.HTTPServerName,
                                                out ChargingPoolAdminStatusType NewChargingPoolAdminStatus,
                                                request,
                                                out httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    //Log.WriteLine("SetChargingPoolAdminStatus : " + RoamingNetwork.Id + " / " + ChargingPoolId + " => " + NewChargingPoolAdminStatus);

                    //HTTPServer.Get<JObject>(DebugLogId).
                    //    SubmitEvent("SetChargingPoolAdminStatusRequest",
                    //                new JObject(
                    //                    new JProperty("Timestamp",       Timestamp.Now.ToISO8601()),
                    //                    new JProperty("RoamingNetwork",  _RoamingNetwork.ToString()),
                    //                    new JProperty("ChargingPoolId",  ChargingPoolId.ToString()),
                    //                    new JProperty("NewStatus",       NewChargingPoolAdminStatus.ToString())
                    //                )).Wait();


                    roamingNetwork.ChargingStationOperators.ForEach(evseoperator => {

                        if (evseoperator.ChargingPoolExists(ChargingPoolId))
                            evseoperator.SetChargingPoolAdminStatus(ChargingPoolId, new Timestamped<ChargingPoolAdminStatusType>(NewChargingPoolAdminStatus), SendUpstream: true);

                    });

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.OK,
                                   Date            = Timestamp.Now,
                                   Connection      = ConnectionType.KeepAlive
                               }.AsImmutable
                           );

                }
            );

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}

            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStation(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingStation,
                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "SET" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = chargingStation.ToJSON().ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStations/{StationId}/AdminStatus

            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*S000001*1
            //
            // curl -v -X SET -H "Content-Type: application/json" \
            //                -H "Accept:       application/json" \
            //      -d "{ \"newstatus\":  \"OutOfService\" }" \
            //      http://127.0.0.1:3004/RNs/Test/ChargingStations/DE*GEF*S000001*1
            AddHandler(

                HTTPMethod.SET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStations/{ChargingStationId}/AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: async request => {

                    #region Data

                    var eventTrackingId = EventTracking_Id.New;

                    ChargingStation_Id ChargingStationId;

                    #endregion

                    #region Parse Roaming Network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion

                    try
                    {

                        #region Parse Charging Station Id

                        if (!request.TryParseURLParameter<ChargingStation_Id>(
                             OpenChargingCloudAPIExtensions.ChargingStationId,
                             ChargingStation_Id.TryParse,
                             out var chargingStationId))
                        {

                            return new HTTPResponse.Builder(request) {
                                       HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                       Content         = new JObject(
                                                             new JProperty(
                                                                 "Description",
                                                                 "Invalid charging station identification!"
                                                             )
                                                         ).ToUTF8Bytes(),
                                       ContentType     = HTTPContentType.Application.JSON_UTF8
                                   };

                        }

                        ChargingStationId = chargingStationId;

                        #endregion

                        #region Check Charging Station Id

                        if (!roamingNetwork.ContainsChargingStation(ChargingStationId))
                            return new HTTPResponse.Builder(request) {
                                       HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                       Content         = new JObject(
                                                             new JProperty(
                                                                 "Description",
                                                                 "Unknown ChargingStationId query parameter!"
                                                             )
                                                         ).ToUTF8Bytes(),
                                       ContentType     = HTTPContentType.Application.JSON_UTF8
                                   };

                        #endregion

                        if (!request.TryParseJSONObjectRequestBody(out var JSON, out httpResponse))
                            return httpResponse;

                        #region Parse new Charging Station Admin Status

                        if (!JSON.ParseMandatoryEnum("newstatus",
                                                     "charging station admin status",
                                                     HTTPServer.HTTPServerName,
                                                     out ChargingStationAdminStatusType NewChargingStationAdminStatus,
                                                     request,
                                                     out httpResponse))
                        {
                            return httpResponse;
                        }

                        #endregion


                        await roamingNetwork.SetChargingStationAdminStatus(
                                  ChargingStationId,
                                  [ new Timestamped<ChargingStationAdminStatusType>(NewChargingStationAdminStatus) ]
                              );


                        return new HTTPResponse.Builder(request) {
                            HTTPStatusCode  = HTTPStatusCode.OK,
                            Date            = Timestamp.Now,
                            Content         = new JObject(
                                                new JProperty("Description", "Ok")
                                            ).ToString().
                                              Replace(Environment.NewLine, "").
                                              ToUTF8Bytes(),
                            ContentType     = HTTPContentType.Application.JSON_UTF8,
                            Connection      = ConnectionType.KeepAlive
                        };

                    }

                    #region Catch errors...

                    catch (Exception e)
                    {

                        //Log.Timestamp("Exception occurred: " + e.Message);

                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.InternalServerError,
                                   Content         = new JObject(
                                                         new JProperty("@context",           "http://wwcp.graphdefined.org/contexts/SETSTATUS-Response.jsonld"),
                                                         new JProperty("RoamingNetwork",     roamingNetwork.ToString()),
                                                         //new JProperty("ChargingStationId",  ChargingStationId.ToString()),
                                                         new JProperty("Description",        "An exception occurred: " + e.Message)
                                                     ).ToUTF8Bytes(),
                                   ContentType     = HTTPContentType.Application.JSON_UTF8
                               };

                    }

                    #endregion

                });

            #endregion

            #endregion



            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/ChargingStationGroups
            // ----------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount             = chargingStationOperator.ChargingStationGroups.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = chargingStationOperator.ChargingStationGroups.
                                                                 ToJSON(skip,
                                                                        take,
                                                                        false,
                                                                        expandChargingPoolIds,
                                                                        expandChargingStationIds,
                                                                        expandEVSEIds,
                                                                        expandDataLicenseIds).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/ChargingStationGroups/{ChargingStationGroupId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperatorAndChargingStationGroup(this,
                                                                                                         out var RoamingNetwork,
                                                                                                         out var ChargingStationOperator,
                                                                                                         out var ChargingStationGroup,
                                                                                                         out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var include                   = request.QueryString.GetStrings("include");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandChargingStationIds  = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandEVSEIds             = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandDataLicenseIds      = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    var chargingStationGroupJSON  = ChargingStationGroup.ToJSON(false,
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
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = chargingStationGroupJSON.ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/EVSEGroups
            // ----------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount             = chargingStationOperator.EVSEGroups.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = chargingStationOperator.EVSEGroups.
                                                                 ToJSON(skip,
                                                                         take,
                                                                         false,
                                                                         expandChargingPoolIds,
                                                                         expandEVSEIds,
                                                                         expandEVSEIds,
                                                                         expandDataLicenseIds).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/EVSEGroups/{EVSEGroupId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperatorAndEVSEGroup(this,
                                                                                              out var roamingNetwork,
                                                                                              out var chargingStationOperator,
                                                                                              out var evseGroup,
                                                                                              out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var include                   = request.QueryString.GetStrings("include");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandChargingStationIds  = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandEVSEIds             = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandDataLicenseIds      = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    var evseGroupJSON             = evseGroup.ToJSON(false,
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
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = evseGroupJSON.ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Brands
            // ----------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount             = chargingStationOperator.Brands.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = chargingStationOperator.Brands.
                                                                 ToJSON(skip,
                                                                        take,
                                                                        false,
                                                                        expandDataLicenseIds).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}

            // --------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Brands/{BrandId}
            // --------------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Brands/{BrandId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperatorAndBrand(this,
                                                                                          out var roamingNetwork,
                                                                                          out var chargingStationOperator,
                                                                                          out var brand,
                                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                       = request.QueryString.GetUInt64("skip");
                    var take                       = request.QueryString.GetUInt64("take");
                    var include                    = request.QueryString.GetStrings("include");
                    var expand                     = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds      = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : include.Contains("pools")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandChargingStationIds   = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : include.Contains("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandEVSEIds              = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : include.Contains("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var expandDataLicenseIds       = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded : include.Contains("operators") ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    var brandJSON                  = brand.ToJSON(false,
                                                                  expandDataLicenseIds);

                    #region Include charging pools

                    if (expandChargingPoolIds == InfoStatus.ShowIdOnly)
                    {

                        var pools  = chargingStationOperator.
                                        ChargingPools.
                                        Where(pool => pool.Brands.Contains(brand)).
                                        ToArray();

                        if (pools.Length > 0)
                        brandJSON["chargingPoolIds"] = new JArray(pools.Select(pool => pool.Id.ToString()));

                    }

                    else if (expandChargingPoolIds == InfoStatus.Expanded) {

                        var pools  = chargingStationOperator.
                                        ChargingPools.
                                        Where(pool => pool.Brands.Contains(brand)).
                                        ToArray();

                        if (pools.Length > 0)
                            brandJSON["chargingPools"]  = new JArray(pools.Select(pool => pool.ToJSON(Embedded:                        true,
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

                        var stations  = chargingStationOperator.
                                            ChargingStations.
                                            Where(station => station.Brands.Contains(brand)).
                                            ToArray();

                        if (stations.Length > 0)
                        brandJSON["chargingStationIds"] = new JArray(stations.Select(station => station.Id.ToString()));

                    }

                    else if (expandChargingStationIds == InfoStatus.Expanded) {

                        var stations  = chargingStationOperator.
                                            ChargingStations.
                                            Where(station => station.Brands.Contains(brand)).
                                            ToArray();

                        if (stations.Length > 0)
                            brandJSON["chargingStations"]  = new JArray(chargingStationOperator.
                                                                            ChargingStations.
                                                                            Where (station => station.Brands.SafeAny(brand => brand.Id == brand.Id)).
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

                        var evses  = chargingStationOperator.
                                        EVSEs.
                                        Where(evse => evse.Brands.Any(brand => brand.Id == brand.Id)).
                                        ToArray();

                        if (evses.Length > 0)
                        brandJSON["EVSEIds"] = new JArray(evses.Select(station => station.Id.ToString()));

                    }

                    else if (expandEVSEIds == InfoStatus.Expanded) {

                        var evses  = chargingStationOperator.
                                        EVSEs.
                                        Where(evse => evse.Brands.Any(brand => brand.Id == brand.Id)).
                                        ToArray();

                        if (evses.Length > 0)
                            brandJSON["EVSEs"]   = new JArray(chargingStationOperator.
                                                                  EVSEs.
                                                                  Where (evse => evse.Brands.Any(brand => brand.Id == brand.Id)).
                                                                  Select(evse => evse.ToJSON(Embedded:                        true,
                                                                                              ExpandRoamingNetworkId:          InfoStatus.Hidden,
                                                                                              ExpandChargingStationOperatorId: InfoStatus.Hidden,
                                                                                              ExpandChargingPoolId:            InfoStatus.Hidden,
                                                                                              ExpandChargingStationId:         InfoStatus.Hidden,
                                                                                              ExpandBrandIds:                  InfoStatus.Hidden)));

                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = brandJSON.ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion



            #region ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Tariffs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Tariffs

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/Tariffs
            // ----------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/Tariffs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount             = chargingStationOperator.ChargingTariffs.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = chargingStationOperator.ChargingTariffs.
                                                                ToJSON(skip,
                                                                        take,
                                                                        false,
                                                                        expandChargingPoolIds,
                                                                        expandChargingStationIds,
                                                                        expandEVSEIds,
                                                                        expandDataLicenseIds).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/TariffOverview

            // ----------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingStationOperators/{CSOId}/TariffOverview
            // ----------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStationOperators/{CSOId}/TariffOverview",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetworkAndChargingStationOperator(this,
                                                                                  out var roamingNetwork,
                                                                                  out var chargingStationOperator,
                                                                                  out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                      = request.QueryString.GetUInt64("skip");
                    var take                      = request.QueryString.GetUInt64("take");
                    var expand                    = request.QueryString.GetStrings("expand");
                    var expandChargingPoolIds     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStationIds  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandEVSEIds             = expand.ContainsIgnoreCase("evses")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandDataLicenseIds      = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Text.PLAIN,
                            Content                        = chargingStationOperator.ChargingStations.
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
                            X_ExpectedTotalNumberOfItems   = chargingStationOperator.ChargingTariffs.ULongCount(),
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion


            // Charging Pools

            #region ~/RNs/{RoamingNetworkId}/ChargingPools

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/ChargingPools

            // -----------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs/{RoamingNetworkId}/ChargingPools

            // ------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools
            // ------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools

            // ------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools
            // ------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   GetChargingPoolsRequest,
                HTTPResponseLogger:  GetChargingPoolsResponse,
                HTTPDelegate:        request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder!.AsImmutable);
                    }

                    #endregion


                    var withMetadata            = request.QueryString.GetBoolean("withMetadata",      false);
                    var includeRemoved          = request.QueryString.GetBoolean("includeRemoved",    false);
                    var includeCustomData       = request.QueryString.GetBoolean("includeCustomData", false);

                    var matchFilter             = request.QueryString.CreateStringFilter<IChargingPool>(
                                                      "match",
                                                      (chargingPool, pattern) => chargingPool.ToString()?.Contains(pattern) == true
                                                                                 //chargingPool.Name?.             Contains(pattern) == true ||
                                                                                 //chargingPool.Address.           Contains(pattern)         ||
                                                                                 //chargingPool.City.              Contains(pattern)         ||
                                                                                 //chargingPool.PostalCode.        Contains(pattern)         ||
                                                                                 //chargingPool.Country.ToString()?.Contains(pattern) == true         ||
                                                                                 //chargingPool.Directions.        Matches (pattern)         ||
                                                                                 //chargingPool.Operator?.   Name. Contains(pattern) == true ||
                                                                                 //chargingPool.SubOperator?.Name. Contains(pattern) == true ||
                                                                                 //chargingPool.Owner?.      Name. Contains(pattern) == true ||
                                                                                 //chargingPool.Facilities.        Matches (pattern)         ||
                                                                                 //chargingPool.EVSEUIds.          Matches (pattern)         ||
                                                                                 //chargingPool.EVSEIds.           Matches (pattern)         ||
                                                                                 //chargingPool.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                  );
                    var skip                    = request.QueryString.GetUInt64 ("skip");
                    var take                    = request.QueryString.GetUInt64 ("take");

                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   = expand. ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperators         = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = expand. ContainsIgnoreCase("stations")  ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandEVSEs             = expand. ContainsIgnoreCase("evses")     ? InfoStatus.Expanded   : InfoStatus.Hidden;
                    var expandBrands            = expand. ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses      = expand. ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                    var showIds                 = request.QueryString.GetStrings("showIds");
                    var showEVSEIds             = showIds.ContainsIgnoreCase("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showBrandIds            = showIds.ContainsIgnoreCase("brands")    ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showDataLicenseIds      = showIds.ContainsIgnoreCase("licenses")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    var allResults              = roamingNetwork.ChargingPools;
                    var totalCount              = allResults.ULongCount();

                    var filteredResults         = allResults.Where(matchFilter).ToArray();
                    var filteredCount           = filteredResults.ULongCount();

                    var jsonResults             = filteredResults.
                                                      OrderBy(chargingPool => chargingPool.Id).
                                                      ToJSON (skip,
                                                              take,
                                                              Embedded:                            false,
                                                              ExpandRoamingNetworkId:              expandRoamingNetworks,
                                                              ExpandChargingStationOperatorId:     expandOperators,
                                                              ExpandChargingStationIds:            expandChargingStations,
                                                              ExpandEVSEIds:                       InfoStatusExtensions.Max(showEVSEIds,        expandEVSEs),
                                                              ExpandBrandIds:                      InfoStatusExtensions.Max(showBrandIds,       expandBrands),
                                                              ExpandDataLicenses:                  InfoStatusExtensions.Max(showDataLicenseIds, expandDataLicenses),
                                                              IncludeCustomData:                   includeCustomData,
                                                              CustomChargingPoolSerializer:        null,
                                                              CustomChargingStationSerializer:     null,
                                                              CustomEVSESerializer:                null,
                                                              CustomChargingConnectorSerializer:   null);

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode                = HTTPStatusCode.OK,
                                   Server                        = DefaultHTTPServerName,
                                   Date                          = Timestamp.Now,
                                   AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                   AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                   Content                       = withMetadata
                                                                       ? JSONObject.Create(
                                                                             new JProperty("totalCount",     totalCount),
                                                                             new JProperty("filteredCount",  filteredCount),
                                                                             new JProperty("data",           jsonResults)
                                                                         ).ToUTF8Bytes()
                                                                       : new JArray(jsonResults).ToUTF8Bytes(),
                                   ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                   X_ExpectedTotalNumberOfItems  = filteredCount,
                                   Connection                    = ConnectionType.KeepAlive,
                                   Vary                          = "Accept"
                               }.AsImmutable
                           );

                }
            );

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingPools

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingPools
            // -----------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var matchFilter      = request.QueryString.CreateStringFilter<IChargingPool>(
                                               "match",
                                               (chargingPool, pattern) => chargingPool.ToString()?.Contains(pattern) == true
                                                                          //chargingPool.Name?.             Contains(pattern) == true ||
                                                                          //chargingPool.Address.           Contains(pattern)         ||
                                                                          //chargingPool.City.              Contains(pattern)         ||
                                                                          //chargingPool.PostalCode.        Contains(pattern)         ||
                                                                          //chargingPool.Country.ToString()?.Contains(pattern) == true         ||
                                                                          //chargingPool.Directions.        Matches (pattern)         ||
                                                                          //chargingPool.Operator?.   Name. Contains(pattern) == true ||
                                                                          //chargingPool.SubOperator?.Name. Contains(pattern) == true ||
                                                                          //chargingPool.Owner?.      Name. Contains(pattern) == true ||
                                                                          //chargingPool.Facilities.        Matches (pattern)         ||
                                                                          //chargingPool.EVSEUIds.          Matches (pattern)         ||
                                                                          //chargingPool.EVSEIds.           Matches (pattern)         ||
                                                                          //chargingPool.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                           );

                    var allResults       = roamingNetwork.ChargingPools;
                    var totalCount       = allResults.ULongCount();

                    var filteredResults  = allResults.Where(matchFilter).ToArray();
                    var filteredCount    = filteredResults.ULongCount();


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(
                                                               new JProperty("totalCount",     totalCount),
                                                               new JProperty("filteredCount",  filteredCount)
                                                           ).ToUTF8Bytes(),
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->Id
            // -------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->Id",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                roamingNetwork.ChargingPools.
                                                                    Select(pool => pool.Id.ToString()).
                                                                    Skip  (request.QueryString.GetUInt64("skip")).
                                                                    Take  (request.QueryString.GetUInt64("take"))
                                                           ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.ChargingPools.ULongCount()
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->AdminStatus

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->AdminStatus
            // -------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip         = request.QueryString.GetUInt64("skip");
                    var take         = request.QueryString.GetUInt64("take");
                    var sinceFilter  = request.QueryString.CreateDateTimeFilter<ChargingPoolAdminStatus>("since", (status, timestamp) => status.Timestamp >= timestamp);
                    var matchFilter  = request.QueryString.CreateStringFilter  <ChargingPoolAdminStatus>("match", (status, pattern)   => status.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingPoolAdminStatus().
                                                                Where (matchFilter).
                                                                Where (sinceFilter).
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.ChargingPoolAdminStatus().ULongCount()
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools->Status

            // --------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools->Status
            // --------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip         = request.QueryString.GetUInt64("skip");
                    var take         = request.QueryString.GetUInt64("take");
                    var sinceFilter  = request.QueryString.CreateDateTimeFilter<ChargingPoolStatus>("since", (status, timestamp) => status.Timestamp >= timestamp);
                    var matchFilter  = request.QueryString.CreateStringFilter  <ChargingPoolStatus>("match", (status, pattern)   => status.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingPoolStatus().
                                                                Where (matchFilter).
                                                                Where (sinceFilter).
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.ChargingPoolStatus().ULongCount()
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingPools/DynamicStatusReport",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(

                                                            new JProperty("count",  roamingNetwork.ChargingPools.Count()),

                                                            new JProperty("status", JSONObject.Create(
                                                                roamingNetwork.ChargingPools.GroupBy(pool => pool.Status.Value).Select(group =>
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
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "SET" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            ContentType                = HTTPContentType.Application.JSON_UTF8,
                            Content                    = chargingPool.ToJSON().ToUTF8Bytes()
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var _HTTPResponse))
                    {
                        return Task.FromResult(_HTTPResponse.AsImmutable);
                    }

                    #endregion

                    var skip                  = request.QueryString.GetUInt64 ("skip");
                    var take                  = request.QueryString.GetUInt64 ("take");

                    var expand                = request.QueryString.GetStrings("expand");
                    var expandRoamingNetwork  = expand. ContainsIgnoreCase("network")   ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperators       = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingPools   = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEVSEs           = expand. ContainsIgnoreCase("-evses")    ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandBrands          = expand. ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses    = expand. ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                    var showIds               = request.QueryString.GetStrings("showIds");
                    var showEVSEIds           = showIds.ContainsIgnoreCase("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showBrandIds          = showIds.ContainsIgnoreCase("brands")    ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showDataLicenseIds    = showIds.ContainsIgnoreCase("licenses")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount         = chargingPool.ChargingStations.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.ChargingStations.
                                                                OrderBy(station => station.Id).
                                                                ToJSON (Skip:                                skip,
                                                                        Take:                                take,
                                                                        Embedded:                            false,
                                                                        IncludeRemoved:                      false,
                                                                        ExpandRoamingNetworkId:              expandRoamingNetwork,
                                                                        ExpandChargingStationOperatorId:     expandOperators,
                                                                        ExpandChargingPoolId:                expandChargingPools,
                                                                        ExpandEVSEIds:                       expandEVSEs,
                                                                        ExpandBrandIds:                      expandBrands,
                                                                        ExpandDataLicenses:                  expandDataLicenses,
                                                                        IncludeRemovedEVSEs:                 null,
                                                                        IncludeCustomData:                   null,
                                                                        CustomChargingStationSerializer:     null,
                                                                        CustomEVSESerializer:                null,
                                                                        CustomChargingConnectorSerializer:   null).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations->AdminStatus

            // -----------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/ChargingStations->AdminStatus
            // -----------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/ChargingStations->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingPool.ChargingStationAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.ChargingStationAdminStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations->Status

            // ------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/ChargingStations->Status
            // ------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/ChargingStations->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingPool.ChargingStationStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.ChargingStationStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPoolAndChargingStation(this,
                                                                                         out var roamingNetwork,
                                                                                         out var chargingPool,
                                                                                         out var chargingStation,
                                                                                         out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip                   = request.QueryString.GetUInt64("skip");
                    var take                   = request.QueryString.GetUInt64("take");
                    var expand                 = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks  = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandOperators        = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandBrands           = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount          = chargingStation.EVSEs.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingStation.EVSEs.
                                                                OrderBy(evse => evse.Id).
                                                                ToJSON (skip,
                                                                        take,
                                                                        false,
                                                                        false,
                                                                        expandRoamingNetworks,
                                                                        expandOperators,
                                                                        expandBrands).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../ChargingStations
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/ChargingStations/{ChargingStationId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPoolAndChargingStationAndEVSE(this,
                                                                                                out var roamingNetwork,
                                                                                                out var chargingPool,
                                                                                                out var chargingStation,
                                                                                                out var evse,
                                                                                                out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            ContentType                = HTTPContentType.Application.JSON_UTF8,
                            Content                    = evse.ToJSON()?.ToUTF8Bytes()
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/.../EVSEs
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                    = request.QueryString.GetUInt64("skip");
                    var take                    = request.QueryString.GetUInt64("take");
                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount           = chargingPool.EVSEs.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.EVSEs.
                                                                OrderBy(evse => evse.Id).
                                                                ToJSON (skip,
                                                                        take,
                                                                        false,
                                                                        false,
                                                                        expandRoamingNetworks,
                                                                        expandOperators,
                                                                        expandChargingPools,
                                                                        expandChargingStations,
                                                                        expandBrands).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus

            // ------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus
            // ------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingPool.EVSEAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.EVSEAdminStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->Status

            // -------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingPools/{ChargingPoolId}/EVSEs->Status
            // -------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingPools/{ChargingPoolId}/EVSEs->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingPool(this,
                                                                       out var roamingNetwork,
                                                                       out var chargingPool,
                                                                       out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingPool.EVSEStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingPool.EVSEStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #endregion


            // Charging Stations

            #region ~/RNs/{RoamingNetworkId}/ChargingStations

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations
            // --------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations",
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.NoContent,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations
            // --------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations
            // --------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   GetChargingStationsRequest,
                HTTPResponseLogger:  GetChargingStationsResponse,
                HTTPDelegate:        request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var withMetadata            = request.QueryString.GetBoolean("withMetadata",      false);
                    var includeRemoved          = request.QueryString.GetBoolean("includeRemoved",    false);
                    var includeCustomData       = request.QueryString.GetBoolean("includeCustomData", false);

                    var matchFilter             = request.QueryString.CreateStringFilter<IChargingStation>(
                                                      "match",
                                                      (chargingStation, pattern) => chargingStation.ToString()?.Contains(pattern) == true
                                                                                    //chargingStation.Name?.             Contains(pattern) == true ||
                                                                                    //chargingStation.Address.           Contains(pattern)         ||
                                                                                    //chargingStation.City.              Contains(pattern)         ||
                                                                                    //chargingStation.PostalCode.        Contains(pattern)         ||
                                                                                    //chargingStation.Country.ToString()?.Contains(pattern) == true         ||
                                                                                    //chargingStation.Directions.        Matches (pattern)         ||
                                                                                    //chargingStation.Operator?.   Name. Contains(pattern) == true ||
                                                                                    //chargingStation.SubOperator?.Name. Contains(pattern) == true ||
                                                                                    //chargingStation.Owner?.      Name. Contains(pattern) == true ||
                                                                                    //chargingStation.Facilities.        Matches (pattern)         ||
                                                                                    //chargingStation.EVSEUIds.          Matches (pattern)         ||
                                                                                    //chargingStation.EVSEIds.           Matches (pattern)         ||
                                                                                    //chargingStation.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                  );
                    var skip                    = request.QueryString.GetUInt64 ("skip");
                    var take                    = request.QueryString.GetUInt64 ("take");

                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   = expand. ContainsIgnoreCase("networks")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperators         = expand. ContainsIgnoreCase("operators") ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingPools     = expand. ContainsIgnoreCase("pools")     ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = expand. ContainsIgnoreCase("stations")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandEVSEs             = expand. ContainsIgnoreCase("-evses")    ? InfoStatus.ShowIdOnly : InfoStatus.Expanded;
                    var expandBrands            = expand. ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses      = expand. ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                    var showIds                 = request.QueryString.GetStrings("showIds");
                    var showEVSEIds             = showIds.ContainsIgnoreCase("evses")     ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showBrandIds            = showIds.ContainsIgnoreCase("brands")    ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showDataLicenseIds      = showIds.ContainsIgnoreCase("licenses")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;

                    var allResults              = roamingNetwork.ChargingStations;
                    var totalCount              = allResults.ULongCount();

                    var filteredResults         = allResults.Where(matchFilter).ToArray();
                    var filteredCount           = filteredResults.ULongCount();

                    var jsonResults             = filteredResults.
                                                      OrderBy(station => station.Id).
                                                      ToJSON (skip,
                                                              take,
                                                              Embedded:                            false,
                                                              IncludeRemoved:                      includeRemoved,
                                                              ExpandRoamingNetworkId:              expandRoamingNetworks,
                                                              ExpandChargingStationOperatorId:     expandOperators,
                                                              ExpandChargingPoolId:                expandChargingPools,
                                                              ExpandEVSEIds:                       expandEVSEs,
                                                              ExpandBrandIds:                      expandBrands,
                                                              ExpandDataLicenses:                  expandDataLicenses,
                                                              CustomChargingStationSerializer:     null,
                                                              CustomEVSESerializer:                null,
                                                              CustomChargingConnectorSerializer:   null);

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode                = HTTPStatusCode.OK,
                                   Server                        = DefaultHTTPServerName,
                                   Date                          = Timestamp.Now,
                                   AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                   AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                   Content                       = withMetadata
                                                                       ? JSONObject.Create(
                                                                             new JProperty("totalCount",     totalCount),
                                                                             new JProperty("filteredCount",  filteredCount),
                                                                             new JProperty("data",           jsonResults)
                                                                         ).ToUTF8Bytes()
                                                                       : new JArray(jsonResults).ToUTF8Bytes(),
                                   ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                   X_ExpectedTotalNumberOfItems  = filteredCount,
                                   Connection                    = ConnectionType.KeepAlive,
                                   Vary                          = "Accept"
                               }.AsImmutable
                           );

                });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingStations

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingStations
            // --------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var matchFilter             = request.QueryString.CreateStringFilter<IChargingStation>(
                                                      "match",
                                                      (chargingStation, pattern) => chargingStation.ToString()?.Contains(pattern) == true
                                                                                    //chargingStation.Name?.             Contains(pattern) == true ||
                                                                                    //chargingStation.Address.           Contains(pattern)         ||
                                                                                    //chargingStation.City.              Contains(pattern)         ||
                                                                                    //chargingStation.PostalCode.        Contains(pattern)         ||
                                                                                    //chargingStation.Country.ToString()?.Contains(pattern) == true         ||
                                                                                    //chargingStation.Directions.        Matches (pattern)         ||
                                                                                    //chargingStation.Operator?.   Name. Contains(pattern) == true ||
                                                                                    //chargingStation.SubOperator?.Name. Contains(pattern) == true ||
                                                                                    //chargingStation.Owner?.      Name. Contains(pattern) == true ||
                                                                                    //chargingStation.Facilities.        Matches (pattern)         ||
                                                                                    //chargingStation.EVSEUIds.          Matches (pattern)         ||
                                                                                    //chargingStation.EVSEIds.           Matches (pattern)         ||
                                                                                    //chargingStation.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                  );

                    var allResults              = roamingNetwork.ChargingStations;
                    var totalCount              = allResults.ULongCount();

                    var filteredResults         = allResults.Where(matchFilter).ToArray();
                    var filteredCount           = filteredResults.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(
                                                               new JProperty("totalCount",     totalCount),
                                                               new JProperty("filteredCount",  filteredCount)
                                                           ).ToUTF8Bytes(),
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->Id
            // -------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->Id",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                roamingNetwork.ChargingStations.
                                                                    Select(station => station.Id.ToString()).
                                                                    Skip  (request.QueryString.GetUInt64("skip")).
                                                                    Take  (request.QueryString.GetUInt64("take"))
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.ChargingStations.ULongCount()
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->AdminStatus

            // ----------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->AdminStatus
            // ----------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip         = request.QueryString.GetUInt64("skip");
                    var take         = request.QueryString.GetUInt64("take");
                    var sinceFilter  = request.QueryString.CreateDateTimeFilter<ChargingStationAdminStatus>("since", (status, timestamp) => status.Timestamp >= timestamp);
                    var matchFilter  = request.QueryString.CreateStringFilter  <ChargingStationAdminStatus>("match", (status, pattern)   => status.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = roamingNetwork.ChargingStationAdminStatus().
                                                                 Where (matchFilter).
                                                                 Where (sinceFilter).
                                                                 ToJSON(skip, take).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = roamingNetwork.ChargingStationAdminStatus().ULongCount()
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations->Status

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations->Status
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip         = request.QueryString.GetUInt64                         ("skip");
                    var take         = request.QueryString.GetUInt64                         ("take");
                    var sinceFilter  = request.QueryString.CreateDateTimeFilter<ChargingStationStatus>("since", (status, timestamp) => status.Timestamp >= timestamp);
                    var matchFilter  = request.QueryString.CreateStringFilter  <ChargingStationStatus>("match", (status, pattern)   => status.Id.ToString()?.Contains(pattern) == true);

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = roamingNetwork.ChargingStationStatus().
                                                                 Where (matchFilter).
                                                                 Where (sinceFilter).
                                                                 ToJSON(skip, take).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = roamingNetwork.ChargingStationStatus().ULongCount()
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/ChargingStations/DynamicStatusReport",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(

                                                               new JProperty("count",  roamingNetwork.ChargingStations.Count()),

                                                               new JProperty("status", JSONObject.Create(
                                                                   roamingNetwork.ChargingStations.GroupBy(station => station.Status.Value).Select(group =>
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
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStation(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingStation,
                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "SET" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            ContentType                = HTTPContentType.Application.JSON_UTF8,
                            Content                    = chargingStation.ToJSON().ToUTF8Bytes()
                        }.AsImmutable);

                }
            );

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/.../EVSEs
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStation(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingStation,
                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                    = request.QueryString.GetUInt64("skip");
                    var take                    = request.QueryString.GetUInt64("take");
                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetworks   = expand.ContainsIgnoreCase("networks")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandOperators         = expand.ContainsIgnoreCase("operators") ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingPools     = expand.ContainsIgnoreCase("pools")     ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandChargingStations  = expand.ContainsIgnoreCase("stations")  ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;
                    var expandBrands            = expand.ContainsIgnoreCase("brands")    ? InfoStatus.Expanded : InfoStatus.ShowIdOnly;


                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount           = chargingStation.EVSEs.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingStation.EVSEs.
                                                                OrderBy(evse => evse.Id).
                                                                ToJSON (skip,
                                                                        take,
                                                                        false,
                                                                        false, // IncludeRemovedEVSEs
                                                                        expandRoamingNetworks,
                                                                        expandOperators,
                                                                        expandChargingPools,
                                                                        expandChargingStations,
                                                                        expandBrands).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus

            // ------------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus
            // ------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStation(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingStation,
                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingStation.EVSEAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingStation.EVSEAdminStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->Status

            // -------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingStations/{ChargingStationId}/EVSEs->Status
            // -------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingStations/{ChargingStationId}/EVSEs->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndChargingStation(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingStation,
                                                                          out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = chargingStation.EVSEStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingStation.EVSEStatus().
                                                                ToJSON(skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                }
            );

            #endregion

            #endregion




            // EVSEs

            #region ~/RNs/{RoamingNetworkId}/EVSEs

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/EVSEs

            // ---------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs
            // ---------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.NoContent,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region HEAD        ~/RNs/{RoamingNetworkId}/EVSEs

            // ----------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs
            // ----------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.HEAD,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            Connection                 = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs

            // ----------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs
            // ----------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   GetEVSEsRequest,
                HTTPResponseLogger:  GetEVSEsResponse,
                HTTPDelegate:        request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var withMetadata            = request.QueryString.GetBoolean("withMetadata",      false);
                    var includeRemoved          = request.QueryString.GetBoolean("includeRemoved",    false);
                    var includeCustomData       = request.QueryString.GetBoolean("includeCustomData", false);

                    var matchFilter             = request.QueryString.CreateStringFilter<IEVSE>(
                                                      "match",
                                                      (evse, pattern) => evse.ToString()?.Contains(pattern) == true
                                                                         //evse.Name?.             Contains(pattern) == true ||
                                                                         //evse.Address.           Contains(pattern)         ||
                                                                         //evse.City.              Contains(pattern)         ||
                                                                         //evse.PostalCode.        Contains(pattern)         ||
                                                                         //evse.Country.ToString()?.Contains(pattern) == true         ||
                                                                         //evse.Directions.        Matches (pattern)         ||
                                                                         //evse.Operator?.   Name. Contains(pattern) == true ||
                                                                         //evse.SubOperator?.Name. Contains(pattern) == true ||
                                                                         //evse.Owner?.      Name. Contains(pattern) == true ||
                                                                         //evse.Facilities.        Matches (pattern)         ||
                                                                         //evse.EVSEUIds.          Matches (pattern)         ||
                                                                         //evse.EVSEIds.           Matches (pattern)         ||
                                                                         //evse.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                                  );
                    var skip                    = request.QueryString.GetUInt64 ("skip");
                    var take                    = request.QueryString.GetUInt64 ("take");

                    var expand                  = request.QueryString.GetStrings("expand");
                    var expandRoamingNetwork    = expand. ContainsIgnoreCase("network")   ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandOperator          = expand. ContainsIgnoreCase("operator")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingPool      = expand. ContainsIgnoreCase("pool")      ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandChargingStation   = expand. ContainsIgnoreCase("station")   ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandBrands            = expand. ContainsIgnoreCase("brands")    ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;
                    var expandDataLicenses      = expand. ContainsIgnoreCase("licenses")  ? InfoStatus.Expanded   : InfoStatus.ShowIdOnly;

                    var showIds                 = request.QueryString.GetStrings("showIds");
                    var showBrandIds            = showIds.ContainsIgnoreCase("brands")    ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;
                    var showDataLicenseIds      = showIds.ContainsIgnoreCase("licenses")  ? InfoStatus.ShowIdOnly : InfoStatus.Hidden;


                    var allResults              = roamingNetwork.EVSEs;
                    var totalCount              = allResults.ULongCount();

                    var filteredResults         = allResults.Where(matchFilter).ToArray();
                    var filteredCount           = filteredResults.ULongCount();

                    var jsonResults             = filteredResults.
                                                      OrderBy(evse => evse.Id).
                                                      ToJSON (skip,
                                                              take,
                                                              Embedded:                         false,
                                                              IncludeRemoved:                   includeRemoved,
                                                              ExpandRoamingNetworkId:           expandRoamingNetwork,
                                                              ExpandChargingStationOperatorId:  expandOperator,
                                                              ExpandChargingPoolId:             expandChargingPool,
                                                              ExpandChargingStationId:          expandChargingStation,
                                                              ExpandBrandIds:                   expandBrands,
                                                              ExpandDataLicenses:               expandDataLicenses,
                                                              IncludeCustomData:                includeCustomData);

                    return Task.FromResult(
                               new HTTPResponse.Builder(request) {
                                   HTTPStatusCode                = HTTPStatusCode.OK,
                                   Server                        = DefaultHTTPServerName,
                                   Date                          = Timestamp.Now,
                                   AccessControlAllowMethods     = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                   AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                   Content                       = withMetadata
                                                                       ? JSONObject.Create(
                                                                             new JProperty("totalCount",     totalCount),
                                                                             new JProperty("filteredCount",  filteredCount),
                                                                             new JProperty("data",           jsonResults)
                                                                         ).ToUTF8Bytes()
                                                                       : new JArray(jsonResults).ToUTF8Bytes(),
                                   ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                   X_ExpectedTotalNumberOfItems  = filteredCount,
                                   Connection                    = ConnectionType.KeepAlive,
                                   Vary                          = "Accept"
                               }.AsImmutable
                           );

                });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/EVSEs

            // ---------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/EVSEs
            // ---------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check anonymous access

                    if (!AllowsAnonymousReadAccesss)
                        return Task.FromResult(
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                WWWAuthenticate            = WWWAuthenticateDefaults,
                                Connection                 = ConnectionType.KeepAlive
                            }.AsImmutable);

                    #endregion

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var matchFilter      = request.QueryString.CreateStringFilter<IEVSE>(
                                               "match",
                                               (evse, pattern) => evse.ToString()?.Contains(pattern) == true
                                                                  //evse.Name?.             Contains(pattern) == true ||
                                                                  //evse.Address.           Contains(pattern)         ||
                                                                  //evse.City.              Contains(pattern)         ||
                                                                  //evse.PostalCode.        Contains(pattern)         ||
                                                                  //evse.Country.ToString()?.Contains(pattern) == true         ||
                                                                  //evse.Directions.        Matches (pattern)         ||
                                                                  //evse.Operator?.   Name. Contains(pattern) == true ||
                                                                  //evse.SubOperator?.Name. Contains(pattern) == true ||
                                                                  //evse.Owner?.      Name. Contains(pattern) == true ||
                                                                  //evse.Facilities.        Matches (pattern)         ||
                                                                  //evse.EVSEUIds.          Matches (pattern)         ||
                                                                  //evse.EVSEIds.           Matches (pattern)         ||
                                                                  //evse.EVSEs.Any(evse => evse.Connectors.Any(connector => connector?.GetTariffId(emspId).ToString()?.Contains(pattern) == true))
                                           );

                    var allResults       = roamingNetwork.EVSEs;
                    var totalCount       = allResults.ULongCount();

                    var filteredResults  = allResults.Where(matchFilter).ToArray();
                    var filteredCount    = filteredResults.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "OPTIONS", "HEAD", "GET", "COUNT" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(
                                                               new JProperty("totalCount",     totalCount),
                                                               new JProperty("filteredCount",  filteredCount)
                                                           ).ToUTF8Bytes(),
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->Id

            // --------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->Id
            // --------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->Id",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                roamingNetwork.EVSEs.
                                                                    Select(evse => evse.Id.ToString()).
                                                                    Skip  (request.QueryString.GetUInt64("skip")).
                                                                    Take  (request.QueryString.GetUInt64("take"))
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.EVSEs.ULongCount(),
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->AdminStatus

            // -----------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->AdminStatus
            // -----------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip             = request.QueryString.GetUInt64("skip");
                    var take             = request.QueryString.GetUInt64("take");
                    var statusSkip       = request.QueryString.GetUInt64                            ("statusSkip",  1);
                    var statusTake       = request.QueryString.GetUInt64                            ("statusTake",  1);
                    var notBeforeFilter  = request.QueryString.CreateDateTimeFilter<EVSEAdminStatus>("before",      (evseAdminStatus, timestamp) => evseAdminStatus.Timestamp >= timestamp);
                    var notAfterFilter   = request.QueryString.CreateDateTimeFilter<EVSEAdminStatus>("after",       (evseAdminStatus, timestamp) => evseAdminStatus.Timestamp <  timestamp);
                    var matchFilter      = request.QueryString.CreateStringFilter  <EVSE_Id>        ("match",       (evseId,          pattern)   => evseId.ToString()?.Contains(pattern) == true);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount    = roamingNetwork.EVSEAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = roamingNetwork.
                                                                 EVSEAdminStatus(evse            => matchFilter (evse.Id)).
                                                                 Where          (evseAdminStatus => notBeforeFilter(evseAdminStatus) &&
                                                                                                 notAfterFilter (evseAdminStatus)).
                                                                 ToJSON         (skip, take).
                                                                 ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->AdminStatusSchedule

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->AdminStatusSchedule
            // -------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->AdminStatusSchedule",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var statusSkip     = request.QueryString.GetUInt64("statusSkip",  1);
                    var statusTake     = request.QueryString.GetUInt64("statusTake",  1);
                    var since          = request.QueryString.CreateDateTimeFilter<EVSEAdminStatusSchedule>("since", (status, timestamp) => status.StatusSchedule.First().Timestamp >= timestamp);
                    var afterFilter    = request.QueryString.CreateDateTimeFilter<DateTime>("after",       (timestamp1, timestamp2) => timestamp1 >= timestamp2);
                    var beforeFilter   = request.QueryString.CreateDateTimeFilter<DateTime>("before",      (timestamp1, timestamp2) => timestamp1 <= timestamp2);
                    var matchFilter    = request.QueryString.CreateStringFilter  <EVSE_Id> ("match",       (evseId,     pattern)    => evseId.ToString()?.Contains(pattern) == true);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.EVSEAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            //Content                        = RoamingNetwork.EVSEAdminStatusSchedule(IncludeEVSEs:    evse      => matchFilter (evse.Id),
                            //                                                                        TimestampFilter: timestamp => beforeFilter(timestamp) &&
                            //                                                                                                      afterFilter (timestamp),
                            //                                                                        Skip:            statusSkip,
                            //                                                                        Take:            statusTake).
                            //                                                ToJSON(skip, take).
                            //                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->Status

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->Status
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:  SendGetEVSEsStatusRequest,
                HTTPResponseLogger: SendGetEVSEsStatusResponse,
                HTTPDelegate:       request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip             = request.QueryString.GetUInt64                       ("skip");
                    var take             = request.QueryString.GetUInt64                       ("take");
                    var statusSkip       = request.QueryString.GetUInt64                       ("statusSkip",  1);
                    var statusTake       = request.QueryString.GetUInt64                       ("statusTake",  1);
                    var notBeforeFilter  = request.QueryString.CreateDateTimeFilter<EVSEStatus>("notBefore",   (evseStatus, timestamp) => evseStatus.Timestamp >= timestamp);
                    var notAfterFilter   = request.QueryString.CreateDateTimeFilter<EVSEStatus>("notAfter",    (evseStatus, timestamp) => evseStatus.Timestamp <  timestamp);
                    var matchFilter      = request.QueryString.CreateStringFilter  <EVSE_Id>   ("match",       (evseId,     pattern)   => evseId.ToString()?.Contains(pattern) == true);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount    = roamingNetwork.EVSEStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = roamingNetwork.EVSEStatus    (evse       => matchFilter (evse.Id)).
                                                                            Where         (evseStatus => notBeforeFilter(evseStatus) &&
                                                                                                        notAfterFilter (evseStatus)).
                                                                            ToJSON(skip, take).
                                                                            ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                    }, AllowReplacement: URLReplacement.Allow);

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs->StatusSchedule

            // -----------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs->StatusSchedule
            // -----------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs->StatusSchedule",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendGetEVSEsStatusRequest,
                HTTPResponseLogger:  SendGetEVSEsStatusResponse,
                HTTPDelegate:        request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64                           ("skip");
                    var take           = request.QueryString.GetUInt64                           ("take");
                    var statusSkip     = request.QueryString.GetUInt64                           ("statusSkip",  1);
                    var statusTake     = request.QueryString.GetUInt64                           ("statusTake",  1);
                    var afterFilter    = request.QueryString.CreateDateTimeFilter<DateTimeOffset>("after",       (timestamp, pattern) => timestamp >= pattern);
                    var beforeFilter   = request.QueryString.CreateDateTimeFilter<DateTimeOffset>("before",      (timestamp, pattern) => timestamp <= pattern);
                    var matchFilter    = request.QueryString.CreateStringFilter  <EVSE_Id>       ("match",       (evseId,    pattern) => evseId.ToString()?.Contains(pattern) == true);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.EVSEStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                 = HTTPStatusCode.OK,
                            Server                         = HTTPServer.HTTPServerName,
                            Date                           = Timestamp.Now,
                            AccessControlAllowOrigin       = "*",
                            AccessControlAllowMethods      = [ "GET" ],
                            AccessControlAllowHeaders      = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                           = "1",
                            ContentType                    = HTTPContentType.Application.JSON_UTF8,
                            Content                        = new JArray(
                                                                 roamingNetwork.EVSEStatusSchedule(IncludeEVSEs:    evse      => matchFilter (evse.Id),
                                                                                                   TimestampFilter: timestamp => beforeFilter(timestamp) &&
                                                                                                                                   afterFilter (timestamp),
                                                                                                   Skip:            statusSkip,
                                                                                                   Take:            statusTake).
                                                                                SkipTakeFilter    (skip, take).
                                                                                Select            (kvp => JSONObject.Create(
                                                                                                                new JProperty("evseId",       kvp.Item1.ToString()),
                                                                                                                new JProperty("status")
                                                                                                            //   new JProperty()
                                                                                                            ))
                                                             ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems   = expectedCount,
                            Connection                     = ConnectionType.KeepAlive
                        }.AsImmutable);

                    }, AllowReplacement: URLReplacement.Allow);

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport

            // --------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport
            // --------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/DynamicStatusReport",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "OPTIONS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(

                                                               new JProperty("count",  roamingNetwork.EVSEs.Count()),

                                                               new JProperty("status", JSONObject.Create(
                                                                   roamingNetwork.EVSEs.GroupBy(evse => evse.Status.Value).Select(group =>
                                                                       new JProperty(group.Key.ToString().ToLower(),
                                                                                       group.Count()))
                                                               ))

                                                           ).ToUTF8Bytes(),
                            Connection                   = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            #region OPTIONS     ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // --------------------------------------------------------------------------------------------------------
            // curl -v -X OPTIONS -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            // --------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.OPTIONS,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPDelegate: Request => {

                    #region Check RoamingNetworkId and EVSEId URI parameters

                    if (!Request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(Request) {
                            HTTPStatusCode              = HTTPStatusCode.NoContent,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR", "OPTIONS" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                        }.AsImmutable);

                },
                AllowReplacement: URLReplacement.Allow
            );

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // ---------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            // ---------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = (evse?.ToJSON() ?? []).ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus

            // -----------------------------------------------------------------------
            // curl -v -H "Accept:       application/json" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*E0001*1/AdminStatus
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Parse RoamingNetworkId and EVSEId parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = evse.AdminStatus.
                                                              ToJSON().
                                                              ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                }
            );

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status

            // -----------------------------------------------------------------------
            // curl -v -H "Accept:       application/json" \
            //      http://127.0.0.1:5500/RNs/TEST/EVSEs/DE*GEF*E0001*1/Status
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Parse RoamingNetworkId and EVSEId parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode              = HTTPStatusCode.OK,
                            Server                      = HTTPServer.HTTPServerName,
                            Date                        = Timestamp.Now,
                            AccessControlAllowOrigin    = "*",
                            AccessControlAllowMethods   = [ "GET" ],
                            AccessControlAllowHeaders   = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                        = "1",
                            ContentType                 = HTTPContentType.Application.JSON_UTF8,
                            Content                     = evse.Status.
                                                              ToJSON().
                                                              ToUTF8Bytes(),
                            Connection                  = ConnectionType.KeepAlive
                        }.AsImmutable);

                }
            );

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
            AddHandler(

                RESERVE,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendReserveEVSERequest,
                HTTPResponseLogger:  SendReserveEVSEResponse,
                HTTPDelegate:        async request => {

                    #region Check RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder.AsImmutable;
                    }

                    #endregion

                    #region Define (optional) parameters

                    ChargingReservation_Id?  ReservationId         = null;
                    ChargingReservation_Id?  LinkedReservationId   = null;
                    EMobilityProvider_Id?    ProviderId            = null;
                    EMobilityAccount_Id      eMAId                 = default;
                    DateTime?                StartTime             = null;
                    TimeSpan?                Duration              = null;
                    Auth_Path?               AuthenticationPath    = null;

                    // IntendedCharging
                    ChargingProduct_Id?      ChargingProductId     = null;
                    DateTime?                ChargingStartTime     = null;
                    TimeSpan?                CharingDuration       = null;
                    ChargingPlugTypes?       Plug                  = null;
                    var                      Consumption           = 0U;

                    // AuthorizedIds
                    var                      authenticationTokens  = new List<AuthenticationToken>();
                    var                      eMAIds                = new List<EMobilityAccount_Id>();
                    var                      PINs                  = new List<UInt32>();

                    #endregion

                    #region Parse  (optional) JSON

                    if (request.TryParseJSONObjectRequestBody(out var JSON,
                                                              out httpResponseBuilder,
                                                              AllowEmptyHTTPBody: true))
                    {

                        #region Check ReservationId        [optional]

                        if (JSON.ParseOptionalStruct2("ReservationId",
                                                      "ReservationId",
                                                      HTTPServer.HTTPServerName,
                                                      ChargingReservation_Id.TryParse,
                                                      out ReservationId,
                                                      request,
                                                      out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                        }

                        #endregion

                        #region Check LinkedReservationId  [optional]

                        if (JSON.ParseOptionalStruct2("linkedReservationId",
                                                      "linked reservation identification",
                                                      HTTPServer.HTTPServerName,
                                                      ChargingReservation_Id.TryParse,
                                                      out LinkedReservationId,
                                                      request,
                                                      out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                        }

                        #endregion

                        #region Check ProviderId           [optional]

                        if (JSON.ParseOptionalStruct2("ProviderId",
                                                      "ProviderId",
                                                      HTTPServer.HTTPServerName,
                                                      EMobilityProvider_Id.TryParse,
                                                      out ProviderId,
                                                      request,
                                                      out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                        }

                        #endregion

                        #region Check eMAId                [mandatory]

                        if (!JSON.ParseMandatory("eMAId",
                                                 "eMAId",
                                                 HTTPServer.HTTPServerName,
                                                 EMobilityAccount_Id.TryParse,
                                                 out eMAId,
                                                 request,
                                                 out httpResponseBuilder))
                        {
                            return httpResponseBuilder;
                        }

                        #endregion

                        #region Check StartTime            [optional]

                        if (JSON.ParseOptional("StartTime",
                                               "start time!",
                                               HTTPServer.HTTPServerName,
                                               out StartTime,
                                               request,
                                               out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                            if (StartTime <= Timestamp.Now)
                                return new HTTPResponse.Builder(request) {
                                            HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                            ContentType     = HTTPContentType.Application.JSON_UTF8,
                                            Content         = new JObject(new JProperty("description", "The starting time must be in the future!")).ToUTF8Bytes()
                                        };

                        }

                        #endregion

                        #region Check Duration             [optional]

                        if (JSON.ParseOptional("Duration",
                                               "Duration",
                                               HTTPServer.HTTPServerName,
                                               out Duration,
                                               request,
                                               out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                        }

                        #endregion

                        #region Check IntendedCharging     [optional]

                        if (JSON.ParseOptional("IntendedCharging",
                                               "IntendedCharging",
                                               HTTPServer.HTTPServerName,
                                               out JObject IntendedChargingJSON,
                                               request,
                                               out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                            #region Check ChargingStartTime    [optional]

                            if (IntendedChargingJSON.ParseOptional("StartTime",
                                                                   "IntendedCharging/StartTime",
                                                                   HTTPServer.HTTPServerName,
                                                                   out ChargingStartTime,
                                                                   request,
                                                                   out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is not null)
                                    return httpResponseBuilder;

                            }

                            #endregion

                            #region Check Duration             [optional]

                            if (IntendedChargingJSON.ParseOptional("Duration",
                                                                   "IntendedCharging/Duration",
                                                                   HTTPServer.HTTPServerName,
                                                                   out CharingDuration,
                                                                   request,
                                                                   out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is not null)
                                    return httpResponseBuilder;

                            }

                            #endregion

                            #region Check ChargingProductId    [optional]

                            if (!JSON.ParseOptional("ChargingProductId",
                                                    "IntendedCharging/ChargingProductId",
                                                    HTTPServer.HTTPServerName,
                                                    out ChargingProductId,
                                                    request,
                                                    out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is not null)
                                    return httpResponseBuilder;

                            }

                            #endregion

                            #region Check Plug                 [optional]

                            if (IntendedChargingJSON.ParseOptional("Plug",
                                                                   "IntendedCharging/ChargingProductId",
                                                                   HTTPServer.HTTPServerName,
                                                                   out Plug,
                                                                   request,
                                                                   out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is not null)
                                    return httpResponseBuilder;

                            }

                            #endregion

                            #region Check Consumption          [optional, kWh]

                            if (IntendedChargingJSON.ParseOptional("Consumption",
                                                                   "IntendedCharging/Consumption",
                                                                   HTTPServer.HTTPServerName,
                                                                   UInt32.Parse,
                                                                   out Consumption,
                                                                   request,
                                                                   out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is not null)
                                    return httpResponseBuilder;

                            }

                            #endregion

                        }

                        #endregion

                        #region Check AuthorizedIds        [optional]

                        if (JSON.ParseOptional("AuthorizedIds",
                                               "AuthorizedIds",
                                               HTTPServer.HTTPServerName,
                                               out JObject AuthorizedIdsJSON,
                                               request,
                                               out httpResponseBuilder))
                        {

                            if (httpResponseBuilder is not null)
                                return httpResponseBuilder;

                            #region Check AuthTokens   [optional]

                            if (AuthorizedIdsJSON.ParseOptional("AuthTokens",
                                                                "AuthorizedIds/AuthTokens",
                                                                HTTPServer.HTTPServerName,
                                                                out JArray AuthTokensJSON,
                                                                request,
                                                                out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is null)
                                    return httpResponseBuilder;

                                foreach (var jtoken in AuthTokensJSON)
                                {

                                    if (!AuthenticationToken.TryParse(jtoken?.Value<String>() ?? "", out var authenticationToken))
                                        return new HTTPResponse.Builder(request) {
                                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                Server                     = HTTPServer.HTTPServerName,
                                                Date                       = Timestamp.Now,
                                                AccessControlAllowOrigin   = "*",
                                                AccessControlAllowMethods  = [ "RESERVE", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                                Content                    = new JObject(new JProperty("description", "Invalid AuthorizedIds/RFIDId '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                            };

                                    authenticationTokens.Add(authenticationToken);

                                }

                            }

                            #endregion

                            #region Check eMAIds       [optional]

                            if (AuthorizedIdsJSON.ParseOptional("eMAIds",
                                                                "AuthorizedIds/eMAIds",
                                                                HTTPServer.HTTPServerName,
                                                                out JArray eMAIdsJSON,
                                                                request,
                                                                out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is null)
                                    return httpResponseBuilder;

                                foreach (var jtoken in eMAIdsJSON)
                                {

                                    if (!EMobilityAccount_Id.TryParse(jtoken?.Value<String>() ?? "", out EMobilityAccount_Id eMAId2))
                                        return new HTTPResponse.Builder(request)
                                        {
                                            HTTPStatusCode = HTTPStatusCode.BadRequest,
                                            Server = HTTPServer.HTTPServerName,
                                            Date = Timestamp.Now,
                                            AccessControlAllowOrigin = "*",
                                            AccessControlAllowMethods = ["RESERVE", "REMOTESTART", "REMOTESTOP", "SENDCDR"],
                                            AccessControlAllowHeaders = ["Content-Type", "Accept", "Authorization"],
                                            ContentType = HTTPContentType.Application.JSON_UTF8,
                                            Content = new JObject(new JProperty("description", "Invalid AuthorizedIds/eMAIds '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                        };

                                    eMAIds.Add(eMAId2);

                                }

                            }

                            #endregion

                            #region Check PINs         [optional]

                            if (AuthorizedIdsJSON.ParseOptional("PINs",
                                                                "AuthorizedIds/PINs",
                                                                HTTPServer.HTTPServerName,
                                                                out JArray PINsJSON,
                                                                request,
                                                                out httpResponseBuilder))
                            {

                                if (httpResponseBuilder is null)
                                    return httpResponseBuilder;


                                UInt32 PIN = 0;

                                foreach (var jtoken in PINsJSON)
                                {

                                    if (!UInt32.TryParse(jtoken?.Value<String>() ?? "", out PIN))
                                        return new HTTPResponse.Builder(request) {
                                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                                Server                     = HTTPServer.HTTPServerName,
                                                Date                       = Timestamp.Now,
                                                AccessControlAllowOrigin   = "*",
                                                AccessControlAllowMethods  = [ "RESERVE", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                                Content                    = new JObject(new JProperty("description", "Invalid AuthorizedIds/PINs '" + jtoken.Value<String>() + "' section!")).ToUTF8Bytes()
                                            };

                                    PINs.Add(PIN);

                                }

                            }

                            #endregion

                        }

                        #endregion

                        #region Parse AuthenticationPath    [optional]

                        if (JSON.ParseOptional("authenticationPath",
                                               "authentication path",
                                               Auth_Path.TryParse,
                                               out AuthenticationPath,
                                               out var errorResponse))
                        {
                            if (errorResponse is not null)
                                return new HTTPResponse.Builder(request) {
                                            HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                            ContentType     = HTTPContentType.Application.JSON_UTF8,
                                            Content         = new JObject(new JProperty("description", "Invalid authentication path: " + errorResponse)).ToUTF8Bytes()
                                        };
                        }

                        #endregion

                    }

                    if (httpResponseBuilder                is not null &&
                        httpResponseBuilder.HTTPStatusCode == HTTPStatusCode.BadRequest)
                    {
                        return httpResponseBuilder;
                    }

                    #endregion


                    var result = await roamingNetwork.Reserve(
                                           ChargingLocation.FromEVSEId(evse.Id),
                                           ChargingReservationLevel.EVSE,
                                           StartTime,
                                           Duration,
                                           ReservationId,
                                           LinkedReservationId,
                                           ProviderId,
                                           RemoteAuthentication.FromRemoteIdentification(eMAId),
                                           AuthenticationPath,
                                           ChargingProductId.HasValue
                                               ? new ChargingProduct(ChargingProductId.Value)
                                               : null,
                                           authenticationTokens,
                                           eMAIds,
                                           PINs,
                                           null,

                                           request.Timestamp,
                                           request.EventTrackingId,
                                           null,
                                           request.CancellationToken
                                       ).ConfigureAwait(false);


                    var Now = Timestamp.Now;

                    #region Success

                    if (result.Result == ReservationResultType.Success && result.Reservation is not null)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Created,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                Location                   = Location.From(URLPathPrefix + "RNs/" + roamingNetwork.Id.ToString() + "/Reservations/" + result.Reservation.Id.ToString()),
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                    new JProperty("reservationId",           result.Reservation.Id.       ToString()),
                                                                    new JProperty("startTime",               result.Reservation.StartTime.ToISO8601()),
                                                                    new JProperty("duration",       (UInt32) result.Reservation.Duration. TotalSeconds)
                                                                ).ToUTF8Bytes()
                            };

                    #endregion

                    #region AlreadInUse

                    //else if (result.Result == ReservationResultType.ReservationId_AlreadyInUse)
                    //    return new HTTPResponse.Builder(HTTPRequest) {
                    //        HTTPStatusCode             = HTTPStatusCode.Conflict,
                    //        Server                     = API.HTTPTestServer?.HTTPServerName,
                    //        Date                       = Timestamp.Now,
                    //        AccessControlAllowOrigin   = "*",
                    //        AccessControlAllowMethods  = [ "RESERVE", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                    //        AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                    //        ContentType                = HTTPContentType.Application.JSON_UTF8,
                    //        Content                    = new JObject(new JProperty("description",  "ReservationId is already in use!")).ToUTF8Bytes(),
                    //        Connection                 = ConnectionType.KeepAlive
                    //    };

                    #endregion

                    #region ...or fail

                    else
                    return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                Connection                 = ConnectionType.KeepAlive
                            };

                    #endregion

                }, AllowReplacement: URLReplacement.Allow);

            #endregion

            #region AUTHSTART   ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X AUTHSTART -H "Content-Type: application/json" \
            //                      -H "Accept:       application/json" \
            //      -d "{ \"AuthToken\":  \"00112233\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            AddHandler(

                AUTHSTART,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendAuthStartEVSERequest,
                HTTPResponseLogger:  SendAuthStartEVSEResponse,
                HTTPDelegate:        async request => {

                    #region Parse RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #region Parse OperatorId             [optional]

                    if (!json.ParseOptional("OperatorId",
                                            "Charging Station Operator identification",
                                            HTTPServer.HTTPServerName,
                                            ChargingStationOperator_Id.TryParse,
                                            out ChargingStationOperator_Id operatorId,
                                            request,
                                            out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse AuthToken              [mandatory]

                    if (!json.ParseMandatory("AuthToken",
                                             "authentication token",
                                             HTTPServer.HTTPServerName,
                                             AuthenticationToken.TryParse,
                                             out AuthenticationToken authToken,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse SessionId              [optional]

                    if (!json.ParseOptionalStruct2("SessionId",
                                                   "Charging session identification",
                                                   HTTPServer.HTTPServerName,
                                                   ChargingSession_Id.TryParse,
                                                   out ChargingSession_Id? sessionId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse CPOPartnerSessionId    [optional]

                    if (!json.ParseOptionalStruct2("CPOPartnerSessionId",
                                                   "CPO partner charging session identification",
                                                   HTTPServer.HTTPServerName,
                                                   ChargingSession_Id.TryParse,
                                                   out ChargingSession_Id? cpoPartnerSessionId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse ChargingProductId      [optional]

                    if (!json.ParseOptionalStruct2("ChargingProductId",
                                                   "Charging product identification",
                                                   HTTPServer.HTTPServerName,
                                                   ChargingProduct_Id.TryParse,
                                                   out ChargingProduct_Id? chargingProductId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #endregion


                    var result  = await roamingNetwork.AuthorizeStart(
                                            LocalAuthentication.FromAuthToken(authToken),
                                            ChargingLocation.FromEVSEId(evse.Id),
                                            chargingProductId.HasValue
                                                ? new ChargingProduct(chargingProductId.Value)
                                                : null,
                                            sessionId,
                                            cpoPartnerSessionId,
                                            operatorId,

                                            request.Timestamp,
                                            request.EventTrackingId,
                                            null,
                                            request.CancellationToken
                                        );


                    #region Authorized

                    if (result.Result == AuthStartResultTypes.Authorized)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                    #region NotAuthorized

                    else if (result.Result == AuthStartResultTypes.Error)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                    #region Forbidden

                    else
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Forbidden, //ToDo: Is this smart?
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                },
                AllowReplacement: URLReplacement.Allow
            );

            #endregion

            #region AUTHSTOP    ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X AUTHSTOP -H "Content-Type: application/json" \
            //                     -H "Accept:       application/json" \
            //      -d "{ \"SessionId\":  \"60ce73f6-0a88-1296-3d3d-623fdd276ddc\", \
            //            \"AuthToken\":  \"00112233\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            AddHandler(

                AUTHSTOP,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:  SendAuthStopEVSERequest,
                HTTPResponseLogger: SendAuthStopEVSEResponse,
                HTTPDelegate:       async request => {

                    #region Parse RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var JSON,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #region Parse SessionId    [mandatory]

                    if (!JSON.ParseMandatory("SessionId",
                                             "Charging session identification",
                                             HTTPServer.HTTPServerName,
                                             ChargingSession_Id.TryParse,
                                             out ChargingSession_Id sessionId,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse AuthToken    [mandatory]

                    if (!JSON.ParseMandatory("AuthToken",
                                             "Authentication token",
                                             HTTPServer.HTTPServerName,
                                             AuthenticationToken.TryParse,
                                             out AuthenticationToken AuthToken,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse CPOPartnerSessionId    [optional]

                    if (!JSON.ParseOptionalStruct2("CPOPartnerSessionId",
                                                   "CPO partner charging session identification",
                                                   HTTPServer.HTTPServerName,
                                                   ChargingSession_Id.TryParse,
                                                   out ChargingSession_Id? CPOPartnerSessionId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse OperatorId   [optional]

                    if (!JSON.ParseOptional("OperatorId",
                                            "Charging Station Operator identification",
                                            HTTPServer.HTTPServerName,
                                            ChargingStationOperator_Id.TryParse,
                                            out ChargingStationOperator_Id chargingStationOperatorId,
                                            request,
                                            out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #endregion


                    var result = await roamingNetwork.AuthorizeStop(
                                           sessionId,
                                           LocalAuthentication.FromAuthToken(AuthToken),
                                           ChargingLocation.   FromEVSEId    (evse.Id),
                                           CPOPartnerSessionId,
                                           chargingStationOperatorId,

                                           request.Timestamp,
                                           request.EventTrackingId,
                                           null,
                                           request.CancellationToken
                                       );


                    #region Authorized

                    if (result.Result == AuthStopResultTypes.Authorized)
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.OK,
                                   Server                     = HTTPServer.HTTPServerName,
                                   Date                       = Timestamp.Now,
                                   AccessControlAllowOrigin   = "*",
                                   AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                   AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                   ContentType                = HTTPContentType.Application.JSON_UTF8,
                                   Content                    = result.ToJSON().ToUTF8Bytes()
                               };

                    #endregion

                    #region NotAuthorized

                    else if (result.Result == AuthStopResultTypes.NotAuthorized)
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                   Server                     = HTTPServer.HTTPServerName,
                                   Date                       = Timestamp.Now,
                                   AccessControlAllowOrigin   = "*",
                                   AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                   AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                   ContentType                = HTTPContentType.Application.JSON_UTF8,
                                   Content                    = result.ToJSON().ToUTF8Bytes()
                               };

                    #endregion

                    #region Forbidden

                    return new HTTPResponse.Builder(request) {
                               HTTPStatusCode             = HTTPStatusCode.Forbidden, //ToDo: Is this smart?
                               Server                     = HTTPServer.HTTPServerName,
                               Date                       = Timestamp.Now,
                               AccessControlAllowOrigin   = "*",
                               AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                               AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                               ContentType                = HTTPContentType.Application.JSON_UTF8,
                               Content                    = result.ToJSON().ToUTF8Bytes()
                           };

                    #endregion

                },
                AllowReplacement: URLReplacement.Allow
            );

            #endregion

            #region REMOTESTART ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X REMOTESTART -H "Content-Type: application/json" \
            //                        -H "Accept:       application/json"  \
            //      -d "{ \"ProviderId\":  \"DE*GDF\", \
            //            \"eMAId\":       \"DE*GDF*00112233*1\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            AddHandler(

                REMOTESTART,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendRemoteStartEVSERequest,
                HTTPResponseLogger:  SendRemoteStartEVSEResponse,
                HTTPDelegate:        async request => {

                    #region Get RoamingNetwork and EVSE URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON  [optional]

                    ChargingProduct_Id?      ChargingProductId    = null;
                    ChargingReservation_Id?  ReservationId        = null;
                    ChargingSession_Id?      SessionId            = null;
                    EMobilityProvider_Id?    ProviderId           = null;
                    EMobilityAccount_Id      eMAId                = default;
                    Auth_Path?               AuthenticationPath   = null;

                    if (request.TryParseJSONObjectRequestBody(out var json,
                                                              out httpResponseBuilder))
                    {

                        #region Check ChargingProductId  [optional]

                        if (!json.ParseOptionalStruct2("ChargingProductId",
                                                       "Charging product identification",
                                                       HTTPServer.HTTPServerName,
                                                       ChargingProduct_Id.TryParse,
                                                       out ChargingProductId,
                                                       request,
                                                       out httpResponseBuilder))
                        {

                            return httpResponseBuilder;

                        }

                        #endregion

                        // MaxKWh
                        // MaxPrice

                        #region Check ReservationId      [optional]

                        if (!json.ParseOptionalStruct2("ReservationId",
                                                       "Charging reservation identification",
                                                       HTTPServer.HTTPServerName,
                                                       ChargingReservation_Id.TryParse,
                                                       out ReservationId,
                                                       request,
                                                       out httpResponseBuilder))
                        {

                            return httpResponseBuilder;

                        }

                        #endregion

                        #region Parse SessionId          [optional]

                        if (!json.ParseOptionalStruct2("SessionId",
                                                       "Charging session identification",
                                                       HTTPServer.HTTPServerName,
                                                       ChargingSession_Id.TryParse,
                                                       out SessionId,
                                                       request,
                                                       out httpResponseBuilder))
                        {

                            return httpResponseBuilder;

                        }

                        #endregion

                        #region Parse ProviderId         [optional]

                        if (!json.ParseOptionalStruct2("ProviderId",
                                                       "EV service provider identification",
                                                       HTTPServer.HTTPServerName,
                                                       EMobilityProvider_Id.TryParse,
                                                       out ProviderId,
                                                       request,
                                                       out httpResponseBuilder))
                        {

                            return httpResponseBuilder;

                        }

                        #endregion

                        #region Parse eMAId             [mandatory]

                        if (!json.ParseMandatory("eMAId",
                                                 "e-Mobility account identification",
                                                 HTTPServer.HTTPServerName,
                                                 EMobilityAccount_Id.TryParse,
                                                 out eMAId,
                                                 request,
                                                 out httpResponseBuilder))

                            return httpResponseBuilder;

                        #endregion

                        #region Parse AuthenticationPath    [optional]

                        if (json.ParseOptional("authenticationPath",
                                               "authentication path",
                                               Auth_Path.TryParse,
                                               out AuthenticationPath,
                                               out var errorResponse))
                        {
                            if (errorResponse is not null)
                                return new HTTPResponse.Builder(request) {
                                        HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                        ContentType     = HTTPContentType.Application.JSON_UTF8,
                                        Content         = new JObject(new JProperty("description", "Invalid authentication path: " + errorResponse)).ToUTF8Bytes()
                                    };
                        }

                        #endregion

                    }

                    else
                        return httpResponseBuilder;

                    #endregion


                    var result = await roamingNetwork.RemoteStart(
                                           ChargingLocation.FromEVSEId(evse.Id),
                                           ChargingProductId.HasValue
                                               ? new ChargingProduct(ChargingProductId.Value)
                                               : null,
                                           ReservationId,
                                           SessionId,
                                           ProviderId,
                                           RemoteAuthentication.FromRemoteIdentification(eMAId),
                                           null,
                                           AuthenticationPath,
                                           null,

                                           request.Timestamp,
                                           request.EventTrackingId,
                                           null,
                                           request.CancellationToken
                                       );


                    #region Success

                    if (result.Result == RemoteStartResultTypes.Success)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Created,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                new JProperty("SessionId",  result.Session.Id.ToString())
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                    #region ...or fail!

                    else
                        return 
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                result.Session is not null
                                                                    ? new JProperty("SessionId",  result.Session.Id.ToString())
                                                                    : null,
                                                                new JProperty("Result",       result.Result.ToString()),
                                                                result.Description is not null
                                                                    ? new JProperty("Description",  result.Description)
                                                                    : null
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                }, AllowReplacement: URLReplacement.Allow);

            #endregion

            #region REMOTESTOP  ~/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}

            // -----------------------------------------------------------------------
            // curl -v -X REMOTESTOP -H "Content-Type: application/json" \
            //                       -H "Accept:       application/json"  \
            //      -d "{ \"ProviderId\":  \"DE*8BD\", \
            //            \"SessionId\":   \"60ce73f6-0a88-1296-3d3d-623fdd276ddc\", \
            //            \"eMAId\":       \"DE*GDF*00112233*1\" }" \
            //      http://127.0.0.1:5500/RNs/Test/EVSEs/DE*GEF*E000001*1
            AddHandler(

                REMOTESTOP,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendRemoteStopEVSERequest,
                HTTPResponseLogger:  SendRemoteStopEVSEResponse,
                HTTPDelegate:        async request => {

                    #region Get RoamingNetwork and EVSE URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder,
                                                               AllowEmptyHTTPBody: false))
                    {
                        return httpResponseBuilder;
                    }

                    // Bypass SessionId check for remote safety admins
                    // coming from the same ev service provider

                    #region Parse SessionId         [mandatory]

                    if (!json.ParseMandatory("SessionId",
                                             "Charging session identification",
                                             HTTPServer.HTTPServerName,
                                             ChargingSession_Id.TryParse,
                                             out ChargingSession_Id SessionId,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse ProviderId         [optional]

                    if (!json.ParseOptionalStruct2("ProviderId",
                                                   "EV service provider identification",
                                                   HTTPServer.HTTPServerName,
                                                   EMobilityProvider_Id.TryParse,
                                                   out EMobilityProvider_Id? ProviderId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder!;
                    }

                    #endregion

                    #region Parse eMAId              [optional]

                    if (!json.ParseOptionalStruct2("eMAId",
                                                   "e-Mobility account identification",
                                                   HTTPServer.HTTPServerName,
                                                   EMobilityAccount_Id.TryParse,
                                                   out EMobilityAccount_Id? eMAId,
                                                   request,
                                                   out httpResponseBuilder))
                    {
                        return httpResponseBuilder!;
                    }

                    #endregion

                    // ReservationHandling

                    #region Parse AuthenticationPath    [optional]

                    if (json.ParseOptional("authenticationPath",
                                           "authentication path",
                                           Auth_Path.TryParse,
                                           out Auth_Path? AuthenticationPath,
                                           out var errorResponse))
                    {
                        if (errorResponse is not null)
                            return new HTTPResponse.Builder(request) {
                                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                                    Content         = new JObject(new JProperty("description", "Invalid authentication path: " + errorResponse)).ToUTF8Bytes()
                                };
                    }

                    #endregion

                    #endregion


                    var result = await roamingNetwork.RemoteStop(
                                           SessionId,
                                           ReservationHandling.Close, // ToDo: Parse this property!
                                           ProviderId,
                                           RemoteAuthentication.FromRemoteIdentification(eMAId),
                                           null,
                                           AuthenticationPath,
                                           null,

                                           request.Timestamp,
                                           request.EventTrackingId,
                                           null,
                                           request.CancellationToken
                                       );


                    #region Success

                    if (result.Result == RemoteStopResultTypes.Success)
                    {

                        if (result.ReservationHandling.IsKeepAlive == false)
                            return new HTTPResponse.Builder(request) {
                                    HTTPStatusCode             = HTTPStatusCode.NoContent,
                                    Server                     = HTTPServer.HTTPServerName,
                                    Date                       = Timestamp.Now,
                                    AccessControlAllowOrigin   = "*",
                                    AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                    AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                };

                        else
                            return 
                                new HTTPResponse.Builder(request) {
                                    HTTPStatusCode             = HTTPStatusCode.OK,
                                    Server                     = HTTPServer.HTTPServerName,
                                    Date                       = Timestamp.Now,
                                    AccessControlAllowOrigin   = "*",
                                    AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                    AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                    ContentType                = HTTPContentType.Application.JSON_UTF8,
                                    Content                    = new JObject(
                                                                    new JProperty("KeepAlive", (Int32) result.ReservationHandling.KeepAliveTime.TotalSeconds)
                                                                ).ToUTF8Bytes()
                                };

                    }

                    #endregion

                    #region ...or fail

                    else
                        return 
                            new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = new JObject(
                                                                new JProperty("description",  result.Result.ToString())
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                }, AllowReplacement: URLReplacement.Allow);

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
            AddHandler(

                SENDCDR,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendCDRsRequest,
                HTTPResponseLogger:  SendCDRsResponse,
                HTTPDelegate:        async request => {

                    #region Parse RoamingNetwork and EVSE

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #region Parse SessionId            [mandatory]

                    if (!json.ParseMandatory("SessionId",
                                             "charging session identification",
                                             HTTPServer.HTTPServerName,
                                             ChargingSession_Id.TryParse,
                                             out ChargingSession_Id SessionId,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse ChargingProductId    [optional]

                    if (json.ParseOptionalStruct2("ChargingProductId",
                                                  "charging product identification",
                                                  HTTPServer.HTTPServerName,
                                                  ChargingProduct_Id.TryParse,
                                                  out ChargingProduct_Id? ChargingProductId,
                                                  request,
                                                  out httpResponseBuilder))
                    {
                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse AuthToken or eMAId

                    if (json.ParseOptional("AuthToken",
                                           "authentication token",
                                           HTTPServer.HTTPServerName,
                                           AuthenticationToken.TryParse,
                                           out AuthenticationToken? AuthToken,
                                           request,
                                           out httpResponseBuilder))
                    {
                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;
                    }

                    if (json.ParseOptionalStruct2("eMAId",
                                                  "e-mobility account identification",
                                                  HTTPServer.HTTPServerName,
                                                  EMobilityAccount_Id.TryParse,
                                                  out EMobilityAccount_Id? eMAId,
                                                  request,
                                                  out httpResponseBuilder))
                    {
                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;
                    }


                    if (AuthToken is null && eMAId is null)
                        return new HTTPResponse.Builder(request) {
                            HTTPStatusCode  = HTTPStatusCode.BadRequest,
                            Server          = HTTPServer.HTTPServerName,
                            Date            = Timestamp.Now,
                            ContentType     = HTTPContentType.Application.JSON_UTF8,
                            Content         = new JObject(
                                                  new JProperty("description", "Missing authentication token or eMAId!")
                                              ).ToUTF8Bytes()
                        };

                    #endregion

                    #region Parse ChargeStart/End...

                    if (!json.ParseMandatory("ChargeStart",
                                             "Charging start time",
                                             HTTPServer.HTTPServerName,
                                             out DateTimeOffset ChargingStart,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    if (!json.ParseMandatory("ChargeEnd",
                                             "Charging end time",
                                             HTTPServer.HTTPServerName,
                                             out DateTimeOffset ChargingEnd,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse SessionStart/End...

                    if (!json.ParseMandatory("SessionStart",
                                             "Charging start time",
                                             HTTPServer.HTTPServerName,
                                             out DateTime SessionStart,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    if (!json.ParseMandatory("SessionEnd",
                                             "Charging end time",
                                             HTTPServer.HTTPServerName,
                                             out DateTime SessionEnd,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse MeterValueStart/End...

                    if (!json.ParseMandatory("MeterValueStart",
                                             "Energy meter start value",
                                             HTTPServer.HTTPServerName,
                                             WattHour.TryParseKWh,
                                             out WattHour MeterValueStart,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    if (!json.ParseMandatory("MeterValueEnd",
                                             "Energy meter end value",
                                             HTTPServer.HTTPServerName,
                                             WattHour.TryParseKWh,
                                             out WattHour MeterValueEnd,
                                             request,
                                             out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #endregion


                    var chargeDetailRecord  = new ChargeDetailRecord(
                                                  Id:                     ChargeDetailRecord_Id.Parse(SessionId.ToString()),
                                                  SessionId:              SessionId,
                                                  EVSEId:                 evse.Id,
                                                  EVSE:                   evse,
                                                  ChargingProduct:        ChargingProductId.HasValue
                                                                              ? new ChargingProduct(ChargingProductId.Value)
                                                                              : null,
                                                  SessionTime:            new StartEndDateTime(SessionStart, SessionEnd),
                                                  AuthenticationStart:    AuthToken.HasValue
                                                                              ? (AAuthentication) LocalAuthentication. FromAuthToken(AuthToken.Value)
                                                                              : (AAuthentication) RemoteAuthentication.FromRemoteIdentification(eMAId.Value),
                                                  //ChargingTime:         new StartEndDateTime(ChargingStart.Value, ChargingEnd.Value),
                                                  EnergyMeteringValues:   [
                                                                              new EnergyMeteringValue(ChargingStart, MeterValueStart, EnergyMeteringValueTypes.Start),
                                                                              new EnergyMeteringValue(ChargingEnd,   MeterValueEnd,   EnergyMeteringValueTypes.Stop)
                                                                          ]
                                              );

                    var result = await roamingNetwork.
                                            SendChargeDetailRecords([ chargeDetailRecord ],
                                                                    TransmissionTypes.Enqueue,

                                                                    request.Timestamp,
                                                                    request.EventTrackingId,
                                                                    null,
                                                                    request.CancellationToken);


                    #region Forwarded

                    if (result.Result == SendCDRsResultTypes.Success)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                new JProperty("Status",          "forwarded"),
                                                                new JProperty("AuthorizatorId",  result.AuthorizatorId.ToString())
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                    #region NotForwared

                    else if (result.Result == SendCDRsResultTypes.Error)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                new JProperty("Status",    "Not forwarded")
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                    #region ...or fail!

                    else
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.NotFound,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(
                                                                new JProperty("SessionId",       SessionId.ToString()),
                                                                new JProperty("Description",     result.Description),
                                                                new JProperty("AuthorizatorId",  result.AuthorizatorId.ToString())
                                                            ).ToUTF8Bytes()
                            };

                    #endregion

                },
                AllowReplacement: URLReplacement.Allow
            );

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
            AddHandler(

                HTTPMethod.SET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SetEVSEAdminStatusRequest,
                HTTPResponseLogger:  SetEVSEAdminStatusResponse,
                HTTPDelegate:        async request => {

                    #region Parse RoamingNetwork and EVSE

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #region Parse CurrentStatus  [optional]

                    if (json.ParseOptional("currentStatus",
                                           "EVSE admin status",
                                           HTTPServer.HTTPServerName,
                                           out EVSEAdminStatusType? currentStatus,
                                           request,
                                           out httpResponseBuilder))
                    {
                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse StatusList     [optional]

                    Timestamped<EVSEAdminStatusType>[]? statusList  = null;

                    if (json.ParseOptional("statusList",
                                           "status list",
                                           HTTPServer.HTTPServerName,
                                           out JObject statusListJSON,
                                           request,
                                           out httpResponseBuilder))
                    {

                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;

                        if (statusListJSON is not null)
                        {

                            try
                            {

                                statusList = [.. statusListJSON.
                                                     Values<JProperty>().
                                                     Select(jproperty => new Timestamped<EVSEAdminStatusType>(
                                                                             DateTimeOffset.Parse(jproperty.Name),
                                                                             Enum.Parse<EVSEAdminStatusType>(jproperty.Value.ToString())
                                                                         )).
                                                     OrderBy(status   => status.Timestamp) ];

                            }
                            catch
                            {
                                // Will send the below BadRequest HTTP reply...
                            }

                        }

                        if (statusListJSON is null || statusList?.Length == 0)
                            return new HTTPResponse.Builder(request) {
                                HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                Server          = HTTPServer.HTTPServerName,
                                Date            = Timestamp.Now,
                                ContentType     = HTTPContentType.Application.JSON_UTF8,
                                Content         = new JObject(
                                                    new JProperty("description", "Invalid status list!")
                                                ).ToUTF8Bytes()
                            };

                    }

                    #endregion

                    #region Fail, if both CurrentStatus and StatusList are missing...

                    if (!currentStatus.HasValue && statusList is null)
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = new JObject(
                                                         new JProperty("description", "Either a 'currentStatus' or a 'statusList' must be send!")
                                                     ).ToUTF8Bytes()
                               };

                    #endregion

                    #endregion


                    if (currentStatus.HasValue && statusList is null)
                        statusList = [ new Timestamped<EVSEAdminStatusType>(request.Timestamp, currentStatus.Value) ];

                    if (statusList?.Length > 0)
                        roamingNetwork.SetEVSEAdminStatus(
                            evse.Id,
                            statusList
                        );


                    return new HTTPResponse.Builder(request) {
                               HTTPStatusCode  = HTTPStatusCode.OK,
                               Server          = HTTPServer.HTTPServerName,
                               Date            = Timestamp.Now,
                               Connection      = ConnectionType.KeepAlive
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
            AddHandler(

                HTTPMethod.SET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/EVSEs/{EVSEId}/Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SetEVSEStatusRequest,
                HTTPResponseLogger:  SetEVSEStatusResponse,
                HTTPDelegate:        async request => {

                    #region Check RoamingNetworkId and EVSEId URI parameters

                    if (!request.TryParseRoamingNetworkAndEVSE(this,
                                                               out var roamingNetwork,
                                                               out var evse,
                                                               out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #region Parse Current status  [optional]

                    if (json.ParseOptional("currentStatus",
                                           "EVSE status",
                                           HTTPServer.HTTPServerName,
                                           out EVSEStatusType? currentStatus,
                                           request,
                                           out httpResponseBuilder))
                    {
                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse Status list     [optional]

                    Timestamped<EVSEStatusType>[]? statusList = null;

                    if (json.ParseOptional("statusList",
                                           "status list",
                                           HTTPServer.HTTPServerName,
                                           out JObject statusListJSON,
                                           request,
                                           out httpResponseBuilder))
                    {

                        if (httpResponseBuilder is not null)
                            return httpResponseBuilder;

                        if (statusListJSON is not null)
                        {

                            try
                            {

                                statusList = [.. statusListJSON.
                                                     Values<JProperty>().
                                                     Select(jproperty => new Timestamped<EVSEStatusType>(
                                                                             DateTime.Parse(jproperty.Name),
                                                                             Enum.Parse<EVSEStatusType>(jproperty.Value.ToString())
                                                                         )).
                                                     OrderBy(status   => status.Timestamp) ];

                            }
                            catch
                            {
                                // Will send the below BadRequest HTTP reply...
                            }

                        }

                        if (statusListJSON is null || statusList?.Length == 0)
                            return new HTTPResponse.Builder(request) {
                                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                    Server          = HTTPServer.HTTPServerName,
                                    Date            = Timestamp.Now,
                                    ContentType     = HTTPContentType.Application.JSON_UTF8,
                                    Content         = new JObject(
                                                        new JProperty("description", "Invalid status list!")
                                                    ).ToUTF8Bytes()
                                };

                    }

                    #endregion

                    #region Fail, if both CurrentStatus and StatusList are missing...

                    if (!currentStatus.HasValue && statusList is null)
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = new JObject(
                                                         new JProperty("description", "Either a 'currentStatus' or a 'statusList' must be send!")
                                                     ).ToUTF8Bytes()
                               };

                    #endregion

                    #endregion


                    if (currentStatus.HasValue && statusList is null)
                        statusList = [ new Timestamped<EVSEStatusType>(request.Timestamp, currentStatus.Value) ];

                    if (statusList?.Length > 0)
                        roamingNetwork.SetEVSEStatus(
                            evse.Id,
                            statusList
                        );


                    return new HTTPResponse.Builder(request) {
                               HTTPStatusCode  = HTTPStatusCode.OK,
                               Server          = HTTPServer.HTTPServerName,
                               Date            = Timestamp.Now,
                               Connection      = ConnectionType.KeepAlive
                           };

                });

            #endregion

            #endregion


            // Charging Sessions

            #region ~/RNs/{RoamingNetworkId}/ChargingSessions

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions

            // ---------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingSessions
            // ---------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    //ToDo: Filter sessions by HTTPUser organization!


                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount           = roamingNetwork.ChargingSessions.ULongCount();


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingSessions.
                                                                OrderBy(session => session.SessionTime.StartTime).
                                                                ToJSON (Embedded:    false,
                                                                        OnlineInfos: false,
                                                                        CustomChargingSessionSerializer,
                                                                        CustomCDRReceivedInfoSerializer,
                                                                        CustomChargeDetailRecordSerializer,
                                                                        CustomSendCDRResultSerializer,
                                                                        request.QueryString.GetUInt64("skip"),
                                                                        request.QueryString.GetUInt64("take")
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount,
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/ChargingSessions

            // ------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingSessions
            // ------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions",
                HTTPContentType.Text.PLAIN,
                HTTPDelegate: request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Text.PLAIN,
                            Content                       = roamingNetwork.SessionsStore.NumberOfStoredSessions.ToString().ToUTF8Bytes(),
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions->Id

            // -------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Prod/ChargingSessions->Id
            // -------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions->Id",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                roamingNetwork.ChargingSessions.
                                                                    Skip  (request.QueryString.GetUInt64("skip")).
                                                                    Take  (request.QueryString.GetUInt64("take")).
                                                                    Select(session => session.Id.ToString())
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = roamingNetwork.ChargingSessions.ULongCount(),
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions

            //// ---------------------------------------------------------------------------------------
            //// curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingSessions
            //// ---------------------------------------------------------------------------------------
            //AddHandler(
            //                  HTTPMethod.GET,
            //                  URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions",
            //                  HTTPContentType.Application.JSON_UTF8,
            //                  HTTPDelegate: Request => {

            //                      #region Get HTTP user and its organizations

            //                      // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
            //                      if (!TryGetHTTPUser(Request,
            //                                          out var httpUser,
            //                                          out var httpOrganizations,
            //                                          out var httpResponseBuilder,
            //                                          Recursive: true))
            //                      {
            //                          return Task.FromResult(httpResponseBuilder.AsImmutable);
            //                      }

            //                      #endregion

            //                      #region Get roaming network

            //                      if (!Request.ParseRoamingNetwork(this,
            //                                                       out var roamingNetwork,
            //                                                       out httpResponseBuilder))
            //                      {
            //                          return Task.FromResult(httpResponseBuilder.AsImmutable);
            //                      }

            //                      #endregion

            //                      roamingNetwork.SessionsStore.LoadLogfiles();



            //                      return Task.FromResult(
            //                          new HTTPResponse.Builder(Request) {
            //                              HTTPStatusCode                = HTTPStatusCode.OK,
            //                              Server                        = HTTPTestServer?.HTTPServerName,
            //                              Date                          = Timestamp.Now,
            //                              AccessControlAllowOrigin      = "*",
            //                              AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
            //                              AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
            //                              ETag                          = "1",
            //                              ContentType                   = HTTPContentType.Application.JSON_UTF8,
            //                              Content                       = roamingNetwork.ChargingSessions.
            //                                                                  OrderBy(session => session.Id).
            //                                                                  ToJSON (false,
            //                                                                          null,
            //                                                                          null,
            //                                                                          null,
            //                                                                          skip,
            //                                                                          take).
            //                                                                  ToUTF8Bytes(),
            //                              X_ExpectedTotalNumberOfItems  = expectedCount,
            //                              Connection                    = ConnectionType.KeepAlive
            //                          }.AsImmutable);

            //                  });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}

            // -----------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:5500/RNs/Test/ChargingSessions/{ChargingSessionId}
            // -----------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network and charging session

                    if (!request.TryParseRoamingNetworkAndChargingSession(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingSession,
                                                                          out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    //ToDo: Filter sessions by HTTPUser organization!


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = chargingSession.
                                                                ToJSON(Embedded:                         false,
                                                                       CustomChargingSessionSerializer:  CustomChargingSessionSerializer).
                                                                ToUTF8Bytes(),
                            Connection                    = ConnectionType.KeepAlive
                        }.AsImmutable);

                });

            #endregion

            #region SET         ~/RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}/{command}

            // -------------------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -X SET -H "Content-Type: application/json" --data @session.json http://127.0.0.1:3004/RNs/Prod/ChargingSessions/{ChargingSessionId}/{command}
            // -------------------------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.SET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}/{command}",
                HTTPContentType.Application.JSON_UTF8,
                async request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return httpResponseBuilder.AsImmutable;
                    }

                    #endregion

                    #region Get roaming network and charging session identification

                    if (!request.TryParseRoamingNetworkAndChargingSessionId(this,
                                                                            out var roamingNetwork,
                                                                            out var chargingSessionId,
                                                                            out httpResponseBuilder))
                    {
                        return httpResponseBuilder.AsImmutable;
                    }

                    #endregion

                    #region Parse command

                    if (!request.TryGetURLParameter("command", out var command))
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = @"{ ""description"": ""Invalid command!"" }".ToUTF8Bytes(),
                                   Connection      = ConnectionType.KeepAlive
                               };

                    #endregion

                    #region Parse Charging Session JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    if (!ChargingSession.TryParse(json, out var chargingSession, out var errorResponse))
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = new JObject(
                                                         new JProperty("description", errorResponse)
                                                     ).ToUTF8Bytes()
                               };

                    #endregion


                    var result = await roamingNetwork.AddExternalChargingSession(
                                           Timestamp.Now,
                                           this,
                                           command,
                                           chargingSession
                                       );


                    return result

                               ? new HTTPResponse.Builder(request) {
                                     HTTPStatusCode             = HTTPStatusCode.OK,
                                     Server                     = HTTPServer.HTTPServerName,
                                     Date                       = Timestamp.Now,
                                     AccessControlAllowOrigin   = "*",
                                     AccessControlAllowMethods  = [ "SET" ],
                                     AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                     ContentType                = HTTPContentType.Application.JSON_UTF8,
                                     Content                    = chargingSession.
                                                                     ToJSON(Embedded:                         false,
                                                                            CustomChargingSessionSerializer:  CustomChargingSessionSerializer).
                                                                     ToUTF8Bytes(),
                                     Connection                 = ConnectionType.KeepAlive
                                 }

                               : new HTTPResponse.Builder(request) {
                                     HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                     Server                     = HTTPServer.HTTPServerName,
                                     Date                       = Timestamp.Now,
                                     AccessControlAllowOrigin   = "*",
                                     AccessControlAllowMethods  = [ "SET" ],
                                     AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                     //ContentType                = HTTPContentType.Application.JSON_UTF8,
                                     //Content                    = chargingSession.
                                     //                                 ToJSON(Embedded:                         false,
                                     //                                        CustomChargingSessionSerializer:  CustomChargingSessionSerializer).
                                     //                                 ToUTF8Bytes(),
                                     ContentLength              = 0,
                                     Connection                 = ConnectionType.KeepAlive
                                 };

                });

            #endregion

            #region UPDATE      ~/RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}/{command}

            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            // curl -v -X UPDATE -H "Content-Type: application/json" --data @session.json http://127.0.0.1:3004/RNs/Prod/ChargingSessions/{ChargingSessionId}/{command}
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.UPDATE,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}/{command}",
                HTTPContentType.Application.JSON_UTF8,
                async request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return httpResponseBuilder.AsImmutable;
                    }

                    #endregion

                    #region Get roaming network and charging session identification

                    if (!request.TryParseRoamingNetworkAndChargingSessionId(this,
                                                                            out var roamingNetwork,
                                                                            out var chargingSessionId,
                                                                            out httpResponseBuilder))
                    {
                        return httpResponseBuilder.AsImmutable;
                    }

                    #endregion

                    #region Parse command

                    if (!request.TryGetURLParameter("command", out var command))
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = @"{ ""description"": ""Invalid command!"" }".ToUTF8Bytes(),
                                   Connection      = ConnectionType.KeepAlive
                               };

                    #endregion

                    #region Parse Charging Session JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json,
                                                               out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    if (!ChargingSession.TryParse(json, out var chargingSession, out var errorResponse))
                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = new JObject(
                                                         new JProperty("description", errorResponse)
                                                     ).ToUTF8Bytes()
                               };

                    #endregion


                    var result = await roamingNetwork.UpdateExternalChargingSession(
                                           Timestamp.Now,
                                           this,
                                           command,
                                           chargingSession
                                       );


                    return result

                               ? new HTTPResponse.Builder(request) {
                                     HTTPStatusCode             = HTTPStatusCode.OK,
                                     Server                     = HTTPServer.HTTPServerName,
                                     Date                       = Timestamp.Now,
                                     AccessControlAllowOrigin   = "*",
                                     AccessControlAllowMethods  = [ "SET" ],
                                     AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                     ContentType                = HTTPContentType.Application.JSON_UTF8,
                                     Content                    = chargingSession.
                                                                     ToJSON(Embedded:                         false,
                                                                            CustomChargingSessionSerializer:  CustomChargingSessionSerializer).
                                                                     ToUTF8Bytes(),
                                     Connection                 = ConnectionType.KeepAlive
                                 }

                               : new HTTPResponse.Builder(request) {
                                     HTTPStatusCode             = HTTPStatusCode.BadRequest,
                                     Server                     = HTTPServer.HTTPServerName,
                                     Date                       = Timestamp.Now,
                                     AccessControlAllowOrigin   = "*",
                                     AccessControlAllowMethods  = [ "SET" ],
                                     AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                     //ContentType                = HTTPContentType.Application.JSON_UTF8,
                                     //Content                    = chargingSession.
                                     //                                 ToJSON(Embedded:                         false,
                                     //                                        CustomChargingSessionSerializer:  CustomChargingSessionSerializer).
                                     //                                 ToUTF8Bytes(),
                                     ContentLength              = 0,
                                     Connection                 = ConnectionType.KeepAlive
                                 };

                });

            #endregion


            #region RESENDCDR   ~/RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}

            // ------------------------------------------------------------------------------------------
            // curl -v -X RESENDCDR http://127.0.0.1:5500/RNs/Test/ChargingSessions/{ChargingSessionId}
            // ------------------------------------------------------------------------------------------
            AddHandler(

                RESENDCDR,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/{ChargingSessionId}",
                HTTPContentType.Application.JSON_UTF8,
                async request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Get roaming network and charging session

                    if (!request.TryParseRoamingNetworkAndChargingSession(this,
                                                                          out var roamingNetwork,
                                                                          out var chargingSession,
                                                                          out httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    //ToDo: Filter sessions by HTTPUser organization!


                    var lastCDR = chargingSession.ReceivedCDRInfos.LastOrDefault()?.ChargeDetailRecord;

                    if (lastCDR is not null)
                    {

                        var sendCDRResult = await roamingNetwork.SendChargeDetailRecord(lastCDR);

                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.OK,
                                   ContentType     = HTTPContentType.Text.PLAIN,
                                   Content         = sendCDRResult.ToJSON().ToUTF8Bytes(),
                                   Connection      = ConnectionType.Close
                               }.AsImmutable;

                    }


                    return new HTTPResponse.Builder(request) {
                               HTTPStatusCode                = HTTPStatusCode.NotFound,
                               Server                        = HTTPServer.HTTPServerName,
                               Date                          = Timestamp.Now,
                               AccessControlAllowOrigin      = "*",
                               AccessControlAllowMethods     = [ "GET", "COUNT", "OPTIONS", "RESENDCDR" ],
                               AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                               Connection                    = ConnectionType.Close
                           };

                });

            #endregion


            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions/MissingCDRResponses

            // ----------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingSessions/MissingCDRResponses?ExpandCDRs=false
            // ----------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/MissingCDRResponses",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var skip                 = request.QueryString.GetUInt64("skip");
                    var take                 = request.QueryString.GetUInt64("take");
                    var from                 = request.QueryString.ParseFromTimestampFilter();
                    var to                   = request.QueryString.ParseToTimestampFilter();
                    var expandCDRs           = request.QueryString.GetBoolean("ExpandCDRs") ?? false;
                    var now                  = Timestamp.Now - TimeSpan.FromMinutes(10);

                    var missingCDRResponses  = roamingNetwork.ChargingSessions.
                                                   Where  (session => session.ReceivedCDRInfos.Any() &&
                                                                      session.ReceivedCDRInfos.All(receivedCDRInfo => receivedCDRInfo.Timestamp < now) &&
                                                                     !session.SendCDRResults.  Any() &&
                                                                    (!from.HasValue ||                                                                                   session.SessionTime.StartTime     >= from.Value) &&
                                                                    (!to.  HasValue || !session.SessionTime.EndTime.HasValue || (session.SessionTime.EndTime.HasValue && session.SessionTime.EndTime.Value <= to.  Value))).
                                                   ToArray();


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                expandCDRs
                                                                    ? missingCDRResponses.
                                                                          OrderBy(session => session.SessionTime.StartTime).
                                                                          ToJSON (Embedded:    false,
                                                                                  OnlineInfos: false,
                                                                                  CustomChargingSessionSerializer,
                                                                                  CustomCDRReceivedInfoSerializer,
                                                                                  CustomChargeDetailRecordSerializer,
                                                                                  CustomSendCDRResultSerializer,
                                                                                  skip,
                                                                                  take)
                                                                    : missingCDRResponses.
                                                                          OrderBy       (session => session.SessionTime.StartTime).
                                                                          SkipTakeFilter(skip, take).
                                                                          Select        (session => session.Id.ToString())
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = missingCDRResponses.ULongCount(),
                            Connection                    = ConnectionType.Close
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/ChargingSessions/FailedCDRResponses

            // ---------------------------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Prod/ChargingSessions/FailedCDRResponses?ExpandCDRs=false
            // ---------------------------------------------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/ChargingSessions/FailedCDRResponses",
                HTTPContentType.Application.JSON_UTF8,
                request => {

                    #region Get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var httpUser,
                                        out var httpOrganizations,
                                        out var httpResponseBuilder,
                                        Recursive: true))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion

                    #region Get roaming network

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out httpResponseBuilder))
                    {
                        return Task.FromResult(httpResponseBuilder.AsImmutable);
                    }

                    #endregion


                    var skip                 = request.QueryString.GetUInt64("skip");
                    var take                 = request.QueryString.GetUInt64("take");
                    var from                 = request.QueryString.ParseFromTimestampFilter();
                    var to                   = request.QueryString.ParseToTimestampFilter();
                    var expandCDRs           = request.QueryString.GetBoolean("ExpandCDRs") ?? false;

                    var session              = roamingNetwork.ChargingSessions.First(session => session.Id.ToString() == "fac089b1-1541-4370-869d-567d9c6bc436");

                    var missingCDRResponses  = roamingNetwork.ChargingSessions.
                                                   Where  (session => session.ReceivedCDRInfos.Any() &&
                                                                      session.SendCDRResults.  Any() &&
                                                                      session.SendCDRResults.  All(sendCDRResult => sendCDRResult.Result != SendCDRResultTypes.Success) &&
                                                                     (!from.HasValue ||                                                                                   session.SessionTime.StartTime     >= from.Value) &&
                                                                     (!to.  HasValue || !session.SessionTime.EndTime.HasValue || (session.SessionTime.EndTime.HasValue && session.SessionTime.EndTime.Value <= to.  Value))).
                                                   ToArray();

                    var xxx                  = missingCDRResponses.Select(session => session.Id.ToString()).Contains(session.Id.ToString());


                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = new JArray(
                                                                expandCDRs
                                                                    ? missingCDRResponses.
                                                                        OrderBy(session => session.SessionTime.StartTime).
                                                                        ToJSON (Embedded:    false,
                                                                                OnlineInfos: false,
                                                                                CustomChargingSessionSerializer,
                                                                                CustomCDRReceivedInfoSerializer,
                                                                                CustomChargeDetailRecordSerializer,
                                                                                CustomSendCDRResultSerializer,
                                                                                skip,
                                                                                take)
                                                                    : missingCDRResponses.
                                                                          OrderBy       (session => session.SessionTime.StartTime).
                                                                          SkipTakeFilter(skip, take).
                                                                          Select        (session => $"{session.Id}\t{(session.SessionTime.StartTime.ToISO8601())}\t{(session.SendCDRResults.Select(sendCDRResult => sendCDRResult.Result.ToString()).AggregateWith(", "))}")
                                                            ).ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = missingCDRResponses.ULongCount(),
                            Connection                    = ConnectionType.Close
                        }.AsImmutable);

                });

            #endregion

            #endregion


            // Charge Detail Records

            #region ~/RNs/{RoamingNetworkId}/ChargeDetailRecords

            #endregion

            #region ~/RNs/{RoamingNetworkId}/ChargeDetailRecords/{ChargeDetailRecordId}

            #endregion


            // Reservations

            #region ~/RNs/{RoamingNetworkId}/Reservations

            #region GET         ~/RNs/{RoamingNetworkId}/Reservations

            // ----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // ----------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/Reservations",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate:  async request => {

                    #region Check HTTP Basic Authentication

                    //if (HTTPRequest.Authorization          is null        ||
                    //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                    //    HTTPRequest.Authorization.Password != HTTPPassword)
                    //    return new HTTPResponse.Builder(HTTPRequest) {
                    //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                    //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                    //        Server           = _API.HTTPTestServer?.HTTPServerName,
                    //        Date             = Timestamp.Now,
                    //        Connection       = ConnectionType.KeepAlive
                    //    };

                    #endregion

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    var skip                     = request.QueryString.GetUInt64("skip");
                    var take                     = request.QueryString.GetUInt32("take");

                    var allChargingReservations  = roamingNetwork.
                                                    ChargingReservations.
                                                    OrderBy(reservation => reservation.Id.ToString()).
                                                    Skip   (skip).
                                                    Take   (take).
                                                    ToArray();

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount            = roamingNetwork.ChargingReservations.LongCount();

                    return new HTTPResponse.Builder(request) {
                        HTTPStatusCode             = allChargingReservations.Length > 0
                                                        ? HTTPStatusCode.OK
                                                        : HTTPStatusCode.NoContent,
                        Server                     = HTTPServer.HTTPServerName,
                        Date                       = Timestamp.Now,
                        AccessControlAllowOrigin   = "*",
                        AccessControlAllowMethods  = [ "GET" ],
                        AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                        ETag                       = "1",
                        ContentType                = HTTPContentType.Application.JSON_UTF8,
                        Content                    = (allChargingReservations.Length != 0
                                                        ? allChargingReservations.ToJSON()
                                                        : []
                                                    ).ToUTF8Bytes()
                    }.Set(new HTTPHeaderField("X-ExpectedTotalNumberOfItems", HeaderFieldType.Response, RequestPathSemantic.EndToEnd),
                                              expectedCount);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            #region GET         ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: async request => {

                    #region Check HTTP Basic Authentication

                    //if (HTTPRequest.Authorization          is null        ||
                    //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                    //    HTTPRequest.Authorization.Password != HTTPPassword)
                    //    return new HTTPResponse.Builder(HTTPRequest) {
                    //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                    //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                    //        Server           = _API.HTTPTestServer?.HTTPServerName,
                    //        Date             = Timestamp.Now,
                    //        Connection       = ConnectionType.KeepAlive
                    //    };

                    #endregion

                    #region Check ChargingReservationId parameter

                    if (!request.TryParseRoamingNetworkAndReservation(this,
                                                                      out var roamingNetwork,
                                                                      out var reservation,
                                                                      out var httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    return new HTTPResponse.Builder(request) {
                        HTTPStatusCode             = HTTPStatusCode.OK,
                        Server                     = HTTPServer.HTTPServerName,
                        Date                       = Timestamp.Now,
                        AccessControlAllowOrigin   = "*",
                        AccessControlAllowMethods  = [ "GET", "SETEXPIRED", "DELETE" ],
                        AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                        ETag                       = "1",
                        ContentType                = HTTPContentType.Application.JSON_UTF8,
                        Content                    = reservation.ToJSON().ToUTF8Bytes()
                    };

                });

            #endregion

            #region SETEXPIRED  ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            AddHandler(
                SETEXPIRED,
                URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: async request => {

                    #region Check HTTP Basic Authentication

                    //if (HTTPRequest.Authorization          is null        ||
                    //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                    //    HTTPRequest.Authorization.Password != HTTPPassword)
                    //    return new HTTPResponse.Builder(HTTPRequest) {
                    //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                    //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                    //        Server           = _API.HTTPTestServer?.HTTPServerName,
                    //        Date             = Timestamp.Now,
                    //        Connection       = ConnectionType.KeepAlive
                    //    };

                    #endregion

                    #region Check ChargingReservationId parameter

                    if (!request.TryParseRoamingNetworkAndReservation(this,
                                                                      out var roamingNetwork,
                                                                      out var reservation,
                                                                      out var httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    var response = await roamingNetwork.CancelReservation(
                                            reservation.Id,
                                            ChargingReservationCancellationReason.Deleted,
                                        //   null, //ToDo: Refactor me to make use of the ProviderId!
                                            null,

                                            request.Timestamp,
                                            request.EventTrackingId,
                                            TimeSpan.FromSeconds(60),
                                            request.CancellationToken
                                        ).ConfigureAwait(false);

                    switch (response.Result)
                    {

                        case CancelReservationResultTypes.Success:
                            return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "SETEXPIRED", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(new JProperty("en", "Reservation removed. Additional costs may be charged!")).ToUTF8Bytes()
                            };

                        default:
                            return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.InternalServerError,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(new JProperty("description", "Could not remove reservation!")).ToUTF8Bytes()
                            };

                    }

                });

            #endregion

            #region DELETE      ~/RNs/{RoamingNetworkId}/Reservations/{ReservationId}

            // -----------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/Reservations
            // -----------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.DELETE,
                URLPathPrefix + "RNs/{RoamingNetworkId}/Reservations/{ReservationId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: async request => {

                    #region Check HTTP Basic Authentication

                    //if (HTTPRequest.Authorization          is null        ||
                    //    HTTPRequest.Authorization.Username != HTTPLogin   ||
                    //    HTTPRequest.Authorization.Password != HTTPPassword)
                    //    return new HTTPResponse.Builder(HTTPRequest) {
                    //        HTTPStatusCode   = HTTPStatusCode.Unauthorized,
                    //        WWWAuthenticate  = @"Basic realm=""WWCP EV Charging""",
                    //        Server           = _API.HTTPTestServer?.HTTPServerName,
                    //        Date             = Timestamp.Now,
                    //        Connection       = ConnectionType.KeepAlive
                    //    };

                    #endregion

                    #region Check ChargingReservationId parameter

                    if (!request.TryParseRoamingNetworkAndReservation(this,
                                                                      out var roamingNetwork,
                                                                      out var reservation,
                                                                      out var httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    var response = await roamingNetwork.CancelReservation(
                                             reservation.Id,
                                             ChargingReservationCancellationReason.Deleted,
                                         //    null, //ToDo: Refactor me to make use of the ProviderId!
                                             null,

                                             request.Timestamp,
                                             request.EventTrackingId,
                                             TimeSpan.FromSeconds(60),
                                             request.CancellationToken
                                         ).ConfigureAwait(false);

                    switch (response.Result)
                    {

                        case CancelReservationResultTypes.Success:
                            return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "SETEXPIRED", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(new JProperty("en", "Reservation removed. Additional costs may be charged!")).ToUTF8Bytes()
                            };

                        default:
                            return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.InternalServerError,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "DELETE" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = JSONObject.Create(new JProperty("description", "Could not remove reservation!")).ToUTF8Bytes()
                            };

                    }

                });

            #endregion

            #endregion



            // E-Mobility Providers

            #region ~/RNs/{RoamingNetworkId}/eMobilityProviders

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders

            // -----------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders
            // -----------------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip                    = request.QueryString.GetUInt64("skip");
                    var take                    = request.QueryString.GetUInt64("take");
                    var expand                  = request.QueryString.GetStrings("expand");
                    //var expandChargingPools     = !expand.ContainsIgnoreCase("-chargingpools");
                    //var expandChargingStations  = !expand.ContainsIgnoreCase("-chargingstations");
                    //var expandBrands            = expand.ContainsIgnoreCase("brands");

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount           = roamingNetwork.EMobilityProviders.ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = roamingNetwork.EMobilityProviders.
                                                               ToJSON(skip,
                                                                      take,
                                                                      false).
                                                                       //expandChargingPools,
                                                                       //expandChargingStations,
                                                                       //expandBrands).
                                                               ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #region COUNT       ~/RNs/{RoamingNetworkId}/eMobilityProviders

            // ----------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RoamingNetworkId}/eMobilityProviders
            // ----------------------------------------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.COUNT,
                URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode               = HTTPStatusCode.OK,
                            Server                       = HTTPServer.HTTPServerName,
                            Date                         = Timestamp.Now,
                            AccessControlAllowOrigin     = "*",
                            AccessControlAllowMethods    = [ "GET", "COUNT", "STATUS" ],
                            AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                         = "1",
                            ContentType                  = HTTPContentType.Application.JSON_UTF8,
                            Content                      = JSONObject.Create(
                                                               new JProperty("count",  roamingNetwork.ChargingStationOperators.ULongCount())
                                                           ).ToUTF8Bytes()
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders->AdminStatus

            // ------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders->AdminStatus
            // ------------------------------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders->AdminStatus",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");
                    var historysize    = request.QueryString.GetUInt64("historysize", 1);

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.ChargingStationOperatorAdminStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingStationOperatorAdminStatus().
                                                                OrderBy(adminStatus => adminStatus.Id).
                                                                ToJSON (skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders->Status

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/eMobilityProviders->Status
            // -------------------------------------------------------------------------------------------------
            AddHandler(
                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders->Status",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    var skip           = request.QueryString.GetUInt64("skip");
                    var take           = request.QueryString.GetUInt64("take");

                    //ToDo: Getting the expected total count might be very expensive!
                    var expectedCount  = roamingNetwork.ChargingStationOperatorStatus().ULongCount();

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode                = HTTPStatusCode.OK,
                            Server                        = HTTPServer.HTTPServerName,
                            Date                          = Timestamp.Now,
                            AccessControlAllowOrigin      = "*",
                            AccessControlAllowMethods     = [ "GET" ],
                            AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                          = "1",
                            ContentType                   = HTTPContentType.Application.JSON_UTF8,
                            Content                       = roamingNetwork.ChargingStationOperatorStatus().
                                                                OrderBy(status => status.Id).
                                                                ToJSON (skip, take).
                                                                ToUTF8Bytes(),
                            X_ExpectedTotalNumberOfItems  = expectedCount
                        }.AsImmutable);

                });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}

            #region GET         ~/RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}

            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "RNs/{RoamingNetworkId}/eMobilityProviders/{eMobilityProviderId}",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: request => {

                    #region Check HTTP parameters

                    if (!request.TryParseRoamingNetworkAndEMobilityProvider(this,
                                                                            out var roamingNetwork,
                                                                            out var eMobilityProvider,
                                                                            out var httpResponse))
                    {
                        return Task.FromResult(httpResponse.AsImmutable);
                    }

                    #endregion

                    return Task.FromResult(
                        new HTTPResponse.Builder(request) {
                            HTTPStatusCode             = HTTPStatusCode.OK,
                            Server                     = HTTPServer.HTTPServerName,
                            Date                       = Timestamp.Now,
                            AccessControlAllowOrigin   = "*",
                            AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
                            AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            ETag                       = "1",
                            ContentType                = HTTPContentType.Application.JSON_UTF8,
                            Content                    = eMobilityProvider.ToJSON()?.ToUTF8Bytes()
                        }.AsImmutable);

                });

            #endregion

            #endregion


            #region ~/RNs/{RoamingNetworkId}/AuthStartCache

            //ToDo: OPTIONS

            #region GET    ~/RNs/{RoamingNetworkId}/AuthStartCache

            // -------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/AuthStartCache
            // -------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.GET,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/AuthStartCache",
                HTTPContentType.Application.JSON_UTF8,
                HTTPDelegate: async request => {

                    #region Try to get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var       httpUser,
                                        out var       httpOrganizations,
                                        out var       httpResponse,
                                        AccessLevel:  Access_Levels.Admin,
                                        Recursive:    true))
                    {
                        return httpResponse!;
                    }

                    #endregion

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out     httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    var withMetadata              = request.QueryString.GetBoolean("withMetadata", false);

                    var includeFilter             = request.QueryString.CreateStringFilter<AuthenticationToken>("match", (authenticationToken, include) => authenticationToken.ToString().Contains(include));

                    var allAuthStartResults       = roamingNetwork.CachedAuthStartResults.
                                                        ToArray();

                    var totalCount                = allAuthStartResults.ULongLength();

                    var filteredAuthStartResults  = allAuthStartResults.
                                                        Where            (authStartResultKeyValuePair => includeFilter(authStartResultKeyValuePair.Key)).
                                                        OrderByDescending(authStartResultKeyValuePair => authStartResultKeyValuePair.Value.CachedResultEndOfLifeTime).
                                                        Skip             (request.QueryString.GetUInt64("skip")).
                                                        Take             (request.QueryString.GetUInt32("take")).
                                                        Select           (authStartResultKeyValuePair => {
                                                                            var authStartResultInfo = authStartResultKeyValuePair.Value.ToJSON(Embedded: true);
                                                                            authStartResultInfo.Add("authenticationToken", authStartResultKeyValuePair.Key.ToString());
                                                                            return authStartResultInfo;
                                                                        }).
                                                        ToArray          ();

                    var filteredCount             = filteredAuthStartResults.ULongLength();

                    var jsonResults               = new JArray(filteredAuthStartResults);


                    return new HTTPResponse.Builder(request) {
                                HTTPStatusCode                = HTTPStatusCode.OK,
                                Server                        = HTTPServer.HTTPServerName,
                                Date                          = Timestamp.Now,
                                AccessControlAllowOrigin      = "*",
                                AccessControlAllowMethods     = [ "GET", "COUNT", "CLEAR" ],
                                AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                   = HTTPContentType.Application.JSON_UTF8,
                                Content                       = withMetadata
                                                                    ? JSONObject.Create(
                                                                        new JProperty("totalCount",        totalCount),
                                                                        new JProperty("filteredCount",     filteredCount),
                                                                        new JProperty("authStartResults",  jsonResults)
                                                                    ).ToUTF8Bytes()
                                                                    : jsonResults.ToUTF8Bytes(),
                                X_ExpectedTotalNumberOfItems  = filteredCount,
                                Connection                    = ConnectionType.KeepAlive,
                                Vary                          = "Accept"
                            };

                });

            #endregion

            #region COUNT  ~/RNs/{RoamingNetworkId}/AuthStartCache

            // -------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/AuthStartCache
            // -------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.COUNT,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/AuthStartCache",
                HTTPContentType.Text.PLAIN,
                HTTPDelegate:  async request => {

                    #region Try to get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var       httpUser,
                                        out var       httpOrganizations,
                                        out var       httpResponse,
                                        AccessLevel:  Access_Levels.Admin,
                                        Recursive:    true))
                    {
                        return httpResponse;
                    }

                    #endregion

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out     httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "COUNT", "CLEAR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Text.PLAIN,
                                Content                    = roamingNetwork.CachedAuthStartResults.Count().ToString().ToUTF8Bytes()
                            };

                });

            #endregion

            #region CLEAR  ~/RNs/{RoamingNetworkId}/AuthStartCache

            // -------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/AuthStartCache
            // -------------------------------------------------------------------------------------
            AddHandler(

                HTTPMethod.CLEAR,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/AuthStartCache",
                HTTPDelegate: async request => {

                    #region Try to get HTTP user and its organizations

                    // Will return HTTP 401 Unauthorized, when the HTTP user is unknown!
                    if (!TryGetHTTPUser(request,
                                        out var       httpUser,
                                        out var       httpOrganizations,
                                        out var       httpResponse,
                                        AccessLevel:  Access_Levels.Admin,
                                        Recursive:    true))
                    {
                        return httpResponse;
                    }

                    #endregion

                    #region Check parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out     httpResponse))
                    {
                        return httpResponse;
                    }

                    #endregion


                    await roamingNetwork.ClearAuthStartResultCache();


                    return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "COUNT", "CLEAR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                            };

                });

            #endregion

            #endregion

            #region POST        ~/RNs/{RoamingNetworkId}/token

            // ==== PROD =============================================
            // curl -v -X AUTH -H "Content-Type: application/json" \
            //                 -H "Accept:       application/json" \
            //      -d "{ \"token\": \"B6D15211\"}" \
            //      http://127.0.0.1:3004/RNs/Prod/token
            AddHandler(

                HTTPMethod.AUTH,
                URLPathPrefix + "/RNs/{RoamingNetworkId}/token",
                HTTPContentType.Application.JSON_UTF8,
                HTTPRequestLogger:   SendTokenAuthRequest,
                HTTPResponseLogger:  SendTokenAuthResponse,
                HTTPDelegate:        async request => {

                    #region Parse RoamingNetwork URL parameters

                    if (!request.TryParseRoamingNetwork(this,
                                                        out var roamingNetwork,
                                                        out var httpResponseBuilder))
                    {
                        return httpResponseBuilder;
                    }

                    #endregion

                    #region Parse JSON

                    if (!request.TryParseJSONObjectRequestBody(out var json, out httpResponseBuilder) ||
                         json is null)
                    {
                        return httpResponseBuilder!;
                    }

                    #region Parse Token            [mandatory]

                    if (!json.ParseMandatory("token",
                                             "local authentication token",
                                             AuthenticationToken.TryParse,
                                             out AuthenticationToken token,
                                             out var errorResponse))
                    {

                        return new HTTPResponse.Builder(request) {
                                   HTTPStatusCode  = HTTPStatusCode.BadRequest,
                                   Server          = HTTPServer.HTTPServerName,
                                   Date            = Timestamp.Now,
                                   ContentType     = HTTPContentType.Application.JSON_UTF8,
                                   Content         = JSONObject.Create(
                                                         new JProperty("token",        json["token"]?.Value<String>()),
                                                         new JProperty("description",  errorResponse),
                                                         new JProperty("runtime",      0)
                                                     ).ToUTF8Bytes(),
                                   Connection      = ConnectionType.KeepAlive
                               };

                    }

                    #endregion

                    #endregion


                    var result = await roamingNetwork.AuthorizeStart(
                                           LocalAuthentication.FromAuthToken(token),

                                           Timestamp:          Timestamp.Now,
                                           EventTrackingId:    request.EventTrackingId,
                                           RequestTimeout:     null,
                                           CancellationToken:  request.CancellationToken
                                       );


                    #region Authorized

                    if (result.Result == AuthStartResultTypes.Authorized)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.OK,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                    #region NotAuthorized

                    else if (result.Result == AuthStartResultTypes.Error)
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Unauthorized,
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                    #region Forbidden

                    else
                        return new HTTPResponse.Builder(request) {
                                HTTPStatusCode             = HTTPStatusCode.Forbidden, //ToDo: Is this smart?
                                Server                     = HTTPServer.HTTPServerName,
                                Date                       = Timestamp.Now,
                                AccessControlAllowOrigin   = "*",
                                AccessControlAllowMethods  = [ "GET", "RESERVE", "AUTHSTART", "AUTHSTOP", "REMOTESTART", "REMOTESTOP", "SENDCDR" ],
                                AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
                                ContentType                = HTTPContentType.Application.JSON_UTF8,
                                Content                    = result.ToJSON().ToUTF8Bytes()
                            };

                    #endregion

                },
                AllowReplacement: URLReplacement.Allow
            );

            #endregion


        }

        #endregion

        #region (protected) GetOpenChargingCloudAPIRessource(Ressource)

        ///// <summary>
        ///// Get an embedded ressource of the Open Charging Cloud API.
        ///// </summary>
        ///// <param name="Ressource">The path and name of the ressource to load.</param>
        //protected Stream GetOpenChargingCloudAPIRessource(String Ressource)

        //    => GetType().Assembly.GetManifestResourceStream(HTTPRoot + Ressource);

        #endregion


        private readonly IWWCPCore roamingNetworks = new WWCPCore();

        #region CreateNewRoamingNetwork(          Id, Name, Description = null, Configurator = null, ...)

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
        public IRoamingNetwork CreateNewRoamingNetwork(RoamingNetwork_Id                          Id,
                                                       I18NString                                 Name,
                                                       I18NString?                                Description                                  = null,
                                                       Action<RoamingNetwork>?                    Configurator                                 = null,
                                                       RoamingNetworkAdminStatusTypes?            AdminStatus                                  = null,
                                                       RoamingNetworkStatusTypes?                 Status                                       = null,
                                                       UInt16?                                    MaxAdminStatusListSize                       = null,
                                                       UInt16?                                    MaxStatusListSize                            = null,

                                                       Boolean?                                   DisableAuthenticationCache                   = false,
                                                       TimeSpan?                                  AuthenticationCacheTimeout                   = null,
                                                       UInt32?                                    MaxAuthStartResultCacheElements              = null,
                                                       UInt32?                                    MaxAuthStopResultCacheElements               = null,

                                                       Boolean?                                   DisableAuthenticationRateLimit               = true,
                                                       TimeSpan?                                  AuthenticationRateLimitTimeSpan              = null,
                                                       UInt16?                                    AuthenticationRateLimitPerChargingLocation   = null,

                                                       ChargingStationSignatureDelegate?          ChargingStationSignatureGenerator            = null,
                                                       ChargingPoolSignatureDelegate?             ChargingPoolSignatureGenerator               = null,
                                                       ChargingStationOperatorSignatureDelegate?  ChargingStationOperatorSignatureGenerator    = null,

                                                       IEnumerable<RoamingNetworkInfo>?           RoamingNetworkInfos                          = null,
                                                       Boolean                                    DisableNetworkSync                           = false)


            => CreateNewRoamingNetwork(
                   HTTPHostname.Any,
                   Id,
                   Name,
                   Description,
                   Configurator,
                   AdminStatus,
                   Status,
                   MaxAdminStatusListSize,
                   MaxStatusListSize,

                   DisableAuthenticationCache,
                   AuthenticationCacheTimeout,
                   MaxAuthStartResultCacheElements,
                   MaxAuthStopResultCacheElements,

                   DisableAuthenticationRateLimit,
                   AuthenticationRateLimitTimeSpan,
                   AuthenticationRateLimitPerChargingLocation,

                   ChargingStationSignatureGenerator,
                   ChargingPoolSignatureGenerator,
                   ChargingStationOperatorSignatureGenerator,

                   RoamingNetworkInfos,
                   DisableNetworkSync
               );

        #endregion

        #region CreateNewRoamingNetwork( Id, Name, Description = null, Configurator = null, ...)

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
        public IRoamingNetwork CreateNewRoamingNetwork(HTTPHostname                               Hostname,
                                                       RoamingNetwork_Id                          Id,
                                                       I18NString                                 Name,
                                                       I18NString?                                Description                                  = null,
                                                       Action<RoamingNetwork>?                    Configurator                                 = null,
                                                       RoamingNetworkAdminStatusTypes?            AdminStatus                                  = null,
                                                       RoamingNetworkStatusTypes?                 Status                                       = null,
                                                       UInt16?                                    MaxAdminStatusListSize                       = null,
                                                       UInt16?                                    MaxStatusListSize                            = null,

                                                       Boolean?                                   DisableAuthenticationCache                   = false,
                                                       TimeSpan?                                  AuthenticationCacheTimeout                   = null,
                                                       UInt32?                                    MaxAuthStartResultCacheElements              = null,
                                                       UInt32?                                    MaxAuthStopResultCacheElements               = null,

                                                       Boolean?                                   DisableAuthenticationRateLimit               = true,
                                                       TimeSpan?                                  AuthenticationRateLimitTimeSpan              = null,
                                                       UInt16?                                    AuthenticationRateLimitPerChargingLocation   = null,

                                                       ChargingStationSignatureDelegate?          ChargingStationSignatureGenerator            = null,
                                                       ChargingPoolSignatureDelegate?             ChargingPoolSignatureGenerator               = null,
                                                       ChargingStationOperatorSignatureDelegate?  ChargingStationOperatorSignatureGenerator    = null,

                                                       IEnumerable<RoamingNetworkInfo>?           RoamingNetworkInfos                          = null,
                                                       Boolean                                    DisableNetworkSync                           = false)

        {

            #region Initial checks

            if (Hostname.IsNullOrEmpty)
                throw new ArgumentNullException(nameof(Hostname), "The given HTTP hostname must not be null!");

            #endregion

            //var ExistingRoamingNetwork = WWCPHTTPServer.
            //                                 GetAllTenants(Hostname).
            //                                 FirstOrDefault(roamingnetwork => roamingnetwork.Id == Id);

            //if (ExistingRoamingNetwork is null)
            if (!roamingNetworks.TryGetRoamingNetwork(Id, out var ExistingRoamingNetwork))
            {

                //if (!WWCPHTTPServer.TryGetTenants(out RoamingNetworks _RoamingNetworks))
                //{

                //    _RoamingNetworks = new RoamingNetworks();

                //    if (!WWCPHTTPServer.TryAddTenants( _RoamingNetworks))
                //        throw new Exception("Could not add new roaming networks object to the HTTP host!");

                //}

                var newRoamingNetwork = roamingNetworks.CreateNewRoamingNetwork(
                                            Id,
                                            Name,
                                            Description,
                                            Configurator,
                                            AdminStatus,
                                            Status,
                                            MaxAdminStatusListSize,
                                            MaxStatusListSize,

                                            DisableAuthenticationCache,
                                            AuthenticationCacheTimeout,
                                            MaxAuthStartResultCacheElements,
                                            MaxAuthStopResultCacheElements,

                                            DisableAuthenticationRateLimit,
                                            AuthenticationRateLimitTimeSpan,
                                            AuthenticationRateLimitPerChargingLocation,

                                            ChargingStationSignatureGenerator,
                                            ChargingPoolSignatureGenerator,
                                            ChargingStationOperatorSignatureGenerator,

                                            RoamingNetworkInfos,
                                            DisableNetworkSync,
                                            OpenChargingCloudAPIPath
                                        );

                #region Link log events to HTTP-SSE...

                //#region OnAuthorizeStartRequest/-Response

                //newRoamingNetwork.OnAuthorizeStartRequest += async (LogTimestamp,
                //                                                    RequestTimestamp,
                //                                                    Sender,
                //                                                    SenderId,
                //                                                    EventTrackingId,
                //                                                    RoamingNetworkId,
                //                                                    EMPRoamingProviderId,
                //                                                    CSORoamingProviderId,
                //                                                    OperatorId,
                //                                                    Authentication,
                //                                                    ChargingLocation,
                //                                                    ChargingProduct,
                //                                                    SessionId,
                //                                                    CPOPartnerSessionId,
                //                                                    ISendAuthorizeStartStop,
                //                                                    RequestTimeout)

                //    => await DebugLog.SubmitEvent("AUTHSTARTRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   RequestTimestamp.    ToISO8601()),
                //                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      OperatorId.    HasValue
                //                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                //                                          : null,
                //                                      Authentication is not null
                //                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                //                                          : null,
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      ChargingProduct is not null
                //                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                //                                          : null,
                //                                      SessionId.     HasValue
                //                                          ? new JProperty("sessionId",             SessionId.           ToString())
                //                                          : null,
                //                                      CPOPartnerSessionId.HasValue
                //                                          ? new JProperty("CPOPartnerSessionId",   CPOPartnerSessionId. ToString())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null
                //                               ));


                //newRoamingNetwork.OnAuthorizeStartResponse += async (LogTimestamp,
                //                                                     RequestTimestamp,
                //                                                     Sender,
                //                                                     SenderId,
                //                                                     EventTrackingId,
                //                                                     RoamingNetworkId2,
                //                                                     EMPRoamingProviderId,
                //                                                     CSORoamingProviderId,
                //                                                     OperatorId,
                //                                                     Authentication,
                //                                                     ChargingLocation,
                //                                                     ChargingProduct,
                //                                                     SessionId,
                //                                                     CPOPartnerSessionId,
                //                                                     ISendAuthorizeStartStop,
                //                                                     RequestTimeout,
                //                                                     Result,
                //                                                     Runtime)

                //    => await DebugLog.SubmitEvent("AUTHSTARTResponse",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   RequestTimestamp.    ToISO8601()),
                //                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      OperatorId.HasValue
                //                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                //                                          : null,
                //                                      new JProperty("authentication",              Authentication.      ToJSON()),
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      ChargingProduct is not null
                //                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                //                                          : null,
                //                                      SessionId.HasValue
                //                                          ? new JProperty("sessionId",             SessionId.           ToString())
                //                                          : null,
                //                                      CPOPartnerSessionId.HasValue
                //                                          ? new JProperty("CPOPartnerSessionId",   CPOPartnerSessionId. ToString())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null,

                //                                      new JProperty("result",                      Result.              ToJSON()),
                //                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))

                //                                  ));

                //#endregion

                //#region OnAuthorizeStopRequest/-Response

                //newRoamingNetwork.OnAuthorizeStopRequest += async (LogTimestamp,
                //                                                   RequestTimestamp,
                //                                                   Sender,
                //                                                   SenderId,
                //                                                   EventTrackingId,
                //                                                   RoamingNetworkId2,
                //                                                   EMPRoamingProviderId,
                //                                                   CSORoamingProviderId,
                //                                                   OperatorId,
                //                                                   ChargingLocation,
                //                                                   SessionId,
                //                                                   CPOPartnerSessionId,
                //                                                   Authentication,
                //                                                   RequestTimeout)

                //    => await DebugLog.SubmitEvent("AUTHSTOPRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   RequestTimestamp.    ToISO8601()),
                //                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      OperatorId is not null
                //                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                //                                          : null,
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      new JProperty("sessionId",                   SessionId.           ToString()),
                //                                      CPOPartnerSessionId.HasValue
                //                                          ? new JProperty("CPOPartnerSessionId",   CPOPartnerSessionId. ToString())
                //                                          : null,
                //                                      new JProperty("authentication",              Authentication.      ToString()),
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null
                //                                  ));

                //newRoamingNetwork.OnAuthorizeStopResponse += async (LogTimestamp,
                //                                                    RequestTimestamp,
                //                                                    Sender,
                //                                                    SenderId,
                //                                                    EventTrackingId,
                //                                                    RoamingNetworkId2,
                //                                                    EMPRoamingProviderId,
                //                                                    CSORoamingProviderId,
                //                                                    OperatorId,
                //                                                    ChargingLocation,
                //                                                    SessionId,
                //                                                    CPOPartnerSessionId,
                //                                                    Authentication,
                //                                                    RequestTimeout,
                //                                                    Result,
                //                                                    Runtime)

                //    => await DebugLog.SubmitEvent("AUTHSTOPResponse",
                //                                  JSONObject.Create(

                //                                      new JProperty("timestamp",                   RequestTimestamp.    ToISO8601()),
                //                                      new JProperty("eventTrackingId",             EventTrackingId.     ToString()),
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId2.   ToString()),
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      OperatorId.HasValue
                //                                          ? new JProperty("operatorId",            OperatorId.          ToString())
                //                                          : null,
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      SessionId.HasValue
                //                                          ? new JProperty("sessionId",             SessionId.           ToString())
                //                                          : null,
                //                                      CPOPartnerSessionId.HasValue
                //                                          ? new JProperty("CPOPartnerSessionId",   CPOPartnerSessionId. ToString())
                //                                          : null,
                //                                      new JProperty("authentication",              Authentication.      ToString()),
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null,

                //                                      new JProperty("result",                      Result.              ToJSON()),
                //                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))

                //                              ));

                //#endregion


                //#region OnReserveEVSERequest/-Response

                //newRoamingNetwork.OnReserveRequest += async (LogTimestamp,
                //                                             Timestamp,
                //                                             Sender,
                //                                             EventTrackingId,
                //                                             RoamingNetworkId2,
                //                                             ReservationId,
                //                                             LinkedReservationId,
                //                                             ChargingLocation,
                //                                             StartTime,
                //                                             Duration,
                //                                             ProviderId,
                //                                             eMAId,
                //                                             ChargingProduct,
                //                                             AuthTokens,
                //                                             eMAIds,
                //                                             PINs,
                //                                             RequestTimeout)

                //    => await DebugLog.SubmitEvent("OnReserveRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("Timestamp",                 Timestamp.ToISO8601()),
                //                                      EventTrackingId is not null
                //                                         ? new JProperty("EventTrackingId",      EventTrackingId.ToString())
                //                                         : null,
                //                                      new JProperty("RoamingNetwork",            Id.ToString()),
                //                                      ReservationId.HasValue
                //                                         ? new JProperty("ReservationId",        ReservationId.ToString())
                //                                         : null,
                //                                      LinkedReservationId.HasValue
                //                                         ? new JProperty("LinkedReservationId",  LinkedReservationId.ToString())
                //                                         : null,
                //                                      ChargingLocation is not null
                //                                          ? new JProperty("ChargingLocation",    ChargingLocation.ToString())
                //                                          : null,
                //                                      StartTime.HasValue
                //                                          ? new JProperty("StartTime",           StartTime.Value.ToISO8601())
                //                                          : null,
                //                                      Duration.HasValue
                //                                          ? new JProperty("Duration",            Duration.Value.TotalSeconds.ToString())
                //                                          : null,
                //                                      ProviderId.HasValue
                //                                          ? new JProperty("ProviderId",          ProviderId.ToString())
                //                                          : null,
                //                                      eMAId is not null
                //                                          ? new JProperty("eMAId",               eMAId.ToString())
                //                                          : null,
                //                                      ChargingProduct is not null
                //                                          ? new JProperty("ChargingProduct",     JSONObject.Create(
                //                                                new JProperty("Id",                              ChargingProduct.Id.ToString()),
                //                                                ChargingProduct.MinDuration.HasValue
                //                                                    ? new JProperty("MinDuration",               ChargingProduct.MinDuration.Value.TotalSeconds)
                //                                                    : null,
                //                                                ChargingProduct.StopChargingAfterTime.HasValue
                //                                                    ? new JProperty("StopChargingAfterTime",     ChargingProduct.StopChargingAfterTime.Value.TotalSeconds)
                //                                                    : null,
                //                                                ChargingProduct.MinPower.HasValue
                //                                                    ? new JProperty("MinPower",                  ChargingProduct.MinPower.Value)
                //                                                    : null,
                //                                                ChargingProduct.MaxPower.HasValue
                //                                                    ? new JProperty("MaxPower",                  ChargingProduct.MaxPower.Value)
                //                                                    : null,
                //                                                ChargingProduct.MinEnergy.HasValue
                //                                                    ? new JProperty("MinEnergy",                 ChargingProduct.MinEnergy.Value)
                //                                                    : null,
                //                                                ChargingProduct.StopChargingAfterKWh.HasValue
                //                                                    ? new JProperty("StopChargingAfterKWh",      ChargingProduct.StopChargingAfterKWh.Value)
                //                                                    : null
                //                                               ))
                //                                          : null,
                //                                      AuthTokens is not null
                //                                          ? new JProperty("AuthTokens",          new JArray(AuthTokens.Select(_ => _.ToString())))
                //                                          : null,
                //                                      eMAIds is not null
                //                                          ? new JProperty("eMAIds",              new JArray(eMAIds.Select(_ => _.ToString())))
                //                                          : null,
                //                                      PINs is not null
                //                                          ? new JProperty("PINs",                new JArray(PINs.Select(_ => _.ToString())))
                //                                          : null
                //                                  ));

                //newRoamingNetwork.OnReserveResponse += async (LogTimestamp,
                //                                              Timestamp,
                //                                              Sender,
                //                                              EventTrackingId,
                //                                              RoamingNetworkId2,
                //                                              ReservationId,
                //                                              LinkedReservationId,
                //                                              ChargingLocation,
                //                                              StartTime,
                //                                              Duration,
                //                                              ProviderId,
                //                                              eMAId,
                //                                              ChargingProduct,
                //                                              AuthTokens,
                //                                              eMAIds,
                //                                              PINs,
                //                                              Result,
                //                                              Runtime,
                //                                              RequestTimeout)

                //    => await DebugLog.SubmitEvent("OnReserveResponse",
                //                                  JSONObject.Create(
                //                                      new JProperty("Timestamp",                 Timestamp.ToISO8601()),
                //                                        EventTrackingId is not null
                //                                           ? new JProperty("EventTrackingId",      EventTrackingId.ToString())
                //                                           : null,
                //                                        new JProperty("RoamingNetwork",            Id.ToString()),
                //                                        ReservationId.HasValue
                //                                           ? new JProperty("ReservationId",        ReservationId.ToString())
                //                                           : null,
                //                                        LinkedReservationId.HasValue
                //                                           ? new JProperty("LinkedReservationId",  LinkedReservationId.ToString())
                //                                           : null,
                //                                        ChargingLocation is not null
                //                                            ? new JProperty("ChargingLocation",    ChargingLocation.ToString())
                //                                            : null,
                //                                        StartTime.HasValue
                //                                            ? new JProperty("StartTime",           StartTime.Value.ToISO8601())
                //                                            : null,
                //                                        Duration.HasValue
                //                                            ? new JProperty("Duration",            Duration.Value.TotalSeconds.ToString())
                //                                            : null,
                //                                        ProviderId is not null
                //                                            ? new JProperty("ProviderId",          ProviderId.ToString()+"X")
                //                                            : null,
                //                                        eMAId is not null
                //                                            ? new JProperty("eMAId",               eMAId.ToString())
                //                                            : null,
                //                                        ChargingProduct is not null
                //                                          ? new JProperty("ChargingProduct",     JSONObject.Create(
                //                                                new JProperty("Id",                              ChargingProduct.Id.ToString()),
                //                                                ChargingProduct.MinDuration.HasValue
                //                                                    ? new JProperty("MinDuration",               ChargingProduct.MinDuration.Value.TotalSeconds)
                //                                                    : null,
                //                                                ChargingProduct.StopChargingAfterTime.HasValue
                //                                                    ? new JProperty("StopChargingAfterTime",     ChargingProduct.StopChargingAfterTime.Value.TotalSeconds)
                //                                                    : null,
                //                                                ChargingProduct.MinPower.HasValue
                //                                                    ? new JProperty("MinPower",                  ChargingProduct.MinPower.Value)
                //                                                    : null,
                //                                                ChargingProduct.MaxPower.HasValue
                //                                                    ? new JProperty("MaxPower",                  ChargingProduct.MaxPower.Value)
                //                                                    : null,
                //                                                ChargingProduct.MinEnergy.HasValue
                //                                                    ? new JProperty("MinEnergy",                 ChargingProduct.MinEnergy.Value)
                //                                                    : null,
                //                                                ChargingProduct.StopChargingAfterKWh.HasValue
                //                                                    ? new JProperty("StopChargingAfterKWh",      ChargingProduct.StopChargingAfterKWh.Value)
                //                                                    : null
                //                                               ))
                //                                          : null,
                //                                        AuthTokens is not null
                //                                            ? new JProperty("AuthTokens",          new JArray(AuthTokens.Select(_ => _.ToString())))
                //                                            : null,
                //                                        eMAIds is not null
                //                                            ? new JProperty("eMAIds",              new JArray(eMAIds.Select(_ => _.ToString())))
                //                                            : null,
                //                                        PINs is not null
                //                                            ? new JProperty("PINs",                new JArray(PINs.Select(_ => _.ToString())))
                //                                            : null,
                //                                        new JProperty("Result",                    Result.Result.ToString()),
                //                                        Result.Message.IsNotNullOrEmpty()
                //                                            ? new JProperty("ErrorMessage",        Result.Message)
                //                                            : null,
                //                                        new JProperty("Runtime",                   Math.Round(Runtime.TotalMilliseconds, 0))
                //                                  ));

                //#endregion

                //#region OnCancelReservationResponse

                //newRoamingNetwork.OnCancelReservationResponse += async (LogTimestamp,
                //                                                   Timestamp,
                //                                                   Sender,
                //                                                   EventTrackingId,
                //                                                   RoamingNetworkId,
                //                                                   //ProviderId,
                //                                                   ReservationId,
                //                                                   Reservation,
                //                                                   Reason,
                //                                                   Result,
                //                                                   Runtime,
                //                                                   RequestTimeout)

                //    => await DebugLog.SubmitEvent("OnCancelReservation",
                //                                  JSONObject.Create(
                //                                      new JProperty("Timestamp",                Timestamp.ToISO8601()),
                //                                      EventTrackingId is not null
                //                                          ? new JProperty("EventTrackingId",    EventTrackingId.ToString())
                //                                          : null,
                //                                      new JProperty("ReservationId",            ReservationId.ToString()),

                //                                      new JProperty("RoamingNetwork",           RoamingNetworkId.ToString()),

                //                                      Reservation?.EVSEId is not null
                //                                          ? new JProperty("EVSEId",             Reservation.EVSEId.ToString())
                //                                          : null,
                //                                      Reservation?.ChargingStationId is not null
                //                                          ? new JProperty("ChargingStationId",  Reservation.ChargingStationId.ToString())
                //                                          : null,
                //                                      Reservation?.ChargingPoolId is not null
                //                                          ? new JProperty("ChargingPoolId",     Reservation.EVSEId.ToString())
                //                                          : null,

                //                                      new JProperty("Reason",                   Reason.ToString()),

                //                                      new JProperty("Result",                   Result.Result.ToString()),
                //                                      new JProperty("Message",                  Result.Message),
                //                                      new JProperty("AdditionalInfo",           Result.AdditionalInfo),
                //                                      new JProperty("Runtime",                  Result.Runtime)

                //                                  ));

                ////ToDo: OnCancelReservationResponse Result!

                //#endregion


                //#region OnRemoteStartRequest/-Response

                //newRoamingNetwork.OnRemoteStartRequest += async (LogTimestamp,
                //                                                 Timestamp,
                //                                                 Sender,
                //                                                 EventTrackingId,
                //                                                 RoamingNetworkId,
                //                                                 ChargingLocation,
                //                                                 remoteAuthentication,
                //                                                 SessionId,
                //                                                 ReservationId,
                //                                                 ChargingProduct,
                //                                                 EMPRoamingProviderId,
                //                                                 CSORoamingProviderId,
                //                                                 ProviderId,
                //                                                 RequestTimeout)

                //    => await DebugLog.SubmitEvent("OnRemoteStartRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   Timestamp.           ToISO8601()),
                //                                      EventTrackingId is not null
                //                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                //                                          : null,
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      ChargingProduct is not null
                //                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                //                                          : null,
                //                                      ReservationId.HasValue
                //                                          ? new JProperty("reservationId",         ReservationId.       ToString())
                //                                          : null,
                //                                      SessionId.HasValue
                //                                          ? new JProperty("sessionId",             SessionId.           ToString())
                //                                          : null,
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      ProviderId.HasValue
                //                                          ? new JProperty("providerId",            ProviderId.          ToString())
                //                                          : null,
                //                                      remoteAuthentication is not null
                //                                          ? new JProperty("authentication",        remoteAuthentication.ToJSON())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null
                //                                  ));

                //newRoamingNetwork.OnRemoteStartResponse += async (LogTimestamp,
                //                                                  Timestamp,
                //                                                  Sender,
                //                                                  EventTrackingId,
                //                                                  RoamingNetworkId,
                //                                                  ChargingLocation,
                //                                                  remoteAuthentication,
                //                                                  SessionId,
                //                                                  ReservationId,
                //                                                  ChargingProduct,
                //                                                  EMPRoamingProviderId,
                //                                                  CSORoamingProviderId,
                //                                                  ProviderId,
                //                                                  RequestTimeout,
                //                                                  Result,
                //                                                  Runtime)

                //    => await DebugLog.SubmitEvent("OnRemoteStartResponse",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   Timestamp.           ToISO8601()),
                //                                      EventTrackingId      is not null
                //                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                //                                          : null,
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                //                                      ChargingLocation.IsDefined()
                //                                          ? new JProperty("chargingLocation",      ChargingLocation.    ToJSON())
                //                                          : null,
                //                                      ChargingProduct      is not null
                //                                          ? new JProperty("chargingProduct",       ChargingProduct.     ToJSON())
                //                                          : null,
                //                                      ReservationId        is not null
                //                                          ? new JProperty("reservationId",         ReservationId.       ToString())
                //                                          : null,
                //                                      SessionId            is not null
                //                                          ? new JProperty("sessionId",             SessionId.           ToString())
                //                                          : null,
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      ProviderId           is not null
                //                                          ? new JProperty("providerId",            ProviderId.          ToString())
                //                                          : null,
                //                                      remoteAuthentication is not null
                //                                          ? new JProperty("authentication",        remoteAuthentication.ToJSON())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null,
                //                                      new JProperty("result",                      Result.              ToJSON()),
                //                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))
                //                                  ));

                //#endregion

                //#region OnRemoteStopRequest/-Response

                //newRoamingNetwork.OnRemoteStopRequest += async (LogTimestamp,
                //                                                Timestamp,
                //                                                Sender,
                //                                                EventTrackingId,
                //                                                RoamingNetworkId,
                //                                                SessionId,
                //                                                ReservationHandling,
                //                                                EMPRoamingProviderId,
                //                                                CSORoamingProviderId,
                //                                                ProviderId,
                //                                                Authentication,
                //                                                RequestTimeout)

                //    => await DebugLog.SubmitEvent("OnRemoteStopRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   Timestamp.           ToISO8601()),
                //                                      EventTrackingId is not null
                //                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                //                                          : null,
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                //                                      new JProperty("sessionId",                   SessionId.           ToString()),
                //                                      ReservationHandling.HasValue
                //                                          ? new JProperty("reservationHandling",   ReservationHandling. ToString())
                //                                          : null,
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      ProviderId.HasValue
                //                                          ? new JProperty("providerId",            ProviderId.          ToString())
                //                                          : null,
                //                                      Authentication is not null
                //                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null
                //                                  ));

                //newRoamingNetwork.OnRemoteStopResponse += async (LogTimestamp,
                //                                                 Timestamp,
                //                                                 Sender,
                //                                                 EventTrackingId,
                //                                                 RoamingNetworkId,
                //                                                 SessionId,
                //                                                 ReservationHandling,
                //                                                 EMPRoamingProviderId,
                //                                                 CSORoamingProviderId,
                //                                                 ProviderId,
                //                                                 Authentication,
                //                                                 RequestTimeout,
                //                                                 Result,
                //                                                 Runtime)

                //    => await DebugLog.SubmitEvent("OnRemoteStopResponse",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                   Timestamp.           ToISO8601()),
                //                                      EventTrackingId is not null
                //                                          ? new JProperty("eventTrackingId",       EventTrackingId.     ToString())
                //                                          : null,
                //                                      new JProperty("roamingNetworkId",            RoamingNetworkId.    ToString()),
                //                                      new JProperty("sessionId",                   SessionId.           ToString()),
                //                                      ReservationHandling.HasValue
                //                                          ? new JProperty("reservationHandling",   ReservationHandling. ToString())
                //                                          : null,
                //                                      EMPRoamingProviderId.HasValue
                //                                          ? new JProperty("EMPRoamingProviderId",  EMPRoamingProviderId.ToString())
                //                                          : null,
                //                                      CSORoamingProviderId.HasValue
                //                                          ? new JProperty("CSORoamingProviderId",  CSORoamingProviderId.ToString())
                //                                          : null,
                //                                      ProviderId.HasValue
                //                                          ? new JProperty("providerId",            ProviderId.          ToString())
                //                                          : null,
                //                                      Authentication is not null
                //                                          ? new JProperty("authentication",        Authentication.      ToJSON())
                //                                          : null,
                //                                      RequestTimeout.HasValue
                //                                          ? new JProperty("requestTimeout",        Math.Round(RequestTimeout.Value.TotalSeconds, 0))
                //                                          : null,
                //                                      new JProperty("result",                      Result.              ToJSON()),
                //                                      new JProperty("runtime",                     Math.Round(Runtime.TotalMilliseconds, 0))
                //                                  ));

                //#endregion


                //#region OnSendCDRsRequest/-Response

                //newRoamingNetwork.OnSendCDRsRequest += async (LogTimestamp,
                //                                              RequestTimestamp,
                //                                              Sender,
                //                                              SenderId,
                //                                              EventTrackingId,
                //                                              RoamingNetworkId2,
                //                                              ChargeDetailRecords,
                //                                              RequestTimeout)


                //    => await DebugLog.SubmitEvent("OnSendCDRsRequest",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",                RequestTimestamp.  ToISO8601()),
                //                                      new JProperty("eventTrackingId",          EventTrackingId.   ToString()),
                //                                      new JProperty("roamingNetworkId",         RoamingNetworkId2. ToString()),
                //                                      //new JProperty("LogTimestamp",                     LogTimestamp.                                          ToISO8601()),
                //                                      //new JProperty("RequestTimestamp",                 RequestTimestamp.                                      ToISO8601()),

                //                                      new JProperty("chargeDetailRecords",              new JArray(
                //                                          ChargeDetailRecords.Select(ChargeDetailRecord => JSONObject.Create(

                //                                             new JProperty("@id",                              ChargeDetailRecord.Id.                                      ToString()),

                //                                             new JProperty("sessionId",                        ChargeDetailRecord.SessionId.                               ToString()),

                //                                             ChargeDetailRecord.SessionTime is not null
                //                                                 ? new JProperty("sessionStart",               ChargeDetailRecord.SessionTime.StartTime.                   ToISO8601())
                //                                                 : null,
                //                                             ChargeDetailRecord.SessionTime is not null && ChargeDetailRecord.SessionTime.EndTime.HasValue
                //                                                 ? new JProperty("sessionStop",                ChargeDetailRecord.SessionTime.EndTime.Value.               ToISO8601())
                //                                                 : null,

                //                                             ChargeDetailRecord.AuthenticationStart is not null
                //                                                 ? new JProperty("authenticationStart",        ChargeDetailRecord.AuthenticationStart.                     ToJSON())
                //                                                 : null,
                //                                             ChargeDetailRecord.AuthenticationStop is not null
                //                                                 ? new JProperty("authenticationStop",         ChargeDetailRecord.AuthenticationStop.                      ToJSON())
                //                                                 : null,
                //                                             ChargeDetailRecord.ProviderIdStart.HasValue
                //                                                 ? new JProperty("providerIdStart",            ChargeDetailRecord.ProviderIdStart.                         ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ProviderIdStop.HasValue
                //                                                 ? new JProperty("providerIdStop",             ChargeDetailRecord.ProviderIdStop.                          ToString())
                //                                                 : null,

                //                                             ChargeDetailRecord.ReservationId.HasValue
                //                                                 ? new JProperty("reservationId",              ChargeDetailRecord.ReservationId.                           ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ReservationTime is not null
                //                                                 ? new JProperty("reservationStart",           ChargeDetailRecord.ReservationTime.StartTime.               ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ReservationTime is not null && ChargeDetailRecord.ReservationTime.EndTime.HasValue
                //                                                 ? new JProperty("reservationStop",            ChargeDetailRecord.ReservationTime.EndTime.Value.           ToISO8601())
                //                                                 : null,
                //                                             ChargeDetailRecord.Reservation is not null
                //                                                 ? new JProperty("reservationLevel",           ChargeDetailRecord.Reservation.ReservationLevel.            ToString())
                //                                                 : null,

                //                                             ChargeDetailRecord.ChargingStationOperator is not null
                //                                                 ? new JProperty("chargingStationOperator",    ChargeDetailRecord.ChargingStationOperator.                 ToString())
                //                                                 : null,

                //                                             ChargeDetailRecord.EVSE is not null
                //                                                 ? new JProperty("EVSEId",                     ChargeDetailRecord.EVSE.Id.                                 ToString())
                //                                                 : ChargeDetailRecord.EVSEId.HasValue
                //                                                       ? new JProperty("EVSEId",               ChargeDetailRecord.EVSEId.                                  ToString())
                //                                                       : null,

                //                                             ChargeDetailRecord.ChargingProduct is not null
                //                                                 ? new JProperty("chargingProduct",            ChargeDetailRecord.ChargingProduct.ToJSON())
                //                                                 : null,

                //                                             ChargeDetailRecord.EnergyMeterId.HasValue
                //                                                 ? new JProperty("energyMeterId",              ChargeDetailRecord.EnergyMeterId.                      ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ConsumedEnergy.HasValue
                //                                                 ? new JProperty("consumedEnergy",             ChargeDetailRecord.ConsumedEnergy.Value.kWh)
                //                                                 : null,
                //                                             ChargeDetailRecord.EnergyMeteringValues.Any()
                //                                                 ? new JProperty("energyMeteringValues", JSONObject.Create(
                //                                                       ChargeDetailRecord.EnergyMeteringValues.Select(metervalue => new JProperty(metervalue.Timestamp.ToISO8601(),
                //                                                                                                                                  metervalue.WattHours.kWh)))
                //                                                   )
                //                                                 : null,
                //                                             //ChargeDetailRecord.MeteringSignature.IsNotNullOrEmpty()
                //                                             //    ? new JProperty("meteringSignature",          ChargeDetailRecord.MeteringSignature)
                //                                             //    : null,

                //                                             ChargeDetailRecord.ParkingSpaceId.HasValue
                //                                                 ? new JProperty("parkingSpaceId",             ChargeDetailRecord.ParkingSpaceId.                      ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ParkingTime is not null
                //                                                 ? new JProperty("parkingTimeStart",           ChargeDetailRecord.ParkingTime.StartTime.               ToISO8601())
                //                                                 : null,
                //                                             ChargeDetailRecord.ParkingTime is not null && ChargeDetailRecord.ParkingTime.EndTime.HasValue
                //                                                 ? new JProperty("parkingTimeEnd",             ChargeDetailRecord.ParkingTime.EndTime.Value.           ToString())
                //                                                 : null,
                //                                             ChargeDetailRecord.ParkingFee.HasValue
                //                                                 ? new JProperty("parkingFee",                 ChargeDetailRecord.ParkingFee.                          ToString())
                //                                                 : null)

                //                                                 )
                //                                         )
                //                                     )

                //                                  ));

                //#endregion


                //#region OnEVSEData/(Admin)StatusChanged

                //newRoamingNetwork.OnEVSEDataChanged += async (Timestamp,
                //                                              EventTrackingId,
                //                                              EVSE,
                //                                              PropertyName,
                //                                              NewValue,
                //                                              OldValue,
                //                                              dataSource)

                //    => await DebugLog.SubmitEvent("OnEVSEDataChanged",
                //                                  JSONObject.Create(
                //                                      new JProperty("timestamp",        Timestamp.           ToISO8601()),
                //                                      new JProperty("eventTrackingId",  EventTrackingId.     ToString()),
                //                                      new JProperty("roamingNetworkId", newRoamingNetwork.Id.ToString()),
                //                                      new JProperty("EVSEId",           EVSE.Id.             ToString()),
                //                                      new JProperty("propertyName",     PropertyName),
                //                                      new JProperty("oldValue",         OldValue?.           ToString()),
                //                                      new JProperty("newValue",         NewValue?.           ToString())
                //                                  ));



                //newRoamingNetwork.OnEVSEStatusChanged += async (Timestamp,
                //                                                EventTrackingId,
                //                                                EVSE,
                //                                                NewStatus,
                //                                                OldStatus,
                //                                                dataSource)

                //    => await DebugLog.SubmitEvent("OnEVSEStatusChanged",
                //                                  JSONObject.Create(
                //                                            new JProperty("timestamp",         Timestamp.           ToISO8601()),
                //                                            new JProperty("eventTrackingId",   EventTrackingId.     ToString()),
                //                                            new JProperty("roamingNetworkId",  newRoamingNetwork.Id.ToString()),
                //                                            new JProperty("EVSEId",            EVSE.Id.             ToString()),
                //                                      OldStatus.HasValue
                //                                          ? new JProperty("oldStatus",         OldStatus?.Value.    ToString())
                //                                          : null,
                //                                            new JProperty("newStatus",         NewStatus. Value.    ToString())
                //                                  ));



                //newRoamingNetwork.OnEVSEAdminStatusChanged += async (Timestamp,
                //                                                     EventTrackingId,
                //                                                     EVSE,
                //                                                     NewStatus,
                //                                                     OldStatus,
                //                                                     dataSource)

                //    => await DebugLog.SubmitEvent("OnEVSEAdminStatusChanged",
                //                                  JSONObject.Create(
                //                                            new JProperty("timestamp",         Timestamp.           ToISO8601()),
                //                                            new JProperty("eventTrackingId",   EventTrackingId.     ToString()),
                //                                            new JProperty("roamingNetworkId",  newRoamingNetwork.Id.ToString()),
                //                                            new JProperty("EVSEId",            EVSE.Id.             ToString()),
                //                                      OldStatus.HasValue
                //                                          ? new JProperty("oldStatus",         OldStatus?.Value.    ToString())
                //                                          : null,
                //                                            new JProperty("newStatus",         NewStatus.Value.     ToString())
                //                                  ));

                //#endregion

                #endregion

                return newRoamingNetwork;

            }

            throw new RoamingNetworkAlreadyExists(Id);

        }

        #endregion

        #region GetAllRoamingNetworks(Hostname)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        public IEnumerable<IRoamingNetwork> GetAllRoamingNetworks(HTTPHostname  Hostname)

            => roamingNetworks;// WWCPHTTPServer.GetAllTenants(Hostname);

        #endregion

        #region GetRoamingNetwork( RoamingNetworkId)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public IRoamingNetwork? GetRoamingNetwork(HTTPHostname       Hostname,
                                                 RoamingNetwork_Id  RoamingNetworkId)

        // => WWCPHTTPServer.GetAllTenants(Hostname).
        //        FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);
        {

            if (roamingNetworks.TryGetRoamingNetwork(RoamingNetworkId, out var roamingNetwork))
                return roamingNetwork;

            return null;

        }


        #endregion

        #region TryGetRoamingNetwork( RoamingNetworkId, out RoamingNetwork)

        /// <summary>
        ///Try to return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        /// <param name="RoamingNetwork">A roaming network.</param>
        public Boolean TryGetRoamingNetwork(HTTPHostname                              Hostname,
                                            RoamingNetwork_Id                         RoamingNetworkId,
                                            [NotNullWhen(true)] out IRoamingNetwork?  RoamingNetwork)
        {

            //RoamingNetwork  = WWCPHTTPServer.GetAllTenants(Hostname).
            //                      FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            //return RoamingNetwork is not null;

            if (roamingNetworks.TryGetRoamingNetwork(RoamingNetworkId, out var roamingNetwork))
            {
                RoamingNetwork = roamingNetwork;
                return true;
            }

            RoamingNetwork = null;
            return false;

        }

        #endregion

        #region RoamingNetworkExists( RoamingNetworkId)

        /// <summary>
        /// Check if a roaming networks exists for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public Boolean RoamingNetworkExists(HTTPHostname        Hostname,
                                            RoamingNetwork_Id   RoamingNetworkId)

            => roamingNetworks.Contains(RoamingNetworkId);

        #endregion

        #region RemoveRoamingNetwork( RoamingNetworkId)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        public IRoamingNetwork? RemoveRoamingNetwork(HTTPHostname       Hostname,
                                                     RoamingNetwork_Id  RoamingNetworkId)
        {

            return roamingNetworks.RemoveRoamingNetwork(RoamingNetworkId);

        }

        #endregion

        #region TryRemoveRoamingNetwork( RoamingNetworkId, out RoamingNetwork)

        /// <summary>
        /// Return all roaming networks available for the given hostname.
        /// </summary>
        /// <param name="Hostname">The HTTP hostname.</param>
        /// <param name="RoamingNetworkId">The unique identification of the new roaming network.</param>
        /// <param name="RoamingNetwork">The removed roaming network.</param>
        public Boolean TryRemoveRoamingNetwork(HTTPHostname         Hostname,
                                               RoamingNetwork_Id    RoamingNetworkId,
                                               out IRoamingNetwork  RoamingNetwork)
        {

            //if (!WWCPHTTPServer.TryGetTenants( out RoamingNetworks _RoamingNetworks))
            //{
            //    RoamingNetwork = null;
            //    return false;
            //}

            RoamingNetwork = roamingNetworks.RemoveRoamingNetwork(RoamingNetworkId);

            return RoamingNetwork is not null;

        }

        #endregion


    }

}
