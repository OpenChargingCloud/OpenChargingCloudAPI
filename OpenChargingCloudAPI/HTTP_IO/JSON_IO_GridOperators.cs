/*
 * Copyright (c) 2014-2026 GraphDefined GmbH <achim.friedland@graphdefined.com>
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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
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
        /// <param name="URLPrefix">A common URL prefix for all URLs within this API.</param>
        public static void Attach_JSON_IO_GridOperators(this OpenChargingCloudAPI  OpenChargingCloudAPI,
                                                        HTTPHostname?              Hostname    = null,
                                                        HTTPPath?                  URLPrefix   = null)
        {

            var _Hostname   = Hostname  ?? HTTPHostname.Any;
            var _URIPrefix  = URLPrefix ?? HTTPPath.Parse("/");

            //#region ~/RNs/{RoamingNetworkId}/GridOperators

            //#region GET         ~/RNs/{RoamingNetworkId}/GridOperators

            //// -----------------------------------------------------------------------------------------
            //// curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/GridOperators
            //// -----------------------------------------------------------------------------------------
            //OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
            //                                                  _Hostname,
            //                                                  HTTPMethod.GET,
            //                                                  _URIPrefix + "RNs/{RoamingNetworkId}/GridOperators",
            //                                                  HTTPContentType.Application.JSON_UTF8,
            //                                                  HTTPDelegate: Request => {

            //                                                      #region Check parameters

            //                                                      if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
            //                                                                                       out var roamingNetwork,
            //                                                                                       out var httpResponse) ||
            //                                                           roamingNetwork is null)
            //                                                      {
            //                                                          return Task.FromResult(httpResponse.AsImmutable);
            //                                                      }

            //                                                      #endregion

            //                                                      var skip                    = Request.QueryString.GetUInt64("skip");
            //                                                      var take                    = Request.QueryString.GetUInt64("take");
            //                                                      var expand                  = Request.QueryString.GetStrings("expand");
            //                                                      //var expandChargingPools     = !expand.Contains("-chargingpools");
            //                                                      //var expandChargingStations  = !expand.Contains("-chargingstations");
            //                                                      //var expandBrands            = expand.Contains("brands");

            //                                                      //ToDo: Getting the expected total count might be very expensive!
            //                                                      var _ExpectedCount = roamingNetwork.GridOperators.ULongCount();

            //                                                      return Task.FromResult(
            //                                                          new HTTPResponse.Builder(Request) {
            //                                                              HTTPStatusCode               = HTTPStatusCode.OK,
            //                                                              Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
            //                                                              Date                         = Timestamp.Now,
            //                                                              AccessControlAllowOrigin     = "*",
            //                                                              AccessControlAllowMethods    = [ "GET", "COUNT", "STATUS" ],
            //                                                              AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
            //                                                              ETag                         = "1",
            //                                                              ContentType                  = HTTPContentType.Application.JSON_UTF8,
            //                                                              Content                      = roamingNetwork.GridOperators.
            //                                                                                                 ToJSON(skip,
            //                                                                                                        take,
            //                                                                                                        false).
            //                                                                                                        //expandChargingPools,
            //                                                                                                        //expandChargingStations,
            //                                                                                                        //expandBrands).
            //                                                                                                 ToUTF8Bytes(),
            //                                                              X_ExpectedTotalNumberOfItems  = _ExpectedCount
            //                                                          }.AsImmutable);

            //                                                  });

            //#endregion

            //#region COUNT       ~/RNs/{RoamingNetworkId}/GridOperators

            //// ----------------------------------------------------------------------------------------------------------------
            //// curl -v -X COUNT -H "Accept: application/json" http://127.0.0.1:3004/RNs/{RoamingNetworkId}/GridOperators
            //// ----------------------------------------------------------------------------------------------------------------
            //OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
            //                                                  _Hostname,
            //                                                  HTTPMethod.COUNT,
            //                                                  _URIPrefix + "RNs/{RoamingNetworkId}/GridOperators",
            //                                                  HTTPContentType.Application.JSON_UTF8,
            //                                                  HTTPDelegate: Request => {

            //                                                      #region Check parameters

            //                                         if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
            //                                                                          out var _RoamingNetwork,
            //                                                                          out var _HTTPResponse))
            //                                         {
            //                                             return Task.FromResult(_HTTPResponse.AsImmutable);
            //                                         }

            //                                         #endregion

            //                                                      return Task.FromResult(
            //                                                          new HTTPResponse.Builder(Request) {
            //                                                              HTTPStatusCode               = HTTPStatusCode.OK,
            //                                                              Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
            //                                                              Date                         = Timestamp.Now,
            //                                                              AccessControlAllowOrigin     = "*",
            //                                                              AccessControlAllowMethods    = [ "GET", "COUNT", "STATUS" ],
            //                                                              AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
            //                                                              ETag                         = "1",
            //                                                              ContentType                  = HTTPContentType.Application.JSON_UTF8,
            //                                                              Content                      = JSONObject.Create(
            //                                                                                                 new JProperty("count",  _RoamingNetwork.ChargingStationOperators.ULongCount())
            //                                                                                             ).ToUTF8Bytes()
            //                                                          }.AsImmutable);

            //                                                  });

            //#endregion

            //#region GET         ~/RNs/{RoamingNetworkId}/GridOperators->AdminStatus

            //// ------------------------------------------------------------------------------------------------------
            //// curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/GridOperators->AdminStatus
            //// ------------------------------------------------------------------------------------------------------
            //OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
            //                                                  _Hostname,
            //                                                  HTTPMethod.GET,
            //                                                  _URIPrefix + "RNs/{RoamingNetworkId}/GridOperators->AdminStatus",
            //                                                  HTTPContentType.Application.JSON_UTF8,
            //                                                  HTTPDelegate: Request => {

            //                                                      #region Check parameters

            //                                         if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
            //                                                                          out var _RoamingNetwork,
            //                                                                          out var _HTTPResponse))
            //                                         {
            //                                             return Task.FromResult(_HTTPResponse.AsImmutable);
            //                                         }

            //                                         #endregion

            //                                                      var skip         = Request.QueryString.GetUInt64("skip");
            //                                                      var take         = Request.QueryString.GetUInt64("take");
            //                                                      var historysize  = Request.QueryString.GetUInt64("historysize", 1);

            //                                                      //ToDo: Getting the expected total count might be very expensive!
            //                                                      var _ExpectedCount = _RoamingNetwork.ChargingStationOperatorAdminStatus().ULongCount();

            //                                                      return Task.FromResult(
            //                                                          new HTTPResponse.Builder(Request) {
            //                                                              HTTPStatusCode               = HTTPStatusCode.OK,
            //                                                              Server                       = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
            //                                                              Date                         = Timestamp.Now,
            //                                                              AccessControlAllowOrigin     = "*",
            //                                                              AccessControlAllowMethods    = [ "GET" ],
            //                                                              AccessControlAllowHeaders    = [ "Content-Type", "Accept", "Authorization" ],
            //                                                              ETag                         = "1",
            //                                                              ContentType                  = HTTPContentType.Application.JSON_UTF8,
            //                                                              Content                      = _RoamingNetwork.ChargingStationOperatorAdminStatus().
            //                                                                                                 OrderBy(status => status.Id).
            //                                                                                                 ToJSON (skip, take).
            //                                                                                                 ToUTF8Bytes(),
            //                                                              X_ExpectedTotalNumberOfItems  = _ExpectedCount
            //                                                          }.AsImmutable);

            //                                                  });

            //#endregion

            //#region GET         ~/RNs/{RoamingNetworkId}/GridOperators->Status

            //// -------------------------------------------------------------------------------------------------
            //// curl -v -H "Accept: application/json" http://127.0.0.1:3004/RNs/Test/GridOperators->Status
            //// -------------------------------------------------------------------------------------------------
            //OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
            //                                                  _Hostname,
            //                                                  HTTPMethod.GET,
            //                                                  _URIPrefix + "RNs/{RoamingNetworkId}/GridOperators->Status",
            //                                                  HTTPContentType.Application.JSON_UTF8,
            //                                                  HTTPDelegate: Request => {

            //                                                      #region Check parameters

            //                                         if (!Request.ParseRoamingNetwork(OpenChargingCloudAPI,
            //                                                                          out var _RoamingNetwork,
            //                                                                          out var _HTTPResponse))
            //                                         {
            //                                             return Task.FromResult(_HTTPResponse.AsImmutable);
            //                                         }

            //                                         #endregion

            //                                                      var skip           = Request.QueryString.GetUInt64("skip");
            //                                                      var take           = Request.QueryString.GetUInt64("take");

            //                                                      //ToDo: Getting the expected total count might be very expensive!
            //                                                      var expectedCount  = _RoamingNetwork.ChargingStationOperatorStatus().ULongCount();

            //                                                      return Task.FromResult(
            //                                                          new HTTPResponse.Builder(Request) {
            //                                                              HTTPStatusCode                = HTTPStatusCode.OK,
            //                                                              Server                        = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
            //                                                              Date                          = Timestamp.Now,
            //                                                              AccessControlAllowOrigin      = "*",
            //                                                              AccessControlAllowMethods     = [ "GET" ],
            //                                                              AccessControlAllowHeaders     = [ "Content-Type", "Accept", "Authorization" ],
            //                                                              ETag                          = "1",
            //                                                              ContentType                   = HTTPContentType.Application.JSON_UTF8,
            //                                                              Content                       = _RoamingNetwork.ChargingStationOperatorStatus().
            //                                                                                                  OrderBy(status => status.Id).
            //                                                                                                  ToJSON (skip, take).
            //                                                                                                  ToUTF8Bytes(),
            //                                                              X_ExpectedTotalNumberOfItems  = expectedCount
            //                                                          }.AsImmutable);

            //                                                  });

            //#endregion

            //#endregion

            //#region ~/RNs/{RoamingNetworkId}/GridOperators/{GridOperatorId}

            //#region GET         ~/RNs/{RoamingNetworkId}/GridOperators/{GridOperatorId}

            //OpenChargingCloudAPI.HTTPServer.AddMethodCallback(OpenChargingCloudAPI,
            //                                                  HTTPHostname.Any,
            //                                                  HTTPMethod.GET,
            //                                                  _URIPrefix + "RNs/{RoamingNetworkId}/GridOperators/{GridOperatorId}",
            //                                                  HTTPContentType.Application.JSON_UTF8,
            //                                                  HTTPDelegate: Request => {

            //                                                      #region Check HTTP parameters

            //                                         if (!Request.ParseRoamingNetworkAndGridOperator(OpenChargingCloudAPI,
            //                                                                                         out var roamingNetwork,
            //                                                                                         out var gridOperator,
            //                                                                                         out var httpResponseBuilder))
            //                                         {
            //                                             return Task.FromResult(httpResponseBuilder.AsImmutable);
            //                                         }

            //                                         #endregion

            //                                                      return Task.FromResult(
            //                                                          new HTTPResponse.Builder(Request) {
            //                                                              HTTPStatusCode             = HTTPStatusCode.OK,
            //                                                              Server                     = OpenChargingCloudAPI.HTTPServer.DefaultServerName,
            //                                                              Date                       = Timestamp.Now,
            //                                                              AccessControlAllowOrigin   = "*",
            //                                                              AccessControlAllowMethods  = [ "GET", "CREATE", "DELETE" ],
            //                                                              AccessControlAllowHeaders  = [ "Content-Type", "Accept", "Authorization" ],
            //                                                              ETag                       = "1",
            //                                                              ContentType                = HTTPContentType.Application.JSON_UTF8,
            //                                                              Content                    = gridOperator.ToJSON().ToUTF8Bytes()
            //                                                          }.AsImmutable);

            //                                                   });

            //#endregion

            //#endregion

        }

    }

}
