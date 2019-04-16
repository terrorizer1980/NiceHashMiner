﻿using System;
using System.Collections.Generic;
using System.Linq;
using MinerPlugin;
using NiceHashMinerLegacy.Common.Algorithm;
using NiceHashMinerLegacy.Common.Device;
using NiceHashMinerLegacy.Common.Enums;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using MinerPluginToolkitV1.Interfaces;
using MinerPluginToolkitV1.Configs;
using System.IO;
using NiceHashMinerLegacy.Common;

namespace TTMiner
{
    public class TTMinerPlugin : IMinerPlugin, IInitInternals
    {
        public TTMinerPlugin(string pluginUUID = "5ee2e280-4bfc-11e9-a481-e144ccd86993")
        {
            _pluginUUID = pluginUUID;
        }
        private readonly string _pluginUUID;
        public string PluginUUID => _pluginUUID;

        public Version Version => new Version(1, 1);
        public string Name => "TTMiner";
        public string Author => "stanko@nicehash.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 398.26
            var minDrivers = new Version(398, 26);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice gpu && gpu.SM_major >= 5)
                .Cast<CUDADevice>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithms(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        private IEnumerable<Algorithm> GetSupportedAlgorithms(CUDADevice dev)
        {
            return new List<Algorithm>{
                new Algorithm(PluginUUID, AlgorithmType.MTP),
                new Algorithm(PluginUUID, AlgorithmType.Lyra2REv3),
            };
        }

        public IMiner CreateMiner()
        {
            return new TTMiner(PluginUUID)
            {
                MinerOptionsPackage = _minerOptionsPackage,
                MinerSystemEnvironmentVariables = _minerSystemEnvironmentVariables
            };
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return a.Algorithm.FirstAlgorithmType == b.Algorithm.FirstAlgorithmType;
        }


        #region Internal Settings
        public void InitInternals()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), PluginUUID);

            var readFromFileEnvSysVars = InternalConfigs.InitMinerSystemEnvironmentVariablesSettings(pluginRoot, _minerSystemEnvironmentVariables);
            if (readFromFileEnvSysVars != null) _minerSystemEnvironmentVariables = readFromFileEnvSysVars;

            var fileMinerOptionsPackage = InternalConfigs.InitInternalsHelper(pluginRoot, _minerOptionsPackage);
            if (fileMinerOptionsPackage != null) _minerOptionsPackage = fileMinerOptionsPackage;
        }

        protected static MinerOptionsPackage _minerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Comma or space separated list of intensities that should be used mining.
			    /// First value for first GPU and so on. A single value sets the same intensity to all GPUs. A value of -1 uses the default intensity of the miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity",
                    ShortName = "-i",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid. Same as intensity (-i) just defines the size for the grid directly.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_intensity_grid",
                    ShortName = "-ig",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid-size. This will give you more and finer control about the gridsize.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "ttminer_grid_size",
                    ShortName = "-gs",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// Enable logging of screen output and additional information, the file is created in the folder 'Logs'.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "ttminer_log",
                    ShortName = "-log",
                },
                /// <summary>
                /// This option set the process priority for TT-Miner to a different level:
                /// 1 low
                /// 2 below normal
                /// 3 normal
                /// 4 above normal
                /// 5 high
                /// Default: -PP 3
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "ttminer_processPriority",
                    ShortName = "-PP",
                    DefaultValue = "3"
                }
            }
        };

        protected static MinerSystemEnvironmentVariables _minerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables { };
        #endregion Internal Settings
    }
}