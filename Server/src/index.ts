import { monitor } from '@colyseus/monitor';
import { Server } from 'colyseus';
import cors from 'cors';
import express from 'express';
import http from 'http';
import * as rooms from './rooms';

const port = Number(process.env.PORT || 2567);
const app = express()

app.use(cors());
app.use(express.json())

const server = http.createServer(app);
const gameServer = new Server({ server });

Object.values(rooms).forEach(room => {
  gameServer.define(room.roomName, room);
});

// register colyseus monitor AFTER registering your room handlers
app.use('/colyseus', monitor());

gameServer.listen(port);
// tslint:disable-next-line: no-console
console.log(`Listening on ws://localhost:${port}`)
