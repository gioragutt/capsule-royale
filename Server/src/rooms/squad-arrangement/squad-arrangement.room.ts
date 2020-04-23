import { Dispatcher } from '@colyseus/command';
import { Client, Room } from 'colyseus';
import { JoinCommand, ReadyCommand } from './squad-arrangement.commands';
import { ReadyMessage, SquadArrangementState } from './squad-arrangement.schemas';

export class SquadArrangementRoom extends Room<SquadArrangementState> {
  static readonly roomName = 'squad_arrangement';
  dispatcher = new Dispatcher(this);

  onCreate(): void {
    this.setState(new SquadArrangementState());

    this.onMessage('ready', ({ sessionId }, msg: ReadyMessage) =>
      this.dispatcher.dispatch(new ReadyCommand(), { sessionId, msg }));
  }

  async onJoin({sessionId}: Client, options?: any): Promise<void> {
    this.dispatcher.dispatch(new JoinCommand(), { sessionId, ...(options || {}) });
  }
}