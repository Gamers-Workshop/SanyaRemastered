using Dissonance;
using Dissonance.Extensions;
using Dissonance.Integrations.MirrorIgnorance;
using Dissonance.Networking;
using Dissonance.Networking.Client;
using Dissonance.Networking.Server;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanyaRemastered.Patches
{
    //[HarmonyPatch(typeof(BaseServer<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>), nameof(BaseServer<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>.NetworkReceivedPacket))]
    public static class GetAudioPacket
    {
        public static bool Prefix(BaseServer<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn> __instance,
            MirrorConn source, ArraySegment<byte> data)
        {
			try
			{
				PacketReader packetReader = new PacketReader(data);
				if (!packetReader.ReadPacketHeader(out MessageTypes messageTypes))
				{
					Exiled.API.Features.Log.Warn("Discarding packet - incorrect magic number.");
					return true;
				}
				switch (messageTypes)
				{
					case MessageTypes.ServerRelayReliable:
					case MessageTypes.ServerRelayUnreliable:
						if (__instance.CheckSessionId(ref packetReader, source))
						{

							packetReader.ReadRelay(null, out ArraySegment<byte> data1);
							PacketReader packetReader1 = new PacketReader(data1);
							if (!packetReader1.ReadPacketHeader(out MessageTypes messageTypes1))
							{
								Exiled.API.Features.Log.Warn("Discarding packet - incorrect magic number.");
								return true;
							}
							if (messageTypes1 == MessageTypes.VoiceData)
							{
								packetReader1.ReadVoicePacketHeader1(out ushort senderid);
								packetReader1.ReadVoicePacketHeader2(out VoicePacketOptions options, out ushort sequenceNumber, out ushort numChannels);


								List<ChannelBitField> bitFieldlist = new List<ChannelBitField>();
								List<ushort> recipientlist = new List<ushort>();

								for (int i = 0; i < numChannels; i++)
								{
									packetReader1.ReadVoicePacketChannel(out ChannelBitField bitField, out ushort recipient);
									bitFieldlist.Add(bitField);
									recipientlist.Add(recipient);
								}
								ArraySegment<byte> RawAudio = packetReader1.ReadByteSegment();
							}
						}
						break;

					case MessageTypes.ClientState:
					case MessageTypes.HandshakeRequest:
					case MessageTypes.DeltaChannelState:
						break;
					default:
						__instance.Log.Error("Pas normal d'avoir un packet Comme ça ici MessageTypes =" + messageTypes);
						break;
				}
				Exiled.API.Features.Log.Info($"[GetAudioPacket] Postfix Is Work AdressIp = {source.Connection.address} messageTypes {messageTypes} PlayerIds:{__instance._clients.PlayerIds} \n" +
					$"PacketReader All: {packetReader.All} ");
			}
			catch (Exception ex)
            {
				Exiled.API.Features.Log.Error("AudioPacket Sanya" + ex);
            }
			return true;
		}
	}
	//[HarmonyPatch(typeof(ServerRelay<MirrorConn>), nameof(ServerRelay<MirrorConn>.ProcessPacketRelay))]
	public static class GetAudioPacket2
	{
		public static void Prefix(ServerRelay<MirrorConn> __instance, PacketReader reader, bool reliable)
		{
			if (!reliable)
			Exiled.API.Features.Log.Info($"[GetAudioPacket2] Postfix Is unreliable {__instance._peers.PlayerIds}");
			else
				Exiled.API.Features.Log.Warn($"[GetAudioPacket2] Postfix Is reliable {__instance._peers.PlayerIds}");

		}
	}
	//[HarmonyPatch(typeof(PlayerCollection), nameof(PlayerCollection.TryGet))]
	public static class PatchingThisForMArtin
	{
		public static bool Prefix(PlayerCollection __instance,ref bool __result, string playerId,out VoicePlayerState state)
		{
			if (playerId == null)
				throw new ArgumentNullException("playerId");

			var found = __instance._playersLookup.TryGetValue(playerId, out state);

			if (!found)
			{
				var log = $"PLAYER NOT FOUND: {playerId}\nKnown Players:";
				foreach (var item in __instance._playersLookup)
					log += $"\n - {item.Key} => {item.Value}";
				log += "\nKnown States:";
		
				foreach (var item in __instance._players)
					log += $"\n - {item.Name}";
				Exiled.API.Features.Log.Error(log);
			}
			__result = found;
			return false;
		}
	}
}