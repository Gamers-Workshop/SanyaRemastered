using System;
using System.Collections.Generic;
using Exiled;
using Exiled.API.Enums;
using Exiled.API.Features;
using UnityEngine;

namespace SanyaRemastered.Data
{

	public struct MemoryMetrics
	{
		public double Total { get; set; }
		public double Used { get; set; }
		public double Free { get; set; }
	}
	public static class OutsideRandomAirbombPos
	{
		public static List<Vector3> Load()
		{
			return new List<Vector3>{
				new Vector3(UnityEngine.Random.Range(175, 182),  984, UnityEngine.Random.Range( 25,  29)),
				new Vector3(UnityEngine.Random.Range(174, 182),  984, UnityEngine.Random.Range( 36,  39)),
				new Vector3(UnityEngine.Random.Range(174, 182),  984, UnityEngine.Random.Range( 36,  39)),
				new Vector3(UnityEngine.Random.Range(166, 174),  984, UnityEngine.Random.Range( 26,  39)),
				new Vector3(UnityEngine.Random.Range(169, 171),  987, UnityEngine.Random.Range(  9,  24)),
				new Vector3(UnityEngine.Random.Range(174, 175),  988, UnityEngine.Random.Range( 10,  -2)),
				new Vector3(UnityEngine.Random.Range(186, 174),  990, UnityEngine.Random.Range( -1,  -2)),
				new Vector3(UnityEngine.Random.Range(186, 189),  991, UnityEngine.Random.Range( -1, -24)),
				new Vector3(UnityEngine.Random.Range(186, 189),  991, UnityEngine.Random.Range( -1, -24)),
				new Vector3(UnityEngine.Random.Range( 20,  21),  993, UnityEngine.Random.Range(-48, -49)),
				new Vector3(UnityEngine.Random.Range(185, 189),  993, UnityEngine.Random.Range(-26, -34)),
				new Vector3(UnityEngine.Random.Range(180, 195),  995, UnityEngine.Random.Range(-36, -91)),
				new Vector3(UnityEngine.Random.Range(148, 179),  995, UnityEngine.Random.Range(-45, -72)),
				new Vector3(UnityEngine.Random.Range(118, 148),  995, UnityEngine.Random.Range(-47, -65)),
				new Vector3(UnityEngine.Random.Range( 83, 118),  995, UnityEngine.Random.Range(-47, -65)),
				new Vector3(UnityEngine.Random.Range( 13,  15),  995, UnityEngine.Random.Range(-18, -48)),
				new Vector3(UnityEngine.Random.Range( 84,  86),  995, UnityEngine.Random.Range(-46, -48)),
				new Vector3(UnityEngine.Random.Range( 84,  88),  988, UnityEngine.Random.Range(-67, -70)),
				new Vector3(UnityEngine.Random.Range( 68,  83),  988, UnityEngine.Random.Range(-52, -66)),
				new Vector3(UnityEngine.Random.Range( 53,  68),  988, UnityEngine.Random.Range(-53, -63)),
				new Vector3(UnityEngine.Random.Range( 12,  49),  988, UnityEngine.Random.Range(-47, -66)),
				new Vector3(UnityEngine.Random.Range(  9,  11),  988, UnityEngine.Random.Range(-48, -51)),
				new Vector3(UnityEngine.Random.Range( 45,  48),  988, UnityEngine.Random.Range(-48, -51)),
				new Vector3(UnityEngine.Random.Range( 38,  42),  988, UnityEngine.Random.Range(-40, -47)),
				new Vector3(UnityEngine.Random.Range( 38,  43),  988, UnityEngine.Random.Range(-32, -38)),
				new Vector3(UnityEngine.Random.Range(-25,  12),  988, UnityEngine.Random.Range(-50, -66)),
				new Vector3(UnityEngine.Random.Range(-26, -56),  988, UnityEngine.Random.Range(-50, -66)),
				new Vector3(UnityEngine.Random.Range( -3, -24), 1001, UnityEngine.Random.Range(-66, -73)),
				new Vector3(UnityEngine.Random.Range(  5,  28), 1001, UnityEngine.Random.Range(-66, -73)),
				new Vector3(UnityEngine.Random.Range( 29,  55), 1001, UnityEngine.Random.Range(-66, -73)),
				new Vector3(UnityEngine.Random.Range( 50,  54), 1001, UnityEngine.Random.Range(-49, -66)),
				new Vector3(UnityEngine.Random.Range( 24,  48), 1001, UnityEngine.Random.Range(-41, -46)),
				new Vector3(UnityEngine.Random.Range(  5,  24), 1001, UnityEngine.Random.Range(-41, -46)),
				new Vector3(UnityEngine.Random.Range( -4, -17), 1001, UnityEngine.Random.Range(-41, -46)),
				new Vector3(UnityEngine.Random.Range(  4,  -4), 1001, UnityEngine.Random.Range(-25, -40)),
				new Vector3(UnityEngine.Random.Range( 11, -11), 1001, UnityEngine.Random.Range(-18, -21)),
				new Vector3(UnityEngine.Random.Range(  3,  -3), 1001, UnityEngine.Random.Range( -4, -17)),
				new Vector3(UnityEngine.Random.Range(  2,  14), 1001, UnityEngine.Random.Range(  3,  -3)),
				new Vector3(UnityEngine.Random.Range( -1, -13), 1001, UnityEngine.Random.Range(  4,  -3))
			};
		}
	}
	public class ContainRoom
    {
		public enum ContainmentRoom
        {
			//LightZone
			Lcz914Containment,
			Lcz173Containment,
			Lcz330Containment,
			Lcz330Controller,
			LczArmory,
			//HeavyZone
			HczHid,
			Hcz106Bottom,
			Hcz106FemurBreaker,
			Hcz079Hall,
			Hcz079Containment,
			Hcz049Containment,
			Hcz049Armory,
			Hcz096Containment,
			HczArmory,
			HczNuke,
			//EntranceZone
			EzIntercom,
			//SurfaceZone
			SurfaceNuke,
        }
		Dictionary<ContainmentRoom, List<Tuple<Vector3,Vector3>>> c = new()
        {
			//LightZone
			{ContainmentRoom.Lcz914Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(2.9f, 0, 10.1f), new Vector3(-10.2f, -5f, -10.2f)) } },
			{ContainmentRoom.Lcz173Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(-16.4f, -16.8f, -5.2f), new Vector3(-30.2f, -22.3f, -16.7f)) } },
			{ContainmentRoom.Lcz330Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(Vector3.one, Vector3.one), Tuple.Create(Vector3.one, Vector3.one), Tuple.Create(Vector3.one, Vector3.one) } },
			{ContainmentRoom.Lcz330Controller,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(Vector3.one, Vector3.one), Tuple.Create(Vector3.one, Vector3.one), Tuple.Create(Vector3.one, Vector3.one) } },
			{ContainmentRoom.LczArmory,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(1.2f, -1f, 6f), new Vector3(-9.5f, -10f, -7f)) } },
			//HeavyZone
			{ContainmentRoom.HczHid,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(3.7f, 0f, 9.8f), new Vector3(-4.0f, -5f, 7.4f)) } },
			{ContainmentRoom.Hcz106Bottom,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(9.6f, 20f, 30.8f), new Vector3(-24.4f, 13f, -1.9f)) } },
			{ContainmentRoom.Hcz106FemurBreaker,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(-25.6f, 20f, 32f), new Vector3(-33.7f, -10f, -4.6f)) } },
			{ContainmentRoom.Hcz079Hall,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(-12.3f, 7f, 18.7f), new Vector3(-20.8f, 0f, -2.5f)) } },
			{ContainmentRoom.Hcz079Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(10.3f, 10f, 22.5f), new Vector3(-8.2f, 0f, 5.2f)) } },
			{ContainmentRoom.Hcz049Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(9.3f, -260f, -11f), new Vector3(-9.6f, -270f, -16.8f)) } },
			{ContainmentRoom.Hcz049Armory,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(-3f, -260f, -4.6f), new Vector3(-8.6f, -270f, -10.1f)) } },
			{ContainmentRoom.Hcz096Containment,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(4.4f, 0f, 1.9f), new Vector3(0.5f, -5f, -1.9f)) } },
			{ContainmentRoom.HczArmory,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(new Vector3(0.1f, 0f, 2.9f), new Vector3(-5.6f, -5f, -2.8f)) } },
			{ContainmentRoom.HczNuke,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(Vector3.one, Vector3.one) } },
			//EntranceZone
			{ContainmentRoom.EzIntercom,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(Vector3.one, Vector3.one), Tuple.Create(Vector3.one, Vector3.one) } },
			//SurfaceZone
			{ContainmentRoom.SurfaceNuke,new List<Tuple<Vector3,Vector3>>{ Tuple.Create(Vector3.one, Vector3.one) } },
		};
	}
}