﻿using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Net;

namespace MasterServer
{
	public enum MasterServerMessageType
	{
		RegisterHost,
		RequestHostList,
		RequestIntroduction,
	}

	public class Program
	{
		static void Main(string[] args)
		{
			List<IPEndPoint> registeredHosts = new List<IPEndPoint>();

			NetPeerConfiguration config = new NetPeerConfiguration("masterserver");

			NetPeer peer = new NetPeer(config);
			peer.Start();

			// keep going until ESCAPE is pressed
			Console.WriteLine("Press ESC to quit");
			while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
			{
				NetIncomingMessage msg;
				while((msg = peer.ReadMessage()) != null)
				{
					switch (msg.MessageType)
					{
						case NetIncomingMessageType.UnconnectedData:
							//
							// We've received a message from a client or a host
							//

							// by design, the first byte always indicates action
							switch ((MasterServerMessageType)msg.ReadByte())
							{
								case MasterServerMessageType.RegisterHost:
									// It's a host wanting to register its presence
									registeredHosts.Add(msg.SenderEndpoint);
									break;

								case MasterServerMessageType.RequestHostList:
									// It's a client wanting a list of registered hosts
									foreach (IPEndPoint ep in registeredHosts)
									{
										// send registered host to client
										NetOutgoingMessage om = peer.CreateMessage();
										om.Write(ep);
										peer.SendUnconnectedMessage(om, msg.SenderEndpoint);
									}

									break;
								case MasterServerMessageType.RequestIntroduction:
									// It's a client wanting to connect to a specific host
									IPEndPoint rh = msg.ReadIPEndpoint();
									peer.Introduce(rh, msg.SenderEndpoint);
									break;
							}
							break;

						case NetIncomingMessageType.DebugMessage:
						case NetIncomingMessageType.VerboseDebugMessage:
						case NetIncomingMessageType.WarningMessage:
						case NetIncomingMessageType.ErrorMessage:
							// print diagnostics message
							Console.WriteLine(msg.ReadString());
							break;
					}
				}
			}

			peer.Shutdown("shutting down");
		}
	}
}