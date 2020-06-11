import React, { useEffect, useState } from 'react';
import { Trans, useTranslation } from 'react-i18next';
import { Button } from 'antd';
import config from './config';
import { GameTypeInfo } from './games/GameType';
import { navigate } from '@reach/router';

const PlayGame = props => {
    const { t } = useTranslation();
    const [error, setError] = useState([]);
    const gameInfo = GameTypeInfo.ByName(props.gameName);

    /*
    useEffect(() => {
        (async function () {
            try {
                const statuses = GameStatus.WaitingForPlayers;
                const response = await fetch(`${config.GameServiceUri}/games?types=${gameInfo.type}&statuses=${statuses}`, props.getFetchOptions());
                const fetchedGames = await response.json();
                setGames(fetchedGames);
            } catch (error) {
                console.log(error);
            }
        })();
    }, [props, gameInfo]);
    */

    const join = () => {
        (async () => {
            const response = await props.fetchWithUi.patch(
                `${config.GameServiceUri}/gameProposals/joinAny`,
                { gameType: gameInfo.type }
            );
            if (response.error) {
                setError(response.error);
                return;
            }
            debugger;
            const gameTypeInfo = GameTypeInfo.ById(response.metadata.gameType);
            navigate(`/games/${gameTypeInfo.name}/${response.id}`);
        })();
    };

    const create = () => {

    };

    // most likely we want to support additional options when joining

    /*
    
                {games.map(g => {
                    return (<div>{g.metadata.players.map(p => p.name).join(', ')}</div>);
                })}*/

    return (
        <div>
            <h1>
                <Trans i18nKey="PlayGame">
                    Play {{game: t(gameInfo.name)}}
                </Trans>

                <Button onClick={join}>
                    Join an existing game
                </Button>

                (see game preferences)

                OR

                <Button onClick={create}>
                    Create a new game, that your friends or others can join
                </Button>
                

            </h1>
        </div>
    );
};

export default PlayGame;