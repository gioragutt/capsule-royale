import { Room, Client } from 'colyseus';

export class BattleRoyaleWaitRoom extends Room {
  static readonly roomName = 'battle_royale_wait_room';

  onMessage(client: Client, data: any): void {
    throw new Error('Method not implemented.');
  }
}