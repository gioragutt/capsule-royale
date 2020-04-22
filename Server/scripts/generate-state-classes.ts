import execa from 'execa';
import * as fs from 'fs';
import Listr, { ListrTask } from 'listr';
import * as path from 'path';
import readdirp from 'readdirp';

const roomsDir = path.join(__dirname, '..', 'src', 'rooms');
const csharpOutputDir = path.join(__dirname, '..', '..', 'Assets', 'States');
const roomFilesFilter = ['room', 'schemas', 'schema', 'commands', 'command'].map(s => `*.${s}.ts`)

const extractRoomName = (fileName: string) => {
  const [name] = fileName.split('.');
  return name.split('-').map(p => p[0].toUpperCase() + p.substring(1)).join('')
}

interface GenerationOpts {
  roomName: string;
  files: {
    shortFilePath: string;
    fullFilePath: string;
  }[]
}

const generateCSharpClasses = ({ roomName, files }: GenerationOpts): ListrTask => ({
  title: `Generate ${roomName} Schemas`,
  task: () => new Listr(files.map(({ fullFilePath, shortFilePath }) => ({
    title: shortFilePath,
    task: async () => {
      const namespace = `CapsuleRoyale.${roomName}`;
      const output = path.join(csharpOutputDir, roomName)
      await execa('npx', ['schema-codegen', fullFilePath, '--csharp', '--output', output, '--namespace', namespace])
    },
  }))),
});

async function main() {
  try {
    const roomFiles = await readdirp.promise(roomsDir, { fileFilter: roomFilesFilter })
    const generationTasks = Object.values(roomFiles
      .map(roomFile => ({
        shortFilePath: roomFile.path,
        fullFilePath: roomFile.fullPath,
        roomName: extractRoomName(roomFile.basename),
      }))
      .reduce((acc: Record<string, GenerationOpts>, { roomName, ...file }) => {
        acc[roomName] = acc[roomName] || { roomName, files: [] };
        acc[roomName].files.push(file);
        return acc;
      }, {}))
      .map(generateCSharpClasses);

    await new Listr([
      {
        title: `Ensure output path exists`,
        skip: () => fs.existsSync(csharpOutputDir) && `Output folder already exists 👍`,
        task: () => fs.mkdirSync(csharpOutputDir),
      },
      ...generationTasks,
    ]).run();
  } catch (e) {
    console.error(e);
  }
}

main();