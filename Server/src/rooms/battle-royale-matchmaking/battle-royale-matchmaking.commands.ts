import { Command as BaseCommand } from '@colyseus/command';
import { Client, Room } from 'colyseus';
import { BattleRoyaleMatchmakingRoom } from '../battle-royale-matchmaking/battle-royale-matchmaking.room';
import { BattleRoyaleMatchmakingState, Player, Position } from './battle-royale-matchmaking.schemas';
import { SquadMemberSavedSettings } from './interfaces';
import roomLogger from './logger';

abstract class Command<Opts = any> extends BaseCommand<BattleRoyaleMatchmakingState, Opts> {
  room!: BattleRoyaleMatchmakingRoom;
}

const logger = roomLogger.extend('commands');

const log = (room: Room, format: string, ...args: any[]) =>
  logger(`[Room %s] ${format}`, room.roomId, ...args);

interface JoinOptions {
  client: Client;
  previousData: SquadMemberSavedSettings;
}

export class JoinCommand extends Command<JoinOptions> {
  execute({ client, previousData: { name } }: this['payload']): void {
    const player = Player.create({
      id: client.sessionId,
      name,
      pos: Position.create(Math.random() * 10 - 5, Math.random() * 10 - 5),
    });

    this.state.players[player.id] = player;
    log(this.room, `%s joined: %O`, player.name, player.toJSON());
  }
}

export class LeaveCommand extends Command<{ client: Client, consented: boolean }> {
  async execute({ client, consented }: this['payload']): Promise<void> {
    const player: Player = this.state.players[client.sessionId];
    player.connected = false;

    if (consented) {
      log(this.room, `${player.id}(${player.name}) left the room (consented)`)
      delete this.state.players[client.sessionId];
      return;
    }

    try {
      log(this.room, `${player.id}(${player.name}) disconnected, allowing reconnection`)
      await this.room.allowReconnection(client, 10);
      log(this.room, `${player.id}(${player.name}) reconnected!`)
      player.connected = true;
    } catch (e) {
      log(this.room, `${player.id}(${player.name}) left the room`)
      delete this.state.players[client.sessionId];
    }
  }
}


export class MoveCommand extends Command<{ sessionId: Client['sessionId'], pos: Position }> {
  execute({ sessionId, pos }: this['payload']): void {
    const member: Player = this.state.players[sessionId];
    log(this.room, `updating %s: %O`, member.name, pos);
    Object.assign(member.pos, pos);
  }
}