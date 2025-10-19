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

using System.Xml.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Aegir;

using cloud.charging.open.API;
using cloud.charging.open.protocols.WWCP;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.IO.XML
{

    public static class XMLExport
    {

        public static readonly XNamespace NS_WWCP             = "http://graphdefined.org/wwcp/XML";
        public static readonly XNamespace NS_ChargingPool     = "http://graphdefined.org/wwcp/XML/ChargingPool";
        public static readonly XNamespace NS_ChargingStation  = "http://graphdefined.org/wwcp/XML/ChargingStation";
        public static readonly XNamespace NS_EVSE             = "http://graphdefined.org/wwcp/XML/EVSE";
        public static readonly XNamespace NS_SocketOutlet     = "http://graphdefined.org/wwcp/XML/SocketOutlet";


        public static readonly String eMI3_Root     = "eMI3";
        public static readonly String eMI3_Version  = "version";



        public static XDocument Generate(XElement Element)
        {

            var xmlRoot = new XElement(
                              NS_WWCP + eMI3_Root,
                              new XAttribute(NS_WWCP + eMI3_Version, "0.1"),
                              new XAttribute(XNamespace.Xmlns + "eMI3",             NS_WWCP.           NamespaceName),
                              new XAttribute(XNamespace.Xmlns + "ChargingPool",     NS_ChargingPool.   NamespaceName),
                              new XAttribute(XNamespace.Xmlns + "ChargingStation",  NS_ChargingStation.NamespaceName),
                              new XAttribute(XNamespace.Xmlns + "EVSE",             NS_EVSE.           NamespaceName),
                              new XAttribute(XNamespace.Xmlns + "SocketOutlet",     NS_SocketOutlet.   NamespaceName),
                              Element
                          );

            return new XDocument(
                       new XDeclaration("1.0", "utf-8", null),
                       xmlRoot
                   );

        }


        #region ToXML(this Object, Namespace, ElementName)

        public static XElement ToXML(this Object  Object,
                                     XNamespace   Namespace,
                                     String       ElementName)

            => new (Namespace + ElementName,
                    Object.ToString());

        #endregion

        #region ToXML(this DateTime, Namespace, ElementName, Format = ...)

        public static XElement ToXML(this DateTime  DateTime,
                                     XNamespace     Namespace,
                                     String         ElementName,
                                     String         Format   = "yyyyMMdd HHmmss")

            => new (
                   Namespace + ElementName,
                   DateTime.ToUniversalTime().ToString(Format)
               );

        #endregion

        #region ToXML(this I8N, Namespace, ElementName)

        public static readonly String I8N_Root      = "I8N";
        public static readonly String I8N_Language  = "lang";

        public static XElement ToXML(this I18NString  I8N,
                                     XNamespace       Namespace,
                                     String           ElementName)

            => new (
                   Namespace + ElementName,
                   I8N.Select(v => new XElement(
                                       NS_WWCP + I8N_Root,
                                       new XAttribute(
                                           NS_WWCP + I8N_Language,
                                           v.Language
                                       ),
                                       v.Text
                                   )
                             )
               );

        #endregion

        #region ToXML(this Location, Namespace, ElementName)

        public static readonly String Geo_Model     = "model";
        public static readonly String Geo_Latitude  = "latitude";
        public static readonly String Geo_Longitude = "longitude";
        public static readonly String Geo_Altitude  = "altitude";

        public static XElement? ToXML(this GeoCoordinate  Location,
                                      XNamespace          Namespace,
                                      String              ElementName)
        {

            if (Location.Longitude.Value == 0 && Location.Latitude.Value == 0)
                return null;

            return new XElement(Namespace + ElementName,
                       Location.Projection != GravitationalModel.WGS84 ? new XAttribute(NS_WWCP + Geo_Model,    Location.Projection)           : null,
                       new XElement(NS_WWCP + Geo_Latitude,  Location.Latitude),
                       new XElement(NS_WWCP + Geo_Longitude, Location.Longitude),
                       Location.Altitude.HasValue                      ? new XElement  (NS_WWCP + Geo_Altitude, Location.Altitude.Value.Value) : null
                   );

        }

        #endregion


        #region ToXML(this Pool)

        public static readonly String NS_EVSPool_Enumeration       = "Enumeration";
        public static readonly String NS_EVSPool_Root              = "Instance";
        public static readonly String NS_EVSPool_Id                = "Id";
        public static readonly String NS_EVSPool_Timestamp         = "Timestamp";
        public static readonly String NS_EVSPool_Name              = "Name";
        public static readonly String NS_EVSPool_Description       = "Description";
        public static readonly String NS_EVSPool_LocationLanguage  = "LocationLanguage";

        public static XElement ToXML(this ChargingPool Pool)
        {

            return new XElement(NS_ChargingPool + NS_EVSPool_Root,
                        Pool.Id.              ToXML(NS_ChargingPool, NS_EVSPool_Id),
                        Pool.LastChangeDate.       ToXML(NS_ChargingPool, NS_EVSPool_Timestamp),
                        Pool.Name.            ToXML(NS_ChargingPool, NS_EVSPool_Name),
                        (!Pool.Description.IsNullOrEmpty()) ?
                        Pool.Description.     ToXML(NS_ChargingPool, NS_EVSPool_Description)      : null,
                 //       (Pool.LocationLanguage != Languages.unknown) ?
                 //       Pool.LocationLanguage.ToXML(NS_ChargingPool, NS_EVSPool_LocationLanguage) : null,
                        Pool.GeoLocation.     ToXML(NS_ChargingPool, "GeoLocation"),
                        Pool.EntranceLocation.ToXML(NS_ChargingPool, "EntranceLocation")
                   );

        }

        #endregion


        public static XDocument ToXML(this ChargingStation Station)
        {

            var _XDocument = new XDocument();




            return _XDocument;

        }

        public static XDocument ToXML(this EVSE EVSE)
        {

            var _XDocument = new XDocument();




            return _XDocument;

        }

        public static XDocument ToXML(this ChargingConnector Socket)
        {

            var _XDocument = new XDocument();




            return _XDocument;

        }


        public static void RegisterXML(this OpenChargingCloudAPI  OpenChargingCloudAPI,
                                       WWCPCore                   RoamingNetworks)
        {
        }

    }

}
