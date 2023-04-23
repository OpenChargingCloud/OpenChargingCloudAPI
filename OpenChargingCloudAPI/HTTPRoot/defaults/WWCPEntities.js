/*
 * Copyright (c) 2014-2023, GaphDefined GmbH <achim.friedland@graphdefined.com>
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
 * Copyright (c) 2014-2023, GaphDefined GmbH <achim.friedland@graphdefined.com>
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
    class RoamingNetwork {
    }
    WWCP.RoamingNetwork = RoamingNetwork;
    class EVSEOperator {
    }
    WWCP.EVSEOperator = EVSEOperator;
    class ChargingPool {
        get ChargingPoolId() { return this._ChargingPoolId; }
        get Name() { return this._Name; }
        get Description() { return this._Description; }
        get GeoLocation() { return this._GeoLocation; }
        get ChargingStations() { return this._ChargingStations; }
        constructor(JSON) {
            if (JSON !== undefined) {
                this._ChargingPoolId = JSON.hasOwnProperty("ChargingPoolId") ? JSON.ChargingPoolId : null;
                this._Name = JSON.hasOwnProperty("Name") ? new WWCP.I18NString(JSON.Name) : null;
                this._Description = JSON.hasOwnProperty("Description") ? new WWCP.I18NString(JSON.Description) : null;
                this._GeoLocation = JSON.hasOwnProperty("GeoLocation") ? WWCP.GeoCoordinate.Parse(JSON.GeoLocation) : null;
                this._ChargingStations = (JSON.hasOwnProperty("ChargingStations") &&
                    JSON.ChargingStations instanceof Array) ? JSON.ChargingStations.map((station, index, array) => new ChargingStation(station)) : null;
            }
        }
    }
    WWCP.ChargingPool = ChargingPool;
    class ChargingStation {
        get ChargingStationId() { return this._ChargingStationId; }
        get Name() { return this._Name; }
        get Description() { return this._Description; }
        get EVSEs() { return this._EVSEs; }
        constructor(JSON) {
            if (JSON !== undefined) {
                this._ChargingStationId = JSON.hasOwnProperty("ChargingStationId") ? JSON.ChargingStationId : null;
                this._Name = JSON.hasOwnProperty("Name") ? new WWCP.I18NString(JSON.Name) : null;
                this._Description = JSON.hasOwnProperty("Description") ? new WWCP.I18NString(JSON.Description) : null;
                this._EVSEs = (JSON.hasOwnProperty("EVSEs") &&
                    JSON.EVSEs instanceof Array) ? JSON.EVSEs.map((evse, index, array) => new EVSE(evse)) : null;
            }
        }
    }
    WWCP.ChargingStation = ChargingStation;
    class EVSE {
        get EVSEId() { return this._EVSEId; }
        get Description() { return this._Description; }
        get MaxPower() { return this._MaxPower; }
        get SocketOutlets() { return this._SocketOutlets; }
        constructor(JSON) {
            this._EVSEId = JSON.EVSEId;
            this._Description = new WWCP.I18NString(JSON.Description);
            this._MaxPower = JSON.MaxPower;
            this._SocketOutlets = JSON.SocketOutlets.map((socketOutlet, index, array) => new SocketOutlet(socketOutlet));
        }
    }
    WWCP.EVSE = EVSE;
    class SocketOutlet {
        get Plug() { return this._Plug; }
        get PlugImage() { return this._PlugImage; }
        constructor(JSON) {
            const prefix = "images/Ladestecker/";
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
    }
    WWCP.SocketOutlet = SocketOutlet;
    class EVSEStatusRecord {
        get EVSEId() { return this._EVSEId; }
        get EVSEStatus() { return this._EVSEStatus; }
        static Parse(EVSEId, JSON) {
            var status;
            for (var timestamp in JSON) {
                status = JSON[timestamp];
                break;
            }
            if (JSON !== undefined) {
                return new EVSEStatusRecord(EVSEId, status);
            }
        }
        constructor(EVSEId, EVSEStatus) {
            this._EVSEId = EVSEId;
            this._EVSEStatus = EVSEStatus;
        }
    }
    WWCP.EVSEStatusRecord = EVSEStatusRecord;
})(WWCP || (WWCP = {}));
//# sourceMappingURL=WWCPEntities.js.map