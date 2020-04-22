import { Command } from '@colyseus/command';
import { Client } from 'colyseus';
import { MapSchema, Schema, type, Context } from '@colyseus/schema';
import { SquadArrangementState, SquadMember } from './squad-arrangement.schemas';

const ctx = new Context();

interface JoinOptions {
  sessionId: string;
  name?: string;
}

export class JoinCommand extends Command<SquadArrangementState, JoinOptions> {
  execute({ sessionId, name }: this['payload']): void {
    const currentSquadSize = Object.keys(this.state.members).length;
    if (currentSquadSize === 0) {
      this.state.owner = sessionId;
    }

    this.state.members[sessionId] = new SquadMember({
      name: name || `Member ${currentSquadSize}`,
    });
  }
}

export class ReadyMessage extends Schema {
  @type('boolean')
  ready!: boolean;
}

export class ReadyCommand extends Command<SquadArrangementState, { sessionId: Client['sessionId'], msg: ReadyMessage }> {
  execute({ sessionId, msg }: this['payload']): void {
    (this.state.members[sessionId] as SquadMember).ready = msg.ready;
  }
}
