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
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60*1000) { // we've been reconnecting for less than 60 seconds so far
                            return 2*1000;
                        } else if (retryContext.elapsedMilliseconds < 10*60*1000) { // we've been reconnecting for less than 10 minutes so far
                            return 5*1000;
                        } else {
                            return 10*1000;
                        }
                    }
                })
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