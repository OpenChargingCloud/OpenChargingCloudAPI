﻿/*
 * Copyright (c) 2010-2016, Achim 'ahzf' Friedland <achim.friedland@graphdefined.com>
 * This file is part of the Open Charging Cloud API <https://github.com/OpenChargingCloud/WWCP_Net>
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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Aegir;

using cloud.charging.open.protocols.WWCP.Net.IO.JSON;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.IO.GeoJSON
{

    /// <summary>
    /// GeoJSON export utilities.
    /// </summary>
    public static class GeoJSON_IO
    {

        #region ToFeatureCollection(this JSONObjects,              Properties = null)

        public static JObject ToFeatureCollection(this IEnumerable<JObject>  JSONObjects,
                                                  JObject                    Properties  = null)

        {

            #region Initial checks

            if (JSONObjects == null)
                throw new ArgumentNullException(nameof(JSONObjects), "The given enumeration of JSON Objects must not be null!");

            #endregion

            #region Documentation

            // {
            //   "type": "FeatureCollection",
            //   "properties": {
            //     "aaa": "abcde"
            //   },
            //   "features": [
            //     {
            //       "type": "Feature",
            //       "properties": {},
            //       "geometry": {
            //         "type": "Point",
            //         "coordinates": [
            //           10.579833984375,
            //           50.48197825997291
            //         ]
            //       }
            //     },
            //     {
            //       "type": "Feature",
            //       "properties": {
            //         "qqqqqq": 123
            //       },
            //       "geometry": {
            //         "type": "Point",
            //         "coordinates": [
            //           10.70068359375,
            //           50.466246274560476
            //         ]
            //       }
            //     }
            //   ]
            // }

            #endregion

            return new JObject(
                       new JProperty("type",        "FeatureCollection"),
                       new JProperty("properties",  Properties ?? new JObject()),
                       new JProperty("features",    new JArray(JSONObjects))
                   );

        }

        #endregion

        #region ToFeatureCollection(this ChargingStationOperators, Properties = null)

        public static JObject ToFeatureCollection(this IEnumerable<ChargingStationOperator>  ChargingStationOperators,
                                                  JObject                                    Properties  = null)

        {

            #region Initial checks

            if (ChargingStationOperators == null)
                throw new ArgumentNullException(nameof(ChargingStationOperators), "The given enumeration of charging station operators must not be null!");

            #endregion

            return ChargingStationOperators.
                       Select(pool => pool.ToFeature()).
                       ToFeatureCollection(Properties);

        }

        #endregion

        #region ToFeatureCollection(this ChargingPools,            Properties = null)

        public static JObject ToFeatureCollection(this IEnumerable<ChargingPool>  ChargingPools,
                                                  JObject                         Properties  = null)

        {

            #region Initial checks

            if (ChargingPools == null)
                throw new ArgumentNullException(nameof(ChargingPools), "The given enumeration of charging pools must not be null!");

            #endregion

            return ChargingPools.
                       Select(pool => pool.ToFeature()).
                       ToFeatureCollection(Properties);

        }

        #endregion

        #region ToFeatureCollection(this ChargingStations,         Properties = null)

        public static JObject ToFeatureCollection(this IEnumerable<ChargingStation>  ChargingStations,
                                                  JObject                            Properties  = null)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");

            #endregion

            return ChargingStations.
                       Select(station => station.ToFeature()).
                       ToFeatureCollection(Properties);

        }

        #endregion

        #region ToFeatureCollection(this EVSEs,                    Properties = null)

        public static JObject ToFeatureCollection(this IEnumerable<EVSE>  EVSEs,
                                                  JObject                 Properties  = null)

        {

            #region Initial checks

            if (EVSEs == null)
                throw new ArgumentNullException(nameof(EVSEs), "The given enumeration of EVSEs must not be null!");

            #endregion

            return EVSEs.
                       Select(EVSE => EVSE.ToFeature()).
                       ToFeatureCollection(Properties);

        }

        #endregion


        #region ToFeature(this ChargingStationOperator, Properties = null)

        public static JObject ToFeature(this ChargingStationOperator  ChargingStationOperator,
                                        JObject                       Properties  = null)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator), "The given charging station operator must not be null!");

            #endregion

            #region Documentation

            // {
            //
            //   [...]
            //
            //   {
            //     "type": "Feature",
            //     "properties": {},
            //     "geometry": {
            //       "type": "Point",
            //       "coordinates": [
            //         10.579833984375,
            //         50.48197825997291
            //       ]
            //     }
            //   }
            //
            // }

            #endregion

            return JSONObject.Create(

                       new JProperty("type",  "Feature"),

                       new JProperty("properties",  Properties ?? new JObject(

                           new JProperty("LastChange",              ChargingStationOperator.LastChangeDate),
                           new JProperty("UserComment",             ""),
                           new JProperty("ServiceProviderComment",  ""),
                           new JProperty("Features",                new JArray()),
                           new JProperty("AuthorizationOptions",    new JArray()),
                           new JProperty("Photos",                  new JArray()),

                           new JProperty("EVSEs",                   new JObject(
                               ChargingStationOperator.EVSEs.Select(evse => new JProperty(evse.Id.ToString(), "lala"))
                           ))

                       )),

                       ChargingStationOperator.GeoLocation.HasValue
                           ? new JProperty("geometry",  new JObject(
                                                            new JProperty("type",         "Point"),
                                                            new JProperty("coordinates",  new JArray(
                                                                                              ChargingStationOperator.GeoLocation.Value.Longitude.Value,
                                                                                              ChargingStationOperator.GeoLocation.Value.Latitude. Value
                                                                                          ))
                                                        ))
                           : null
                   );

        }

        #endregion

        #region ToFeature(this ChargingPool,            Properties = null)

        public static JObject ToFeature(this ChargingPool  ChargingPool,
                                        JObject            Properties  = null)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool), "The given charging pool must not be null!");

            #endregion

            #region Documentation

            // {
            //
            //   [...]
            //
            //   {
            //     "type": "Feature",
            //     "properties": {},
            //     "geometry": {
            //       "type": "Point",
            //       "coordinates": [
            //         10.579833984375,
            //         50.48197825997291
            //       ]
            //     }
            //   }
            //
            // }

            #endregion

            return new JObject(

                       new JProperty("type",  "Feature"),

                       new JProperty("properties",  Properties ?? new JObject(

                           new JProperty("LastChange",              ChargingPool.LastChangeDate),
                           new JProperty("UserComment",             ""),
                           new JProperty("ServiceProviderComment",  ""),
                           new JProperty("Features",                new JArray()),
                           new JProperty("AuthorizationOptions",    new JArray()),
                           new JProperty("Photos",                  new JArray()),

                           new JProperty("EVSEs",                   new JObject(
                               ChargingPool.EVSEs.Select(evse => new JProperty(evse.Id.ToString(), "lala"))
                           ))

                       )),

                       new JProperty("geometry",  new JObject(
                                                      new JProperty("type",         "Point"),
                                                      new JProperty("coordinates",  new JArray(
                                                                                        ChargingPool.GeoLocation.Value.Longitude.Value,
                                                                                        ChargingPool.GeoLocation.Value.Latitude. Value
                                                                                    ))
                                                  ))
                   );

        }

        #endregion

        #region ToFeature(this ChargingStation,         Properties = null)

        public static JObject ToFeature(this ChargingStation  ChargingStation,
                                        JObject               Properties  = null)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation), "The given charging station must not be null!");

            #endregion

            #region Documentation

            // {
            //
            //   [...]
            //
            //   {
            //     "type": "Feature",
            //     "properties": {},
            //     "geometry": {
            //       "type": "Point",
            //       "coordinates": [
            //         10.579833984375,
            //         50.48197825997291
            //       ]
            //     }
            //   }
            //
            // }

            #endregion

            return JSONObject.Create(

                       new JProperty("type",  "Feature"),

                       new JProperty("properties",  Properties ?? JSONObject.Create(

                           new JProperty("@id",                     ChargingStation.Id.ToString()),
                           new JProperty("@context",                "https://open.charging.cloud/contexts/wwcp+geojson/ChargingStation"),
                           new JProperty("name",                    ChargingStation.Name.         ToJSON()),
                           new JProperty("description",             ChargingStation.Description.  ToJSON()),
                           new JProperty("lastChange",              ChargingStation.LastChangeDate),
                           new JProperty("adminStatus",             ChargingStation.AdminStatus.  ToJSON()),
                           new JProperty("status",                  ChargingStation.Status.       ToJSON()),
                           ChargingStation.SafeAny()
                               ? new JProperty("brand",             ChargingStation.Brands.       ToJSON())
                               : null,
                           new JProperty("address",                 ChargingStation.Address.      ToJSON()),

                           ChargingStation.OpeningTimes is not null
                               ? new JProperty("openingTimes",      ChargingStation.OpeningTimes. ToJSON())
                               : null,

                           new JProperty("accessibility",           ChargingStation.Accessibility.      ToString()),
                           new JProperty("authenticationModes",     ChargingStation.AuthenticationModes.ToJSON()),
                           new JProperty("paymentOptions",          ChargingStation.PaymentOptions.     ToJSON()),

                           new JProperty("EVSEs",                   new JArray(
                               ChargingStation.EVSEs.Select(evse => JSONObject.Create(
                                   new JProperty("Id",                  evse.Id.ToString()),
                                   new JProperty("AdminStatus",         evse.AdminStatus.   ToJSON()),
                                   new JProperty("Status",              evse.Status.        ToJSON()),
                                   new JProperty("RMSVoltage",          evse.RMSVoltage.ToString()),
                                   new JProperty("MaxCurrent",          evse.MaxCurrent.    ToString()),
                                   new JProperty("MaxPower",            evse.MaxPower.      ToString())
                                   //new JProperty("ChargingModes",       new JArray(evse.ChargingModes.Select(mode => mode.ToString())))
                               )
                           )))

                       )),

                       new JProperty("geometry",  JSONObject.Create(
                                                      new JProperty("type",         "Point"),
                                                      new JProperty("coordinates",  new JArray(
                                                                                        ChargingStation.GeoLocation.Value.Longitude.Value,
                                                                                        ChargingStation.GeoLocation.Value.Latitude. Value
                                                                                    ))
                                                  ))
                   );

        }

        #endregion

        #region ToFeature(this EVSE,                    Properties = null)

        public static JObject ToFeature(this EVSE  EVSE,
                                        InfoStatus ExpandRoamingNetworkId    = InfoStatus.ShowIdOnly,
                                        InfoStatus ExpandOperatorId          = InfoStatus.ShowIdOnly,
                                        InfoStatus ExpandChargingPoolId      = InfoStatus.ShowIdOnly,
                                        InfoStatus ExpandChargingStationId   = InfoStatus.ShowIdOnly,
                                        InfoStatus ExpandBrandIds            = InfoStatus.ShowIdOnly,
                                        InfoStatus ExpandDataLicenses        = InfoStatus.ShowIdOnly,
                                        JObject    Properties                = null)

        {

            #region Initial checks

            if (EVSE == null)
                throw new ArgumentNullException(nameof(EVSE), "The given EVSE must not be null!");

            #endregion

            #region Documentation

            // {
            //
            //   [...]
            //
            //   {
            //     "type": "Feature",
            //     "properties": {},
            //     "geometry": {
            //       "type": "Point",
            //       "coordinates": [
            //         10.579833984375,
            //         50.48197825997291
            //       ]
            //     }
            //   }
            //
            // }

            #endregion

            return JSONObject.Create(

                       new JProperty("type",  "Feature"),

                       new JProperty("properties",  Properties ?? JSONObject.Create(

                           new JProperty("@id",             EVSE.Id.ToString()),
                           new JProperty("@context",        "https://open.charging.cloud/contexts/wwcp+geojson/EVSE"),

                           EVSE.Description.IsNotNullOrEmpty()
                             ? new JProperty("description", EVSE.Description.ToJSON())
                             : null,

                           EVSE.Brands.SafeAny()
                               ? ExpandBrandIds.Switch(
                                     () => new JProperty("brandId",      EVSE.Brands.Select(brand => brand.Id.ToString())),
                                     () => new JProperty("brand",        EVSE.Brands.ToJSON()))
                               : null,

                           new JProperty("dataSource", EVSE.DataSource),

                           ExpandDataLicenses.Switch(
                               () => new JProperty("openDataLicenseIds",         new JArray(EVSE.DataLicenses.SafeSelect(license => license.Id.ToString()))),
                               () => new JProperty("openDataLicenses",           EVSE.DataLicenses.ToJSON())),

                           ExpandRoamingNetworkId.Switch(
                               () => new JProperty("roamingNetworkId",           EVSE.RoamingNetwork.Id. ToString()),
                               () => new JProperty("roamingNetwork",             EVSE.RoamingNetwork.    ToJSON(ExpandChargingStationOperatorIds:  InfoStatus.Hidden,
                                                                                                                ExpandChargingPoolIds:             InfoStatus.Hidden,
                                                                                                                ExpandChargingStationIds:          InfoStatus.Hidden,
                                                                                                                ExpandEVSEIds:                     InfoStatus.Hidden,
                                                                                                                ExpandBrandIds:                    InfoStatus.Hidden,
                                                                                                                ExpandDataLicenses:                InfoStatus.Hidden))),

                           ExpandOperatorId.Switch(
                               () => new JProperty("chargingStationOperatorId",  EVSE.Operator.Id.       ToString()),
                               () => new JProperty("chargingStationOperator",    EVSE.Operator.          ToJSON(Embedded:                          true,
                                                                                                                ExpandRoamingNetworkId:            InfoStatus.Hidden,
                                                                                                                ExpandChargingPoolIds:             InfoStatus.Hidden,
                                                                                                                ExpandChargingStationIds:          InfoStatus.Hidden,
                                                                                                                ExpandEVSEIds:                     InfoStatus.Hidden,
                                                                                                                ExpandBrandIds:                    InfoStatus.Hidden,
                                                                                                                ExpandDataLicenses:                InfoStatus.Hidden))),

                           ExpandChargingPoolId.Switch(
                               () => new JProperty("chargingPoolId",             EVSE.ChargingPool.Id.   ToString()),
                               () => new JProperty("chargingPool",               EVSE.ChargingPool.      ToJSON(Embedded:                          true,
                                                                                                                ExpandRoamingNetworkId:            InfoStatus.Hidden,
                                                                                                                ExpandChargingStationOperatorId:   InfoStatus.Hidden,
                                                                                                                ExpandChargingStationIds:          InfoStatus.Hidden,
                                                                                                                ExpandEVSEIds:                     InfoStatus.Hidden,
                                                                                                                ExpandBrandIds:                    InfoStatus.Hidden,
                                                                                                                ExpandDataLicenses:                InfoStatus.Hidden))),

                           ExpandChargingStationId.Switch(
                               () => new JProperty("chargingStationId",          EVSE.ChargingStation.Id.ToString()),
                               () => new JProperty("chargingStation",            EVSE.ChargingStation.   ToJSON(Embedded:                          true,
                                                                                                                ExpandRoamingNetworkId:            InfoStatus.Hidden,
                                                                                                                ExpandChargingStationOperatorId:   InfoStatus.Hidden,
                                                                                                                ExpandChargingPoolId:              InfoStatus.Hidden,
                                                                                                                ExpandBrandIds:                    InfoStatus.Hidden,
                                                                                                                ExpandDataLicenses:                InfoStatus.Hidden))),

                           new JProperty("geoLocation",         EVSE.ChargingStation.GeoLocation.Value.  ToJSON()),
                           new JProperty("address",             EVSE.ChargingStation.Address.            ToJSON()),
                           new JProperty("authenticationModes", EVSE.ChargingStation.AuthenticationModes.ToJSON()),

                           EVSE.ChargingModes.Any()
                               ? new JProperty("chargingModes",  new JArray(EVSE.ChargingModes.Select(chargingMode => chargingMode.ToText())))
                               : null,

                           new JProperty("currentTypes",   EVSE.CurrentType.ToText()),

                           EVSE.RMSVoltage.HasValue && EVSE.RMSVoltage.Value.Value > 0 ? new JProperty("averageVoltage",  String.Format("{0:0.00}", EVSE.RMSVoltage.Value.Value)) : null,
                           EVSE.MaxCurrent.    HasValue && EVSE.MaxCurrent.    Value.Value > 0 ? new JProperty("maxCurrent",      String.Format("{0:0.00}", EVSE.MaxCurrent.    Value.Value)) : null,
                           EVSE.MaxPower.      HasValue && EVSE.MaxPower.      Value.Value > 0 ? new JProperty("maxPower",        String.Format("{0:0.00}", EVSE.MaxPower.      Value.Value)) : null,
                           EVSE.MaxCapacity.   HasValue && EVSE.MaxCapacity.   Value.Value > 0 ? new JProperty("maxCapacity",     String.Format("{0:0.00}", EVSE.MaxCapacity.   Value.Value)) : null,

                           EVSE.ChargingConnectors.Count > 0
                               ? new JProperty("chargingConnectors",  new JArray(EVSE.ChargingConnectors.ToJSON()))
                               : null,

                           EVSE.EnergyMeter is not null
                               ? new JProperty("energyMeterId",       EVSE.EnergyMeter.Id)
                               : null,

                           EVSE.ChargingStation.OpeningTimes is not null
                               ? new JProperty("openingTimes",        EVSE.ChargingStation.OpeningTimes.ToJSON())
                               : null

                       )),

                       new JProperty("geometry",
                                     JSONObject.Create(
                                         new JProperty("type",         "Point"),
                                         new JProperty("coordinates",  new JArray(
                                                                           EVSE.ChargingStation.GeoLocation.Value.Longitude.Value,
                                                                           EVSE.ChargingStation.GeoLocation.Value.Latitude. Value
                                                                       ))
                                     ))
                   );

        }

        #endregion


        #region ToFeature(this ChargingStationOperator, PropertyCreator)

        public static JObject ToFeature(this ChargingStationOperator                           ChargingStationOperator,
                                        Func<ChargingStationOperator, IEnumerable<JProperty>>  PropertyCreator)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator),  "The given charging station operator must not be null!");

            if (PropertyCreator == null)
                throw new ArgumentNullException(nameof(PropertyCreator),          "The given JSON property creator delegate must not be null!");

            #endregion

            return ChargingStationOperator.ToFeature(new JObject(PropertyCreator(ChargingStationOperator)));

        }

        #endregion

        #region ToFeature(this ChargingPool,            PropertyCreator)

        public static JObject ToFeature(this ChargingPool                           ChargingPool,
                                        Func<ChargingPool, IEnumerable<JProperty>>  PropertyCreator)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool),     "The given charging pool must not be null!");

            if (PropertyCreator == null)
                throw new ArgumentNullException(nameof(PropertyCreator),  "The given JSON property creator delegate must not be null!");

            #endregion

            return ChargingPool.ToFeature(new JObject(PropertyCreator(ChargingPool)));

        }

        #endregion

        #region ToFeature(this ChargingStation,         PropertyCreator)

        public static JObject ToFeature(this ChargingStation                           ChargingStation,
                                        Func<ChargingStation, IEnumerable<JProperty>>  PropertyCreator)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation),  "The given charging station must not be null!");

            if (PropertyCreator == null)
                throw new ArgumentNullException(nameof(PropertyCreator),  "The given JSON property creator delegate must not be null!");

            #endregion

            return ChargingStation.ToFeature(new JObject(PropertyCreator(ChargingStation)));

        }

        #endregion


        #region Attach_GeoJSON_IO(this WWCPAPI, HTTPHostname = null, URIPrefix = "/")

        ///// <summary>
        ///// Attach GeoJSON I/O to the given WWCP HTTP API.
        ///// </summary>
        ///// <param name="WWCPAPI">A WWCP HTTP API.</param>
        ///// <param name="HTTPHostname">Limit this GeoJSON I/O handling to the given HTTP hostname.</param>
        ///// <param name="URIPrefix">A common URI prefix for all URIs within this API.</param>
        //public static void Attach_GeoJSON_IO(this WWCP_HTTPAPI  WWCPAPI,
        //                                     HTTPHostname?      HTTPHostname  = null,
        //                                     String             URIPrefix     = "/")
        //{
        //   // throw new NotImplementedException();
        //}

        #endregion


    }

}
