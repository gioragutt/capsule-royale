import { MapSchema, Schema, type, Context } from '@colyseus/schema';
import { Client, Room } from 'colyseus';

const context = new Context();

class SquadMember extends Schema {
  @type('boolean', context)
  ready!: boolean;

  @type('string', context)
  name!: string;
}

class SquadArrangementState extends Schema {
  @type({ map: SquadMember }, context)
  members = new MapSchema<SquadMember>();

  @type('string', context)
  owner!: string;
}

export class SquadArrangementRoom extends Room<SquadArrangementState> {
  static readonly roomName = 'squad_arrangement';

  async onJoin(client: Client, options?: any, auth?: any): Promise<any> {
    const currentSquadSize = Object.keys(this.state.members).length;
    if (currentSquadSize === 0) {
      this.state.owner = client.sessionId;
    }
    const member = new SquadMember();
    member.name = options.name || `Member ${currentSquadSize}`;
    member.ready = false;

    this.state.members[client.sessionId] = member;
  }

  onMessage(client: Client, data: any): void {
    throw new Error('Method not implemented.');
  }
}