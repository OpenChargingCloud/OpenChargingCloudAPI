/*
 * Copyright (c) 2014-2026 GraphDefined GmbH <achim.friedland@graphdefined.com>
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

using NUnit.Framework;
using NUnit.Framework.Legacy;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

using cloud.charging.open.API;
using org.GraphDefined.Vanaheimr.Hermod.HTTPTest;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.UnitTests
{

    /// <summary>
    /// Abstract Unit Tests.
    /// </summary>
    public abstract class ATests
    {

        #region Data

        protected readonly IPv4Address           remoteAddress  = IPv4Address.Localhost;
        protected readonly IPPort                remotePort     = IPPort.Parse(8001);

        protected readonly HTTPTestServerX       httpServer;
        protected          OpenChargingCloudAPI  openChargingCloudAPI;
        protected readonly TimeSpan              timeout        = TimeSpan.FromSeconds(20);

        protected          DNSClient             dnsClient;
        protected          HTTPClient            httpClient;

        #endregion

        #region ATests()

        protected ATests()
        {

            dnsClient = new DNSClient(SearchForIPv6DNSServers: false);

            if (remoteAddress == IPv4Address.Localhost)
            {

                httpServer            = new HTTPTestServerX(
                                            TCPPort:         remotePort,
                                            DNSClient:       dnsClient,
                                            HTTPServerName:  "GraphDefined WWCP Unit Tests"
                                        );

                openChargingCloudAPI  = new OpenChargingCloudAPI(httpServer);

                httpServer.Start().GetAwaiter().GetResult();

            }

        }

        #endregion


        #region Init()

        [OneTimeSetUp]
        public void Init()
        {

            httpClient = new HTTPClient(
                             remoteAddress,
                             RemotePort: remotePort,
                             DNSClient:  dnsClient
                         );

        }

        #endregion


        #region Cleanup()

        [TearDown]
        public async Task Cleanup()
        {

            var      URI                = HTTPPath.Parse("/RNs");
            String[] RoamingNetworkIds  = null;

            using (var HTTPTask  = httpClient.Execute(client => client.GETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))

            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode);

                    RoamingNetworkIds = JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).
                                               AsEnumerable().
                                               Select(v => (v as JObject)["RoamingNetworkId"].Value<String>()).
                                               ToArray();

                }

            }


            foreach (var RoamingNetworkId in RoamingNetworkIds)
            {

                URI = HTTPPath.Parse("/RNs/" + RoamingNetworkId);

                using (var HTTPTask  = httpClient.Execute(client => client.DELETERequest(URI,
                                                                                          RequestBuilder: requestBuilder => {
                                                                                              requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                              requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                              requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                          }),
                                                                                           RequestTimeout: timeout,
                                                                                           CancellationToken: new CancellationTokenSource().Token))

                {

                    HTTPTask.Wait(timeout);

                    using (var HTTPResult = HTTPTask.Result)
                    {

                        ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode);

                    }

                }

            }



            if (remoteAddress == IPv4Address.Localhost)
                await httpServer.Stop();

        }

        #endregion

    }

}
