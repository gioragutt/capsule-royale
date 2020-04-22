import { Room, Client } from 'colyseus.js';

export function requestJoinOptions(this: Client, i: number) {
  return { requestNumber: i };
}

export function onJoin(this: Room) {
  console.log(this.sessionId, 'joined.');

  setTimeout(() => {
    console.log('asdasdads', this.state.owner);
    (this as any).send({ type: 'ready', ready: true });
  }, 5000);
}

export function onMessage(this: Room, message) {
  console.log(this.sessionId, 'received:', message);
}

export function onLeave(this: Room) {
  console.log(this.sessionId, 'left.');
}

export function onError(this: Room, code, err) {
  console.error(this.sessionId, '!! ERROR !!', code, err.message);
}

export function onStateChange(this: Room, state) {
  console.log(this.sessionId, "new state:", state);
}