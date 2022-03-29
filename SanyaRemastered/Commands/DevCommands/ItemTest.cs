using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using InventorySystem;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using UnityEngine;


namespace SanyaRemastered.Commands.DevCommands
{

	public class ItemTest : ICommand
    {
        public string Command => "ItemTest";

        public string[] Aliases => new string[] { };

        public string Description => "Spawn an sublime Item UwU";

		private ItemPickupBase targetitem = null;

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission($"sanya.dev"))
            {
                response = $"You don't have permission to execute this command. Required permission: sanya.dev";
                return false;
            }
			if (targetitem is null)
			{
				var itemtype = (ItemType)Enum.Parse(typeof(ItemType), arguments.At(0));
				var itemBase = InventoryItemLoader.AvailableItems[itemtype];
				var pickup = UnityEngine.Object.Instantiate(itemBase.PickupDropModel,
					new UnityEngine.Vector3(float.Parse(arguments.At(1)), float.Parse(arguments.At(2)), float.Parse(arguments.At(3))),
					Quaternion.Euler(Vector3.up * float.Parse(arguments.At(4))));
				pickup.Info.ItemId = itemtype;
				pickup.Info.Weight = itemBase.Weight;
				pickup.Info.Locked = true;
				pickup.GetComponent<Rigidbody>().useGravity = false;
				pickup.transform.localScale = new UnityEngine.Vector3(float.Parse(arguments.At(5)), float.Parse(arguments.At(6)), float.Parse(arguments.At(7)));

				targetitem = pickup;
				ItemDistributor.SpawnPickup(pickup);
			}
			else
			{
				NetworkServer.Destroy(targetitem.gameObject);
				targetitem = null;
			}
			response = "ok.";
            return true;
        }
    }
}