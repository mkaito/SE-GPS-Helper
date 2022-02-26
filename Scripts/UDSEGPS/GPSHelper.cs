using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace UDSEGPS
{
    // ReSharper disable once UnusedType.Global
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class GPSHelper : MySessionComponentBase
    {
        private bool isInitialized;

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (isInitialized) return;
            MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
            isInitialized = true;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            isInitialized = false;
        }


        // ReSharper disable once RedundantAssignment
        private void OnMessageEntered(string messageText, ref bool sendToOthers)
        {
            string cmd, extra;
            sendToOthers = false;

            var idx = messageText.IndexOf(' ');

            if (idx > 1)
            {
                cmd = messageText.Substring(0, idx).Trim();
                extra = messageText.Substring(idx).Trim();
            }
            else
            {
                cmd = messageText;
                extra = null;
            }

            switch (cmd.ToLowerInvariant())
            {
                case "/fgps":
                    CreateFactionGPS(extra, false);
                    break;
                case "/fgpsx":
                    CreateFactionGPS(extra, true);
                    break;
                case "/gpsx":
                    CreateGPS(extra);
                    break;
                case "/gpsx_reset":
                    ResetGPSCounter(extra);
                    break;
                case "/gps_export":
                    ExportAllGPSToClipboard(extra);
                    break;
                case "/gps_toggle":
                    GPSToggle(extra);
                    break;
                case "/gps_off":
                    GPSToggle(extra, false);
                    break;
                case "/gps_on":
                    GPSToggle(extra, true);
                    break;
                case "/gps_remove":
                    RemoveGPS(extra);
                    break;
                case "/gps_ping":
                    MyAPIGateway.Utilities.ShowNotification("Pong!");
                    break;
                default:
                    sendToOthers = true;
                    break;
            }
        }

        private static string ConvertToBaseAlpha(double number, string baseStr = "0123456789ABCDEFGHIKJLMNOPQRSTUVWXYZ")
        {
            var ans = "";
            var power = baseStr.Length;

            while ((int) number != 0)
            {
                var m = (int) (number % power);
                number /= power;
                ans = baseStr.Substring(m, 1) + ans;
            }

            return ans;
        }

        private static ulong ConvertFromBaseAlpha(string alpha, string baseStr = "0123456789ABCDEFGHIKJLMNOPQRSTUVWXYZ")
        {
            ulong val = 0;
            ulong power = 1;
            foreach (var c in alpha.ToCharArray().Reverse())
            {
                var posValue = (ulong) baseStr.IndexOf(c);
                val += posValue * power;
                power *= (ulong) baseStr.Length;
            }

            return val;
        }

        private void ResetGPSCounter(string extra)
        {
            var player = MyAPIGateway.Session.Player;
            var playerId = player.IdentityId;
            ulong autoid = 0;

            if (!string.IsNullOrWhiteSpace(extra))
            {
                autoid = ConvertFromBaseAlpha(extra);
            }

            MyAPIGateway.Utilities.SetVariable("autoid_" + playerId, autoid);
        }

        private void CreateFactionGPS(string extra, bool useGPSX)
        {
            var player = MyAPIGateway.Session.Player;
            // var playerPosition = player.Character.GetPosition();
            var playerId = player.IdentityId;

            var gps = CreateGPS(extra, useGPSX);

            var faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return;
            foreach (var member in faction.Members)
            {
                MyAPIGateway.Session.GPS.AddGps(member.Value.PlayerId, gps);
            }
        }

        private const double radius = 50.0;

        private IMyGps CreateGPS(string extra, bool useAutoID = true)
        {
            var player = MyAPIGateway.Session.Player;
            var playerPosition = player.Character.GetPosition();
            var playerId = player.IdentityId;

            ulong autoid = 0;
            if (useAutoID)
            {
                MyAPIGateway.Utilities.GetVariable("autoid_" + playerId, out autoid);
                autoid++;
            }

            var voxelMaps = new List<MyVoxelBase>();
            var sphere = new BoundingSphereD(playerPosition, radius);

            MyGamePruningStructure.GetAllVoxelMapsInSphere(ref sphere, voxelMaps);

            var builder = new StringBuilder();
            foreach (var map in voxelMaps.OfType<MyPlanet>())
                builder.AppendLine("Planet: " + map.AsteroidName.Split('-')[0]);

            var locationName = string.IsNullOrWhiteSpace(extra) ? player.DisplayName : extra;

            if (useAutoID)
                locationName += " " + ConvertToBaseAlpha(autoid).PadLeft(3, '0');

            var gps = MyAPIGateway.Session.GPS.Create(locationName, builder.ToString(), playerPosition, true);
            MyAPIGateway.Session.GPS.AddGps(playerId, gps);

            MyAPIGateway.Utilities.SetVariable("autoid_" + playerId, autoid);

            return gps;
        }

        private void processGPS(string args, Action<IMyGps> gpsAction)
        {
            var gpsList = MyAPIGateway.Session.GPS.GetGpsList(MyAPIGateway.Session.Player.IdentityId);
            var matcher = string.IsNullOrWhiteSpace(args) ? null : args;
            foreach (var gps in gpsList)
            {
                // If we don't have a matcher, run on everything
                if (matcher == null)
                {
                    gpsAction(gps);
                    continue;
                }

                // If we have a matcher, only run on matching entries
                if (gps.Name != null && gps.Name.Contains(matcher) ||
                    gps.Description != null && gps.Description.Contains(matcher) ||
                    gps.GPSColor.ToString().Contains(matcher))
                {
                    gpsAction(gps);
                }
            }
        }

        private void ExportAllGPSToClipboard(string args)
        {
            var sb = new StringBuilder();
            processGPS(args, gps => { sb.AppendLine(gps.ToString()); });
            MyClipboardHelper.SetClipboard(sb.ToString());
            MyAPIGateway.Utilities.ShowNotification("GPS Points have been exported to the clipboard");
        }

        private void GPSToggle(string args, bool? setState = null)
        {
            processGPS(args, gps =>
            {
                gps.ShowOnHud = setState ?? !(gps.ShowOnHud);
                MyAPIGateway.Session.GPS.ModifyGps(MyAPIGateway.Session.Player.IdentityId, gps);
            });
            MyAPIGateway.Utilities.ShowNotification("GPS Points have been modified");
        }

        private void RemoveGPS(string args)
        {
            processGPS(args,
                gps => { MyAPIGateway.Session.GPS.RemoveGps(MyAPIGateway.Session.Player.IdentityId, gps); });
            MyAPIGateway.Utilities.ShowNotification("GPS Points have been removed");
        }
    }
}