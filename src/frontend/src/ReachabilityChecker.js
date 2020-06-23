import { sleep } from './Utils';

class ReachabilityChecker {
    constructor(uri) {
        this._uri = uri;
        this._running = false;
        this._isReachable = true;
        this._callbacks = new Set();
    }

    start = () => {
        this._running = true;
        (async () => {
            while (this._running) {
                let reachable;
                try {
                    const result = fetch(this._uri, {
                        method: 'HEAD'
                    });

                    if (result.status) {
                        reachable = true;
                    } else {
                        reachable = false;
                    }
                } catch {
                    reachable = false;
                }

                if (reachable !== this._isReachable) {
                    this._isReachable = reachable;
                    this._callbacks.forEach(callback => {
                        try {
                            callback(reachable);
                        } catch (err) { console.log(err); }
                    });
                }

                await sleep(500);
            }
        })();
    }

    stop = () => {
        this._running = false;
    }

    addReachableChangedHandler(callback) {
        this._callbacks.add(callback);
    }

    removeReachableChangedHandler(callback) {
        this._callbacks.delete(callback);
    }
}

export default ReachabilityChecker;