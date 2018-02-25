/*
 * Copyright (c) 2014-2018, GaphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of WWCP TypeScript Client <http://www.github.com/OpenCharingCloud/WWCP_TypedClient>
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
var WWCP;
/*
 * Copyright (c) 2014-2018, GaphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of WWCP TypeScript Client <http://www.github.com/OpenCharingCloud/WWCP_TypedClient>
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
(function (WWCP) {
    var RoamingNetwork = /** @class */ (function () {
        function RoamingNetwork() {
        }
        return RoamingNetwork;
    }());
    WWCP.RoamingNetwork = RoamingNetwork;
    var EVSEOperator = /** @class */ (function () {
        function EVSEOperator() {
        }
        return EVSEOperator;
    }());
    WWCP.EVSEOperator = EVSEOperator;
    var ChargingPool = /** @class */ (function () {
        function ChargingPool(JSON) {
            if (JSON !== undefined) {
                this._ChargingPoolId = JSON.hasOwnProperty("ChargingPoolId") ? JSON.ChargingPoolId : null;
                this._Name = JSON.hasOwnProperty("Name") ? new WWCP.I18NString(JSON.Name) : null;
                this._Description = JSON.hasOwnProperty("Description") ? new WWCP.I18NString(JSON.Description) : null;
                this._GeoLocation = JSON.hasOwnProperty("GeoLocation") ? WWCP.GeoCoordinate.Parse(JSON.GeoLocation) : null;
                this._ChargingStations = (JSON.hasOwnProperty("ChargingStations") &&
                    JSON.ChargingStations instanceof Array) ? JSON.ChargingStations.map(function (station, index, array) {
                    return new ChargingStation(station);
                }) : null;
            }
        }
        Object.defineProperty(ChargingPool.prototype, "ChargingPoolId", {
            get: function () { return this._ChargingPoolId; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingPool.prototype, "Name", {
            get: function () { return this._Name; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingPool.prototype, "Description", {
            get: function () { return this._Description; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingPool.prototype, "GeoLocation", {
            get: function () { return this._GeoLocation; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingPool.prototype, "ChargingStations", {
            get: function () { return this._ChargingStations; },
            enumerable: true,
            configurable: true
        });
        return ChargingPool;
    }());
    WWCP.ChargingPool = ChargingPool;
    var ChargingStation = /** @class */ (function () {
        function ChargingStation(JSON) {
            if (JSON !== undefined) {
                this._ChargingStationId = JSON.hasOwnProperty("ChargingStationId") ? JSON.ChargingStationId : null;
                this._Name = JSON.hasOwnProperty("Name") ? new WWCP.I18NString(JSON.Name) : null;
                this._Description = JSON.hasOwnProperty("Description") ? new WWCP.I18NString(JSON.Description) : null;
                this._EVSEs = (JSON.hasOwnProperty("EVSEs") &&
                    JSON.EVSEs instanceof Array) ? JSON.EVSEs.map(function (evse, index, array) {
                    return new EVSE(evse);
                }) : null;
            }
        }
        Object.defineProperty(ChargingStation.prototype, "ChargingStationId", {
            get: function () { return this._ChargingStationId; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingStation.prototype, "Name", {
            get: function () { return this._Name; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingStation.prototype, "Description", {
            get: function () { return this._Description; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ChargingStation.prototype, "EVSEs", {
            get: function () { return this._EVSEs; },
            enumerable: true,
            configurable: true
        });
        return ChargingStation;
    }());
    WWCP.ChargingStation = ChargingStation;
    var EVSE = /** @class */ (function () {
        function EVSE(JSON) {
            this._EVSEId = JSON.EVSEId;
            this._Description = new WWCP.I18NString(JSON.Description);
            this._MaxPower = JSON.MaxPower;
            this._SocketOutlets = JSON.SocketOutlets.map(function (socketOutlet, index, array) {
                return new SocketOutlet(socketOutlet);
            });
        }
        Object.defineProperty(EVSE.prototype, "EVSEId", {
            get: function () { return this._EVSEId; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(EVSE.prototype, "Description", {
            get: function () { return this._Description; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(EVSE.prototype, "MaxPower", {
            get: function () { return this._MaxPower; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(EVSE.prototype, "SocketOutlets", {
            get: function () { return this._SocketOutlets; },
            enumerable: true,
            configurable: true
        });
        return EVSE;
    }());
    WWCP.EVSE = EVSE;
    var SocketOutlet = /** @class */ (function () {
        function SocketOutlet(JSON) {
            var prefix = "images/Ladestecker/";
            switch (JSON.Plug) {
                case "TypeFSchuko":
                    this._Plug = WWCP.SocketTypes.TypeFSchuko;
                    this._PlugImage = prefix + "Schuko.svg";
                    break;
                case "Type2Outlet":
                    this._Plug = WWCP.SocketTypes.Type2Outlet;
                    this._PlugImage = prefix + "IEC_Typ_2.svg";
                    break;
                case "Type2Connector_CableAttached":
                    this._Plug = WWCP.SocketTypes.Type2Outlet;
                    this._PlugImage = prefix + "IEC_Typ_2_Cable.svg";
                    break;
                case "CHAdeMO":
                    this._Plug = WWCP.SocketTypes.CHAdeMO;
                    this._PlugImage = prefix + "CHAdeMO.svg";
                    break;
                case "CCSCombo2Plug_CableAttached":
                    this._Plug = WWCP.SocketTypes.CCSCombo2Plug_CableAttached;
                    this._PlugImage = prefix + "CCS_Typ_2.svg";
                    break;
                default:
                    this._Plug = WWCP.SocketTypes.unknown;
                    this._PlugImage = "";
                    break;
            }
        }
        Object.defineProperty(SocketOutlet.prototype, "Plug", {
            get: function () { return this._Plug; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SocketOutlet.prototype, "PlugImage", {
            get: function () { return this._PlugImage; },
            enumerable: true,
            configurable: true
        });
        return SocketOutlet;
    }());
    WWCP.SocketOutlet = SocketOutlet;
    var EVSEStatusRecord = /** @class */ (function () {
        function EVSEStatusRecord(EVSEId, EVSEStatus) {
            this._EVSEId = EVSEId;
            this._EVSEStatus = EVSEStatus;
        }
        Object.defineProperty(EVSEStatusRecord.prototype, "EVSEId", {
            get: function () { return this._EVSEId; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(EVSEStatusRecord.prototype, "EVSEStatus", {
            get: function () { return this._EVSEStatus; },
            enumerable: true,
            configurable: true
        });
        EVSEStatusRecord.Parse = function (EVSEId, JSON) {
            var status;
            for (var timestamp in JSON) {
                status = JSON[timestamp];
                break;
            }
            if (JSON !== undefined) {
                return new EVSEStatusRecord(EVSEId, status);
            }
        };
        return EVSEStatusRecord;
    }());
    WWCP.EVSEStatusRecord = EVSEStatusRecord;
})(WWCP || (WWCP = {}));
//# sourceMappingURL=WWCPEntities.js.map