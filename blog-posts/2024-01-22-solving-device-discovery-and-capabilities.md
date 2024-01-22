# Solving device discovery and capabilities

The challenge of device discovery and capabilities arises from the increasing diversity of embedded devices in modern technology ecosystems. As developers strive to create seamless user experiences across various platforms, identifying and managing devices, along with understanding their capabilities, becomes crucial. This article explores strategies to address these challenges, ensuring efficient device discovery and optimal utilization of features in applications. I will use [.NET nanoFramework](https://www.nanoframework.net) as an illustration but this is applicable to any kind of device!

For this, I will use networking and UDP with broadcast on a specific port with a specific binary protocol. We will look at the details and why.

## UDP or TCP?

User Datagram Protocol (UDP) and Transmission Control Protocol (TCP) are brother and sister protocols. They are from the same family but are quite different. They have their own advantages and inconvenience. Let's first look at each of them.

In the intro, I said, I'll use UDP to solve the problem, so let's first look at the advantages and inconvenience.

### Advantages of UDP

UDP has quite some advantages, the main ones can be summarize as:

* Low Overhead: UDP has less overhead compared to TCP, making it more lightweight and suitable for quick, low-latency communication.
* Broadcasting: Broadcasting messages on a specific port enables easy device discovery in local networks without establishing a connection with each device individually.
* Connectionless: UDP is connectionless, eliminating the need for establishing and maintaining connections. This can be advantageous for quick, stateless communications.

### UDP also  inconveniences

UDP also has quite some inconveniences:

* Unreliable: UDP does not guarantee delivery or order of packets, making it less reliable than TCP. This may result in lost or out-of-sequence messages.
* No Flow Control: Unlike TCP, UDP doesn't have built-in flow control mechanisms, which could lead to congestion or packet loss in high-traffic scenarios.
* Limited Error Handling: UDP provides minimal error checking, and applications need to implement their own error detection and correction mechanisms.

### Comparing UDP to TCP

Putting in perspective UDP and TCP gives the following comparison on some of the key differentiators:

* Reliability: TCP ensures reliable, ordered delivery of data, which is crucial for scenarios where every message must reach its destination accurately. UDP sacrifices reliability for speed and simplicity.
* Connection: TCP establishes a connection before data exchange, ensuring a reliable stream. UDP is connectionless, making it faster but less reliable.
* Overhead: TCP has higher overhead due to its connection-oriented nature and additional features, while UDP is more lightweight.

### Looking at the Use Cases

TCP is suitable for applications requiring reliability and ordered delivery (e.g., file transfers, web browsing), while UDP is favored for real-time applications where low latency is crucial (e.g., video streaming, online gaming).
Choosing between UDP and TCP depends on the specific requirements of your device discovery protocol and the trade-offs you are willing to make in terms of reliability and overhead.

So in short, UDP seems a good candidate for discovery if we think of a way to retry sending messages or using a "ping" mechanism to get device information and also, if, as it is for a discovery protocol, missing a packet is not the most critical, we can go for it. We will also have to make sure, as anyone can write anything on UDP that we have a protocol which can handle this.

## Binary or text based protocol?

Choosing between a binary protocol and a text based protocol in your UDP-based device discovery protocol involves considerations of efficiency, readability, and ease of implementation. Let's have a look at those as well.

### Binary vs Text Payload

A binary protocol example is Message [Queuing Telemetry Transport (MQTT)](https://mqtt.org/mqtt-specification/). Advantages on some of the key elements are:

* Compact Representation: Binary payloads typically require less space than their text counterparts, resulting in more efficient data transmission.
* Lower Bandwidth Usage: Transmitting binary data over the network can lead to lower bandwidth usage compared to sending equivalent information in text format.
* Faster Parsing: Processing binary data is often faster, as it involves simpler parsing mechanisms, which can be beneficial for quick device discovery.

Advantages of Text Payload:

* Human Readability: Text payloads are human-readable, facilitating easier debugging and troubleshooting during development.
* Ease of Implementation: Text data is easier to work with during development and testing, as it can be viewed and modified easily.
* Interoperability: Text payloads are more likely to be interoperable across different platforms and programming languages due to their human-readable nature.

Additional Considerations:

* Efficiency vs. Readability: Choose a payload format based on the trade-off between efficient data transmission (binary) and ease of human interpretation (text).
* Parsing Complexity: Binary payloads might require more careful parsing, while text payloads can be processed with simpler parsing logic.

Ultimately, the choice between binary and text payload depends on the specific needs of your application. If efficiency and speed are critical, binary might be more suitable. If human readability and ease of implementation are prioritized, a text payload could be the better choice. Because I prefer efficiency, I've decided to go for a binary payload.

### What is mandatory in a discovery protocol?

In a discovery protocol, you want to be able to identify a specific device without knowing it and being later able to contact it. So you basically need to know it's IP address. Usually you also need to get a sort of identifier. And on top, you'll add a specific payload. To have all the glue sticked together, add some message type, so that, you can identify what to answer.

#### Device Identifier

Assign a unique identifier to each device in your network. This identifier can be a combination of letters, numbers, or any format that ensures uniqueness.
This identifier serves as a way to distinguish one device from another, allowing you to identify the target device during the discovery process.

#### IP Address

During the device discovery phase, devices broadcast messages to the network to announce their presence. These messages typically include the device's IP address.
The IP address is crucial for later communication. Once a device is discovered, the IP address allows you to establish a direct connection for further interaction.

#### Payload

The payload contains the actual data or information you want to exchange with the discovered devices. This could include details about the device's capabilities, status, or any other relevant information.
The payload is specific to your application's requirements and might be structured in a binary or text format, depending on your chosen approach.

#### Message Type

Include a message type field in your messages to categorize them based on their purpose. This field helps devices identify the nature of the message and how to process it.
For example, you might have different message types for device discovery, acknowledgment, information exchange, etc.

#### Example of text payload and how to parse it

In this example, I am taking a text protocol. A fairly easy one:

* `LEGO:1:DISCO`: message send for discovery
* `LEGO:1:ID=42:IP=192.168.1.125:SW:SO`: the answer to the discovery message
* `LEGO:1:BYEBYE:ID=42:IP=192.168.1.125`: proactively sent by the device when it's leaving

Couple of important elements:

* As we've seen before, the header is important and must be unique, meaning, that we will set `LEGO:1` as a header. `LEGO` would be protocol name and `1` the version.
* We are using `:` to separate each element of the massage making it easy to build, split and parse.
* we're using `=` to separate a variable from a value.

Assuming the .NET nanoFramework device is only answering the discovery request and sending a leaving message, we can implement it like this:

```csharp
// Licensed to the Laurent Ellerbach under one or more agreements.
// Laurent Ellerbach licenses this file to you under the MIT license.

using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;

namespace SharedServices.Services
{
    public class LegoDiscovery : IDisposable
    {
        public const string Signal = ":SI";
        public const string Switch = ":SW";
        public const string Both = ":SW:SI";
        public const string Infrared = ":IR";

        private const int BindingPort = 2024;
        private UdpClient _udpClient;
        private IPAddress _ipAddress;
        private string _capabilities;
        private int _deviceId;
        private CancellationTokenSource _tokenSource;
        private Thread _runner;

        public LegoDiscovery(IPAddress ipaddess, int deviceId, string capabilities)
        {
            _udpClient = new UdpClient();
            _ipAddress = ipaddess;
            _capabilities = capabilities;
            _deviceId = deviceId;
            IsRunning = false;

            // Bind the UDP Client on the port on any address
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, BindingPort));
        }

        public void Dispose()
        {
            SendByeBye();

            Stop();
            // Wait a bit
            if (_runner != null)
            {
                _runner.Join(1500);
            }

            _udpClient?.Dispose();
        }

        public void Run(CancellationToken token)
        {
            IsRunning = true;
            _tokenSource = new CancellationTokenSource();
            // Allow to receive answers from anyone on the network
            var from = (EndPoint)(new IPEndPoint(0, 0));
            _runner = new Thread(() =>
            {
                while (!token.IsCancellationRequested && !_tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        if (_udpClient.Available > 0)
                        {
                            byte[] recvBuffer = new byte[_udpClient.Available];
                            _udpClient.Client.ReceiveFrom(recvBuffer, ref from);
                            var resp = Encoding.UTF8.GetString(recvBuffer, 0, recvBuffer.Length);
                            Console.WriteLine(resp);
                            if (resp == "LEGO:1:DISCO")
                            {
                                SendCapabilities();
                            }
                        }

                        // We do answer in about 1 second, no need to put stress on this
                        _tokenSource.Token.WaitHandle.WaitOne(1000, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LegoDiscovery: {ex.Message}");
                    }                    
                }

                IsRunning = false;
            });
            _runner.Start();
        }

        public bool IsRunning { get; private set; }

        public void Stop() => _tokenSource?.Cancel();

        public void SendCapabilities()
        {
            try
            {
                string capabilities = $"LEGO:1:ID={_deviceId}:IP={_ipAddress}{_capabilities}";
                var data = Encoding.UTF8.GetBytes(capabilities);
                _udpClient.Send(data, 0, data.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), BindingPort));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dico capacity: {ex.Message}");
            }
        }

        public void SendByeBye()
        {
            try
            {
                var data = Encoding.UTF8.GetBytes($"LEGO:1:BYEBYE:ID={_deviceId}:IP={_ipAddress}");
                _udpClient.Send(data, 0, data.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), BindingPort));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dico bybye: {ex.Message}");
            }            
        }
    }
}
```

And now assuming the main client is a full .NET device, the main clisent will look like this:

```csharp
// Licensed to the Laurent Ellerbach under one or more agreements.
// Laurent Ellerbach licenses this file to you under the MIT license.

using LegoTrain.Models.Device;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LegoTrain.Services
{
    public class LegoDiscovery : IDisposable
    {
        public const string Signal = ":SI";
        public const string Switch = ":SW";
        public const string Both = ":SW:SI";
        public const string Infrared = ":IR";
        private const int BindingPort = 2024;
        private UdpClient _udpClient;
        private Thread _runDiscovery;
        private CancellationTokenSource _runDiscoToken;
        private Thread _runReceive;
        private CancellationTokenSource _runReceiveToken;
        private Dictionary<int, DeviceDetails> _deviceDetails = new Dictionary<int, DeviceDetails>();

        public LegoDiscovery(TimeSpan update = default)
        {
            _udpClient = new UdpClient();
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, BindingPort));
            _runDiscoToken = new CancellationTokenSource();
            _runReceiveToken = new CancellationTokenSource();
            if (update == default)
            {
                update = TimeSpan.FromMinutes(1);
            }

            _runDiscovery = new Thread(() =>
            {
                while (!_runDiscoToken.IsCancellationRequested)
                {
                    SendDiscovery();
                    // We ask for a ping to check that everything is still present every minute
                    Thread.Sleep((int)update.TotalMilliseconds);
                    // Check if we do have devices left for more than 3 updates
                    foreach (var device in _deviceDetails.Values)
                    {
                        if (device.LastUpdate > DateTimeOffset.UtcNow.AddMilliseconds(-update.TotalMilliseconds * 3))
                        {
                            _deviceDetails.Remove(device.Id);
                            device.DeviceStatus = DeviceStatus.Absent;
                            // TODO: Send event
                        }
                    }
                }
            });

            // We want to receive from anyone in the network
            var from = new IPEndPoint(0, 0);
            _runReceive = new Thread(() =>
            {
                while (!_runReceiveToken.IsCancellationRequested)
                {
                    try
                    {
                        // This is bloking up to the moment something is received but we do only receive very small parts
                        var recvBuffer = _udpClient.Receive(ref from);
                        var received = Encoding.UTF8.GetString(recvBuffer);
                        Console.WriteLine(received);

                        var devDetails = new DeviceDetails();
                        DeviceDetails oldDevDeatils = null!;
                        // Check what we have received
                        var res = received.Split(":");
                        if (res == null || res.Length <= 3)
                        {
                            continue;
                        }

                        // Result is: LEGO:1:ID=1:IP=192.168.1.10:SI:SW:IR
                        // Where SI, SW, IR can be capacities
                        // Or: LEGO:1:BYEBYE:ID={_deviceId}:IP={_ipAddress}
                        // Check first 2
                        int inc = 0;
                        if (res[inc++] != "LEGO" || res[inc++] != "1")
                        {
                            continue;
                        }

                        bool isGoodbye = res[inc] == "BYEBYE" ? true : false;
                        inc = isGoodbye ? inc++ : inc;

                        // Now the ID
                        var id = res[inc++].Split("=");
                        if (id.Length == 2)
                        {
                            // We have an id
                            if (id[0] != "ID")
                            {
                                continue;
                            }

                            int deviceId = -1;
                            int.TryParse(id[1], out deviceId);
                            if (deviceId < 0)
                            {
                                continue;
                            }

                            devDetails.Id = deviceId;
                            if (_deviceDetails.ContainsKey(devDetails.Id))
                            {
                                oldDevDeatils = _deviceDetails[devDetails.Id];
                            }

                            if (isGoodbye)
                            {
                                if (oldDevDeatils != null)
                                {
                                    _deviceDetails.Remove(oldDevDeatils.Id);
                                    // TODO: notify with event
                                    oldDevDeatils.DeviceStatus = DeviceStatus.Laaving;
                                }

                                continue;
                            }
                        }

                        // Now the IP adress
                        var ip = res[inc++].Split("=");
                        if (ip.Length != 2 || ip[0] != "IP")
                        {
                            continue;
                        }

                        devDetails.IPAddress = IPAddress.Parse(ip[1]);

                        // Now check how many capabilities
                        var capabilities = res.Length - inc;
                        for (int i = 0; i < capabilities; i++)
                        {
                            switch (res[inc++])
                            {
                                case "IR":
                                    devDetails.DeviceCapacity |= DeviceCapability.Infrared;
                                    break;
                                case "SW":
                                    devDetails.DeviceCapacity |= DeviceCapability.Switch;
                                    break;
                                case "SI":
                                    devDetails.DeviceCapacity |= DeviceCapability.Signal;
                                    break;
                                default:
                                    break;
                            }
                        }

                        oldDevDeatils.DeviceStatus = DeviceStatus.Joining;
                        oldDevDeatils.LastUpdate = DateTimeOffset.UtcNow;
                        // Check if we already have one
                        if (oldDevDeatils != null)
                        {
                            // Check if status is different than Joining
                            if ((oldDevDeatils.DeviceStatus != DeviceStatus.Joining) || (oldDevDeatils.DeviceCapacity != devDetails.DeviceCapacity))
                            {
                                // TODO Send notification
                            }
                        }
                        else
                        {
                            // TODO Send notification
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UDP Receive: {ex}");
                    }
                }
            });

            _runReceive.Start();
            _runDiscovery.Start();
        }

        public void Dispose()
        {
            _runDiscoToken?.Cancel();
            _runReceiveToken?.Cancel();
            _udpClient?.Dispose();
            _runDiscovery?.Join();
            _runReceive?.Join();
        }

        public void SendDiscovery()
        {
            try
            {
                var data = Encoding.UTF8.GetBytes("LEGO:1:DISCO");
                _udpClient.Send(data, data.Length, "255.255.255.255", BindingPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(SendDiscovery)}: {ex}");
            }
        }
    }
}
```

This is a fully functional example and you can adjust it for your needs!

#### Example of binary payload and how to parse it

Now, let's implement a similar protocol but in binary. Let's use the following protocol and looking at each byte representation:

`n D c Version MessageType Id IP1 IP2 IP3 IP4 PayloadBytes`

in this case, we will have:

* A header composed by the binary representations on nDC so 0x6E 0x44 0x43. n like nanoFramework and DC like Discovery.
* The version, for this example will always be 1.
* I've implemented multiple message types, the important one is Discovery as it does not require anything after. So a Discovery message will just be `n D c Version Discovery`
* A signed ID because it will allow scenarios with negative ones which can be invalid. And in all cases, you can always cast back to a byte. Having already 255 different devices on the network seems more than reasonable. A version2 can be created to add more!
* 4 bytes for the IP address (and can be moe for IPv6, code is ready).
* An optional payload with multiple bytes. The payload can be empty.

```csharp
// Licensed to the Laurent Ellerbach under one or more agreements.
// Laurent Ellerbach licenses this file to you under the MIT license.

using System.Net;

namespace nanoDiscovery
{
    public class DiscoveryMessage
    {
        public static readonly byte[] Header = new byte[] { (byte)'n', (byte)'D', (byte)'C' };
        public const int Version = 1;

        /// <summary>
        /// Creates a message using the protocol eelemnts.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="id">The ID of the element.</param>
        /// <param name="ipAddress">The IP address of the sender.</param>
        /// <param name="payload">An additional payload that is application specific.</param>
        /// <returns>The message to send.</returns>
        public static byte[] CreateMessage(DiscoveryMessageType messageType, sbyte id, IPAddress ipAddress, byte[] payload)
        {
            // Message looks like in bytes: n D C Version MessageType ID IP1 IP2 IP3 IP4 payload_bytes 
            int inc = 0;
            byte[] ret;
            byte[] ip = new byte[0];
            if (messageType == DiscoveryMessageType.Discovery)
            {
                ret = new byte[Header.Length + 1 + 1];
            }
            else
            {
                ip = ipAddress.GetAddressBytes();
                ret = new byte[Header.Length + 1 + 1 + 1 + ip.Length + payload.Length];
            }

            Header.CopyTo(ret, 0);
            inc += Header.Length;
            ret[inc++] = Version;
            ret[inc++] = (byte)messageType;
            if (messageType != DiscoveryMessageType.Discovery)
            {
                ret[inc++] = (byte)id;
                ip.CopyTo(ret, inc);
                inc += ip.Length;
                if (payload != null)
                {
                    for (int i = 0; i < payload.Length; i++)
                    {
                        ret[inc++] = payload[i];
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Decodes a message with the protocol elements.
        /// </summary>
        /// <param name="message">The full message.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="id">The ID of the element.</param>
        /// <param name="ipAddress">The IP address of the sender.</param>
        /// <param name="payload">An additional payload that is application specific.</param>
        /// <returns>True if the message can be sucessfully decoded, false otherwise.</returns>
        public static bool DecodeMessage(byte[] message, out DiscoveryMessageType messageType, out sbyte id, out IPAddress ipAddress, out byte[] payload)
        {
            messageType = DiscoveryMessageType.None;
            id = -1;
            ipAddress = IPAddress.Any;
            payload = null;
            // Message looks like in bytes: n D C Version MessageType ID IP1 IP2 IP3 IP4 payload_bytes
            int inc = 0;
            // Check we have a minimum size of 5
            if (message.Length < 5)
            {
                return false;
            }

            for (int i = 0; i < Header.Length; i++)
            {
                if (message[i] != Header[i])
                {
                    return false;
                }

                inc++;
            }

            if (message[inc++] != Version)
            {
                return false;
            }

            messageType = (DiscoveryMessageType)message[inc++];

            if (messageType != DiscoveryMessageType.Discovery)
            {
                // Wee need at least ID + IP length
                var ipLength = ipAddress.GetAddressBytes().Length;
                if (message.Length < 6 + ipLength)
                {
                    return false;
                }

                id = (sbyte)message[inc++];
                var ipAddressBytes = new byte[ipLength];
                for (int i = 0; i < ipLength; i++)
                {
                    ipAddressBytes[i] = message[inc++];
                }

                ipAddress = new IPAddress(ipAddressBytes);

                // If we have anything more, then it's the payload
                payload = new byte[message.Length - 6 - ipLength];
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] = message[inc++];
                }
            }

            return true;
        }
    }
}
```

#### Comparing both codes

The encoding/decoding part is quite different between both. In one case, the text based one requires to split the text, check quite some elements. But if many things would be optional, it would be much more flexible. In this case, the protocol is quite strict, so the binary option makes it faster and code wise quite compact.

The challenge will, as always comes from the payload. In that case, it can be anything, including text! You could send json, text or binary.

## What about the capabilities?

Capabilities can be encoded in the payload. This binary implementation make it flexible. The text example shows how to encode as well capabilities directly with the protocol making it less reusable and extendable.

## Conclusion

UDP with bin&ary payload and a simple payload is perfect for those kind of discovery protocols. It gives flexibility and a very quick way. The pattern shown in the text example with a main listener checking every minute or so to see if there is any change, is a must have. UDP is not a controlled protocol, so you are never sure a message arrives.

The binary example gives the flexibility to reuse the protocol for any payload and on any port. You may have devices on a specific pot for a specific application, others on another one for a very different application. In all cases, they'll perfectly live together and you'll be able to fully reuse the class and the listener/producer code from the text example.

To go further, you can imagine using such a protocol without any central
