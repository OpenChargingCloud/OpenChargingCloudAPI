/*
 * Copyright (c) 2014-2019, GaphDefined GmbH <achim.friedland@graphdefined.com>
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
if (typeof (Number.prototype.toRad) === "undefined") {
    Number.prototype.toRad = function () {
        return this * Math.PI / 180;
    };
}
var WWCP;
(function (WWCP) {
    //#region Enums
    var SocketTypes;
    (function (SocketTypes) {
        SocketTypes[SocketTypes["unknown"] = 0] = "unknown";
        SocketTypes[SocketTypes["TypeFSchuko"] = 1] = "TypeFSchuko";
        SocketTypes[SocketTypes["Type2Outlet"] = 2] = "Type2Outlet";
        SocketTypes[SocketTypes["CHAdeMO"] = 3] = "CHAdeMO";
        SocketTypes[SocketTypes["CCSCombo2Plug_CableAttached"] = 4] = "CCSCombo2Plug_CableAttached";
    })(SocketTypes = WWCP.SocketTypes || (WWCP.SocketTypes = {}));
    var EVSEStatusTypes;
    (function (EVSEStatusTypes) {
        EVSEStatusTypes[EVSEStatusTypes["unknown"] = 0] = "unknown";
        EVSEStatusTypes[EVSEStatusTypes["available"] = 1] = "available";
        EVSEStatusTypes[EVSEStatusTypes["reserved"] = 2] = "reserved";
        EVSEStatusTypes[EVSEStatusTypes["charging"] = 3] = "charging";
    })(EVSEStatusTypes = WWCP.EVSEStatusTypes || (WWCP.EVSEStatusTypes = {}));
    //#endregion
    //#region General data types...
    var I18NString = /** @class */ (function () {
        function I18NString(JSON) {
            if (JSON !== undefined) {
                this._de = JSON.hasOwnProperty("de") ? JSON.de : "";
                this._en = JSON.hasOwnProperty("en") ? JSON.en : "";
                this._fr = JSON.hasOwnProperty("fr") ? JSON.fr : "";
            }
        }
        Object.defineProperty(I18NString.prototype, "de", {
            get: function () { return this._de; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(I18NString.prototype, "en", {
            get: function () { return this._en; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(I18NString.prototype, "fr", {
            get: function () { return this._fr; },
            enumerable: true,
            configurable: true
        });
        return I18NString;
    }());
    WWCP.I18NString = I18NString;
    /**
    * A geo coordinate
    * @class WWCP.GeoCoordinate
    */
    var GeoCoordinate = /** @class */ (function () {
        /**
        * Create a new geo coordinate.
        * @param {number} Latitude A geo latitude.
        * @param {number} Longitude A geo longitude.
        */
        function GeoCoordinate(Latitude, Longitude) {
            this._lat = Latitude;
            this._lng = Longitude;
        }
        Object.defineProperty(GeoCoordinate.prototype, "lat", {
            get: function () { return this._lat; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(GeoCoordinate.prototype, "lng", {
            get: function () { return this._lng; },
            enumerable: true,
            configurable: true
        });
        GeoCoordinate.Parse = function (JSON) {
            if (JSON !== undefined) {
                return new GeoCoordinate(JSON.hasOwnProperty("lat") ? JSON.lat : 0, JSON.hasOwnProperty("lng") ? JSON.lng : 0);
            }
        };
        /**
        * Returns the distance to the given geo coordinate in km.
        * @param  {GeoCoordinate} Target A geo coordinate.
        * @param  {number} Decimals Number of decimals of the result.
        * @returns {number} the distance to the given geo coordinate in km.
        */
        GeoCoordinate.prototype.DistanceTo = function (Target, Decimals) {
            Decimals = Decimals || 8;
            var earthRadius = 6371; // km
            var dLat = (Target.lat - this._lat).toRad();
            var dLon = (Target.lng - this._lng).toRad();
            var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) + Math.sin(dLon / 2) * Math.sin(dLon / 2) * Math.cos(this._lat.toRad()) * Math.cos(Target.lat.toRad());
            var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
            var d = earthRadius * c;
            return Math.round(d * Math.pow(10, Decimals)) / Math.pow(10, Decimals);
        };
        return GeoCoordinate;
    }());
    WWCP.GeoCoordinate = GeoCoordinate;
    //#endregion
})(WWCP || (WWCP = {}));
//# sourceMappingURL=WWCPDatatypes.js.map