using System;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

using Nexd.ESX.Server;

namespace Eternar.RCCar.Server
{
    public class Server : BaseScript
    {
        public Server()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
        }

        private void OnResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName)
                return;

            ESX.RegisterServerCallback("Eternar::RCCar::GET", new Action<int, CallbackDelegate, dynamic>((source, cb, data) =>
            {
                xPlayer xPlayer = ESX.GetPlayerFromId(source);

                if(xPlayer.GetInventoryItem("rccar").count <= 0)
                    xPlayer.AddInventoryItem("rccar", 1);
            }));

            ESX.RegisterUsableItem("rccar", new Action<int>((source) =>
            {
                xPlayer xPlayer = ESX.GetPlayerFromId(source);

                if(xPlayer.GetInventoryItem("rccar").count >= 1)
                    xPlayer.RemoveInventoryItem("rccar", 1);

                TriggerClientEvent("Eternar::RCCar::PLACE", source);
            }));

            ESX.RegisterUsableItem("duracell", new Action<int>((source) =>
            {
                xPlayer xPlayer = ESX.GetPlayerFromId(source);

                if (xPlayer.GetInventoryItem("duracell").count >= 1)
                    xPlayer.RemoveInventoryItem("duracell", 1);

                TriggerClientEvent("Eternar::RCCar::CHARGE", source);
            }));
        }
    }
}