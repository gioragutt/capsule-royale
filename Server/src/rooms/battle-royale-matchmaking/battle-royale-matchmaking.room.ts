import { Dispatcher } from '@colyseus/command';
import { Room, Client } from 'colyseus';
import { BattleRoyaleMatchmakingState } from './battle-royale-matchmaking.schemas';

export class BattleRoyaleMatchmakingRoom extends Room<BattleRoyaleMatchmakingState> {
  static readonly roomName = 'battle_royale_matchmaking';
  private dispatcher = new Dispatcher(this);

  onCreate(options?: any): void {
    console.log('CREATED BATTLE ROYALE ROOM - ', JSON.stringify(options));
    this.setState(new BattleRoyaleMatchmakingState());
  }

  onJoin?(client: Client, options?: any, auth?: any): void | Promise<any> {
    console.log(client.sessionId, 'joined');
  }
}