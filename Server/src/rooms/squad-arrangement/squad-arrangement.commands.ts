import { Command } from '@colyseus/command';
import { Client } from 'colyseus';
import roomLogger from './logger';
import { ReadyMessage, SquadArrangementState, SquadMember } from './squad-arrangement.schemas';

const toString = (obj: any) => obj && obj.toJSON ? obj.toJSON() : obj;

const log = roomLogger.extend('commands');

interface JoinOptions {
  sessionId: string;
  name?: string;
}

export class JoinCommand extends Command<SquadArrangementState, JoinOptions> {
  execute({ sessionId, name }: this['payload']): void {
    const currentSquadSize = Object.keys(this.state.members).length;
    const member = new SquadMember();
    member.name = name || `Member ${currentSquadSize + 1}`;

    if (currentSquadSize === 0) {
      log(`[Room %s] setting owner to %s`, this.room.roomId, member.name);
      this.state.owner = sessionId;
    }

    this.state.members[sessionId] = member;
    log(`[Room %s] %s joined: %O`, this.room.roomId, member.name, member.toJSON());
  }
}

export class ReadyCommand extends Command<SquadArrangementState, { sessionId: Client['sessionId'], msg: ReadyMessage }> {
  execute({ sessionId, msg }: this['payload']): void {
    const member: SquadMember = this.state.members[sessionId];
    log(`[Room %s] setting %s ready: %O`, this.room.roomId, member.name, toString(msg))
    member.ready = msg.ready;
  }
}
