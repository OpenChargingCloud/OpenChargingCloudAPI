/*
 * Copyright (c) 2014-2023 GraphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of WWCP Net <https://github.com/OpenChargingCloud/WWCP_Net>
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

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using cloud.charging.open.API;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.IO.JSON
{

    /// <summary>
    /// WWCP HTTP API - JSON I/O.
    /// </summary>
    public static partial class JSON_IO
    {

        /// <summary>
        /// Attach JSON I/O to the given WWCP HTTP API.
        /// </summary>
        /// <param name="OpenChargingCloudAPI">A WWCP HTTP API.</param>
        /// <param name="Hostname">Limit this JSON I/O handling to the given HTTP hostname.</param>
        /// <param name="URIPrefix">A common URI prefix for all URIs within this API.</param>
        public static void Attach_JSON_IO_SmartCities(this OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                      HTTPHostname?              _Hostname   = null,
                                                      HTTPPath?                  _URIPrefix  = null)
        {

            var Hostname   = _Hostname  ?? HTTPHostname.Any;
            var URIPrefix  = _URIPrefix ?? HTTPPath.Parse("/");

            #region ~/RNs/{RoamingNetworkId}/SmartCities

            #region GET         ~/RNs/{RoamingNetworkId}/SmartCities

            // -----------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/SmartCities
            // -----------------------------------------------------------------------------------------
            OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
                                                              Hostname,
                                                              HTTPMethod.GET,
                                                              URIPrefix + "RNs/{RoamingNetworkId}/SmartCities",
                                                              HTTPContentType.JSON_UTF8,
                                                              HTTPDelegate: Request => {

                                                                  #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
                                                                                      out var _RoamingNetwork,
                                                                                      out var _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse.AsImmutable);
                                                     }

                                                     #endregion

                                                                  var skip                    = Request.QueryString.GetUInt64("skip");
                                                                  var take                    = Request.QueryString.GetUInt64("take");
                                                                  var expand                  = Request.QueryString.GetStrings("expand");
                                                                  //var expandChargingPools     = !expand.Contains("-chargingpools");
                                                                  //var expandChargingStations  = !expand.Contains("-chargingstations");
                                                                  //var expandBrands            = expand.Contains("brands");

                                                                  //ToDo: Getting the expected total count might be very expensive!
                                                                  var _ExpectedCount = _RoamingNetwork.SmartCities.ULongCount();

                                                                  return Task.FromResult(
                                                                      new HTTPResponse.Builder(Request) {
                                                                          HTTPStatusCode               = HTTPStatusCode.OK,
                                                                          Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                                                                          Date                         = Timestamp.Now,
                                                                          AccessControlAllowOrigin     = "*",
                                                                          AccessControlAllowMethods    = new[] { "GET", "COUNT", "STATUS" },
                                                                          AccessControlAllowHeaders    = new[] { "Content-Type", "Accept", "Authorization" },
                                                                          ETag                         = "1",
                                                                          ContentType                  = HTTPContentType.JSON_UTF8,
                                                                          Content                      = _RoamingNetwork.SmartCities.
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

            #region COUNT       ~/RNs/{RoamingNetworkId}/SmartCities

            // ----------------------------------------------------------------------------------------------------------------
            // curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RoamingNetworkId}/SmartCities
            // ----------------------------------------------------------------------------------------------------------------
            OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
                                                              Hostname,
                                                              HTTPMethod.COUNT,
                                                              URIPrefix + "RNs/{RoamingNetworkId}/SmartCities",
                                                              HTTPContentType.JSON_UTF8,
                                                              HTTPDelegate: Request => {

                                                                  #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
                                                                                      out var _RoamingNetwork,
                                                                                      out var _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse.AsImmutable);
                                                     }

                                                     #endregion

                                                                  return Task.FromResult(
                                                                      new HTTPResponse.Builder(Request) {
                                                                          HTTPStatusCode               = HTTPStatusCode.OK,
                                                                          Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                                                                          Date                         = Timestamp.Now,
                                                                          AccessControlAllowOrigin     = "*",
                                                                          AccessControlAllowMethods    = new[] { "GET", "COUNT", "STATUS" },
                                                                          AccessControlAllowHeaders    = new[] { "Content-Type", "Accept", "Authorization" },
                                                                          ETag                         = "1",
                                                                          ContentType                  = HTTPContentType.JSON_UTF8,
                                                                          Content                      = JSONObject.Create(
                                                                                                             new JProperty("count",  _RoamingNetwork.ChargingStationOperators.ULongCount())
                                                                                                         ).ToUTF8Bytes()
                                                                      }.AsImmutable);

                                                              });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/SmartCities->AdminStatus

            // ------------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/SmartCities->AdminStatus
            // ------------------------------------------------------------------------------------------------------
            OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
                                                              Hostname,
                                                              HTTPMethod.GET,
                                                              URIPrefix + "RNs/{RoamingNetworkId}/SmartCities->AdminStatus",
                                                              HTTPContentType.JSON_UTF8,
                                                              HTTPDelegate: Request => {

                                                                  #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
                                                                                      out var _RoamingNetwork,
                                                                                      out var _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse.AsImmutable);
                                                     }

                                                     #endregion

                                                                  var skip         = Request.QueryString.GetUInt64("skip");
                                                                  var take         = Request.QueryString.GetUInt64("take");
                                                                  var historysize  = Request.QueryString.GetUInt64("historysize", 1);

                                                                  //ToDo: Getting the expected total count might be very expensive!
                                                                  var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorAdminStatus().ULongCount();

                                                                  return Task.FromResult(
                                                                      new HTTPResponse.Builder(Request) {
                                                                          HTTPStatusCode               = HTTPStatusCode.OK,
                                                                          Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                                                                          Date                         = Timestamp.Now,
                                                                          AccessControlAllowOrigin     = "*",
                                                                          AccessControlAllowMethods    = new[] { "GET" },
                                                                          AccessControlAllowHeaders    = new[] { "Content-Type", "Accept", "Authorization" },
                                                                          ETag                         = "1",
                                                                          ContentType                  = HTTPContentType.JSON_UTF8,
                                                                          Content                      = _RoamingNetwork.ChargingStationOperatorAdminStatus().
                                                                                                             OrderBy(status => status.Id).
                                                                                                             ToJSON (skip, take).
                                                                                                             ToUTF8Bytes(),
                                                                          X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                                      }.AsImmutable);

                                                              });

            #endregion

            #region GET         ~/RNs/{RoamingNetworkId}/SmartCities->Status

            // -------------------------------------------------------------------------------------------------
            // curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/SmartCities->Status
            // -------------------------------------------------------------------------------------------------
            OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
                                                              Hostname,
                                                              HTTPMethod.GET,
                                                              URIPrefix + "RNs/{RoamingNetworkId}/SmartCities->Status",
                                                              HTTPContentType.JSON_UTF8,
                                                              HTTPDelegate: Request => {

                                                                  #region Check parameters

                                                     if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
                                                                                      out var _RoamingNetwork,
                                                                                      out var _HTTPResponse))
                                                     {
                                                         return Task.FromResult(_HTTPResponse.AsImmutable);
                                                     }

                                                     #endregion

                                                                  var skip         = Request.QueryString.GetUInt64("skip");
                                                                  var take         = Request.QueryString.GetUInt64("take");

                                                                  //ToDo: Getting the expected total count might be very expensive!
                                                                  var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorStatus().ULongCount();

                                                                  return Task.FromResult(
                                                                      new HTTPResponse.Builder(Request) {
                                                                          HTTPStatusCode               = HTTPStatusCode.OK,
                                                                          Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                                                                          Date                         = Timestamp.Now,
                                                                          AccessControlAllowOrigin     = "*",
                                                                          AccessControlAllowMethods    = new[] { "GET" },
                                                                          AccessControlAllowHeaders    = new[] { "Content-Type", "Accept", "Authorization" },
                                                                          ETag                         = "1",
                                                                          ContentType                  = HTTPContentType.JSON_UTF8,
                                                                          Content                      = _RoamingNetwork.ChargingStationOperatorStatus().
                                                                                                             OrderBy(status => status.Id).
                                                                                                             ToJSON (skip, take).
                                                                                                             ToUTF8Bytes(),
                                                                          X_ExpectedTotalNumberOfItems  = _ExpectedCount
                                                                      }.AsImmutable);

                                                              });

            #endregion

            #endregion

            #region ~/RNs/{RoamingNetworkId}/SmartCities/{SmartCityId}

            #region GET         ~/RNs/{RoamingNetworkId}/SmartCities/{SmartCityId}

            OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
                                                              HTTPHostname.Any,
                                                              HTTPMethod.GET,
                                                              URIPrefix + "RNs/{RoamingNetworkId}/SmartCities/{SmartCityId}",
                                                              HTTPContentType.JSON_UTF8,
                                                              HTTPDelegate: Request => {

                                                                  #region Check HTTP parameters

                                                     if (!Request.ParseRoamingNetworkAndSmartCity(OpenChargingCloudAPI,
                                                                                                  out var _RoamingNetwork,
                                                                                                  out var _SmartCity,
                                                                                                  out var _HTTPResponse))

                                                         return Task.FromResult(_HTTPResponse.AsImmutable);

                                                     #endregion

                                                                  return Task.FromResult(
                                                                      new HTTPResponse.Builder(Request) {
                                                                          HTTPStatusCode             = HTTPStatusCode.OK,
                                                                          Server                     = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
                                                                          Date                       = Timestamp.Now,
                                                                          AccessControlAllowOrigin   = "*",
                                                                          AccessControlAllowMethods  = new[] { "GET", "CREATE", "DELETE" },
                                                                          AccessControlAllowHeaders  = new[] { "Content-Type", "Accept", "Authorization" },
                                                                          ETag                       = "1",
                                                                          ContentType                = HTTPContentType.JSON_UTF8,
                                                                          Content                    = _SmartCity.ToJSON().ToUTF8Bytes()
                                                                      }.AsImmutable);

                                                              });

            #endregion

            #endregion

        }

    }

}
