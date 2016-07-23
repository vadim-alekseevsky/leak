﻿using Leak.Core.Bencoding;
using Leak.Core.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Leak.Core.Tracker
{
    public class TrackerClientToHttp : TrackerClient
    {
        private readonly string uri;

        public TrackerClientToHttp(string uri)
        {
            this.uri = uri;
        }

        public TrackerAnnounce Announce(Action<TrackerAnnounceConfiguration> configurer)
        {
            TrackerAnnounceConfiguration configuration = Configure(configurer);
            string request = BuildAnnounceUri(uri, configuration);

            BencodedValue decoded = Bencoder.Decode(CallTracker(request));
            TrackerPeer[] peers = FindPeers(decoded);

            return new TrackerAnnounce(peers, TimeSpan.FromMinutes(10));
        }

        private static TrackerPeer[] FindPeers(BencodedValue value)
        {
            List<TrackerPeer> result = new List<TrackerPeer>();

            BencodedValue peers = value.Find("peers", x => x);
            byte[] bytes = peers.Data.GetBytes();

            for (int i = 0; i < bytes.Length; i += 6)
            {
                string host = GetHost(bytes, i);
                int port = GetPort(bytes, i);

                if (port > 0)
                {
                    result.Add(new TrackerPeer(host, port));
                }
            }

            return result.ToArray();
        }

        private static string GetHost(byte[] data, int offset)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(data[0 + offset]);
            builder.Append('.');
            builder.Append(data[1 + offset]);
            builder.Append('.');
            builder.Append(data[2 + offset]);
            builder.Append('.');
            builder.Append(data[3 + offset]);

            return builder.ToString();
        }

        private static int GetPort(byte[] data, int offset)
        {
            return data[4 + offset] * 256 + data[5 + offset];
        }

        private static TrackerAnnounceConfiguration Configure(Action<TrackerAnnounceConfiguration> configurer)
        {
            TrackerAnnounceConfiguration configuration = new TrackerAnnounceConfiguration
            {
                Peer = PeerHash.Random()
            };

            configurer.Invoke(configuration);
            return configuration;
        }

        private static string BuildAnnounceUri(string tracker, TrackerAnnounceConfiguration configuration)
        {
            StringBuilder request = new StringBuilder();

            string hash = Encode(configuration.Hash.ToBytes());
            string peer = Encode(configuration.Peer.ToBytes());

            request.Append($"{tracker}?");
            request.Append($"info_hash={hash}&");
            request.Append($"peer_id={peer}&");
            request.Append($"port={configuration.Port}&");
            request.Append($"uploaded=0&");
            request.Append($"downloaded=0&");
            request.Append($"left=0&");
            request.Append($"compact=1&");
            request.Append($"event=started");

            return request.ToString();
        }

        private static string Encode(byte[] value)
        {
            StringBuilder builder = new StringBuilder();

            foreach (byte item in value)
            {
                builder.Append('%');
                builder.Append(item.ToString("x2"));
            }

            return builder.ToString();
        }

        private static byte[] CallTracker(string request)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadData(request);
            }
        }
    }
}