import { Command } from '@colyseus/command';
import { Context, Schema, type } from '@colyseus/schema';
import { Client } from 'colyseus';
import { SquadArrangementState, SquadMember } from './squad-arrangement.schemas';
import roomLogger from './logger';

const log = roomLogger.extend('commands');

const ctx = new Context();

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

export class ReadyMessage extends Schema {
  @type('boolean', ctx)
  ready!: boolean;
}

export class ReadyCommand extends Command<SquadArrangementState, { sessionId: Client['sessionId'], msg: ReadyMessage }> {
  execute({ sessionId, msg }: this['payload']): void {
    const member: SquadMember = this.state.members[sessionId];
    log(`[Room %s] setting %s ready: %O`, this.room.roomId, member.name, msg.toJSON())
    member.ready = msg.ready;
  }
}
