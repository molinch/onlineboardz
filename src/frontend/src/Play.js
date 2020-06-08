import React, { useState, useEffect } from 'react';
import { Card } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import CardIcon from './CardIcon';
import { GameTypeInfo } from './games/GameType';
import { useTranslation } from 'react-i18next';
import config from './config';

const { Meta } = Card;
const Join = props =>
    CardIcon(
        (<SmileOutlined key="join" onClick={props.onClick} />),
        (<SmileFilled key="join" onClick={props.onClick} />),
        "icon-join", "Join"
    );
const Favorite = props =>
    CardIcon(
        (<HeartOutlined key="heart" onClick={props.onClick} />),
        (<HeartFilled key="heart" onClick={props.onClick} />),
        "icon-favorite", "Favorite"
    );

const Play = props => {
    const { t } = useTranslation();
    const [gameTypes, setGameTypes] = useState([]);

    useEffect(() => {
        if (!props.user) return;

        (async function () {
            try {
                const response = await fetch(`${config.GameServiceUri}/gameTypes/`, props.user.getFetchOptions());
                var fetchedGameTypes = await response.json();
                setGameTypes(fetchedGameTypes);
            } catch (error) {
                console.log(error);
            }
        })();
    }, [props.user]);

    const onJoin = gameTypeInfo => {
        console.log(gameTypeInfo);

    }

    const onFavorite = gameTypeInfo => {

    }

    return (
        <div>
            <h1>{t('ChooseGame')}</h1>

            <div className="seek-cards">
                {gameTypes.map(g => {
                    const gameTypeInfo = GameTypeInfo[g.gameType];

                    return (
                        <Card
                            key={gameTypeInfo.name}
                            style={{ width: 300 }}
                            cover={
                                <img
                                    alt="logo"
                                    src={gameTypeInfo.logo}
                                />
                            }
                            actions={[
                                <Join onClick={() => onJoin(gameTypeInfo)} />,
                                <Favorite onClick={() => onFavorite(gameTypeInfo)} />
                            ]}
                        >
                            <Meta
                                title={t(gameTypeInfo.name)}
                                description={t(gameTypeInfo.name + "-Desc")}
                            />
                        </Card>
                    );
                })}
            </div>
        </div>
    );
}

export default Play;