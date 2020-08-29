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

            EventHandlers["Eternar::RCCar::ManageItem"] += new Action<int>(ManageBattery);
            EventHandlers["Eternar::RCCar::PickUp"] += new Action<int>(PickUp);
        }

        private void OnResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName)
                return;

            ESX.RegisterUsableItem("rccar", new Action<int>((source) =>
            {
                xPlayer xPlayer = ESX.GetPlayerFromId(source);

                if(xPlayer.GetInventoryItem("rccar").count >= 1)
                    xPlayer.RemoveInventoryItem("rccar", 1);

                TriggerClientEvent("Eternar::RCCar::Place", source);
            }));

            ESX.RegisterUsableItem("duracell", new Action<int>((source) =>
            {
                TriggerClientEvent("Eternar::RCCar::Charge", source);
            }));
        }

        private void ManageBattery(int source)
        {
            xPlayer xPlayer = ESX.GetPlayerFromId(source);

            if (xPlayer.GetInventoryItem("duracell").count >= 1)
                xPlayer.RemoveInventoryItem("duracell", 1);
        }

        private void PickUp(int source)
        {
            xPlayer xPlayer = ESX.GetPlayerFromId(source);

            if (xPlayer.GetInventoryItem("rccar").count <= 0)
                xPlayer.AddInventoryItem("rccar", 1);
        }
    }
}