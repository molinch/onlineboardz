import { HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import config from './config';
import { sleep } from './Utils';

class GameNotificationClient {
    constructor() {
        this._handlers = new Set();
    }

    nextRetry = retryContext => {
        console.log(retryContext);

        if (retryContext.elapsedMilliseconds < 60*1000) { // we've been reconnecting for less than 60 seconds so far
            return 2*1000;
        } else if (retryContext.elapsedMilliseconds < 10*60*1000) { // we've been reconnecting for less than 10 minutes so far
            return 5*1000;
        } else {
            return 10*1000;
        }
    }

    _connect = async accessToken => {
        const options = {
            skipNegotiation: true,
            transport: HttpTransportType.WebSockets,
            accessTokenFactory: () => accessToken
        };

        this._connection = new HubConnectionBuilder()
            .withUrl(`${config.GameServiceUri}/hubs/game`, options)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: this.nextRetry
            })
            .build();
        await this._connection.start();
    }
    
    addHandler = (methodName, callback) => {
        const handler = { methodName, callback };
        this._handlers.add(handler);

        if (this._connection) {
            try {
                this._connection.on(handler.methodName, data => handler.callback(data));
            } catch (err) { console.log(err); }
        }

        return () => this._removeHandler(handler);
    }

    _removeHandler = (handler) => {
        // for whatever reason signalr cannot take the callback directly, it needs to be wrapped in another arrow function directly passed to it...
        // that makes the unsubscribing more complex since that exact callback shall be used, but we cannot use it, as it was only directly passed
        // storing it, then passing it, doesn't work, it needs to be passed directly (I have no clue why)
        // hence to remove a handler, we remove them all, and add them back... nasty!
        this._handlers.delete(handler);
        this._connection.off(handler.methodName)
        this._handlers.forEach(h => {
            if (handler.methodName === h.methodName) {
                this._connection.on(h.methodName, data => h.callback(data));
            }
        });
    }

    load = async accessToken => {
        let elapsedMilliseconds = 0;
        let previousRetryCount = 0;
        
        while (true) {
            try {
                await this._connect(accessToken);
                this._handlers.forEach(h => {
                    this._connection.on(h.methodName, data => h.callback(data));
                });
                return;
            } catch (error) {
                const retryContext = { elapsedMilliseconds, previousRetryCount, retryReason: error };
                const nextRetry = this.nextRetry(retryContext);
                await sleep(nextRetry);
                elapsedMilliseconds += nextRetry;
                previousRetryCount++;
            }
        }
    }
}

export default GameNotificationClient;