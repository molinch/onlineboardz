import { useDatabase } from '@nozbe/watermelondb/hooks';
import config from './config';

const useGameData = (fetchWithUi) => {
    const database = useDatabase();

    return {
        getGameTypes: async () => {
            debugger;

            const gameTypesCollection = database.collections.get('game_types');
            const data = await gameTypesCollection.query().fetch();
            if (data) return data;

            const fetched = await fetchWithUi.get(`${config.GameServiceUri}/gameTypes/`);
            if (fetched.error) return fetched.error;
            await gameTypesCollection.create(fetched);
            return fetched;
        }
    }
}

export default useGameData;