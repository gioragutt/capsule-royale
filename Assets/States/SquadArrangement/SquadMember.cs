// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 0.5.37
// 

using Colyseus.Schema;

namespace CapsuleRoyale.SquadArrangement.Schemas {
	public class SquadMember : Schema {
		[Type(0, "boolean")]
		public bool ready = false;

		[Type(1, "string")]
		public string name = "";
	}
}
