import localforage from 'localforage';
import config from '../config';

const getGameData = (fetchWithUi) => {
    return {
        getGameTypes: async () => {
            const gameTypes = await localforage.getItem('gameTypes');
            if (gameTypes) return gameTypes;

            const fetched = await fetchWithUi.get(`${config.GameServiceUri}/gameTypes/`);
            if (!fetched.error) {
                await localforage.setItem('gameTypes', fetched);
            }
            return fetched;
        }
    }
}

export default getGameData;