import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';

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
          .withUrl('https://localhost:5001/hubs/game', options)
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