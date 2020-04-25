// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.37
// 

using Colyseus.Schema;

namespace CapsuleRoyale.BattleRoyaleMatchmaking {
	public class Player : Schema {
		[Type(0, "string")]
		public string id = "";

		[Type(1, "boolean")]
		public bool connected = false;

		[Type(2, "string")]
		public string name = "";

		[Type(3, "ref", typeof(Position))]
		public Position pos = new Position();
	}
}
