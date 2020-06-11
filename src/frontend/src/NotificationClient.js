import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import config from './config';

class GameNotificationClient {
  constructor(onPlayerAdded, onGameStarted) {
    this.onPlayerAdded = onPlayerAdded;
    this.onGameStarted = onGameStarted;
  }

  async load(accessToken) {
    try {
        const options = {
          skipNegotiation: true,
		      transport: HttpTransportType.WebSockets,
          accessTokenFactory: () => accessToken
        };
      
        const connection = new HubConnectionBuilder()
          .withUrl(`${config.GameServiceUri}/hubs/game`, options)
          .withAutomaticReconnect()
          .build();
        await connection.start();

        connection.on('PlayerAdded', game => {
            console.log('Player added to game');
            this.onPlayerAdded(game);
        });

        connection.on('GameStarted', game => {
            console.log('Game started');
            this.onGameStarted(game);
        });
    } catch (error) {
        console.log(error);
    }
  }
}

export default GameNotificationClient;