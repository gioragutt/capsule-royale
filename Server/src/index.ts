import http from "http";
import express from "express";
import cors from "cors";
import { Server } from "colyseus";
import { monitor } from "@colyseus/monitor";
// import socialRoutes from "@colyseus/social/express"

import { MyRoom } from "./MyRoom";
import { BattleRoyaleWaitRoom } from "./BattleRoyaleWaitRoom";
import { PartyLobbyRoom } from "./PartyLobbyRoom";
import { DemoRoom } from "./DemoRoom";

const port = Number(process.env.PORT || 2567);
const app = express()

app.use(cors());
app.use(express.json())

const server = http.createServer(app);
const gameServer = new Server({ server });

gameServer.define('demo', DemoRoom);
gameServer.define('my_room', MyRoom);
gameServer.define('battle_royale_wait_room', BattleRoyaleWaitRoom);
gameServer.define('party_lobby_room', PartyLobbyRoom);

/**
 * Register @colyseus/social routes
 *
 * - uncomment if you want to use default authentication (https://docs.colyseus.io/authentication/)
 * - also uncomment the import statement
 */
// app.use("/", socialRoutes);

// register colyseus monitor AFTER registering your room handlers
app.use("/colyseus", monitor());

gameServer.listen(port);
console.log(`Listening on ws://localhost:${port}`)
