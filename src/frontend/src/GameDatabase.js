import { Database } from '@nozbe/watermelondb'
import LokiJSAdapter from '@nozbe/watermelondb/adapters/lokijs'
import { gameSchema, GameType } from './Schema';

const adapter = new LokiJSAdapter({
    dbName: 'Game',
    schema: gameSchema,
    useWebWorker: false,
    useIncrementalIndexedDB: true,
})

export const gameDatabase = new Database({
    adapter,
    modelClasses: [GameType],
    actionsEnabled: true,
});