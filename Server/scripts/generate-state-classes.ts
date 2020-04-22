import execa from 'execa';
import * as fs from 'fs';
import Listr, { ListrTask } from 'listr';
import * as path from 'path';

const roomsDir = path.join(__dirname, '..', 'src', 'rooms');
const outputDir = path.join(__dirname, '..', '..', 'Assets', 'States');

const generationTasks = fs.readdirSync(roomsDir)
  .filter(f => f.endsWith('Room.ts'))
  .map(roomFile => ({
    filePath: path.join(roomsDir, roomFile),
    roomName: roomFile.replace('Room.ts', ''),
  }))
  .map(({ filePath, roomName }) => ({
    title: `Generate ${roomName}`,
    task: async () => {
      const namespace = `CapsuleRoyale.${roomName}`;
      const output = path.join(outputDir, roomName)
      await execa('npx', ['schema-codegen', filePath, '--csharp', '--output', output, '--namespace', namespace])
    },
  }) as ListrTask);

new Listr([
  {
    title: 'Ensure output path exists',
    skip: () => fs.existsSync(outputDir) ? 'Output folder already exists 👍' : false,
    task: () => fs.mkdirSync(outputDir),
  },
  ...generationTasks,
]).run().catch(console.error);
