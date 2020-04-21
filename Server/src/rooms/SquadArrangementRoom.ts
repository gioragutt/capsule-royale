import { MapSchema, Schema, type } from '@colyseus/schema';
import { Client, Room } from 'colyseus';

class SquadMember extends Schema {
  @type('boolean')
  ready!: boolean;

  @type('string')
  name!: string;
}

class SquadArrangementState extends Schema {
  @type({ map: SquadMember })
  members = new MapSchema<SquadMember>();

  @type('string')
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