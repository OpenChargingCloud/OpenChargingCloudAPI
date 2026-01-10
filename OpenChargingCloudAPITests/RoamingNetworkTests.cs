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
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace cloud.charging.open.protocols.WWCP.Net.UnitTests
{

    /// <summary>
    /// Roaming Network WWCP HTTP Unit Tests.
    /// </summary>
    [TestFixture]
    public class RoamingNetworkTests : ATests
    {

        #region CREATE()

        [Test]
        public void CREATE()
        {

            #region Verify GET   /RNs

            var URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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


            #region CREATE /RNs/TestRN1

            URI = HTTPPath.Parse("/RNs/TestRN1");

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

            if (remoteAddress == IPv4Address.Localhost)
            {

                ClassicAssert.IsTrue(openChargingCloudAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1")), "Roaming network 'TestRN1' was not found via .NET API!");

                var _TestRN1 = openChargingCloudAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1"));
                ClassicAssert.IsNotNull(_TestRN1, "Roaming network 'TestRN1' was not returned via .NET API!");
                ClassicAssert.IsFalse  (_TestRN1.Description.Any(), "The description of roaming network 'TestRN1' must be empty!");

            }

            #endregion


            #region Verify GET   /RNs

            URI = HTTPPath.Parse("/RNs");

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
                    ClassicAssert.AreEqual(new JArray(
                                        new JObject(
                                            new JProperty("RoamingNetworkId",  "TestRN1"),
                                            new JProperty("description",       new JObject())
                                        )
                                    ).ToString(),
                                    JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'GET " + URI + "'!");

                }

            }

            #endregion

            #region Verify COUNT /RNs

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
                    ClassicAssert.AreEqual(new JObject(new JProperty("count", 1)).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'COUNT " + URI + "'!");

                }

            }

            #endregion


            #region CREATE /RNs/TestRN3

            URI = HTTPPath.Parse("/RNs/TestRN3");

            using (var HTTPTask  = httpClient.Execute(client => client.CREATERequest(URI,
                                                                                      RequestBuilder: requestBuilder => {
                                                                                          requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                          requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                          requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                          requestBuilder.Content      = JSONObject.Create(
                                                                                                                            new JProperty("description", JSONObject.Create(
                                                                                                                                new JProperty("en", "This is a roaming network!")
                                                                                                                            ))
                                                                                                                        ).ToUTF8Bytes();
                                                                                      }),
                                                                                       RequestTimeout: timeout,
                                                                                       CancellationToken: new CancellationTokenSource().Token))

            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("RoamingNetworkId",  "TestRN3"),
                                        new JProperty("description",       new JObject(
                                                                               new JProperty("en", "This is a roaming network!")
                                                                           ))
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'CREATE " + URI + "'!");

                }

            }

            if (remoteAddress == IPv4Address.Localhost)
            {

                ClassicAssert.IsTrue(openChargingCloudAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN3")), "Roaming network 'TestRN3' was not found via .NET API!");

                var _TestRN3 = openChargingCloudAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN3"));
                ClassicAssert.IsNotNull(_TestRN3, "Roaming network 'TestRN3' was not returned via .NET API!");
                ClassicAssert.IsTrue   (_TestRN3.Description.Any(), "The description of roaming network 'TestRN3' must not be empty!");
                ClassicAssert.AreEqual (_TestRN3.Description.Count(), 1);
                ClassicAssert.AreEqual (_TestRN3.Description[Languages.en], "This is a roaming network!");

            }

            #endregion

            #region CREATE /RNs/TestRN2

            URI = HTTPPath.Parse("/RNs/TestRN2");

            using (var HTTPTask  = httpClient.Execute(client => client.CREATERequest(URI,
                                                                                      RequestBuilder: requestBuilder => {
                                                                                          requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                          requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                          requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                          requestBuilder.Content      = JSONObject.Create(
                                                                                                                            new JProperty("description", JSONObject.Create(
                                                                                                                                new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
                                                                                                                                new JProperty("en", "This is another roaming network!")
                                                                                                                            ))
                                                                                                                        ).ToUTF8Bytes();
                                                                                      }),
                                                                                       RequestTimeout: timeout,
                                                                                       CancellationToken: new CancellationTokenSource().Token))

            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("RoamingNetworkId",  "TestRN2"),
                                        new JProperty("description",       new JObject(
                                                                               new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
                                                                               new JProperty("en", "This is another roaming network!")
                                                                           ))
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'CREATE " + URI + "'!");

                }

            }

            if (remoteAddress == IPv4Address.Localhost)
            {

                ClassicAssert.IsTrue(openChargingCloudAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN2")), "Roaming network 'TestRN2' was not found via .NET API!");

                var _TestRN2 = openChargingCloudAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN2"));
                ClassicAssert.IsNotNull(_TestRN2, "Roaming network 'TestRN2' was not returned via .NET API!");
                ClassicAssert.IsTrue   (_TestRN2.Description.Any(), "The description of roaming network 'TestRN2' must not be empty!");
                ClassicAssert.AreEqual (_TestRN2.Description.Count(), 2);
                ClassicAssert.AreEqual (_TestRN2.Description[Languages.de], "Auch ein schönes Roaming Netzwerk!");
                ClassicAssert.AreEqual (_TestRN2.Description[Languages.en], "This is another roaming network!");

            }

            #endregion


            #region Verify GET   /RNs

            URI = HTTPPath.Parse("/RNs");

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
                    ClassicAssert.AreEqual(new JArray(
                                        new JObject(
                                            new JProperty("RoamingNetworkId",  "TestRN1"),
                                            new JProperty("description",       new JObject())
                                        ),
                                        new JObject(
                                            new JProperty("RoamingNetworkId",  "TestRN2"),
                                            new JProperty("description",       JSONObject.Create(
                                                                                   new JProperty("de", "Auch ein schönes Roaming Netzwerk!"),
                                                                                   new JProperty("en", "This is another roaming network!")
                                                                               ))
                                        ),
                                        new JObject(
                                            new JProperty("RoamingNetworkId",  "TestRN3"),
                                            new JProperty("description",       JSONObject.Create(
                                                                                   new JProperty("en", "This is a roaming network!"))
                                                                               )
                                        )
                                    ).ToString(),
                                    JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'GET " + URI + "'!");

                }

            }

            #endregion

            #region Verify COUNT /RNs

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
                    ClassicAssert.AreEqual(new JObject(new JProperty("count", 3)).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'COUNT " + URI + "'!");

                }

            }

            #endregion

        }

        #endregion

        #region CREATE_InvalidRoamingNetworkDescription()

        [Test]
        public void CREATE_InvalidRoamingNetworkDescription()
        {

            #region Verify GET   /RNs

            var URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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


            #region CREATE /RNs/TestRN3

            URI = HTTPPath.Parse("/RNs/TestRN3");

            using (var HTTPTask  = httpClient.Execute(client => client.CREATERequest(URI,
                                                                                      RequestBuilder: requestBuilder => {
                                                                                          requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                          requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                          requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                          requestBuilder.Content      = JSONObject.Create(
                                                                                                                            new JProperty("description", "This is an illegal text!")
                                                                                                                        ).ToUTF8Bytes();
                                                                                      }),
                                                                                       RequestTimeout: timeout,
                                                                                       CancellationToken: new CancellationTokenSource().Token))
            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.BadRequest, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' did not fail! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("description", "Invalid roaming network description!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'CREATE " + URI + "'!");

                }

            }

            if (remoteAddress == IPv4Address.Localhost)
            {

                ClassicAssert.IsFalse(openChargingCloudAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN3")), "Roaming network 'TestRN3' should not exist via .NET API!");

            }

            #endregion


            #region Verify GET   /RNs

            URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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

        }

        #endregion

        #region CREATE_DuplicateRoamingNetworkId()

        [Test]
        public void CREATE_DuplicateRoamingNetworkId()
        {

            #region Verify GET   /RNs

            var URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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


            #region CREATE /RNs/TestRN1

            URI = HTTPPath.Parse("/RNs/TestRN1");

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

            if (remoteAddress == IPv4Address.Localhost)
            {

                ClassicAssert.IsTrue(openChargingCloudAPI.RoamingNetworkExists(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1")), "Roaming network 'TestRN1' was not found via .NET API!");

                var _TestRN1 = openChargingCloudAPI.GetRoamingNetwork(HTTPHostname.Localhost, RoamingNetwork_Id.Parse("TestRN1"));
                ClassicAssert.IsNotNull(_TestRN1, "Roaming network 'TestRN1' was not returned via .NET API!");
                ClassicAssert.IsFalse  (_TestRN1.Description.Any(), "The description of roaming network 'TestRN1' must be empty!");

            }

            #endregion

            #region CREATE /RNs/TestRN1

            URI = HTTPPath.Parse("/RNs/TestRN1");

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

                    ClassicAssert.AreEqual(HTTPStatusCode.Conflict, HTTPResult.HTTPStatusCode, "'CREATE " + URI + "' did not fail! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("description", "RoamingNetworkId already exists!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'CREATE " + URI + "'!");

                }

            }

            #endregion

        }

        #endregion

        #region DELETE_ButDoesNotExists()

        [Test]
        public void DELETE_ButDoesNotExists()
        {

            #region Verify GET   /RNs

            var URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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

            #region DELETE /RNs/TestRN1

            URI = HTTPPath.Parse("/RNs/_DoesNotExists");

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

                    ClassicAssert.AreEqual(HTTPStatusCode.NotFound, HTTPResult.HTTPStatusCode, "'DELETE " + URI + "' did not fail! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("description",  "Unknown RoamingNetworkId!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'DELETE " + URI + "'!");

                }

            }

            #endregion

        }

        #endregion

        #region SETandGET_Properties()

        [Test]
        public void SETandGET_Properties()
        {

            #region Verify GET   /RNs

            var URI = HTTPPath.Parse("/RNs");

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

            #region Verify COUNT /RNs

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

            #region CREATE /RNs/TestRN1

            URI = HTTPPath.Parse("/RNs/TestRN1");

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


            #region GET /RNs/TestRN1/UndefinedProperty

            URI = HTTPPath.Parse("/RNs/TestRN1/UndefinedProperty");

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
                    ClassicAssert.AreEqual(HTTPStatusCode.NotFound, HTTPResult.HTTPStatusCode, "'GET " + URI + "' did not fail! " + HTTPResult.HTTPBody.ToUTF8String());
                }

            }

            #endregion

            #region SET /RNs/TestRN1/UndefinedProperty

            using (var HTTPTask  = httpClient.Execute(client => client.SETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                       requestBuilder.Content      = JSONObject.Create(
                                                                                                                         new JProperty("oldValue", ""),
                                                                                                                         new JProperty("newValue", "Test123!")
                                                                                                                     ).ToUTF8Bytes();
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))
            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.Created, HTTPResult.HTTPStatusCode, "'SET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("oldValue",  ""),
                                        new JProperty("newValue",  "Test123!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'SET " + URI + "'!");

                }

            }

            #endregion

            #region GET /RNs/TestRN1/UndefinedProperty

            URI = HTTPPath.Parse("/RNs/TestRN1/UndefinedProperty");

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
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("UndefinedProperty", "Test123!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'GET " + URI + "'!");
                }

            }

            #endregion

            #region GET /RNs/TestRN1/UndefinedProperty

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
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("UndefinedProperty", "Test123!")
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'GET " + URI + "'!");
                }

            }

            #endregion

            #region SET /RNs/TestRN1/UndefinedProperty

            using (var HTTPTask  = httpClient.Execute(client => client.SETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                       requestBuilder.Content      = JSONObject.Create(
                                                                                                                         new JProperty("oldValue", "Test123!"),
                                                                                                                         new JProperty("newValue", "Noch ein Test!")
                                                                                                                     ).ToUTF8Bytes();
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))
            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'SET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    //ClassicAssert.AreEqual(new JObject(
                    //                    new JProperty("description",  "Unknown RoamingNetworkId!")
                    //                ).ToString(),
                    //                JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                    //                "Invalid response for 'DELETE " + URI + "'!");

                }

            }

            #endregion

            #region SET /RNs/TestRN1/UndefinedProperty

            using (var HTTPTask  = httpClient.Execute(client => client.SETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                       requestBuilder.Content      = JSONObject.Create(
                                                                                                                         new JProperty("oldValue", "Test123!"),
                                                                                                                         new JProperty("newValue", "Noch ein Test!")
                                                                                                                     ).ToUTF8Bytes();
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))

            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.Conflict, HTTPResult.HTTPStatusCode, "'SET " + URI + "' did not fail! " + HTTPResult.HTTPBody.ToUTF8String());
                    //ClassicAssert.AreEqual(new JObject(
                    //                    new JProperty("description",  "Unknown RoamingNetworkId!")
                    //                ).ToString(),
                    //                JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                    //                "Invalid response for 'DELETE " + URI + "'!");

                }

            }

            #endregion


            #region SET /RNs/TestRN1/UndefinedProperty

            using (var HTTPTask  = httpClient.Execute(client => client.SETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                       requestBuilder.Content      = JSONObject.Create(
                                                                                                                         new JProperty("oldValue",  "Noch ein Test!"),
                                                                                                                         new JProperty("newValue",  new JObject(new JProperty("a", "b")))
                                                                                                                     ).ToUTF8Bytes();
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))
            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'SET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("oldValue",  "Noch ein Test!"),
                                        new JProperty("newValue",  new JObject(new JProperty("a", "b")))
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'DELETE " + URI + "'!");

                }

            }

            #endregion

            #region SET /RNs/TestRN1/UndefinedProperty

            using (var HTTPTask  = httpClient.Execute(client => client.SETRequest(URI,
                                                                                   RequestBuilder: requestBuilder => {
                                                                                       requestBuilder.Host         = HTTPHostname.Localhost;
                                                                                       requestBuilder.ContentType  = HTTPContentType.Application.JSON_UTF8;
                                                                                       requestBuilder.Accept.Add(HTTPContentType.Application.JSON_UTF8);
                                                                                       requestBuilder.Content      = JSONObject.Create(
                                                                                                                         new JProperty("oldValue",  new JObject(new JProperty("a", "b"))),
                                                                                                                         new JProperty("newValue",  new JArray("1", "2", "3"))
                                                                                                                     ).ToUTF8Bytes();
                                                                                   }),
                                                                                    RequestTimeout: timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))
            {

                HTTPTask.Wait(timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    ClassicAssert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode, "'SET " + URI + "' failed! " + HTTPResult.HTTPBody.ToUTF8String());
                    ClassicAssert.AreEqual(new JObject(
                                        new JProperty("oldValue",  new JObject(new JProperty("a", "b"))),
                                        new JProperty("newValue",  new JArray("1", "2", "3"))
                                    ).ToString(),
                                    JObject.Parse(HTTPResult.HTTPBody.ToUTF8String()).ToString(),
                                    "Invalid response for 'DELETE " + URI + "'!");

                }

            }

            #endregion

        }

        #endregion

    }

}
