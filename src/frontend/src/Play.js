import React, { useState, useEffect } from 'react';
import { Card } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import CardIcon from './CardIcon';
import { GameTypeInfo } from './games/GameType';
import { useTranslation } from 'react-i18next';
import config from './config';
import { navigate } from "@reach/router";

const { Meta } = Card;
const ChooseGame = props =>
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

const Play = ({ user, fetchWithUi }) => {
    const { t } = useTranslation();
    const [gameTypes, setGameTypes] = useState([]);
    const [error, setError] = useState(<></>);

    useEffect(() => {
        if (!user) return;

        (async () => {
            const response = await fetchWithUi.get(`${config.GameServiceUri}/gameTypes/`);
            if (response.error) {
                setError(response.error);
                return;
            }
            setGameTypes(response);
        })();
    }, [user, fetchWithUi]);

    const onGameChosen = gameTypeInfo => {
        navigate(`/play/${gameTypeInfo.name}`);
    }

    const onFavorite = gameTypeInfo => {

    }

    return (
        <div>
            <h1>{t('ChooseGame')}</h1>
            {error}

            <div className="seek-cards">
                {gameTypes.map(g => {
                    const gameTypeInfo = GameTypeInfo.ById(g.gameType);
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
                                <ChooseGame onClick={() => onGameChosen(gameTypeInfo)} />,
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