import { Dispatcher } from '@colyseus/command';
import { Room } from 'colyseus';
import { BattleRoyaleState } from './battle-royale.schemas';

export class BattleRoyaleRoom extends Room<BattleRoyaleState> {
  static readonly roomName = 'squad_arrangement';
  private dispatcher = new Dispatcher(this);

  onCreate(): void {
    this.setState(new BattleRoyaleState());
  }
}