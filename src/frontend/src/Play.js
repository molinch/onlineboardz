import React, { useState, useEffect } from 'react';
import { Card } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import CardIcon from './CardIcon';
import { GameType, GameTypeI18n } from './games/GameType';
import tictactoeLogo from './games/TicTacToe/logo.svg';
import { useTranslation } from 'react-i18next';

const { Meta } = Card;
const Join = () => CardIcon((<SmileOutlined key="join" />), (<SmileFilled key="join" />), "icon-join", "Join");
const Favorite = () => CardIcon((<HeartOutlined key="heart" />), (<HeartFilled key="heart" />), "icon-favorite", "Favorite");

function Play(props) {
    const [gameTypes, setGameTypes] = useState([]);

    useEffect(() => {
        if (!props.user) return;

        (async function() {
            try {
                const response = await fetch('https://localhost:5001/gameTypes/', props.user.getFetchOptions());
                var fetchedGameTypes = await response.json();
                setGameTypes(fetchedGameTypes);
            } catch (error) {
                console.log(error);
            }
        })();
    },[props.user]);

    const { t, i18n } = useTranslation();

    return (
        <div>
            <h1>Choose a game</h1>

            <div className="seek-cards">
                {gameTypes.map(g => {
                    const gameName = GameTypeI18n[g];

                    return (
                        <Card
                            key={gameName}
                            style={{ width: 300 }}
                            cover={
                            <img
                                alt="logo"
                                src={tictactoeLogo}
                            />
                            }
                            actions={[
                                <Join />,
                                <Favorite />,
                            ]}
                        >
                            <Meta
                                title={t(gameName)}
                                description={t(gameName+"-Desc")}
                            />
                        </Card>
                    );
                })}
            </div>
        </div>
    );
}

export default Play;