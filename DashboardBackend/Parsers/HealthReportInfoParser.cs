﻿using DashboardBackend.Database.Models;
using Model;

namespace DashboardBackend.Parsers
{
    public class HealthReportInfoParser : IDataParser<HealthReportEntry, HealthReport>
    {
        public HealthReport Parse(List<HealthReportEntry> data)
        {
            HealthReport result = new();
            foreach (var entry in data)
            {
                switch (entry.ReportType)
                {
                    #region Host data
                    case "INIT" when entry.ReportKey == "Hostname":
                        result.HostName = entry.ReportStringValue;
                        break;
                    case "INIT" when entry.ReportKey == "Monitor Name":
                        result.MonitorName = entry.ReportStringValue;
                        break;
                    #endregion
                    #region CPU data
                    case "CPU_INIT" when entry.ReportKey == "CPU Name":
                        result.Cpu.Name = entry.ReportStringValue;
                        break;
                    case "CPU_INIT" when entry.ReportKey == "PhysicalCores":
                        result.Cpu.Cores = Convert.ToInt32(entry.ReportNumericValue ?? 0);
                        break;
                    case "CPU_INIT" when entry.ReportKey == "CPU Max frequency":
                        result.Cpu.MaxFrequency = Convert.ToInt64(entry.ReportNumericValue ?? 0);
                        break;
                    #endregion
                    #region RAM data
                    case "MEMORY_INIT" when entry.ReportKey == "TOTAL":
                        result.Ram.Total = entry.ReportNumericValue;
                        break;
                    #endregion
                    #region Network data
                    case "NETWORK_INIT" when entry.ReportKey == "Interface 0: Name":
                        result.Network.Name = entry.ReportStringValue;
                        break;
                    case "NETWORK_INIT" when entry.ReportKey == "Interface 0: MAC address":
                        result.Network.MacAddress = entry.ReportStringValue;
                        break;
                    case "NETWORK_INIT" when entry.ReportKey == "Interface 0: Speed":
                        result.Network.Speed = entry.ReportNumericValue ?? 0;
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            return result;
        }

        private Cpu GetCpuComponent(List<HealthReportEntry> data)
        {
            string name = data.FindLast(e => e.ReportKey == "CPU Name")?.ReportStringValue;
            var cores = data.FindLast(e => e.ReportKey == "PhysicalCores")?.ReportNumericValue ?? 0;
            var maxFreq = data.FindLast(e => e.ReportKey == "CPU Max Frequency")?.ReportNumericValue ?? 0;
            return new Cpu(name, Convert.ToInt16(cores), maxFreq);
        }

        private Ram GetRamComponent(List<HealthReportEntry> data)
        {
            var total = data.FindLast(e => e.ReportKey == "TOTAL")?.ReportNumericValue;
            return new Ram(total);
        }

        private Network GetNetworkComponent(List<HealthReportEntry> data)
        {
            string name = data.FindLast(e => e.ReportKey == "Interface 0: Name")?.ReportStringValue;
            string macAddress = data.FindLast(e => e.ReportKey == "Interface 0: MAC address")?.ReportStringValue;
            var speed = data.FindLast(e => e.ReportKey == "Interface 0: Speed")?.ReportNumericValue;
            return new Network(name, macAddress, speed);
        }
    }
}
