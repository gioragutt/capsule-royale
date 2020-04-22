import { MapSchema, Schema, type, Context } from '@colyseus/schema';

const ctx = new Context();

export class SquadMember extends Schema {
  @type('boolean', ctx)
  ready: boolean = false;

  @type('string', ctx)
  name!: string;

  constructor(initial: Partial<SquadMember> = {}) {
    super();
    Object.assign(this, initial);
  }
}

export class SquadArrangementState extends Schema {
  @type({ map: SquadMember }, ctx)
  members = new MapSchema<SquadMember>();

  @type('string', ctx)
  owner!: string;
}