import { Context, MapSchema, Schema, type } from '@colyseus/schema';

const ctx = new Context();

export class ReadyMessage extends Schema {
  @type('boolean', ctx)
  ready!: boolean;
}

export class SquadMember extends Schema {
  @type('boolean', ctx)
  ready: boolean = false;

  @type('string', ctx)
  name!: string;
}

export class SquadArrangementState extends Schema {
  @type({ map: SquadMember }, ctx)
  members = new MapSchema<SquadMember>();

  @type('string', ctx)
  owner!: string;
}