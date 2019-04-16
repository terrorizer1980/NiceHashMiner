﻿using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using NiceHashMiner.Configs;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils.Guid;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner
{
    public static partial class Globals
    {
        // Constants
        public static string[] MiningLocation = { "eu", "usa", "hk", "jp", "in", "br" };

        private static string GetAlgorithmUrlName(AlgorithmType algorithmType)
        {
            if (algorithmType < 0)
            {
                return "";
            }
            var name = AlgorithmNiceHashNames.GetName(algorithmType);
            if (name == AlgorithmNiceHashNames.NOT_FOUND)
            {
                return "";
            }
            // strip out the _UNUSED
            name = name.Replace("_UNUSED", "");
            return name.ToLower();
        }

        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            var name = GetAlgorithmUrlName(algorithmType);
            // if name is empty return
            if (name == "") return "";

            var nPort = 3333 + algorithmType;
            var sslPort = 30000 + nPort;

            // NHMConectionType.NONE
            var prefix = "";
            var port = nPort;
            switch (conectionType)
            {
                case NhmConectionType.LOCKED:
                    return miningLocation;
                case NhmConectionType.STRATUM_TCP:
                    prefix = "stratum+tcp://";
                    break;
                case NhmConectionType.STRATUM_SSL:
                    prefix = "stratum+ssl://";
                    port = sslPort;
                    break;
            }

#if TESTNET
            return prefix
                   + name
                   + "-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#elif TESTNETDEV
            return prefix
                   + "stratum-test." + miningLocation
                   + ".nicehash.com:"
                   + port;
#else
            return prefix
                   + name
                   + "." + miningLocation
                   + ".nicehash.com:"
                   + port;
#endif
        }

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim())
                ? Configs.ConfigManager.GeneralConfig.BitcoinAddress.Trim()
                : DemoUser.BTC;
        }
    }
}