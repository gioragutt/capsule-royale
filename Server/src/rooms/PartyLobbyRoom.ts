import { Room, Client } from "colyseus";

export class PartyLobbyRoom extends Room {
  onMessage(client: Client, data: any): void {
    throw new Error("Method not implemented.");
  }
}