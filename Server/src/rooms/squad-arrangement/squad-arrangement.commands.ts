import { Command as BaseCommand } from '@colyseus/command';
import { Client, Room } from 'colyseus';
import roomLogger from './logger';
import { SquadArrangementRoom } from './squad-arrangement.room';
import { Position, ReadyMessage, SquadArrangementState, SquadMember, SquadMemberState } from './squad-arrangement.schemas';

abstract class Command<Opts = any> extends BaseCommand<SquadArrangementState, Opts> {
  room!: SquadArrangementRoom;

  protected updateReadyState() {
    this.state.readyToStart = Object.values(this.state.members)
      .every((m: SquadMember) => m.state === SquadMemberState.READY);
  }
}

const toString = (obj: any) => obj && obj.toJSON ? obj.toJSON() : obj;

const logger = roomLogger.extend('commands');

const log = (room: Room, format: string, ...args: any[]) =>
  logger(`[Room %s] ${format}`, room.roomId, ...args);

interface JoinOptions {
  client: Client;
  name?: string;
}

export class JoinCommand extends Command<JoinOptions> {
  execute({ client, name }: this['payload']): void {
    const currentSquadSize = Object.keys(this.state.members).length;
    const member = SquadMember.create({
      id: client.sessionId,
      name: name || `Member ${currentSquadSize + 1}`,
      pos: Position.create(Math.random() * 10 - 5, Math.random() * 10 - 5),
    });

    if (currentSquadSize === 0) {
      log(this.room, `setting owner to %s`, member.name)
      this.state.owner = member.id;
    }

    this.state.members[member.id] = member;
    log(this.room, `%s joined: %O`, member.name, member.toJSON());

    this.updateReadyState();
  }
}

export class ReadyCommand extends Command<{ sessionId: Client['sessionId'], ready: ReadyMessage }> {
  execute({ sessionId, ready }: this['payload']): void {
    const member: SquadMember = this.state.members[sessionId];
    log(this.room, `setting %s ready: %O`, member.name, toString(ready))
    member.state = ready.ready ? SquadMemberState.READY : SquadMemberState.NOT_READY;
    this.updateReadyState();
  }
}

export class MoveCommand extends Command<{ sessionId: Client['sessionId'], pos: Position }> {
  execute({ sessionId, pos }: this['payload']): void {
    const member: SquadMember = this.state.members[sessionId];
    log(this.room, `updating %s: %O`, member.name, pos);
    Object.assign(member.pos, pos);
  }
}

export class LeaveCommand extends Command<{ client: Client, consented: boolean }> {
  validate({ consented }: this['payload']): boolean {
    return !consented;
  }

  async execute({ client }: this['payload']): Promise<void> {
    const member: SquadMember = this.state.members[client.sessionId];
    member.state = SquadMemberState.DISCONNECTED;

    try {
      log(this.room, `${member.id}(${member.name}) disconnected, allowing reconnection`)
      await this.room.allowReconnection(client, 10);
      log(this.room, `${member.id}(${member.name}) reconnected!`)
      member.state = SquadMemberState.NOT_READY;
    } catch (e) {
      log(this.room, `${member.id}(${member.name}) left the room`)
      delete this.state.members[client.sessionId];
    }
  }
}

export class StartGameCommand extends Command<{ sessionId: Client['sessionId'] }> {
  execute({ sessionId }: this['payload']): void {
    if (this.state.started) {
      console.log('Game already started');
      return;
    }

    if (sessionId !== this.state.owner || !this.state.readyToStart) {
      return;
    }

    this.state.started = true;

    // Start new game
    console.log('-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-');
    console.log('                  STARTING THE GAME                  ');
    console.log('-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-');
  }
}