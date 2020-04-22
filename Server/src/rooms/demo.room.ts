// tslint:disable: no-console

import { Context, MapSchema, Schema, type } from '@colyseus/schema';
import { Client, generateId, Room } from 'colyseus';

const ctx = new Context();

class Entity extends Schema {
  @type('number', ctx)
  x: number = 0;

  @type('number', ctx)
  y: number = 0;
}

class Player extends Entity {
  @type('boolean', ctx)
  connected: boolean = true;
}

class Enemy extends Entity {
  @type('number', ctx)
  power: number = Math.random() * 10;
}

class State extends Schema {
  @type({ map: Entity }, ctx)
  entities = new MapSchema<Entity>();
}

/**
 * Demonstrate sending schema data types as messages
 */
class Message extends Schema {
  @type('number', ctx) num!: number;
  @type('string', ctx) str!: string;
}

export class DemoRoom extends Room<State> {
  static readonly roomName = 'demo';

  static ENEMY_SPEED = 1;

  onCreate(options: any) {
    console.log('DemoRoom created!', options);

    this.setState(new State());
    this.populateEnemies();

    this.setMetadata({
      str: 'hello',
      number: 10,
    });

    this.setPatchRate(1000 / 20);
    this.setSimulationInterval((dt) => this.update(dt));

    this.onMessage(0, (client, message) => {
      client.send(0, message);
    });

    this.onMessage('schema', (client) => {
      const message = new Message();
      message.num = Math.floor(Math.random() * 100);
      message.str = 'sending to a single client';
      client.send(message);
    })

    this.onMessage('move_right', (client) => {
      this.state.entities[client.sessionId].x += 0.01;

      this.broadcast('hello', { hello: 'hello world' });
    });

    this.onMessage('*', (client: Client, msgType: string | number, message) => {
      console.log(`received message "${msgType}" from ${client.sessionId}:`, message);
    });
  }

  populateEnemies() {
    for (let i = 0; i <= 3; i++) {
      const enemy = new Enemy();
      enemy.x = Math.random() * 2;
      enemy.y = Math.random() * 2;
      this.state.entities[generateId()] = enemy;
    }
  }

  onJoin(client: Client, options: any) {
    console.log('client joined!', client.sessionId);
    this.state.entities[client.sessionId] = new Player();

    client.send('type', { hello: true });
  }

  async onLeave(client: Client, consented: boolean) {
    this.state.entities[client.sessionId].connected = false;

    try {
      if (consented) {
        throw new Error('consented leave!');
      }

      console.log('let\'s wait for reconnection!')
      const newClient = await this.allowReconnection(client, 10);
      console.log('reconnected!', newClient.sessionId);

    } catch (e) {
      console.log('disconnected!', client.sessionId);
      delete this.state.entities[client.sessionId];
    }
  }

  update(dt: number) {
    Object.values(this.state.entities)
      .filter(e => e instanceof Enemy)
      .forEach((e: Enemy) => {
        e.x += dt / 1000 * DemoRoom.ENEMY_SPEED;
      })
  }

  onDispose() {
    console.log('disposing DemoRoom...');
  }

}
