import { Dispatcher } from '@colyseus/command';
import { Client, Room } from 'colyseus';
import { JoinCommand, MoveCommand, LeaveCommand } from './battle-royale-matchmaking.commands';
import { BattleRoyaleMatchmakingState, Position } from './battle-royale-matchmaking.schemas';
import { SquadMemberSavedSettings } from './interfaces';

export class BattleRoyaleMatchmakingRoom extends Room<BattleRoyaleMatchmakingState> {
  static readonly roomName = 'battle_royale_matchmaking';
  private dispatcher = new Dispatcher(this);

  onCreate(): void {
    this.maxClients = 100; // Default is Integer.INFINITY, this crashes client
    this.setState(new BattleRoyaleMatchmakingState());


    this.onMessage('move', ({ sessionId }, pos: Position) =>
      this.dispatcher.dispatch(new MoveCommand(), { sessionId, pos }))
  }

  onJoin(client: Client, previousData: SquadMemberSavedSettings): void {
    this.dispatcher.dispatch(new JoinCommand(), { client, previousData });
  }

  onLeave(client: Client, consented?: boolean) {
    this.dispatcher.dispatch(new LeaveCommand(), { client, consented: !!consented });
  }
}