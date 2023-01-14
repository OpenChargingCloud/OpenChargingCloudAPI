/*
 * Copyright (c) 2014-2023 GraphDefined GmbH <achim.friedland@graphdefined.com>
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

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using cloud.charging.open.API;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.IO.CSV
{

    /// <summary>
    /// WWCP HTTP API - CSV I/O.
    /// </summary>
    public static partial class CSV_IO
    {

        /// <summary>
        /// Attach CSV I/O to the given WWCP HTTP API.
        /// </summary>
        /// <param name="OpenChargingCloudAPI">A WWCP HTTP API.</param>
        /// <param name="Hostname">Limit this CSV I/O handling to the given HTTP hostname.</param>
        /// <param name="URIPrefix">A common URI prefix for all URIs within this API.</param>
        public static void Attach_CSV_IO(this OpenChargingCloudAPI OpenChargingCloudAPI,
                                         HTTPHostname?      Hostname   = null,
                                         HTTPPath?          URIPrefix  = null)
        {

            var _Hostname = Hostname ?? HTTPHostname.Any;

            // /AdminStatus
            // /Status

            //WWCPAPI.Attach_CSV_IO_RoamingNetworks   (Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_ChargingOperators (Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_ParkingOperators  (Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_EMobilityProviders(Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_SmartCities       (Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_Reservations      (Hostname, URIPrefix);
            //WWCPAPI.Attach_CSV_IO_ChargingSessions  (Hostname, URIPrefix);

        }

    }

}
