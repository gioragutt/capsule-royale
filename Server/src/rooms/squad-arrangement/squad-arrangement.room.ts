import { Dispatcher } from '@colyseus/command';
import { Client, Room } from 'colyseus';
import { randomId } from '../../utils';
import { JoinCommand, LeaveCommand, MoveCommand, ReadyCommand, StartGameCommand } from './squad-arrangement.commands';
import { Position, ReadyMessage, SquadArrangementState } from './squad-arrangement.schemas';

export class SquadArrangementRoom extends Room<SquadArrangementState> {
  static readonly roomName = 'squad_arrangement';
  private dispatcher = new Dispatcher(this);

  onCreate(): void {
    this.roomId = randomId();
    this.setState(new SquadArrangementState());

    this.onMessage('ready', ({ sessionId }, ready: ReadyMessage) =>
      this.dispatcher.dispatch(new ReadyCommand(), { sessionId, ready }));

    this.onMessage('move', ({ sessionId }, pos: Position) =>
      this.dispatcher.dispatch(new MoveCommand(), { sessionId, pos }))

    this.onMessage('start', ({ sessionId }) =>
      this.dispatcher.dispatch(new StartGameCommand(), { sessionId }))
  }

  async onJoin(client: Client, options: any = {}): Promise<void> {
    this.dispatcher.dispatch(new JoinCommand(), { client, ...options });
  }

  onLeave(client: Client, consented?: boolean) {
    this.dispatcher.dispatch(new LeaveCommand(), { client, consented: !!consented });
  }
}