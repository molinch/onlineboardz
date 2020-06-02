import React, { useState } from 'react';
import { Card } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import tictactoeLogo from './games/TicTacToe/logo.svg';

const { Meta } = Card;

function Icon(normalIcon, hoveredIcon, className, text) {
    const [isHovered, setIsHovered] = useState(0);

    const icon = isHovered ?
        (hoveredIcon)
        :(normalIcon)

    const classes = `icon-text ${className}`
    return (
        <div class={classes} onMouseEnter={() => setIsHovered(true)} onMouseLeave={() => setIsHovered(false)}>
            {icon} <span class="text">{text}</span>
        </div>
    );
};

const Join = () => Icon((<SmileOutlined key="join" />), (<SmileFilled key="join" />), "icon-join", "Join");
const Favorite = () => Icon((<HeartOutlined key="heart" />), (<HeartFilled key="heart" />), "icon-favorite", "Favorite");

const SeekGame = () => (
    <div>
        <h1>Seek a game</h1>

        <div class="seek-cards">
            <Card
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
        </div>
    </div>
);

export default SeekGame;