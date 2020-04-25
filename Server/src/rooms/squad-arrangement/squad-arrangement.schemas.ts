import { Context, MapSchema, Schema, type } from '@colyseus/schema';

const ctx = new Context();

export const enum SquadMemberState {
  NOT_READY = 'NOT_READY',
  READY = 'READY',
  DISCONNECTED = 'DISCONNECTED',
}

export class GameStartedMessage extends Schema {
  @type('string', ctx)
  roomId!: string;

  static create(roomId: string) {
    return Object.assign(new GameStartedMessage(), { roomId });
  }
}

export class ReadyMessage extends Schema {
  @type('boolean', ctx)
  ready!: boolean;
}

export class Position extends Schema {
  @type('number', ctx)
  x!: number;

  @type('number', ctx)
  y!: number;

  static create(x: number, y: number) {
    return Object.assign(new Position(), { x, y });
  }
}

export class SquadMember extends Schema {
  @type('string', ctx)
  id!: string;

  @type('string', ctx)
  state: SquadMemberState = SquadMemberState.NOT_READY;

  @type('string', ctx)
  name!: string;

  @type(Position, ctx)
  pos!: Position;

  static create({ id, name, pos }: Partial<SquadMember>) {
    return Object.assign(new SquadMember(), { id, name, pos });
  }
}

export class SquadArrangementState extends Schema {
  @type({ map: SquadMember }, ctx)
  members = new MapSchema<SquadMember>();

  @type('string', ctx)
  owner!: string;

  @type('boolean', ctx)
  readyToStart!: boolean;

  @type('boolean', ctx)
  started: boolean = false;
}