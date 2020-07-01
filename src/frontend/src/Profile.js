import React, { useState, useEffect } from 'react';
import { GameTypeInfo } from './games/GameType';
import { useTranslation } from 'react-i18next';
import { Link } from "@reach/router";
import config from './config';

const Profile = ({ user, fetchWithUi, setError }) => {
    const { t } = useTranslation();
    const [myGames, setMyGames] = useState([]);
    useEffect(() => {
        if (!user) return;
        
        (async () => {
            const fetchedGames = await fetchWithUi.get(`${config.GameServiceUri}/games/mine`);
            if (fetchedGames.error) {
                setError(fetchedGames.error);
                return;
            }
            setMyGames(fetchedGames);
        })();
    }, [user, fetchWithUi, setError]);
    
    return (
        <div class="my-games">
            <h3>My games</h3>
            <ul>
            {myGames.map(g => {
                const gameTypeInfo = GameTypeInfo.ById(g.gameType);
                const uri = `/games/${gameTypeInfo.name}/${g.id}`;
                return (<li key={"my-game-"+uri}><Link to={uri}>{t(gameTypeInfo.name)} ({t(`GameStatus_${g.status}`)})</Link></li>);
            })}
            </ul>
        </div>
    );
};

export default Profile;