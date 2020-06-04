import React, { useState, useEffect } from 'react';
import { Card } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import CardIcon from './CardIcon';
import tictactoeLogo from './games/TicTacToe/logo.svg';

const { Meta } = Card;

const Join = () => CardIcon((<SmileOutlined key="join" />), (<SmileFilled key="join" />), "icon-join", "Join");
const Favorite = () => CardIcon((<HeartOutlined key="heart" />), (<HeartFilled key="heart" />), "icon-favorite", "Favorite");

function JoinGame(props) {
    const [games, setGames] = useState([]);

    const getPendingGames = async () => {
        if (!props.user) return;

        try {
            const response = await fetch('https://localhost:5001/gameProposals/', props.user.getFetchOptions());
            var fetchedGames = await response.json();
            setGames(fetchedGames);
        } catch (error) {
            console.log(error);
        }
    };

    useEffect(() => {
        getPendingGames();
    });

    return (
        <div>
            <h1>Join a game waiting for players</h1>

            <div class="seek-cards">
                {games.map(() => {
                    return (
                        <Card
                            key=""
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
                                title="Tic Tac Toe"
                                description="Game for two players. The player who succeeds in placing three of their marks in a horizontal, vertical, or diagonal row is the winner."
                            />
                        </Card>
                    );
                })}
            </div>
        </div>
    );
}

export default JoinGame;