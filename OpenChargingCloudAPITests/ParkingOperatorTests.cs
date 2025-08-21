/*
 * Copyright (c) 2014-2025 GraphDefined GmbH <achim.friedland@graphdefined.com>
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
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.UnitTests
{

    /// <summary>
    /// E-Parking Operator WWCP HTTP Unit Tests.
    /// </summary>
    [TestFixture]
    public class ParkingOperatorTests : ATests
    {

        #region Init()

        [OneTimeSetUp]
        public void Init()
        {

            #region CREATE /RNs/TestRN1

            var URI = HTTPPath.Parse("/RNs/TestRN1");

            using (var HTTPTask  = httpClient.Execute(client => client.CREATERequest(URI,
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

                    ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("RoamingNetworkId",  "TestRN1"),
                                        new JProperty("description",       new JObject())
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'CREATE " + URI + "'!");

                }

            }

            #endregion

        }

        #endregion

        #region CREATE()

        [Test]
        public void CREATE()
        {

            #region Verify GET   /RNs/TestRN1/ParkingOperators

            var URI = HTTPPath.Parse("/RNs/TestRN1/ParkingOperators");

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

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JArray().ToString(),
                                    JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'GET " + URI + "'!");

                }

            }

            #endregion

            #region Verify COUNT /RNs/TestRN1/ParkingOperators

            using (var HTTPTask  = httpClient.Execute(client => client.COUNTRequest(URI,
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

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(new JProperty("count", 0)).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'COUNT " + URI + "'!");

                }

            }

            #endregion


            //#region CREATE /RNs/TestRN1

            //URI = "/RNs/TestRN1";

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.CREATE(URI,
            //                                                                  RequestBuilder: requestBuilder => {
            //                                                                      requestBuilder.Host         = "localhost";
            //                                                                      requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                      requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                                  }),
            //                                                                   RequestTimeout: Timeout,
            //                                                                   CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JObject(
            //                            new JProperty("RoamingNetworkId",  "TestRN1"),
            //                            new JProperty("description",       new JObject())
            //                        ).ToString(),
            //                        JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'CREATE " + URI + "'!");

            //    }

            //}

            //if (RemoteAddress == IPv4Address.Localhost)
            //{

            //    ClassicAssert.IsTrue(WWCPAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1")), "Roaming network 'TestRN1' was not found via .NET API!");

            //    var _TestRN1 = WWCPAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1"));
            //    ClassicAssert.IsNotNull(_TestRN1, "Roaming network 'TestRN1' was not returned via .NET API!");
            //    ClassicAssert.IsFalse  (_TestRN1.Description.Any(), "The description of roaming network 'TestRN1' must be empty!");

            //}

            //#endregion


            //#region Verify GET   /RNs

            //URI = "/RNs";

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.GET(URI,
            //                                                               RequestBuilder: requestBuilder => {
            //                                                                   requestBuilder.Host         = "localhost";
            //                                                                   requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                   requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                               }),
            //                                                                RequestTimeout: Timeout,
            //                                                                CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JArray(
            //                            new JObject(
            //                                new JProperty("RoamingNetworkId",  "TestRN1"),
            //                                new JProperty("description",       new JObject())
            //                            )
            //                        ).ToString(),
            //                        JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'GET " + URI + "'!");

            //    }

            //}

            //#endregion

            //#region Verify COUNT /RNs

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.COUNT(URI,
            //                                                                 RequestBuilder: requestBuilder => {
            //                                                                     requestBuilder.Host         = "localhost";
            //                                                                     requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                     requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                                 }),
            //                                                                  RequestTimeout: Timeout,
            //                                                                  CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JObject(new JProperty("count", 1)).ToString(),
            //                        JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'COUNT " + URI + "'!");

            //    }

            //}

            //#endregion


            //#region CREATE /RNs/TestRN3

            //URI = "/RNs/TestRN3";

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.CREATE(URI,
            //                                                                  RequestBuilder: requestBuilder => {
            //                                                                      requestBuilder.Host         = "localhost";
            //                                                                      requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                      requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                                      requestBuilder.Content      = JSONObject.Create(
            //                                                                                                        new JProperty("description", JSONObject.Create(
            //                                                                                                            new JProperty("en", "This is a roaming network!")
            //                                                                                                        ))
            //                                                                                                    ).ToUTF8Bytes();
            //                                                                  }),
            //                                                                   RequestTimeout: Timeout,
            //                                                                   CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JObject(
            //                            new JProperty("RoamingNetworkId",  "TestRN3"),
            //                            new JProperty("description",       new JObject(
            //                                                                   new JProperty("en", "This is a roaming network!")
            //                                                               ))
            //                        ).ToString(),
            //                        JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'CREATE " + URI + "'!");

            //    }

            //}

            //if (RemoteAddress == IPv4Address.Localhost)
            //{

            //    ClassicAssert.IsTrue(WWCPAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN3")), "Roaming network 'TestRN3' was not found via .NET API!");

            //    var _TestRN3 = WWCPAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN3"));
            //    ClassicAssert.IsNotNull(_TestRN3, "Roaming network 'TestRN3' was not returned via .NET API!");
            //    ClassicAssert.IsTrue   (_TestRN3.Description.Any(), "The description of roaming network 'TestRN3' must not be empty!");
            //    ClassicAssert.AreEqual (_TestRN3.Description.Count(), 1);
            //    ClassicAssert.AreEqual (_TestRN3.Description[Languages.en], "This is a roaming network!");

            //}

            //#endregion

            //#region CREATE /RNs/TestRN2

            //URI = "/RNs/TestRN2";

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.CREATE(URI,
            //                                                                  RequestBuilder: requestBuilder => {
            //                                                                      requestBuilder.Host         = "localhost";
            //                                                                      requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                      requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                                      requestBuilder.Content      = JSONObject.Create(
            //                                                                                                        new JProperty("description", JSONObject.Create(
            //                                                                                                            new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
            //                                                                                                            new JProperty("en", "This is another roaming network!")
            //                                                                                                        ))
            //                                                                                                    ).ToUTF8Bytes();
            //                                                                  }),
            //                                                                   RequestTimeout: Timeout,
            //                                                                   CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JObject(
            //                            new JProperty("RoamingNetworkId",  "TestRN2"),
            //                            new JProperty("description",       new JObject(
            //                                                                   new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
            //                                                                   new JProperty("en", "This is another roaming network!")
            //                                                               ))
            //                        ).ToString(),
            //                        JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'CREATE " + URI + "'!");

            //    }

            //}

            //if (RemoteAddress == IPv4Address.Localhost)
            //{

            //    ClassicAssert.IsTrue(WWCPAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN2")), "Roaming network 'TestRN2' was not found via .NET API!");

            //    var _TestRN2 = WWCPAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN2"));
            //    ClassicAssert.IsNotNull(_TestRN2, "Roaming network 'TestRN2' was not returned via .NET API!");
            //    ClassicAssert.IsTrue   (_TestRN2.Description.Any(), "The description of roaming network 'TestRN2' must not be empty!");
            //    ClassicAssert.AreEqual (_TestRN2.Description.Count(), 2);
            //    ClassicAssert.AreEqual (_TestRN2.Description[Languages.de], "Auch ein schönes Roaming Netzwerk!");
            //    ClassicAssert.AreEqual (_TestRN2.Description[Languages.en], "This is another roaming network!");

            //}

            //#endregion


            //#region Verify GET   /RNs

            //URI = "/RNs";

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.GET(URI,
            //                                                               RequestBuilder: requestBuilder => {
            //                                                                   requestBuilder.Host         = "localhost";
            //                                                                   requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                   requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                               }),
            //                                                                RequestTimeout: Timeout,
            //                                                                CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JArray(
            //                            new JObject(
            //                                new JProperty("RoamingNetworkId",  "TestRN1"),
            //                                new JProperty("description",       new JObject())
            //                            ),
            //                            new JObject(
            //                                new JProperty("RoamingNetworkId",  "TestRN2"),
            //                                new JProperty("description",       JSONObject.Create(
            //                                                                       new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
            //                                                                       new JProperty("en", "This is another roaming network!")
            //                                                                   ))
            //                            ),
            //                            new JObject(
            //                                new JProperty("RoamingNetworkId",  "TestRN3"),
            //                                new JProperty("description",       JSONObject.Create(
            //                                                                       new JProperty("en", "This is a roaming network!"))
            //                                                                   )
            //                            )
            //                        ).ToString(),
            //                        JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'GET " + URI + "'!");

            //    }

            //}

            //#endregion

            //#region Verify COUNT /RNs

            //using (var HTTPTask  = _HTTPClient.Execute(client => client.COUNT(URI,
            //                                                                 RequestBuilder: requestBuilder => {
            //                                                                     requestBuilder.Host         = "localhost";
            //                                                                     requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
            //                                                                     requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
            //                                                                 }),
            //                                                                  RequestTimeout: Timeout,
            //                                                                  CancellationToken: new CancellationTokenSource().Token))

            //{

            //    HTTPTask.Wait(Timeout);

            //    using (var HTTPResult = HTTPTask.Result)
            //    {

            //        ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'GET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
            //        ClassicAssert.AreEqual(new JObject(new JProperty("count", 3)).ToString(),
            //                        JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
            //                        "Invalid response for 'COUNT " + URI + "'!");

            //    }

            //}

            //#endregion

        }

        #endregion

    }

}
