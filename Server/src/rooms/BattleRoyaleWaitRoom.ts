import { Room, Client } from 'colyseus';

export class BattleRoyaleWaitRoom extends Room {
  onMessage(client: Client, data: any): void {
    throw new Error('Method not implemented.');
  }
}